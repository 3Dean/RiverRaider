using UnityEngine;

/// <summary>
/// Manual tester for helicopter explosion system
/// Press T to trigger explosion at current position
/// </summary>
public class ExplosionManualTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool enableTesting = true;
    [SerializeField] private KeyCode testKey = KeyCode.T;
    [SerializeField] private bool showDebugInfo = true;
    
    void Update()
    {
        if (!enableTesting) return;
        
        if (Input.GetKeyDown(testKey))
        {
            TriggerExplosionTest();
        }
    }
    
    private void TriggerExplosionTest()
    {
        if (showDebugInfo)
        {
            Debug.Log("ExplosionManualTester: Attempting to trigger explosion test...");
        }
        
        // Try to load the explosion prefab
        GameObject explosionPrefab = Resources.Load<GameObject>("HelicopterExplosionPrefab");
        
        if (explosionPrefab == null)
        {
            Debug.LogError("ExplosionManualTester: Could not load HelicopterExplosionPrefab from Resources!");
            return;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"ExplosionManualTester: Successfully loaded prefab: {explosionPrefab.name}");
        }
        
        // Get test position (player position or camera position)
        Vector3 testPosition = transform.position;
        
        // Try to get player position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            testPosition = player.transform.position + Vector3.up * 5f; // Slightly above player
        }
        else
        {
            // Use camera position if no player found
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                testPosition = mainCamera.transform.position + mainCamera.transform.forward * 10f;
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"ExplosionManualTester: Creating explosion at position: {testPosition}");
        }
        
        // Create the explosion
        GameObject explosion = Instantiate(explosionPrefab, testPosition, Quaternion.identity);
        
        if (explosion == null)
        {
            Debug.LogError("ExplosionManualTester: Failed to instantiate explosion!");
            return;
        }
        
        // Get the HelicopterExplosion component
        HelicopterExplosion explosionComponent = explosion.GetComponent<HelicopterExplosion>();
        
        if (explosionComponent == null)
        {
            Debug.LogError("ExplosionManualTester: Explosion prefab has no HelicopterExplosion component!");
            return;
        }
        
        if (showDebugInfo)
        {
            Debug.Log("ExplosionManualTester: Successfully created explosion with component!");
        }
        
        // Set test parameters
        explosionComponent.SetDamageDirection(Vector3.forward);
        explosionComponent.SetExplosionParameters(1500f, 10f, 0.4f);
        
        if (showDebugInfo)
        {
            Debug.Log("ExplosionManualTester: Explosion test completed! Watch for shards flying apart.");
        }
    }
    
    void OnGUI()
    {
        if (!enableTesting) return;
        
        GUI.Label(new Rect(10, 10, 300, 20), $"Press {testKey} to trigger explosion test");
        GUI.Label(new Rect(10, 30, 300, 20), "Check console for debug info");
    }
}
