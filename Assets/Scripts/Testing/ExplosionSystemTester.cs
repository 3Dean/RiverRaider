using UnityEngine;

/// <summary>
/// Test script for the helicopter explosion system
/// Provides manual testing controls and debugging information
/// </summary>
public class ExplosionSystemTester : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private GameObject helicopterExplosionPrefab;
    [SerializeField] private Transform testSpawnPoint;
    [SerializeField] private bool autoFindPrefab = true;
    
    [Header("Test Parameters")]
    [SerializeField] private float explosionForce = 1000f;
    [SerializeField] private float explosionRadius = 8f;
    [SerializeField] private float upwardBias = 0.3f;
    [SerializeField] private Vector3 damageDirection = Vector3.forward;
    
    [Header("Controls")]
    [SerializeField] private KeyCode testExplosionKey = KeyCode.E;
    [SerializeField] private KeyCode testAtPlayerKey = KeyCode.R;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showGizmos = true;

    private Transform playerTransform;
    private int explosionCount = 0;

    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Auto-find prefab if enabled
        if (autoFindPrefab && helicopterExplosionPrefab == null)
        {
            FindExplosionPrefab();
        }

        // Set default spawn point
        if (testSpawnPoint == null)
        {
            testSpawnPoint = transform;
        }

        if (showDebugInfo)
        {
            Debug.Log("ExplosionSystemTester: Initialized. Press " + testExplosionKey + " to test explosion, " + testAtPlayerKey + " to test at player position.");
        }
    }

    void Update()
    {
        // Test explosion at spawn point
        if (Input.GetKeyDown(testExplosionKey))
        {
            TestExplosion(testSpawnPoint.position);
        }

        // Test explosion at player position
        if (Input.GetKeyDown(testAtPlayerKey) && playerTransform != null)
        {
            TestExplosion(playerTransform.position + Vector3.up * 5f);
        }
    }

    /// <summary>
    /// Test explosion at specified position
    /// </summary>
    public void TestExplosion(Vector3 position)
    {
        if (helicopterExplosionPrefab == null)
        {
            Debug.LogError("ExplosionSystemTester: No explosion prefab assigned!");
            return;
        }

        explosionCount++;

        if (showDebugInfo)
        {
            Debug.Log($"ExplosionSystemTester: Creating test explosion #{explosionCount} at {position}");
        }

        // Create explosion
        GameObject explosion = Instantiate(helicopterExplosionPrefab, position, Quaternion.identity);
        
        // Configure explosion
        HelicopterExplosion explosionComponent = explosion.GetComponent<HelicopterExplosion>();
        if (explosionComponent != null)
        {
            explosionComponent.SetExplosionParameters(explosionForce, explosionRadius, upwardBias);
            explosionComponent.SetDamageDirection(damageDirection.normalized);

            if (showDebugInfo)
            {
                Debug.Log($"ExplosionSystemTester: Configured explosion with force={explosionForce}, radius={explosionRadius}, direction={damageDirection}");
            }
        }
        else
        {
            Debug.LogError("ExplosionSystemTester: Explosion prefab doesn't have HelicopterExplosion component!");
        }
    }

    /// <summary>
    /// Try to find the explosion prefab automatically
    /// </summary>
    private void FindExplosionPrefab()
    {
        // Try Resources folder first
        helicopterExplosionPrefab = Resources.Load<GameObject>("HelicopterExplosionPrefab");
        
        if (helicopterExplosionPrefab != null)
        {
            if (showDebugInfo)
            {
                Debug.Log("ExplosionSystemTester: Found explosion prefab in Resources folder");
            }
            return;
        }

        // Try to find in scene
        GameObject sceneObject = GameObject.Find("HelicopterExplosionPrefab");
        if (sceneObject != null)
        {
            helicopterExplosionPrefab = sceneObject;
            if (showDebugInfo)
            {
                Debug.Log("ExplosionSystemTester: Found explosion prefab in scene");
            }
            return;
        }

        // Try to find any object with HelicopterExplosion component
        HelicopterExplosion[] explosions = FindObjectsOfType<HelicopterExplosion>();
        if (explosions.Length > 0)
        {
            helicopterExplosionPrefab = explosions[0].gameObject;
            if (showDebugInfo)
            {
                Debug.Log("ExplosionSystemTester: Found explosion prefab by component search");
            }
            return;
        }

        if (showDebugInfo)
        {
            Debug.LogWarning("ExplosionSystemTester: Could not find explosion prefab automatically. Please assign manually.");
        }
    }

    /// <summary>
    /// Test multiple explosions in sequence
    /// </summary>
    [ContextMenu("Test Multiple Explosions")]
    public void TestMultipleExplosions()
    {
        StartCoroutine(TestMultipleExplosionsCoroutine());
    }

    private System.Collections.IEnumerator TestMultipleExplosionsCoroutine()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 10f;
            randomOffset.y = Mathf.Abs(randomOffset.y) + 5f; // Keep above ground
            
            TestExplosion(testSpawnPoint.position + randomOffset);
            
            yield return new WaitForSeconds(2f);
        }
    }

    /// <summary>
    /// Test explosion with random parameters
    /// </summary>
    [ContextMenu("Test Random Explosion")]
    public void TestRandomExplosion()
    {
        float randomForce = Random.Range(500f, 2000f);
        float randomRadius = Random.Range(5f, 15f);
        float randomUpward = Random.Range(0.1f, 0.6f);
        Vector3 randomDirection = Random.insideUnitSphere.normalized;

        // Temporarily override parameters
        float originalForce = explosionForce;
        float originalRadius = explosionRadius;
        float originalUpward = upwardBias;
        Vector3 originalDirection = damageDirection;

        explosionForce = randomForce;
        explosionRadius = randomRadius;
        upwardBias = randomUpward;
        damageDirection = randomDirection;

        TestExplosion(testSpawnPoint.position);

        // Restore original parameters
        explosionForce = originalForce;
        explosionRadius = originalRadius;
        upwardBias = originalUpward;
        damageDirection = originalDirection;

        if (showDebugInfo)
        {
            Debug.Log($"ExplosionSystemTester: Random explosion - Force: {randomForce:F0}, Radius: {randomRadius:F1}, Upward: {randomUpward:F2}, Direction: {randomDirection}");
        }
    }

    /// <summary>
    /// Validate explosion system setup
    /// </summary>
    [ContextMenu("Validate System")]
    public void ValidateSystem()
    {
        Debug.Log("=== Explosion System Validation ===");

        // Check prefab
        if (helicopterExplosionPrefab == null)
        {
            Debug.LogError("❌ No explosion prefab assigned!");
        }
        else
        {
            Debug.Log("✅ Explosion prefab found: " + helicopterExplosionPrefab.name);

            // Check HelicopterExplosion component
            HelicopterExplosion explosionComp = helicopterExplosionPrefab.GetComponent<HelicopterExplosion>();
            if (explosionComp == null)
            {
                Debug.LogError("❌ Explosion prefab missing HelicopterExplosion component!");
            }
            else
            {
                Debug.Log("✅ HelicopterExplosion component found");
            }

            // Check for rigidbodies (shards)
            Rigidbody[] rigidbodies = helicopterExplosionPrefab.GetComponentsInChildren<Rigidbody>();
            if (rigidbodies.Length == 0)
            {
                Debug.LogError("❌ No rigidbodies found in explosion prefab! Shards need Rigidbody components.");
            }
            else
            {
                Debug.Log($"✅ Found {rigidbodies.Length} rigidbodies (shards) in explosion prefab");
            }

            // Check for colliders
            Collider[] colliders = helicopterExplosionPrefab.GetComponentsInChildren<Collider>();
            if (colliders.Length == 0)
            {
                Debug.LogError("❌ No colliders found in explosion prefab! Shards need Collider components.");
            }
            else
            {
                Debug.Log($"✅ Found {colliders.Length} colliders in explosion prefab");
            }
        }

        // Check player
        if (playerTransform == null)
        {
            Debug.LogWarning("⚠️ No player found with 'Player' tag");
        }
        else
        {
            Debug.Log("✅ Player found: " + playerTransform.name);
        }

        // Check scripts compilation
        try
        {
            System.Type helicopterExplosionType = System.Type.GetType("HelicopterExplosion");
            if (helicopterExplosionType == null)
            {
                Debug.LogError("❌ HelicopterExplosion class not found! Check script compilation.");
            }
            else
            {
                Debug.Log("✅ HelicopterExplosion class compiled successfully");
            }

            System.Type explosionShardType = System.Type.GetType("ExplosionShard");
            if (explosionShardType == null)
            {
                Debug.LogError("❌ ExplosionShard class not found! Check script compilation.");
            }
            else
            {
                Debug.Log("✅ ExplosionShard class compiled successfully");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Error checking script compilation: " + e.Message);
        }

        Debug.Log("=== Validation Complete ===");
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw test spawn point
        if (testSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(testSpawnPoint.position, 1f);
            Gizmos.DrawRay(testSpawnPoint.position, Vector3.up * 3f);
        }

        // Draw explosion radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(testSpawnPoint != null ? testSpawnPoint.position : transform.position, explosionRadius);

        // Draw damage direction
        if (damageDirection != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Vector3 startPos = testSpawnPoint != null ? testSpawnPoint.position : transform.position;
            Gizmos.DrawRay(startPos, damageDirection.normalized * 5f);
        }

        // Draw player position
        if (playerTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerTransform.position, 2f);
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Explosion System Tester", GUI.skin.box);
        
        GUILayout.Label($"Explosions Created: {explosionCount}");
        GUILayout.Label($"Prefab: {(helicopterExplosionPrefab != null ? helicopterExplosionPrefab.name : "None")}");
        GUILayout.Label($"Player: {(playerTransform != null ? playerTransform.name : "Not Found")}");
        
        GUILayout.Space(10);
        GUILayout.Label($"Press {testExplosionKey} - Test Explosion");
        GUILayout.Label($"Press {testAtPlayerKey} - Test at Player");
        
        if (GUILayout.Button("Validate System"))
        {
            ValidateSystem();
        }
        
        if (GUILayout.Button("Test Multiple"))
        {
            TestMultipleExplosions();
        }
        
        GUILayout.EndArea();
    }
}
