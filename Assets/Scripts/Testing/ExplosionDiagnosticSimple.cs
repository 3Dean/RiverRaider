using UnityEngine;

/// <summary>
/// Simple diagnostic script to identify why explosion pieces vanish
/// Attach to any GameObject and press D to run diagnostics
/// </summary>
public class ExplosionDiagnosticSimple : MonoBehaviour
{
    [Header("Diagnostic Settings")]
    [SerializeField] private KeyCode diagnosticKey = KeyCode.D;
    [SerializeField] private bool continuousMonitoring = true;
    
    void Update()
    {
        if (Input.GetKeyDown(diagnosticKey))
        {
            RunExplosionDiagnostic();
        }
        
        if (continuousMonitoring)
        {
            MonitorExplosions();
        }
    }
    
    /// <summary>
    /// Run a comprehensive diagnostic of all explosion-related objects
    /// </summary>
    void RunExplosionDiagnostic()
    {
        Debug.Log("=== EXPLOSION DIAGNOSTIC START ===");
        
        // Find all explosion-related objects
        HelicopterExplosion[] oldExplosions = FindObjectsOfType<HelicopterExplosion>();
        HelicopterExplosionAnimated[] newExplosions = FindObjectsOfType<HelicopterExplosionAnimated>();
        ExplosionShard[] oldShards = FindObjectsOfType<ExplosionShard>();
        ExplosionShardAnimated[] newShards = FindObjectsOfType<ExplosionShardAnimated>();
        
        Debug.Log($"Found {oldExplosions.Length} old HelicopterExplosion components");
        Debug.Log($"Found {newExplosions.Length} new HelicopterExplosionAnimated components");
        Debug.Log($"Found {oldShards.Length} old ExplosionShard components");
        Debug.Log($"Found {newShards.Length} new ExplosionShardAnimated components");
        
        // Check explosion prefab in Resources
        GameObject explosionPrefab = Resources.Load<GameObject>("HelicopterExplosionPrefab");
        if (explosionPrefab != null)
        {
            Debug.Log($"Explosion prefab found: {explosionPrefab.name}");
            
            // Check what components it has
            HelicopterExplosion oldComp = explosionPrefab.GetComponent<HelicopterExplosion>();
            HelicopterExplosionAnimated newComp = explosionPrefab.GetComponent<HelicopterExplosionAnimated>();
            
            Debug.Log($"Prefab has old component: {oldComp != null}");
            Debug.Log($"Prefab has new component: {newComp != null}");
            
            // Count child objects
            int childCount = explosionPrefab.transform.childCount;
            Debug.Log($"Prefab has {childCount} child objects (potential shards)");
        }
        else
        {
            Debug.LogError("HelicopterExplosionPrefab not found in Resources!");
        }
        
        // Check active explosions
        foreach (HelicopterExplosionAnimated explosion in newExplosions)
        {
            if (explosion != null)
            {
                ExplosionAnimationStats stats = explosion.GetExplosionStats();
                Debug.Log($"Active explosion: {explosion.name} - {stats.completedShards}/{stats.totalShards} shards, progress: {stats.progress:P0}");
            }
        }
        
        Debug.Log("=== EXPLOSION DIAGNOSTIC END ===");
    }
    
    /// <summary>
    /// Monitor explosions continuously
    /// </summary>
    void MonitorExplosions()
    {
        // Only log every 2 seconds to avoid spam
        if (Time.time % 2f < 0.1f)
        {
            HelicopterExplosionAnimated[] explosions = FindObjectsOfType<HelicopterExplosionAnimated>();
            if (explosions.Length > 0)
            {
                foreach (HelicopterExplosionAnimated explosion in explosions)
                {
                    ExplosionAnimationStats stats = explosion.GetExplosionStats();
                    Debug.Log($"Monitoring: {explosion.name} - {stats.completedShards}/{stats.totalShards} shards at {explosion.transform.position}");
                }
            }
        }
    }
    
    void OnGUI()
    {
        GUI.Label(new Rect(10, 200, 300, 20), $"Press {diagnosticKey} for Explosion Diagnostic");
        
        // Show current explosion count
        HelicopterExplosionAnimated[] explosions = FindObjectsOfType<HelicopterExplosionAnimated>();
        GUI.Label(new Rect(10, 220, 300, 20), $"Active Explosions: {explosions.Length}");
        
        if (explosions.Length > 0)
        {
            for (int i = 0; i < explosions.Length && i < 3; i++)
            {
                ExplosionAnimationStats stats = explosions[i].GetExplosionStats();
                GUI.Label(new Rect(10, 240 + i * 20, 400, 20), 
                    $"Explosion {i+1}: {stats.completedShards}/{stats.totalShards} shards ({stats.progress:P0})");
            }
        }
    }
}
