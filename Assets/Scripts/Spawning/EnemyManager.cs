using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Central enemy management system with pooling, difficulty scaling, and performance monitoring
/// Integrates with the existing ChunkSpawner system for seamless enemy spawning
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Configuration")]
    [SerializeField] private EnemyTypeData[] availableEnemyTypes;
    [SerializeField] private int maxActiveEnemies = 5;
    [SerializeField] private float enemyCleanupDistance = 100f; // Distance behind player to cleanup enemies
    
    [Header("Difficulty Scaling")]
    [SerializeField] private bool enableDifficultyScaling = true;
    [SerializeField] private float difficultyScaleDistance = 500f; // Distance intervals for difficulty increases
    [SerializeField] private float healthScaleMultiplier = 1.2f; // Health increase per difficulty tier
    [SerializeField] private float damageScaleMultiplier = 1.15f; // Damage increase per difficulty tier
    [SerializeField] private float fireRateScaleMultiplier = 1.1f; // Fire rate increase per difficulty tier
    
    [Header("Spawning")]
    [SerializeField] private float spawnCooldown = 2f; // Minimum time between spawns
    [SerializeField] private float spawnChancePerChunk = 0.3f; // Probability of spawning enemies per terrain chunk
    [SerializeField] private int maxEnemiesPerChunk = 2; // Maximum enemies to spawn per chunk
    
    [Header("Performance")]
    [SerializeField] private float updateInterval = 0.2f; // How often to update enemy management (5 times per second)
    [SerializeField] private bool enablePerformanceMonitoring = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private bool showGizmos = false;

    // Singleton pattern
    public static EnemyManager Instance { get; private set; }

    // Enemy tracking
    private List<EnemyAI> activeEnemies = new List<EnemyAI>();
    private Queue<GameObject> enemyPool = new Queue<GameObject>();
    private Transform playerTransform;
    
    // Difficulty tracking
    private int currentDifficultyTier = 1;
    private float playerDistanceTraveled = 0f;
    private Vector3 lastPlayerPosition;
    
    // Performance tracking
    private float lastUpdateTime;
    private float lastSpawnTime;
    private int enemiesSpawnedThisSession = 0;
    private int enemiesDestroyedThisSession = 0;
    
    // Events
    public System.Action<EnemyAI> OnEnemySpawned;
    public System.Action<EnemyAI> OnEnemyDestroyed;
    public System.Action<int> OnDifficultyChanged;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeEnemyManager();
        
        // Subscribe to chunk spawning events
        ChunkSpawner.OnChunkSpawned += OnChunkSpawned;
        ChunkSpawner.OnChunkDestroyed += OnChunkDestroyed;
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (ChunkSpawner.OnChunkSpawned != null)
            ChunkSpawner.OnChunkSpawned -= OnChunkSpawned;
        if (ChunkSpawner.OnChunkDestroyed != null)
            ChunkSpawner.OnChunkDestroyed -= OnChunkDestroyed;
    }

    void Update()
    {
        // Performance optimization - don't update every frame
        if (Time.time - lastUpdateTime < updateInterval) return;
        lastUpdateTime = Time.time;

        UpdateDifficultyScaling();
        CleanupDistantEnemies();
        
        if (enablePerformanceMonitoring)
        {
            MonitorPerformance();
        }
    }

    /// <summary>
    /// Initialize the enemy management system
    /// </summary>
    private void InitializeEnemyManager()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            lastPlayerPosition = playerTransform.position;
        }
        else
        {
            Debug.LogError("EnemyManager: No player found with 'Player' tag!");
        }

        // Validate enemy types
        if (availableEnemyTypes == null || availableEnemyTypes.Length == 0)
        {
            Debug.LogError("EnemyManager: No enemy types assigned! Please assign enemy data in the inspector.");
            return;
        }

        // Validate enemy data
        for (int i = 0; i < availableEnemyTypes.Length; i++)
        {
            if (availableEnemyTypes[i] == null)
            {
                Debug.LogError($"EnemyManager: Enemy type at index {i} is null!");
                continue;
            }

            if (!availableEnemyTypes[i].IsValid())
            {
                Debug.LogError($"EnemyManager: Enemy type '{availableEnemyTypes[i].EnemyName}' is invalid!");
            }
        }

        // Initialize object pool
        InitializeEnemyPool();

        if (showDebugInfo)
        {
            Debug.Log($"EnemyManager: Initialized with {availableEnemyTypes.Length} enemy types, max {maxActiveEnemies} active enemies");
        }
    }

    /// <summary>
    /// Initialize the enemy object pool
    /// </summary>
    private void InitializeEnemyPool()
    {
        // Create a container for pooled enemies
        GameObject poolContainer = new GameObject("EnemyPool");
        poolContainer.transform.SetParent(transform);

        // Pre-populate pool with basic enemies
        for (int i = 0; i < maxActiveEnemies; i++)
        {
            if (availableEnemyTypes.Length > 0 && availableEnemyTypes[0] != null && availableEnemyTypes[0].EnemyPrefab != null)
            {
                GameObject pooledEnemy = Instantiate(availableEnemyTypes[0].EnemyPrefab, poolContainer.transform);
                pooledEnemy.SetActive(false);
                pooledEnemy.name = $"PooledEnemy_{i:D2}";
                enemyPool.Enqueue(pooledEnemy);
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"EnemyManager: Created enemy pool with {enemyPool.Count} enemies");
        }
    }

    /// <summary>
    /// Handle terrain chunk spawning - spawn enemies if conditions are met
    /// </summary>
    private void OnChunkSpawned(float chunkZ, int prefabIndex)
    {
        // Check if we should spawn enemies
        if (!ShouldSpawnEnemies()) return;

        // Calculate how many enemies to spawn
        int enemiesToSpawn = CalculateEnemySpawnCount();
        
        if (enemiesToSpawn > 0)
        {
            SpawnEnemiesInChunk(chunkZ, enemiesToSpawn);
        }
    }

    /// <summary>
    /// Handle terrain chunk destruction
    /// </summary>
    private void OnChunkDestroyed(GameObject chunk)
    {
        // Enemies are automatically cleaned up by distance checking
        // This is just for potential future chunk-specific cleanup
    }

    /// <summary>
    /// Check if we should spawn enemies based on various conditions
    /// </summary>
    private bool ShouldSpawnEnemies()
    {
        // Check cooldown
        if (Time.time - lastSpawnTime < spawnCooldown) return false;

        // Check if we're at max capacity
        if (activeEnemies.Count >= maxActiveEnemies) return false;

        // Random chance
        if (Random.value > spawnChancePerChunk) return false;

        return true;
    }

    /// <summary>
    /// Calculate how many enemies to spawn based on difficulty and current active count
    /// </summary>
    private int CalculateEnemySpawnCount()
    {
        int availableSlots = maxActiveEnemies - activeEnemies.Count;
        int maxForChunk = Mathf.Min(maxEnemiesPerChunk, availableSlots);
        
        // Scale with difficulty
        if (currentDifficultyTier >= 2)
        {
            maxForChunk = Mathf.Min(maxForChunk + 1, availableSlots);
        }
        
        return Random.Range(1, maxForChunk + 1);
    }

    /// <summary>
    /// Spawn enemies in a terrain chunk
    /// </summary>
    private void SpawnEnemiesInChunk(float chunkZ, int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            // Select enemy type based on difficulty and availability
            EnemyTypeData selectedEnemyType = SelectEnemyType();
            if (selectedEnemyType == null) continue;

            // Find spawn position
            Vector3 spawnPosition = FindEnemySpawnPosition(chunkZ);
            
            // Spawn enemy
            SpawnEnemy(selectedEnemyType, spawnPosition);
        }

        lastSpawnTime = Time.time;
    }

    /// <summary>
    /// Select appropriate enemy type based on difficulty and spawn weights
    /// </summary>
    private EnemyTypeData SelectEnemyType()
    {
        // Filter enemy types - ignore difficulty tier if scaling is disabled
        var availableTypes = enableDifficultyScaling 
            ? availableEnemyTypes.Where(e => 
                e != null && 
                e.DifficultyTier <= currentDifficultyTier &&
                CountActiveEnemiesOfType(e) < e.MaxSimultaneous
              ).ToArray()
            : availableEnemyTypes.Where(e => 
                e != null && 
                CountActiveEnemiesOfType(e) < e.MaxSimultaneous
              ).ToArray();

        if (availableTypes.Length == 0)
        {
            if (showDebugInfo)
            {
                string debugMessage = enableDifficultyScaling 
                    ? $"EnemyManager: No available enemy types for current difficulty tier {currentDifficultyTier}!"
                    : "EnemyManager: No available enemy types (all types at max simultaneous count)!";
                Debug.LogWarning(debugMessage);
            }
            return null;
        }

        // Weighted random selection
        float totalWeight = availableTypes.Sum(e => e.SpawnWeight);
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var enemyType in availableTypes)
        {
            currentWeight += enemyType.SpawnWeight;
            if (randomValue <= currentWeight)
            {
                return enemyType;
            }
        }

        // Fallback to first available type
        return availableTypes[0];
    }

    /// <summary>
    /// Count active enemies of a specific type
    /// </summary>
    private int CountActiveEnemiesOfType(EnemyTypeData enemyType)
    {
        return activeEnemies.Count(e => e != null && e.EnemyData == enemyType);
    }

    /// <summary>
    /// Find a valid spawn position for an enemy in a chunk
    /// </summary>
    private Vector3 FindEnemySpawnPosition(float chunkZ)
    {
        // Generate random position within chunk bounds
        float randomX = Random.Range(-20f, 20f); // Adjust based on your terrain width
        float spawnY = Random.Range(15f, 25f); // Helicopter hover height
        float randomZ = chunkZ + Random.Range(-50f, 50f); // Within chunk area

        Vector3 spawnPosition = new Vector3(randomX, spawnY, randomZ);

        // TODO: Add collision checking to avoid spawning inside terrain
        // For now, we'll use the basic position

        return spawnPosition;
    }

    /// <summary>
    /// Spawn an enemy at the specified position
    /// </summary>
    private void SpawnEnemy(EnemyTypeData enemyType, Vector3 position)
    {
        GameObject enemyObject = GetPooledEnemy();
        if (enemyObject == null)
        {
            // Pool exhausted, create new enemy
            enemyObject = Instantiate(enemyType.EnemyPrefab);
            if (showDebugInfo)
                Debug.LogWarning("EnemyManager: Pool exhausted, created new enemy instance");
        }

        // Position and activate enemy
        enemyObject.transform.position = position;
        enemyObject.SetActive(true);

        // Get enemy AI component
        EnemyAI enemyAI = enemyObject.GetComponent<EnemyAI>();
        if (enemyAI == null)
        {
            Debug.LogError($"EnemyManager: Enemy prefab '{enemyType.EnemyName}' has no EnemyAI component!");
            Destroy(enemyObject);
            return;
        }

        // Initialize enemy with scaled data if needed
        EnemyTypeData finalEnemyData = enemyType;
        if (enableDifficultyScaling && currentDifficultyTier > 1)
        {
            float tierMultiplier = currentDifficultyTier - 1;
            finalEnemyData = enemyType.CreateScaledVersion(
                1f + (healthScaleMultiplier - 1f) * tierMultiplier,
                1f + (damageScaleMultiplier - 1f) * tierMultiplier,
                1f + (fireRateScaleMultiplier - 1f) * tierMultiplier
            );
        }

        // Initialize enemy
        enemyAI.Initialize(finalEnemyData, position);

        // Add to active enemies list
        activeEnemies.Add(enemyAI);
        enemiesSpawnedThisSession++;

        // Fire event
        OnEnemySpawned?.Invoke(enemyAI);

        if (showDebugInfo)
        {
            Debug.Log($"EnemyManager: Spawned '{enemyType.EnemyName}' at {position} (Tier {currentDifficultyTier})");
        }
    }

    /// <summary>
    /// Get an enemy from the object pool
    /// </summary>
    private GameObject GetPooledEnemy()
    {
        if (enemyPool.Count > 0)
        {
            return enemyPool.Dequeue();
        }
        return null;
    }

    /// <summary>
    /// Return an enemy to the object pool
    /// </summary>
    public void ReturnEnemyToPool(GameObject enemy)
    {
        if (enemy != null)
        {
            enemy.SetActive(false);
            enemy.transform.SetParent(transform.Find("EnemyPool"));
            enemyPool.Enqueue(enemy);
        }
    }

    /// <summary>
    /// Update difficulty scaling based on player progress
    /// </summary>
    private void UpdateDifficultyScaling()
    {
        if (!enableDifficultyScaling || playerTransform == null) return;

        // Calculate distance traveled
        float distanceDelta = Vector3.Distance(playerTransform.position, lastPlayerPosition);
        if (distanceDelta > 0.1f) // Only update if player moved significantly
        {
            playerDistanceTraveled += distanceDelta;
            lastPlayerPosition = playerTransform.position;
        }

        // Calculate new difficulty tier
        int newDifficultyTier = Mathf.FloorToInt(playerDistanceTraveled / difficultyScaleDistance) + 1;
        newDifficultyTier = Mathf.Clamp(newDifficultyTier, 1, 5); // Cap at tier 5

        if (newDifficultyTier != currentDifficultyTier)
        {
            currentDifficultyTier = newDifficultyTier;
            OnDifficultyChanged?.Invoke(currentDifficultyTier);

            if (showDebugInfo)
            {
                Debug.Log($"EnemyManager: Difficulty increased to tier {currentDifficultyTier} (Distance: {playerDistanceTraveled:F0}m)");
            }
        }
    }

    /// <summary>
    /// Clean up enemies that are too far behind the player
    /// </summary>
    private void CleanupDistantEnemies()
    {
        if (playerTransform == null) return;

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
                continue;
            }

            float distanceToPlayer = Vector3.Distance(activeEnemies[i].transform.position, playerTransform.position);
            
            // Check if enemy is too far behind player
            if (activeEnemies[i].transform.position.z < playerTransform.position.z - enemyCleanupDistance)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"EnemyManager: Cleaning up distant enemy at distance {distanceToPlayer:F1}m");
                }

                DestroyEnemy(activeEnemies[i]);
            }
        }
    }

    /// <summary>
    /// Monitor performance and log statistics
    /// </summary>
    private void MonitorPerformance()
    {
        // Log performance stats every 10 seconds
        if (Time.time % 10f < updateInterval && showDebugInfo)
        {
            Debug.Log($"EnemyManager Performance: Active={activeEnemies.Count}/{maxActiveEnemies}, " +
                     $"Spawned={enemiesSpawnedThisSession}, Destroyed={enemiesDestroyedThisSession}, " +
                     $"Difficulty={currentDifficultyTier}, Distance={playerDistanceTraveled:F0}m");
        }
    }

    /// <summary>
    /// Called when an enemy is destroyed (from EnemyAI)
    /// </summary>
    public void HandleEnemyDestroyed(EnemyAI enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            enemiesDestroyedThisSession++;
            
            OnEnemyDestroyed?.Invoke(enemy);

            if (showDebugInfo)
            {
                Debug.Log($"EnemyManager: Enemy destroyed, {activeEnemies.Count} remaining active");
            }
        }
    }

    /// <summary>
    /// Destroy an enemy and handle cleanup
    /// </summary>
    private void DestroyEnemy(EnemyAI enemy)
    {
        if (enemy != null)
        {
            activeEnemies.Remove(enemy);
            enemiesDestroyedThisSession++;
            
            // Try to return to pool instead of destroying
            if (enemy.gameObject != null)
            {
                ReturnEnemyToPool(enemy.gameObject);
            }
        }
    }

    // Public API methods
    public int GetActiveEnemyCount() => activeEnemies.Count;
    public int GetCurrentDifficultyTier() => currentDifficultyTier;
    public float GetPlayerDistanceTraveled() => playerDistanceTraveled;
    public List<EnemyAI> GetActiveEnemies() => new List<EnemyAI>(activeEnemies);

    /// <summary>
    /// Force spawn an enemy at a specific position (for testing)
    /// </summary>
    public void ForceSpawnEnemy(Vector3 position, int enemyTypeIndex = 0)
    {
        if (enemyTypeIndex >= 0 && enemyTypeIndex < availableEnemyTypes.Length)
        {
            SpawnEnemy(availableEnemyTypes[enemyTypeIndex], position);
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || playerTransform == null) return;

        // Draw cleanup distance
        Gizmos.color = Color.red;
        Vector3 cleanupPosition = playerTransform.position - Vector3.forward * enemyCleanupDistance;
        Gizmos.DrawWireCube(cleanupPosition, new Vector3(50f, 10f, 5f));

        // Draw active enemies
        Gizmos.color = Color.yellow;
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Gizmos.DrawWireSphere(enemy.transform.position, 2f);
                Gizmos.DrawLine(enemy.transform.position, playerTransform.position);
            }
        }
    }
}
