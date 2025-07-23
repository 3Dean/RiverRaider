using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Simple, predictable helicopter explosion using direct transform animation
/// No physics complications - just smooth, controllable movement
/// </summary>
public class HelicopterExplosionAnimated : MonoBehaviour
{
    [Header("Explosion Parameters")]
    [SerializeField] private float separationDistance = 1.5f; // How far pieces separate
    [SerializeField] private float separationDuration = 0.8f; // How long separation takes
    [SerializeField] private float upwardBias = 0.3f; // Slight upward tendency
    [SerializeField] private float randomness = 0.4f; // Random variation in directions
    
    [Header("Animation Timing")]
    [SerializeField] private float explosionDelay = 0.1f; // Delay before pieces start moving
    [SerializeField] private float staggerDelay = 0.02f; // Delay between each piece starting
    [SerializeField] private bool useStaggeredStart = true; // Start pieces at slightly different times
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject explosionFlashPrefab;
    [SerializeField] private GameObject fireAndSmokePrefab;
    [SerializeField] private GameObject sparksPrefab;
    [SerializeField] private bool createGroundScorch = false; // Disabled - shadow looks artificial
    
    [Header("Audio")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip[] metalCollisionSounds;
    [SerializeField] private AudioClip fireCracklingSound;
    [SerializeField] private float audioVolume = 1f;
    
    [Header("Cleanup")]
    [SerializeField] private float shardLifetime = 20f;
    [SerializeField] private float cleanupDistance = 200f;
    [SerializeField] private bool enableDistanceCleanup = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showSeparationGizmos = false;

    // Internal components
    private List<ExplosionShardAnimated> explosionShards = new List<ExplosionShardAnimated>();
    private AudioSource audioSource;
    private Transform playerTransform;
    private Vector3 explosionCenter;
    private bool hasExploded = false;
    private int completedShards = 0;

    // Performance tracking
    private float explosionStartTime;

    void Awake()
    {
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = audioVolume;
        }

        // Find player for distance calculations
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Cache explosion center
        explosionCenter = transform.position;
    }

    void Start()
    {
        // Initialize all shards
        InitializeShards();
        
        // Start explosion immediately
        if (!hasExploded)
        {
            StartCoroutine(ExecuteExplosion());
        }
    }

    void Update()
    {
        // Don't cleanup until explosion has actually started
        if (!hasExploded) return;

        // Handle distance-based cleanup
        if (enableDistanceCleanup && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer > cleanupDistance)
            {
                CleanupExplosion();
                return;
            }
        }

        // Handle lifetime cleanup
        if (Time.time - explosionStartTime > shardLifetime)
        {
            CleanupExplosion();
        }
    }

    /// <summary>
    /// Initialize all explosion shards with the new animated component
    /// </summary>
    private void InitializeShards()
    {
        // Find all child objects that should become shards
        Transform[] childTransforms = GetComponentsInChildren<Transform>();
        
        foreach (Transform child in childTransforms)
        {
            if (child == transform) continue; // Skip parent

            // Remove any existing physics components
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                DestroyImmediate(rb);
            }

            // Remove old explosion shard component if present
            ExplosionShard oldShard = child.GetComponent<ExplosionShard>();
            if (oldShard != null)
            {
                DestroyImmediate(oldShard);
            }

            // Add new animated shard component
            ExplosionShardAnimated shard = child.GetComponent<ExplosionShardAnimated>();
            if (shard == null)
            {
                shard = child.gameObject.AddComponent<ExplosionShardAnimated>();
            }

            // Calculate separation direction for this shard
            Vector3 directionFromCenter = (child.position - explosionCenter);
            if (directionFromCenter.magnitude < 0.1f)
            {
                // If too close to center, use random direction
                directionFromCenter = Random.insideUnitSphere;
            }
            
            // Normalize and add upward bias
            Vector3 separationDirection = directionFromCenter.normalized;
            separationDirection.y += upwardBias;
            separationDirection = separationDirection.normalized;
            
            // Add randomness
            Vector3 randomOffset = Random.insideUnitSphere * randomness;
            separationDirection = (separationDirection + randomOffset).normalized;

            // Initialize the shard
            shard.Initialize(this, separationDirection, separationDistance, metalCollisionSounds);
            explosionShards.Add(shard);
        }

        if (showDebugInfo)
        {
            Debug.Log($"HelicopterExplosionAnimated: Initialized {explosionShards.Count} animated shards");
        }
    }

    /// <summary>
    /// Execute the explosion sequence
    /// </summary>
    private IEnumerator ExecuteExplosion()
    {
        if (hasExploded) yield break;

        hasExploded = true;
        explosionStartTime = Time.time;
        completedShards = 0;

        // Play explosion sound
        PlayExplosionSound();

        // Create visual effects
        CreateVisualEffects();

        // Wait for initial delay
        yield return new WaitForSeconds(explosionDelay);

        // Start all shard animations
        if (useStaggeredStart)
        {
            // Start shards with small delays for more natural look
            for (int i = 0; i < explosionShards.Count; i++)
            {
                if (explosionShards[i] != null)
                {
                    explosionShards[i].StartExplosion();
                    
                    if (staggerDelay > 0f)
                    {
                        yield return new WaitForSeconds(staggerDelay);
                    }
                }
            }
        }
        else
        {
            // Start all shards simultaneously
            foreach (ExplosionShardAnimated shard in explosionShards)
            {
                if (shard != null)
                {
                    shard.StartExplosion();
                }
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"HelicopterExplosionAnimated: Started explosion with {explosionShards.Count} shards");
        }
    }

    /// <summary>
    /// Create visual effects for the explosion
    /// </summary>
    private void CreateVisualEffects()
    {
        // Main explosion flash
        if (explosionFlashPrefab != null)
        {
            GameObject flash = Instantiate(explosionFlashPrefab, explosionCenter, Quaternion.identity);
            Destroy(flash, 2f);
        }

        // Fire and smoke
        if (fireAndSmokePrefab != null)
        {
            GameObject fireSmoke = Instantiate(fireAndSmokePrefab, explosionCenter, Quaternion.identity);
            Destroy(fireSmoke, 10f);
        }

        // Sparks
        if (sparksPrefab != null)
        {
            GameObject sparks = Instantiate(sparksPrefab, explosionCenter, Quaternion.identity);
            Destroy(sparks, 3f);
        }

        // Ground scorch mark
        if (createGroundScorch)
        {
            CreateGroundScorch();
        }
    }

    /// <summary>
    /// Create a scorch mark on the ground
    /// </summary>
    private void CreateGroundScorch()
    {
        // Raycast down to find ground
        RaycastHit hit;
        if (Physics.Raycast(explosionCenter, Vector3.down, out hit, 50f))
        {
            // Create a simple dark circle on the ground
            GameObject scorch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            scorch.name = "ExplosionScorch";
            scorch.transform.position = hit.point + Vector3.up * 0.01f;
            scorch.transform.localScale = new Vector3(separationDistance * 1.2f, 0.01f, separationDistance * 1.2f);
            
            // Make it dark
            Renderer renderer = scorch.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.2f, 0.1f, 0.1f, 0.8f);
            }

            // Remove collider
            Destroy(scorch.GetComponent<Collider>());

            // Auto-cleanup
            Destroy(scorch, 30f);
        }
    }

    /// <summary>
    /// Play explosion sound effects
    /// </summary>
    private void PlayExplosionSound()
    {
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound, audioVolume);
        }

        // Play fire crackling sound with delay
        if (fireCracklingSound != null)
        {
            StartCoroutine(PlayDelayedFireSound());
        }
    }

    /// <summary>
    /// Play fire crackling sound with delay
    /// </summary>
    private IEnumerator PlayDelayedFireSound()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (audioSource != null && fireCracklingSound != null)
        {
            audioSource.PlayOneShot(fireCracklingSound, audioVolume * 0.6f);
        }
    }

    /// <summary>
    /// Called by shards when their animation is complete
    /// </summary>
    public void OnShardAnimationComplete()
    {
        completedShards++;
        
        if (showDebugInfo && completedShards % 10 == 0)
        {
            Debug.Log($"HelicopterExplosionAnimated: {completedShards}/{explosionShards.Count} shards completed");
        }
        
        // Check if all shards are done
        if (completedShards >= explosionShards.Count)
        {
            if (showDebugInfo)
            {
                Debug.Log("HelicopterExplosionAnimated: All shards completed animation");
            }
        }
    }

    /// <summary>
    /// Get explosion progress (0-1)
    /// </summary>
    public float GetExplosionProgress()
    {
        if (!hasExploded) return 0f;
        if (explosionShards.Count == 0) return 1f;
        
        return (float)completedShards / explosionShards.Count;
    }

    /// <summary>
    /// Check if explosion is complete
    /// </summary>
    public bool IsExplosionComplete()
    {
        return hasExploded && completedShards >= explosionShards.Count;
    }

    /// <summary>
    /// Set custom explosion parameters
    /// </summary>
    public void SetExplosionParameters(float distance, float duration, float upward = 0.3f, float random = 0.4f)
    {
        separationDistance = distance;
        separationDuration = duration;
        upwardBias = upward;
        randomness = random;
    }

    /// <summary>
    /// Clean up the entire explosion
    /// </summary>
    private void CleanupExplosion()
    {
        if (showDebugInfo)
        {
            Debug.Log("HelicopterExplosionAnimated: Cleaning up explosion");
        }

        // Stop all shard animations
        foreach (ExplosionShardAnimated shard in explosionShards)
        {
            if (shard != null)
            {
                shard.StopAnimation();
                Destroy(shard.gameObject);
            }
        }

        // Destroy the explosion object
        Destroy(gameObject);
    }

    /// <summary>
    /// Get explosion statistics for debugging
    /// </summary>
    public ExplosionAnimationStats GetExplosionStats()
    {
        return new ExplosionAnimationStats
        {
            totalShards = explosionShards.Count,
            completedShards = completedShards,
            explosionTime = hasExploded ? Time.time - explosionStartTime : 0f,
            separationDistance = separationDistance,
            separationDuration = separationDuration,
            progress = GetExplosionProgress()
        };
    }

    void OnDrawGizmosSelected()
    {
        if (!showSeparationGizmos) return;

        // Draw explosion center
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        // Draw separation radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationDistance);

        // Draw shard separation targets (if exploded)
        if (hasExploded && Application.isPlaying)
        {
            Gizmos.color = Color.green;
            foreach (ExplosionShardAnimated shard in explosionShards)
            {
                if (shard != null)
                {
                    // This would require exposing the separation target from the shard
                    // For now, just show current positions
                    Gizmos.DrawWireSphere(shard.transform.position, 0.05f);
                }
            }
        }
    }
}

/// <summary>
/// Statistics structure for explosion debugging
/// </summary>
[System.Serializable]
public struct ExplosionAnimationStats
{
    public int totalShards;
    public int completedShards;
    public float explosionTime;
    public float separationDistance;
    public float separationDuration;
    public float progress;
}
