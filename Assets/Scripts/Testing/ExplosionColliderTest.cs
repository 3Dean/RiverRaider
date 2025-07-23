using UnityEngine;

/// <summary>
/// Simple test script to verify the collider fix for helicopter explosions
/// </summary>
public class ExplosionColliderTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool runTestOnStart = true;
    [SerializeField] private KeyCode testKey = KeyCode.T;
    
    void Start()
    {
        if (runTestOnStart)
        {
            Invoke("RunColliderTest", 1f); // Delay to ensure everything is initialized
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            RunColliderTest();
        }
    }
    
    /// <summary>
    /// Test the collider fix by creating a test explosion
    /// </summary>
    public void RunColliderTest()
    {
        Debug.Log("=== EXPLOSION COLLIDER TEST STARTING ===");
        
        // Try to find or create a test explosion
        GameObject explosionPrefab = Resources.Load<GameObject>("HelicopterExplosionPrefab");
        
        if (explosionPrefab == null)
        {
            Debug.LogError("ExplosionColliderTest: HelicopterExplosionPrefab not found in Resources folder!");
            return;
        }
        
        // Create test explosion at camera position + forward
        Vector3 testPosition = Camera.main.transform.position + Camera.main.transform.forward * 10f;
        GameObject testExplosion = Instantiate(explosionPrefab, testPosition, Quaternion.identity);
        
        // Get the explosion component
        HelicopterExplosion explosionComponent = testExplosion.GetComponent<HelicopterExplosion>();
        if (explosionComponent != null)
        {
            Debug.Log("ExplosionColliderTest: Test explosion created successfully!");
            Debug.Log("ExplosionColliderTest: Watch for 'colliders disabled' and 'colliders enabled' messages in console");
            Debug.Log("ExplosionColliderTest: Shards should now fly apart instead of staying stuck!");
        }
        else
        {
            Debug.LogError("ExplosionColliderTest: HelicopterExplosion component not found on prefab!");
        }
    }
}
