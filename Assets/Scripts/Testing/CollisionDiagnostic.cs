using UnityEngine;

/// <summary>
/// Comprehensive collision detection diagnostic tool
/// Helps identify and fix bullet/missile collision issues with enemies
/// </summary>
public class CollisionDiagnostic : MonoBehaviour
{
    [Header("Diagnostic Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showVisualDebug = true;
    [SerializeField] private bool autoFixIssues = true;
    
    [Header("Test Controls")]
    [SerializeField] private KeyCode runDiagnosticKey = KeyCode.F1;
    [SerializeField] private KeyCode fixIssuesKey = KeyCode.F2;
    [SerializeField] private KeyCode testCollisionKey = KeyCode.F3;
    
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private Transform testFirePoint;
    
    private void Update()
    {
        if (Input.GetKeyDown(runDiagnosticKey))
        {
            RunCompleteDiagnostic();
        }
        
        if (Input.GetKeyDown(fixIssuesKey))
        {
            FixCollisionIssues();
        }
        
        if (Input.GetKeyDown(testCollisionKey))
        {
            TestCollisionDetection();
        }
    }
    
    [ContextMenu("Run Complete Diagnostic")]
    public void RunCompleteDiagnostic()
    {
        Debug.Log("=== COLLISION DETECTION DIAGNOSTIC ===");
        
        CheckLayerConfiguration();
        CheckTagConfiguration();
        CheckHelicopterPrefabs();
        CheckBulletConfiguration();
        CheckMissileConfiguration();
        CheckPhysicsMatrix();
        
        Debug.Log("=== DIAGNOSTIC COMPLETE ===");
    }
    
    private void CheckLayerConfiguration()
    {
        Debug.Log("--- Layer Configuration Check ---");
        
        // Check if Helicopter layer exists (should be layer 13)
        string helicopterLayerName = LayerMask.LayerToName(13);
        if (helicopterLayerName == "Helicopter")
        {
            Debug.Log("✅ Helicopter layer (13) configured correctly");
        }
        else
        {
            Debug.LogError($"❌ Helicopter layer issue: Layer 13 = '{helicopterLayerName}', expected 'Helicopter'");
        }
        
        // Check if Bullet layer exists (should be layer 7)
        string bulletLayerName = LayerMask.LayerToName(7);
        if (bulletLayerName == "Bullet")
        {
            Debug.Log("✅ Bullet layer (7) configured correctly");
        }
        else
        {
            Debug.LogError($"❌ Bullet layer issue: Layer 7 = '{bulletLayerName}', expected 'Bullet'");
        }
    }
    
    private void CheckTagConfiguration()
    {
        Debug.Log("--- Tag Configuration Check ---");
        
        // Check if Enemy tag exists
        try
        {
            GameObject.FindGameObjectWithTag("Enemy");
            Debug.Log("✅ 'Enemy' tag exists");
        }
        catch
        {
            Debug.LogError("❌ 'Enemy' tag not found in project");
        }
        
        // Check if Player tag exists
        try
        {
            GameObject.FindGameObjectWithTag("Player");
            Debug.Log("✅ 'Player' tag exists");
        }
        catch
        {
            Debug.LogError("❌ 'Player' tag not found in project");
        }
    }
    
    private void CheckHelicopterPrefabs()
    {
        Debug.Log("--- Helicopter Prefab Check ---");
        
        // Find all helicopter enemies in scene
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        
        if (enemies.Length == 0)
        {
            Debug.LogWarning("⚠️ No EnemyAI components found in scene");
            return;
        }
        
        foreach (EnemyAI enemy in enemies)
        {
            GameObject enemyObj = enemy.gameObject;
            
            Debug.Log($"Checking helicopter: {enemyObj.name}");
            
            // Check layer
            if (enemyObj.layer == 13) // Helicopter layer
            {
                Debug.Log($"  ✅ Layer: {enemyObj.layer} (Helicopter)");
            }
            else
            {
                Debug.LogError($"  ❌ Layer: {enemyObj.layer} (Expected: 13 - Helicopter)");
            }
            
            // Check tag
            if (enemyObj.CompareTag("Enemy"))
            {
                Debug.Log($"  ✅ Tag: Enemy");
            }
            else
            {
                Debug.LogError($"  ❌ Tag: {enemyObj.tag} (Expected: Enemy)");
            }
            
            // Check colliders
            Collider[] colliders = enemyObj.GetComponentsInChildren<Collider>();
            if (colliders.Length > 0)
            {
                Debug.Log($"  ✅ Colliders found: {colliders.Length}");
                foreach (Collider col in colliders)
                {
                    Debug.Log($"    - {col.name}: IsTrigger={col.isTrigger}, Type={col.GetType().Name}");
                }
            }
            else
            {
                Debug.LogError($"  ❌ No colliders found on {enemyObj.name}");
            }
        }
    }
    
    private void CheckBulletConfiguration()
    {
        Debug.Log("--- Bullet Configuration Check ---");
        
        // Check bullet prefab
        if (bulletPrefab == null)
        {
            bulletPrefab = Resources.Load<GameObject>("Bullet");
        }
        
        if (bulletPrefab == null)
        {
            Debug.LogError("❌ Bullet prefab not found");
            return;
        }
        
        Bullet bulletScript = bulletPrefab.GetComponent<Bullet>();
        if (bulletScript == null)
        {
            Debug.LogError("❌ Bullet prefab missing Bullet component");
            return;
        }
        
        Debug.Log($"✅ Bullet prefab found: {bulletPrefab.name}");
        
        // Check bullet layer
        if (bulletPrefab.layer == 7) // Bullet layer
        {
            Debug.Log($"  ✅ Bullet layer: {bulletPrefab.layer} (Bullet)");
        }
        else
        {
            Debug.LogError($"  ❌ Bullet layer: {bulletPrefab.layer} (Expected: 7 - Bullet)");
        }
        
        // Check collider
        Collider bulletCollider = bulletPrefab.GetComponent<Collider>();
        if (bulletCollider != null)
        {
            Debug.Log($"  ✅ Bullet collider: {bulletCollider.GetType().Name}, IsTrigger={bulletCollider.isTrigger}");
        }
        else
        {
            Debug.LogError("  ❌ Bullet prefab missing collider");
        }
    }
    
    private void CheckMissileConfiguration()
    {
        Debug.Log("--- Missile Configuration Check ---");
        
        // Check missile prefabs
        string[] missileNames = { "missileARM30", "missileARM60", "missileARM90" };
        
        foreach (string missileName in missileNames)
        {
            GameObject missilePrefab = Resources.Load<GameObject>(missileName);
            if (missilePrefab == null)
            {
                Debug.LogWarning($"⚠️ Missile prefab not found: {missileName}");
                continue;
            }
            
            Missile missileScript = missilePrefab.GetComponent<Missile>();
            if (missileScript == null)
            {
                Debug.LogError($"❌ {missileName} missing Missile component");
                continue;
            }
            
            Debug.Log($"✅ Missile prefab found: {missileName}");
            
            // Check collider
            Collider missileCollider = missilePrefab.GetComponent<Collider>();
            if (missileCollider != null)
            {
                Debug.Log($"  ✅ Missile collider: {missileCollider.GetType().Name}, IsTrigger={missileCollider.isTrigger}");
            }
            else
            {
                Debug.LogError($"  ❌ {missileName} missing collider");
            }
        }
    }
    
    private void CheckPhysicsMatrix()
    {
        Debug.Log("--- Physics Layer Matrix Check ---");
        
        // Check if Bullet layer can collide with Helicopter layer
        bool canCollide = !Physics.GetIgnoreLayerCollision(7, 13); // Bullet vs Helicopter
        
        if (canCollide)
        {
            Debug.Log("✅ Bullet layer (7) can collide with Helicopter layer (13)");
        }
        else
        {
            Debug.LogError("❌ Bullet layer (7) is set to ignore Helicopter layer (13) in Physics Matrix");
        }
    }
    
    [ContextMenu("Fix Collision Issues")]
    public void FixCollisionIssues()
    {
        Debug.Log("=== FIXING COLLISION ISSUES ===");
        
        FixHelicopterConfiguration();
        FixBulletConfiguration();
        FixPhysicsMatrix();
        
        Debug.Log("=== FIXES COMPLETE ===");
    }
    
    private void FixHelicopterConfiguration()
    {
        Debug.Log("--- Fixing Helicopter Configuration ---");
        
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        
        foreach (EnemyAI enemy in enemies)
        {
            GameObject enemyObj = enemy.gameObject;
            bool wasFixed = false;
            
            // Fix layer
            if (enemyObj.layer != 13)
            {
                enemyObj.layer = 13; // Helicopter layer
                Debug.Log($"✅ Fixed layer for {enemyObj.name}: set to 13 (Helicopter)");
                wasFixed = true;
            }
            
            // Fix tag
            if (!enemyObj.CompareTag("Enemy"))
            {
                enemyObj.tag = "Enemy";
                Debug.Log($"✅ Fixed tag for {enemyObj.name}: set to 'Enemy'");
                wasFixed = true;
            }
            
            // Ensure colliders exist and are properly configured
            Collider[] colliders = enemyObj.GetComponentsInChildren<Collider>();
            if (colliders.Length == 0)
            {
                // Add a basic collider if none exists
                BoxCollider newCollider = enemyObj.AddComponent<BoxCollider>();
                newCollider.isTrigger = true;
                newCollider.size = Vector3.one * 2f; // Reasonable size for helicopter
                Debug.Log($"✅ Added trigger collider to {enemyObj.name}");
                wasFixed = true;
            }
            else
            {
                // Ensure at least one collider is a trigger for collision detection
                bool hasTrigger = false;
                foreach (Collider col in colliders)
                {
                    if (col.isTrigger)
                    {
                        hasTrigger = true;
                        break;
                    }
                }
                
                if (!hasTrigger)
                {
                    colliders[0].isTrigger = true;
                    Debug.Log($"✅ Set {colliders[0].name} as trigger on {enemyObj.name}");
                    wasFixed = true;
                }
            }
            
            if (!wasFixed)
            {
                Debug.Log($"✅ {enemyObj.name} already configured correctly");
            }
        }
    }
    
    private void FixBulletConfiguration()
    {
        Debug.Log("--- Fixing Bullet Configuration ---");
        
        // This would typically be done in the prefab, but we can log what needs to be fixed
        Debug.Log("ℹ️ Bullet configuration should be fixed in the prefab:");
        Debug.Log("  - Ensure bullet prefab is on layer 7 (Bullet)");
        Debug.Log("  - Ensure bullet has trigger collider");
        Debug.Log("  - Ensure hitLayers includes layer 13 (Helicopter)");
    }
    
    private void FixPhysicsMatrix()
    {
        Debug.Log("--- Fixing Physics Matrix ---");
        
        // Enable collision between Bullet (7) and Helicopter (13) layers
        Physics.IgnoreLayerCollision(7, 13, false);
        Debug.Log("✅ Enabled collision between Bullet layer (7) and Helicopter layer (13)");
        
        // Also ensure bullets don't collide with each other
        Physics.IgnoreLayerCollision(7, 7, true);
        Debug.Log("✅ Disabled collision between Bullet layer (7) and itself");
    }
    
    [ContextMenu("Test Collision Detection")]
    public void TestCollisionDetection()
    {
        if (testFirePoint == null)
        {
            testFirePoint = transform;
        }
        
        // Find nearest enemy
        EnemyAI nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null)
        {
            Debug.LogWarning("No enemies found for collision test");
            return;
        }
        
        Debug.Log($"Testing collision with {nearestEnemy.name}");
        
        // Test bullet collision
        TestBulletCollision(nearestEnemy);
        
        // Test missile collision (after a delay)
        Invoke(nameof(TestMissileCollisionDelayed), 2f);
    }
    
    private void TestBulletCollision(EnemyAI target)
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet prefab not assigned for test");
            return;
        }
        
        Vector3 direction = (target.transform.position - testFirePoint.position).normalized;
        
        GameObject testBullet = Instantiate(bulletPrefab, testFirePoint.position, Quaternion.LookRotation(direction));
        Bullet bulletScript = testBullet.GetComponent<Bullet>();
        
        if (bulletScript != null)
        {
            bulletScript.Initialize(direction, 50f);
            Debug.Log("🔫 Test bullet fired at enemy");
        }
    }
    
    private void TestMissileCollisionDelayed()
    {
        EnemyAI nearestEnemy = FindNearestEnemy();
        if (nearestEnemy != null && missilePrefab != null)
        {
            Vector3 direction = (nearestEnemy.transform.position - testFirePoint.position).normalized;
            
            GameObject testMissile = Instantiate(missilePrefab, testFirePoint.position, Quaternion.LookRotation(direction));
            Missile missileScript = testMissile.GetComponent<Missile>();
            
            if (missileScript != null)
            {
                missileScript.Initialize(40f, 100f, direction);
                Debug.Log("🚀 Test missile fired at enemy");
            }
        }
    }
    
    private EnemyAI FindNearestEnemy()
    {
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        if (enemies.Length == 0) return null;
        
        EnemyAI nearest = enemies[0];
        float nearestDistance = Vector3.Distance(transform.position, nearest.transform.position);
        
        foreach (EnemyAI enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearest = enemy;
                nearestDistance = distance;
            }
        }
        
        return nearest;
    }
    
    private void OnGUI()
    {
        if (!showVisualDebug) return;
        
        GUILayout.BeginArea(new Rect(10, 250, 400, 200));
        GUILayout.Label("Collision Diagnostic Tool", GUI.skin.box);
        
        GUILayout.Label($"Press {runDiagnosticKey} - Run Diagnostic");
        GUILayout.Label($"Press {fixIssuesKey} - Fix Issues");
        GUILayout.Label($"Press {testCollisionKey} - Test Collision");
        
        if (GUILayout.Button("Run Complete Diagnostic"))
        {
            RunCompleteDiagnostic();
        }
        
        if (GUILayout.Button("Fix All Issues"))
        {
            FixCollisionIssues();
        }
        
        if (GUILayout.Button("Test Collision"))
        {
            TestCollisionDetection();
        }
        
        GUILayout.EndArea();
    }
}
