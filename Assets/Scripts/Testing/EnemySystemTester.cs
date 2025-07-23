using UnityEngine;

/// <summary>
/// Testing utility for the enemy system - helps verify functionality and debug issues
/// </summary>
public class EnemySystemTester : MonoBehaviour
{
    [Header("Test Controls")]
    [SerializeField] private KeyCode spawnEnemyKey = KeyCode.E;
    [SerializeField] private KeyCode increaseDifficultyKey = KeyCode.T;
    [SerializeField] private KeyCode showStatsKey = KeyCode.Y;
    [SerializeField] private KeyCode clearEnemiesKey = KeyCode.C;
    
    [Header("Spawn Testing")]
    [SerializeField] private float spawnDistance = 30f;
    [SerializeField] private float spawnHeight = 20f;
    [SerializeField] private int enemyTypeIndex = 0;
    
    [Header("Debug Display")]
    [SerializeField] private bool showOnScreenStats = true;
    [SerializeField] private bool showDetailedInfo = false;
    
    private EnemyManager enemyManager;
    private Transform playerTransform;
    private GUIStyle guiStyle;

    void Start()
    {
        // Find components
        enemyManager = FindObjectOfType<EnemyManager>();
        if (enemyManager == null)
        {
            Debug.LogError("EnemySystemTester: No EnemyManager found in scene!");
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("EnemySystemTester: No player found with 'Player' tag!");
        }

        // Setup GUI style
        guiStyle = new GUIStyle();
        guiStyle.fontSize = 16;
        guiStyle.normal.textColor = Color.white;
        guiStyle.alignment = TextAnchor.UpperLeft;

        Debug.Log("EnemySystemTester: Initialized. Press E to spawn enemy, T to increase difficulty, Y for stats, C to clear enemies");
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Spawn enemy at player's forward position
        if (Input.GetKeyDown(spawnEnemyKey))
        {
            SpawnTestEnemy();
        }

        // Force increase difficulty
        if (Input.GetKeyDown(increaseDifficultyKey))
        {
            ForceDifficultyIncrease();
        }

        // Show detailed stats
        if (Input.GetKeyDown(showStatsKey))
        {
            ShowDetailedStats();
        }

        // Clear all enemies
        if (Input.GetKeyDown(clearEnemiesKey))
        {
            ClearAllEnemies();
        }
    }

    void SpawnTestEnemy()
    {
        if (enemyManager == null || playerTransform == null) return;

        // Calculate spawn position in front of player
        Vector3 spawnPosition = playerTransform.position + 
                               playerTransform.forward * spawnDistance + 
                               Vector3.up * spawnHeight;

        // Add some random offset
        spawnPosition += new Vector3(
            Random.Range(-10f, 10f), 
            Random.Range(-5f, 5f), 
            Random.Range(-10f, 10f)
        );

        enemyManager.ForceSpawnEnemy(spawnPosition, enemyTypeIndex);
        
        Debug.Log($"EnemySystemTester: Spawned test enemy at {spawnPosition}");
    }

    void ForceDifficultyIncrease()
    {
        if (enemyManager == null) return;

        // This is a hack for testing - we'll simulate distance traveled
        var playerDistanceField = typeof(EnemyManager).GetField("playerDistanceTraveled", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (playerDistanceField != null)
        {
            float currentDistance = (float)playerDistanceField.GetValue(enemyManager);
            playerDistanceField.SetValue(enemyManager, currentDistance + 500f);
            Debug.Log($"EnemySystemTester: Forced difficulty increase (simulated distance: {currentDistance + 500f})");
        }
        else
        {
            Debug.LogWarning("EnemySystemTester: Could not access playerDistanceTraveled field for testing");
        }
    }

    void ShowDetailedStats()
    {
        if (enemyManager == null) return;

        Debug.Log("=== ENEMY SYSTEM STATS ===");
        Debug.Log($"Active Enemies: {enemyManager.GetActiveEnemyCount()}");
        Debug.Log($"Difficulty Tier: {enemyManager.GetCurrentDifficultyTier()}");
        Debug.Log($"Distance Traveled: {enemyManager.GetPlayerDistanceTraveled():F1}m");
        
        var activeEnemies = enemyManager.GetActiveEnemies();
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            var enemy = activeEnemies[i];
            if (enemy != null)
            {
                float distance = Vector3.Distance(enemy.transform.position, playerTransform.position);
                Debug.Log($"Enemy {i}: {enemy.EnemyData?.EnemyName} - Health: {enemy.Health:F0}/{enemy.MaxHealth:F0} - Distance: {distance:F1}m - State: {enemy.CurrentState}");
            }
        }
        Debug.Log("========================");
    }

    void ClearAllEnemies()
    {
        if (enemyManager == null) return;

        var activeEnemies = enemyManager.GetActiveEnemies();
        int clearedCount = 0;

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] != null)
            {
                activeEnemies[i].TakeDamage(9999f); // Instant kill
                clearedCount++;
            }
        }

        Debug.Log($"EnemySystemTester: Cleared {clearedCount} enemies");
    }

    void OnGUI()
    {
        if (!showOnScreenStats || enemyManager == null) return;

        // Basic stats display
        string statsText = $"Enemy System Stats:\n";
        statsText += $"Active Enemies: {enemyManager.GetActiveEnemyCount()}\n";
        statsText += $"Difficulty Tier: {enemyManager.GetCurrentDifficultyTier()}\n";
        statsText += $"Distance: {enemyManager.GetPlayerDistanceTraveled():F0}m\n";
        statsText += $"\nControls:\n";
        statsText += $"E - Spawn Enemy\n";
        statsText += $"T - Increase Difficulty\n";
        statsText += $"Y - Show Detailed Stats\n";
        statsText += $"C - Clear All Enemies";

        if (showDetailedInfo)
        {
            var activeEnemies = enemyManager.GetActiveEnemies();
            if (activeEnemies.Count > 0)
            {
                statsText += $"\n\nActive Enemies:";
                for (int i = 0; i < activeEnemies.Count && i < 5; i++) // Limit to 5 for screen space
                {
                    var enemy = activeEnemies[i];
                    if (enemy != null && playerTransform != null)
                    {
                        float distance = Vector3.Distance(enemy.transform.position, playerTransform.position);
                        statsText += $"\n{enemy.EnemyData?.EnemyName}: {enemy.Health:F0}HP, {distance:F0}m, {enemy.CurrentState}";
                    }
                }
            }
        }

        GUI.Label(new Rect(10, 10, 400, 300), statsText, guiStyle);
    }

    // Public methods for external testing
    public void SpawnEnemyAtPosition(Vector3 position)
    {
        if (enemyManager != null)
        {
            enemyManager.ForceSpawnEnemy(position, enemyTypeIndex);
        }
    }

    public void SetEnemyTypeIndex(int index)
    {
        enemyTypeIndex = index;
    }

    public void ToggleDetailedInfo()
    {
        showDetailedInfo = !showDetailedInfo;
    }

    // Validation methods
    public bool ValidateEnemySystem()
    {
        bool isValid = true;
        
        if (enemyManager == null)
        {
            Debug.LogError("EnemySystemTester: EnemyManager not found!");
            isValid = false;
        }

        if (playerTransform == null)
        {
            Debug.LogError("EnemySystemTester: Player not found!");
            isValid = false;
        }

        var bulletPool = FindObjectOfType<BulletPool>();
        if (bulletPool == null)
        {
            Debug.LogError("EnemySystemTester: BulletPool not found!");
            isValid = false;
        }

        var chunkSpawner = FindObjectOfType<ChunkSpawner>();
        if (chunkSpawner == null)
        {
            Debug.LogWarning("EnemySystemTester: ChunkSpawner not found - enemies may not spawn automatically");
        }

        return isValid;
    }

    [ContextMenu("Validate Enemy System")]
    public void ValidateEnemySystemMenu()
    {
        bool isValid = ValidateEnemySystem();
        Debug.Log($"Enemy System Validation: {(isValid ? "PASSED" : "FAILED")}");
    }

    [ContextMenu("Spawn Test Enemy")]
    public void SpawnTestEnemyMenu()
    {
        SpawnTestEnemy();
    }

    [ContextMenu("Show System Stats")]
    public void ShowSystemStatsMenu()
    {
        ShowDetailedStats();
    }
}
