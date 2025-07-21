using UnityEngine;

/// <summary>
/// Diagnostic script to identify collision issues between player and fuel barge.
/// Attach this to the player ship to debug collision problems.
/// </summary>
public class CollisionDiagnostic : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logAllCollisions = true;
    [SerializeField] private bool logAllTriggers = true;
    
    void Start()
    {
        if (!enableDebugLogging) return;
        
        Debug.Log("=== COLLISION DIAGNOSTIC START ===");
        Debug.Log($"Player GameObject: '{gameObject.name}'");
        Debug.Log($"Player Tag: '{gameObject.tag}'");
        Debug.Log($"Player Layer: {gameObject.layer}");
        
        // Check components
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log($"Rigidbody: IsKinematic={rb.isKinematic}, Mass={rb.mass}, UseGravity={rb.useGravity}");
            if (rb.isKinematic)
            {
                Debug.LogWarning("KINEMATIC RIGIDBODY DETECTED! This may prevent OnCollisionEnter from working properly.");
            }
        }
        else
        {
            Debug.LogError("NO RIGIDBODY FOUND! Collisions will not work without a Rigidbody.");
        }
        
        // Check colliders
        var colliders = GetComponents<Collider>();
        Debug.Log($"Player has {colliders.Length} colliders:");
        for (int i = 0; i < colliders.Length; i++)
        {
            var col = colliders[i];
            Debug.Log($"  Collider {i}: {col.GetType().Name}, IsTrigger={col.isTrigger}, Enabled={col.enabled}");
            Debug.Log($"    Bounds: Size={col.bounds.size}, Center={col.bounds.center}");
        }
        
        // Check health component
        var health = GetComponent<PlayerShipHealth>();
        if (health != null)
        {
            Debug.Log($"PlayerShipHealth: Found, IsAlive={health.IsAlive()}");
        }
        else
        {
            Debug.LogError("NO PlayerShipHealth COMPONENT! Damage system will not work.");
        }
        
        // Check for fuel barges in scene
        var fuelBarges = FindObjectsOfType<FuelBargeCollision>();
        Debug.Log($"Found {fuelBarges.Length} fuel barges in scene:");
        foreach (var barge in fuelBarges)
        {
            Debug.Log($"  Fuel Barge: '{barge.name}', Tag='{barge.tag}', Layer={barge.gameObject.layer}");
            var bargeColliders = barge.GetComponents<Collider>();
            Debug.Log($"    Has {bargeColliders.Length} colliders");
            foreach (var bargeCol in bargeColliders)
            {
                Debug.Log($"      {bargeCol.GetType().Name}: IsTrigger={bargeCol.isTrigger}, Size={bargeCol.bounds.size}");
            }
        }
        
        Debug.Log("=== COLLISION DIAGNOSTIC END ===");
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!enableDebugLogging || !logAllCollisions) return;
        
        Debug.Log($"PLAYER OnCollisionEnter: Hit '{collision.gameObject.name}' (tag: '{collision.gameObject.tag}')");
        Debug.Log($"  Impact force: {collision.relativeVelocity.magnitude:F2}");
        Debug.Log($"  Contact points: {collision.contacts.Length}");
        
        if (collision.gameObject.CompareTag("FuelBarge"))
        {
            Debug.Log("*** FUEL BARGE COLLISION DETECTED! ***");
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        if (!enableDebugLogging || !logAllCollisions) return;
        
        Debug.Log($"PLAYER OnCollisionExit: Left '{collision.gameObject.name}'");
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!enableDebugLogging || !logAllTriggers) return;
        
        Debug.Log($"PLAYER OnTriggerEnter: Entered '{other.gameObject.name}' (tag: '{other.gameObject.tag}')");
        
        if (other.gameObject.CompareTag("FuelBarge"))
        {
            Debug.Log("*** FUEL BARGE TRIGGER DETECTED! ***");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (!enableDebugLogging || !logAllTriggers) return;
        
        Debug.Log($"PLAYER OnTriggerExit: Left '{other.gameObject.name}'");
    }
    
    // Test method to manually trigger damage
    [ContextMenu("Test Fuel Barge Damage")]
    private void TestFuelBargeDamage()
    {
        var health = GetComponent<PlayerShipHealth>();
        if (health != null)
        {
            health.TakeDamage(35f);
            Debug.Log("Test: Applied 35 damage to player (simulating fuel barge crash)");
        }
        else
        {
            Debug.LogError("Cannot test damage - no PlayerShipHealth component!");
        }
    }
}
