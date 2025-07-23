using UnityEngine;

/// <summary>
/// Test script for the new animated (non-physics) explosion system
/// Press T to trigger a predictable, controllable explosion
/// </summary>
public class ExplosionAnimatedTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private KeyCode testKey = KeyCode.T;
    [SerializeField] private GameObject animatedExplosionPrefab;
    [SerializeField] private bool autoFindPrefab = true;
    
    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 5, 10);
    [SerializeField] private bool spawnRelativeToPlayer = true;
    [SerializeField] private bool destroyPreviousExplosion = true;
    
    [Header("Test Parameters")]
    [SerializeField] private float testSeparationDistance = 1.5f;
    [SerializeField] private float testSeparationDuration = 0.8f;
    [SerializeField] private float testUpwardBias = 0.3f;
    [SerializeField] private float testRandomness = 0.4f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool logExplosionStats = true;

    // Internal references
    private Transform playerTransform;
    private GameObject currentExplosion;
    private int explosionCount = 0;

    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Auto-find prefab if needed
        if (autoFindPrefab && animatedExplosionPrefab == null)
        {
            // Try to load from Resources
            animatedExplosionPrefab = Resources.Load<GameObject>("HelicopterExplosionPrefab");
            
            if (animatedExplosionPrefab != null && showDebugInfo)
            {
                Debug.Log("ExplosionAnimatedTester: Auto-found explosion prefab in Resources");
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"ExplosionAnimatedTester: Ready! Press {testKey} to test animated explosion");
            Debug.Log($"  - Separation Distance: {testSeparationDistance}");
            Debug.Log($"  - Separation Duration: {testSeparationDuration}s");
            Debug.Log($"  - Upward Bias: {testUpwardBias}");
            Debug.Log($"  - Randomness: {testRandomness}");
        }
    }

    void Update()
    {
        // Test key input
        if (Input.GetKeyDown(testKey))
        {
            TriggerAnimatedExplosion();
        }

        // Log stats periodically if explosion is active
        if (logExplosionStats && currentExplosion != null)
        {
            HelicopterExplosionAnimated explosion = currentExplosion.GetComponent<HelicopterExplosionAnimated>();
            if (explosion != null)
            {
                ExplosionAnimationStats stats = explosion.GetExplosionStats();
                
                // Log every 0.5 seconds
                if (Time.time % 0.5f < 0.1f)
                {
                    Debug.Log($"Explosion Stats: {stats.completedShards}/{stats.totalShards} shards completed ({stats.progress:P0}) - Time: {stats.explosionTime:F1}s");
                }
            }
        }
    }

    /// <summary>
    /// Trigger a new animated explosion for testing
    /// </summary>
    public void TriggerAnimatedExplosion()
    {
        if (animatedExplosionPrefab == null)
        {
            Debug.LogError("ExplosionAnimatedTester: No explosion prefab assigned!");
            return;
        }

        // Destroy previous explosion if requested
        if (destroyPreviousExplosion && currentExplosion != null)
        {
            if (showDebugInfo)
            {
                Debug.Log("ExplosionAnimatedTester: Destroying previous explosion");
            }
            Destroy(currentExplosion);
        }

        // Calculate spawn position
        Vector3 spawnPosition;
        if (spawnRelativeToPlayer && playerTransform != null)
        {
            spawnPosition = playerTransform.position + spawnOffset;
        }
        else
        {
            spawnPosition = transform.position + spawnOffset;
        }

        // Instantiate explosion
        currentExplosion = Instantiate(animatedExplosionPrefab, spawnPosition, Quaternion.identity);
        explosionCount++;

        // Replace the old physics-based component with the new animated one
        HelicopterExplosion oldExplosion = currentExplosion.GetComponent<HelicopterExplosion>();
        if (oldExplosion != null)
        {
            if (showDebugInfo)
            {
                Debug.Log("ExplosionAnimatedTester: Replacing old physics-based explosion with animated version");
            }
            
            // Destroy the old component
            DestroyImmediate(oldExplosion);
        }

        // Add the new animated component if not present
        HelicopterExplosionAnimated animatedExplosion = currentExplosion.GetComponent<HelicopterExplosionAnimated>();
        if (animatedExplosion == null)
        {
            animatedExplosion = currentExplosion.AddComponent<HelicopterExplosionAnimated>();
        }

        // Set custom parameters
        animatedExplosion.SetExplosionParameters(
            testSeparationDistance, 
            testSeparationDuration, 
            testUpwardBias, 
            testRandomness
        );

        if (showDebugInfo)
        {
            Debug.Log($"ExplosionAnimatedTester: Triggered animated explosion #{explosionCount} at {spawnPosition}");
            Debug.Log($"  - Using parameters: Distance={testSeparationDistance}, Duration={testSeparationDuration}s");
        }
    }

    /// <summary>
    /// Set test parameters at runtime
    /// </summary>
    public void SetTestParameters(float distance, float duration, float upward, float randomness)
    {
        testSeparationDistance = distance;
        testSeparationDuration = duration;
        testUpwardBias = upward;
        testRandomness = randomness;

        if (showDebugInfo)
        {
            Debug.Log($"ExplosionAnimatedTester: Updated parameters - Distance={distance}, Duration={duration}s, Upward={upward}, Random={randomness}");
        }
    }

    /// <summary>
    /// Quick test with gentle parameters
    /// </summary>
    [ContextMenu("Test Gentle Explosion")]
    public void TestGentleExplosion()
    {
        SetTestParameters(1.0f, 1.2f, 0.2f, 0.3f);
        TriggerAnimatedExplosion();
    }

    /// <summary>
    /// Quick test with dramatic parameters
    /// </summary>
    [ContextMenu("Test Dramatic Explosion")]
    public void TestDramaticExplosion()
    {
        SetTestParameters(2.5f, 0.6f, 0.5f, 0.6f);
        TriggerAnimatedExplosion();
    }

    /// <summary>
    /// Quick test with minimal parameters (for screenshots)
    /// </summary>
    [ContextMenu("Test Minimal Explosion")]
    public void TestMinimalExplosion()
    {
        SetTestParameters(0.8f, 1.5f, 0.1f, 0.2f);
        TriggerAnimatedExplosion();
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        // Simple on-screen instructions
        GUI.Label(new Rect(10, 10, 300, 20), $"Press {testKey} for Animated Explosion Test");
        GUI.Label(new Rect(10, 30, 300, 20), $"Explosions Triggered: {explosionCount}");
        
        if (currentExplosion != null)
        {
            HelicopterExplosionAnimated explosion = currentExplosion.GetComponent<HelicopterExplosionAnimated>();
            if (explosion != null)
            {
                ExplosionAnimationStats stats = explosion.GetExplosionStats();
                GUI.Label(new Rect(10, 50, 300, 20), $"Current: {stats.completedShards}/{stats.totalShards} shards ({stats.progress:P0})");
                GUI.Label(new Rect(10, 70, 300, 20), $"Time: {stats.explosionTime:F1}s");
            }
        }

        // Parameter controls
        GUI.Label(new Rect(10, 100, 200, 20), "Test Parameters:");
        GUI.Label(new Rect(10, 120, 150, 20), $"Distance: {testSeparationDistance:F1}");
        GUI.Label(new Rect(10, 140, 150, 20), $"Duration: {testSeparationDuration:F1}s");
        GUI.Label(new Rect(10, 160, 150, 20), $"Upward: {testUpwardBias:F1}");
        GUI.Label(new Rect(10, 180, 150, 20), $"Random: {testRandomness:F1}");
    }

    void OnDrawGizmosSelected()
    {
        // Draw spawn position
        Vector3 spawnPos;
        if (spawnRelativeToPlayer && playerTransform != null)
        {
            spawnPos = playerTransform.position + spawnOffset;
        }
        else
        {
            spawnPos = transform.position + spawnOffset;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(spawnPos, 0.5f);
        Gizmos.DrawWireSphere(spawnPos, testSeparationDistance);

        // Draw line to spawn position
        Gizmos.color = Color.yellow;
        Vector3 fromPos = playerTransform != null ? playerTransform.position : transform.position;
        Gizmos.DrawLine(fromPos, spawnPos);
    }
}
