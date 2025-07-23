using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Comprehensive diagnostic tool for helicopter explosion system
/// Helps identify physics, component, and timing issues
/// </summary>
public class ExplosionDiagnostic : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool enableVisualDebug = true;
    [SerializeField] private bool testOnStart = false;
    [SerializeField] private KeyCode diagnosticKey = KeyCode.F1;
    
    [Header("Force Testing")]
    [SerializeField] private float testForce = 1000f;
    [SerializeField] private bool applyTestForces = false;
    
    private List<DiagnosticInfo> diagnosticResults = new List<DiagnosticInfo>();
    private bool isRunning = false;

    void Start()
    {
        if (testOnStart)
        {
            RunDiagnostic();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(diagnosticKey))
        {
            RunDiagnostic();
        }

        if (applyTestForces && Input.GetKeyDown(KeyCode.F2))
        {
            ApplyTestForcesToAllRigidbodies();
        }
    }

    /// <summary>
    /// Run comprehensive explosion diagnostic
    /// </summary>
    public void RunDiagnostic()
    {
        if (isRunning) return;
        
        isRunning = true;
        diagnosticResults.Clear();
        
        Debug.Log("=== EXPLOSION DIAGNOSTIC STARTING ===");
        
        // Test 1: Check for explosion prefab
        TestExplosionPrefab();
        
        // Test 2: Create test explosion and analyze
        TestExplosionCreation();
        
        // Test 3: Check physics settings
        TestPhysicsSettings();
        
        // Test 4: Check layers and collision matrix
        TestCollisionLayers();
        
        // Generate report
        GenerateReport();
        
        isRunning = false;
        
        Debug.Log("=== EXPLOSION DIAGNOSTIC COMPLETE ===");
    }

    /// <summary>
    /// Test if explosion prefab exists and is properly configured
    /// </summary>
    private void TestExplosionPrefab()
    {
        var result = new DiagnosticInfo("Explosion Prefab Test");
        
        GameObject prefab = Resources.Load<GameObject>("HelicopterExplosionPrefab");
        if (prefab == null)
        {
            result.AddError("HelicopterExplosionPrefab not found in Resources folder!");
            diagnosticResults.Add(result);
            return;
        }
        
        result.AddSuccess("HelicopterExplosionPrefab found in Resources");
        
        // Check for HelicopterExplosion component
        HelicopterExplosion explosionComponent = prefab.GetComponent<HelicopterExplosion>();
        if (explosionComponent == null)
        {
            result.AddError("HelicopterExplosion component missing from prefab!");
        }
        else
        {
            result.AddSuccess("HelicopterExplosion component found");
        }
        
        // Check for child rigidbodies (shards)
        Rigidbody[] rigidbodies = prefab.GetComponentsInChildren<Rigidbody>();
        int shardCount = 0;
        foreach (var rb in rigidbodies)
        {
            if (rb.transform != prefab.transform) // Don't count parent
            {
                shardCount++;
            }
        }
        
        if (shardCount == 0)
        {
            result.AddError("No shard Rigidbodies found in prefab!");
        }
        else if (shardCount < 51)
        {
            result.AddWarning($"Only {shardCount} shards found (expected 51)");
        }
        else
        {
            result.AddSuccess($"{shardCount} shards found in prefab");
        }
        
        diagnosticResults.Add(result);
    }

    /// <summary>
    /// Test explosion creation and analyze the created instance
    /// </summary>
    private void TestExplosionCreation()
    {
        var result = new DiagnosticInfo("Explosion Creation Test");
        
        GameObject prefab = Resources.Load<GameObject>("HelicopterExplosionPrefab");
        if (prefab == null)
        {
            result.AddError("Cannot test creation - prefab not found");
            diagnosticResults.Add(result);
            return;
        }
        
        // Create test explosion
        Vector3 testPosition = Camera.main != null ? 
            Camera.main.transform.position + Camera.main.transform.forward * 10f : 
            Vector3.up * 5f;
            
        GameObject testExplosion = Instantiate(prefab, testPosition, Quaternion.identity);
        result.AddSuccess("Test explosion created successfully");
        
        // Wait a frame for initialization
        StartCoroutine(AnalyzeExplosionAfterDelay(testExplosion, result));
    }

    /// <summary>
    /// Analyze explosion after a short delay to allow initialization
    /// </summary>
    private System.Collections.IEnumerator AnalyzeExplosionAfterDelay(GameObject explosion, DiagnosticInfo result)
    {
        yield return new WaitForSeconds(0.5f); // Wait for explosion to initialize
        
        if (explosion == null)
        {
            result.AddError("Explosion was destroyed before analysis!");
            diagnosticResults.Add(result);
            yield break;
        }
        
        HelicopterExplosion explosionComponent = explosion.GetComponent<HelicopterExplosion>();
        if (explosionComponent == null)
        {
            result.AddError("HelicopterExplosion component not found on instance!");
        }
        else
        {
            // Get explosion stats
            var stats = explosionComponent.GetExplosionStats();
            result.AddInfo($"Total Shards: {stats.totalShards}");
            result.AddInfo($"Active Shards: {stats.activeShards}");
            result.AddInfo($"Explosion Force: {stats.explosionForce}");
            result.AddInfo($"Explosion Time: {stats.explosionTime:F2}s");
            
            if (stats.totalShards == 0)
            {
                result.AddError("No shards found in explosion instance!");
            }
            else if (stats.activeShards == 0)
            {
                result.AddError("No active shards - they may have been destroyed immediately!");
            }
            else
            {
                result.AddSuccess($"Explosion has {stats.activeShards} active shards");
            }
        }
        
        // Analyze individual shards
        AnalyzeShards(explosion, result);
        
        // Clean up test explosion after analysis
        if (explosion != null)
        {
            Destroy(explosion);
        }
        
        diagnosticResults.Add(result);
    }

    /// <summary>
    /// Analyze individual shards in the explosion
    /// </summary>
    private void AnalyzeShards(GameObject explosion, DiagnosticInfo result)
    {
        ExplosionShard[] shards = explosion.GetComponentsInChildren<ExplosionShard>();
        result.AddInfo($"Found {shards.Length} ExplosionShard components");
        
        int activeShards = 0;
        int kinematicShards = 0;
        int missingRigidbodies = 0;
        int missingColliders = 0;
        
        foreach (var shard in shards)
        {
            if (shard.IsActive) activeShards++;
            
            if (shard.Rigidbody == null)
            {
                missingRigidbodies++;
            }
            else if (shard.Rigidbody.isKinematic)
            {
                kinematicShards++;
            }
            
            if (shard.GetComponent<Collider>() == null)
            {
                missingColliders++;
            }
        }
        
        result.AddInfo($"Active Shards: {activeShards}/{shards.Length}");
        result.AddInfo($"Kinematic Shards: {kinematicShards}");
        
        if (missingRigidbodies > 0)
        {
            result.AddError($"{missingRigidbodies} shards missing Rigidbody components!");
        }
        
        if (missingColliders > 0)
        {
            result.AddError($"{missingColliders} shards missing Collider components!");
        }
        
        if (kinematicShards == shards.Length && activeShards > 0)
        {
            result.AddError("All shards are kinematic - physics forces won't work!");
        }
    }

    /// <summary>
    /// Test physics settings
    /// </summary>
    private void TestPhysicsSettings()
    {
        var result = new DiagnosticInfo("Physics Settings Test");
        
        // Check gravity
        if (Physics.gravity.magnitude < 0.1f)
        {
            result.AddError("Gravity is too weak or disabled!");
        }
        else
        {
            result.AddSuccess($"Gravity: {Physics.gravity}");
        }
        
        // Check physics timestep
        result.AddInfo($"Fixed Timestep: {Time.fixedDeltaTime}");
        if (Time.fixedDeltaTime > 0.02f)
        {
            result.AddWarning("Physics timestep is quite large - may affect simulation quality");
        }
        
        // Check physics solver iterations
        result.AddInfo($"Default Solver Iterations: {Physics.defaultSolverIterations}");
        result.AddInfo($"Default Solver Velocity Iterations: {Physics.defaultSolverVelocityIterations}");
        
        diagnosticResults.Add(result);
    }

    /// <summary>
    /// Test collision layers and matrix
    /// </summary>
    private void TestCollisionLayers()
    {
        var result = new DiagnosticInfo("Collision Layers Test");
        
        // Check if layer 19 (ExplosionShards) exists
        string layerName = LayerMask.LayerToName(19);
        if (string.IsNullOrEmpty(layerName))
        {
            result.AddError("Layer 19 (ExplosionShards) not defined!");
        }
        else
        {
            result.AddSuccess($"Layer 19: '{layerName}'");
        }
        
        // Check terrain layer
        int terrainLayer = LayerMask.NameToLayer("Terrain");
        if (terrainLayer == -1)
        {
            result.AddWarning("No 'Terrain' layer found - using Default layer for terrain collision");
        }
        else
        {
            result.AddSuccess($"Terrain layer: {terrainLayer}");
        }
        
        // Check if explosion shards can collide with terrain
        bool canCollideWithTerrain = !Physics.GetIgnoreLayerCollision(19, terrainLayer == -1 ? 0 : terrainLayer);
        if (!canCollideWithTerrain)
        {
            result.AddError("ExplosionShards layer cannot collide with terrain!");
        }
        else
        {
            result.AddSuccess("ExplosionShards can collide with terrain");
        }
        
        diagnosticResults.Add(result);
    }

    /// <summary>
    /// Apply test forces to all rigidbodies in scene
    /// </summary>
    private void ApplyTestForcesToAllRigidbodies()
    {
        Rigidbody[] allRigidbodies = FindObjectsOfType<Rigidbody>();
        int forcesApplied = 0;
        
        foreach (var rb in allRigidbodies)
        {
            if (!rb.isKinematic)
            {
                Vector3 randomForce = Random.insideUnitSphere * testForce;
                randomForce.y = Mathf.Abs(randomForce.y); // Always push upward
                rb.AddForce(randomForce, ForceMode.Impulse);
                forcesApplied++;
            }
        }
        
        Debug.Log($"Applied test forces to {forcesApplied} rigidbodies");
    }

    /// <summary>
    /// Generate diagnostic report
    /// </summary>
    private void GenerateReport()
    {
        Debug.Log("\n=== EXPLOSION DIAGNOSTIC REPORT ===");
        
        int totalErrors = 0;
        int totalWarnings = 0;
        int totalSuccesses = 0;
        
        foreach (var result in diagnosticResults)
        {
            Debug.Log($"\n--- {result.TestName} ---");
            
            foreach (var message in result.Messages)
            {
                switch (message.Type)
                {
                    case DiagnosticMessageType.Error:
                        Debug.LogError($"ERROR: {message.Text}");
                        totalErrors++;
                        break;
                    case DiagnosticMessageType.Warning:
                        Debug.LogWarning($"WARNING: {message.Text}");
                        totalWarnings++;
                        break;
                    case DiagnosticMessageType.Success:
                        Debug.Log($"SUCCESS: {message.Text}");
                        totalSuccesses++;
                        break;
                    case DiagnosticMessageType.Info:
                        Debug.Log($"INFO: {message.Text}");
                        break;
                }
            }
        }
        
        Debug.Log($"\n=== SUMMARY ===");
        Debug.Log($"Errors: {totalErrors}");
        Debug.Log($"Warnings: {totalWarnings}");
        Debug.Log($"Successes: {totalSuccesses}");
        
        if (totalErrors == 0)
        {
            Debug.Log("No critical errors found! The explosion system should work.");
        }
        else
        {
            Debug.LogError($"Found {totalErrors} critical errors that need to be fixed!");
        }
    }

    void OnGUI()
    {
        if (!enableVisualDebug) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Label("Explosion Diagnostic Tool", GUI.skin.box);
        
        if (GUILayout.Button($"Run Diagnostic ({diagnosticKey})"))
        {
            RunDiagnostic();
        }
        
        if (GUILayout.Button("Apply Test Forces (F2)"))
        {
            ApplyTestForcesToAllRigidbodies();
        }
        
        GUILayout.Label($"Test Force: {testForce}");
        testForce = GUILayout.HorizontalSlider(testForce, 100f, 5000f);
        
        if (diagnosticResults.Count > 0)
        {
            GUILayout.Label($"Last Test: {diagnosticResults.Count} results");
        }
        
        GUILayout.EndArea();
    }
}

/// <summary>
/// Diagnostic information container
/// </summary>
public class DiagnosticInfo
{
    public string TestName { get; private set; }
    public List<DiagnosticMessage> Messages { get; private set; }
    
    public DiagnosticInfo(string testName)
    {
        TestName = testName;
        Messages = new List<DiagnosticMessage>();
    }
    
    public void AddError(string message)
    {
        Messages.Add(new DiagnosticMessage(DiagnosticMessageType.Error, message));
    }
    
    public void AddWarning(string message)
    {
        Messages.Add(new DiagnosticMessage(DiagnosticMessageType.Warning, message));
    }
    
    public void AddSuccess(string message)
    {
        Messages.Add(new DiagnosticMessage(DiagnosticMessageType.Success, message));
    }
    
    public void AddInfo(string message)
    {
        Messages.Add(new DiagnosticMessage(DiagnosticMessageType.Info, message));
    }
}

/// <summary>
/// Diagnostic message
/// </summary>
public class DiagnosticMessage
{
    public DiagnosticMessageType Type { get; private set; }
    public string Text { get; private set; }
    
    public DiagnosticMessage(DiagnosticMessageType type, string text)
    {
        Type = type;
        Text = text;
    }
}

/// <summary>
/// Diagnostic message types
/// </summary>
public enum DiagnosticMessageType
{
    Error,
    Warning,
    Success,
    Info
}
