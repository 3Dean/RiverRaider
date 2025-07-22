using UnityEngine;

/// <summary>
/// Test script to verify the enhanced missile system functionality
/// </summary>
public class MissileSystemTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FlightData flightData;
    [SerializeField] private MissileUI missileUI;
    
    [Header("Test Settings")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool runTestsOnStart = false;
    
    void Start()
    {
        // Find references if not assigned
        if (flightData == null)
        {
            flightData = FindObjectOfType<FlightData>();
        }
        
        if (missileUI == null)
        {
            missileUI = FindObjectOfType<MissileUI>();
        }
        
        if (runTestsOnStart)
        {
            Invoke(nameof(RunBasicTests), 1f); // Wait 1 second for initialization
        }
    }
    
    void Update()
    {
        // Test controls
        if (Input.GetKeyDown(KeyCode.F1))
        {
            RunBasicTests();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TestMissileTypeSwitching();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TestMissileFiring();
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            TestMissileResupply();
        }
    }
    
    [ContextMenu("Run Basic Tests")]
    public void RunBasicTests()
    {
        if (flightData == null)
        {
            LogError("FlightData not found!");
            return;
        }
        
        Log("=== MISSILE SYSTEM BASIC TESTS ===");
        
        // Test 1: Check initial state
        Log($"Initial State - Type: {flightData.currentMissileType}, Count: {flightData.currentMissiles}/{flightData.maxMissiles}");
        
        // Test 2: Check missile inventory
        var inventory = flightData.GetMissileInventory();
        if (inventory != null)
        {
            Log($"Missile Inventory - Current Type: {inventory.CurrentMissileTypeName}");
            Log($"Available Types: {inventory.GetAvailableMissileTypes().Length}");
        }
        else
        {
            LogError("Missile Inventory is null!");
        }
        
        // Test 3: Check UI connection
        if (missileUI != null)
        {
            Log($"UI State - Type: {missileUI.MissileType}, Count: {missileUI.CurrentMissiles}/{missileUI.MaxMissiles}");
        }
        else
        {
            LogWarning("MissileUI not found - UI tests skipped");
        }
        
        Log("=== BASIC TESTS COMPLETE ===");
    }
    
    [ContextMenu("Test Missile Type Switching")]
    public void TestMissileTypeSwitching()
    {
        if (flightData == null) return;
        
        Log("=== TESTING MISSILE TYPE SWITCHING ===");
        
        string initialType = flightData.currentMissileType;
        int initialCount = flightData.currentMissiles;
        int initialMax = flightData.maxMissiles;
        
        Log($"Before Switch - Type: {initialType}, Count: {initialCount}/{initialMax}");
        
        // Switch to next type
        bool switched = flightData.SwitchToNextMissileType();
        
        if (switched)
        {
            Log($"After Switch - Type: {flightData.currentMissileType}, Count: {flightData.currentMissiles}/{flightData.maxMissiles}");
            
            // Verify the change
            if (flightData.currentMissileType != initialType)
            {
                Log("✓ Missile type switching works correctly!");
            }
            else
            {
                LogWarning("Missile type didn't change - may only have one type configured");
            }
        }
        else
        {
            LogWarning("Missile type switching failed - check missile inventory setup");
        }
        
        Log("=== MISSILE TYPE SWITCHING TEST COMPLETE ===");
    }
    
    [ContextMenu("Test Missile Firing")]
    public void TestMissileFiring()
    {
        if (flightData == null) return;
        
        Log("=== TESTING MISSILE FIRING ===");
        
        int initialCount = flightData.currentMissiles;
        Log($"Before Firing - Count: {initialCount}");
        
        if (initialCount > 0)
        {
            bool fired = flightData.ConsumeMissile();
            
            if (fired)
            {
                Log($"After Firing - Count: {flightData.currentMissiles}");
                
                if (flightData.currentMissiles == initialCount - 1)
                {
                    Log("✓ Missile firing works correctly!");
                }
                else
                {
                    LogError("Missile count didn't decrease properly!");
                }
            }
            else
            {
                LogError("Missile firing failed!");
            }
        }
        else
        {
            LogWarning("No missiles available to test firing");
        }
        
        Log("=== MISSILE FIRING TEST COMPLETE ===");
    }
    
    [ContextMenu("Test Missile Resupply")]
    public void TestMissileResupply()
    {
        if (flightData == null) return;
        
        Log("=== TESTING MISSILE RESUPPLY ===");
        
        int initialCount = flightData.currentMissiles;
        int maxCount = flightData.maxMissiles;
        
        Log($"Before Resupply - Count: {initialCount}/{maxCount}");
        
        flightData.ResupplyMissiles();
        
        Log($"After Resupply - Count: {flightData.currentMissiles}/{flightData.maxMissiles}");
        
        if (flightData.currentMissiles == flightData.maxMissiles)
        {
            Log("✓ Missile resupply works correctly!");
        }
        else
        {
            LogError("Missile resupply failed!");
        }
        
        Log("=== MISSILE RESUPPLY TEST COMPLETE ===");
    }
    
    [ContextMenu("Force UI Update")]
    public void ForceUIUpdate()
    {
        if (missileUI != null)
        {
            missileUI.ForceUpdate();
            Log("UI update forced");
        }
        else
        {
            LogWarning("MissileUI not found");
        }
    }
    
    private void Log(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[MissileSystemTester] {message}");
        }
    }
    
    private void LogWarning(string message)
    {
        if (enableDebugLogging)
        {
            Debug.LogWarning($"[MissileSystemTester] {message}");
        }
    }
    
    private void LogError(string message)
    {
        if (enableDebugLogging)
        {
            Debug.LogError($"[MissileSystemTester] {message}");
        }
    }
    
    void OnGUI()
    {
        if (!enableDebugLogging) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Missile System Tester", GUI.skin.box);
        GUILayout.Label("F1: Run Basic Tests");
        GUILayout.Label("F2: Test Type Switching");
        GUILayout.Label("F3: Test Missile Firing");
        GUILayout.Label("F4: Test Resupply");
        
        if (flightData != null)
        {
            GUILayout.Space(10);
            GUILayout.Label($"Current Type: {flightData.currentMissileType}");
            GUILayout.Label($"Missiles: {flightData.currentMissiles}/{flightData.maxMissiles}");
        }
        
        GUILayout.EndArea();
    }
}
