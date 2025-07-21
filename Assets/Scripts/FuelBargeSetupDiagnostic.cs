using UnityEngine;

/// <summary>
/// Diagnostic script to quickly identify fuel barge setup issues.
/// Attach this to your fuelbarge1 prefab temporarily to diagnose collision problems.
/// </summary>
public class FuelBargeSetupDiagnostic : MonoBehaviour
{
    [Header("Diagnostic Results")]
    [SerializeField] private bool hasTriggerCollider = false;
    [SerializeField] private bool hasSolidCollider = false;
    [SerializeField] private bool hasFuelBargeCollisionScript = false;
    [SerializeField] private bool hasCorrectTag = false;
    [SerializeField] private int totalColliders = 0;
    
    void Start()
    {
        RunDiagnostic();
    }
    
    [ContextMenu("Run Diagnostic")]
    public void RunDiagnostic()
    {
        Debug.Log("=== FUEL BARGE SETUP DIAGNOSTIC ===");
        
        // Reset flags
        hasTriggerCollider = false;
        hasSolidCollider = false;
        hasFuelBargeCollisionScript = false;
        hasCorrectTag = false;
        totalColliders = 0;
        
        // Check colliders
        var colliders = GetComponents<Collider>();
        totalColliders = colliders.Length;
        
        Debug.Log($"Found {totalColliders} colliders:");
        
        foreach (var collider in colliders)
        {
            if (collider.isTrigger)
            {
                hasTriggerCollider = true;
                Debug.Log($"✅ TRIGGER Collider - Size: {collider.bounds.size}, Center: {collider.bounds.center}");
            }
            else
            {
                hasSolidCollider = true;
                Debug.Log($"✅ SOLID Collider - Size: {collider.bounds.size}, Center: {collider.bounds.center}");
            }
        }
        
        // Check for FuelBargeCollision script
        var fuelBargeScript = GetComponent<FuelBargeCollision>();
        hasFuelBargeCollisionScript = fuelBargeScript != null;
        
        // Check tag
        hasCorrectTag = gameObject.CompareTag("FuelBarge");
        
        // Report results
        Debug.Log("\n=== DIAGNOSTIC RESULTS ===");
        Debug.Log($"Trigger Collider: {(hasTriggerCollider ? "✅ FOUND" : "❌ MISSING")}");
        Debug.Log($"Solid Collider: {(hasSolidCollider ? "✅ FOUND" : "❌ MISSING")}");
        Debug.Log($"FuelBargeCollision Script: {(hasFuelBargeCollisionScript ? "✅ FOUND" : "❌ MISSING")}");
        Debug.Log($"Correct Tag (FuelBarge): {(hasCorrectTag ? "✅ CORRECT" : "❌ WRONG")} - Current: '{gameObject.tag}'");
        
        // Provide specific fix recommendations
        Debug.Log("\n=== RECOMMENDATIONS ===");
        
        if (!hasTriggerCollider && !hasSolidCollider)
        {
            Debug.LogError("❌ NO COLLIDERS FOUND! Add 2 Box Colliders - one trigger, one solid.");
        }
        else if (!hasTriggerCollider)
        {
            Debug.LogError("❌ MISSING TRIGGER COLLIDER! Set one collider to 'Is Trigger = true' for refueling.");
        }
        else if (!hasSolidCollider)
        {
            Debug.LogError("❌ MISSING SOLID COLLIDER! Set one collider to 'Is Trigger = false' for collision damage.");
        }
        else
        {
            Debug.Log("✅ Collider setup looks correct!");
        }
        
        if (!hasFuelBargeCollisionScript)
        {
            Debug.LogError("❌ MISSING SCRIPT! Add 'FuelBargeCollision' component to handle crash damage.");
        }
        else
        {
            Debug.Log("✅ FuelBargeCollision script found!");
        }
        
        if (!hasCorrectTag)
        {
            Debug.LogError($"❌ WRONG TAG! Change tag from '{gameObject.tag}' to 'FuelBarge'.");
        }
        else
        {
            Debug.Log("✅ Tag is correct!");
        }
        
        // Check for player
        var player = FindObjectOfType<PlayerShipHealth>();
        if (player != null)
        {
            Debug.Log($"✅ Player found: '{player.name}' with tag '{player.tag}'");
            
            var playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Debug.Log($"✅ Player Rigidbody: IsKinematic={playerRb.isKinematic}, Mass={playerRb.mass}");
                if (playerRb.isKinematic)
                {
                    Debug.Log("ℹ️ Player is kinematic - collision damage will use trigger-based system");
                }
                else
                {
                    Debug.Log("ℹ️ Player is non-kinematic - collision damage will use OnCollisionEnter");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Player has no Rigidbody - collision detection may not work!");
            }
        }
        else
        {
            Debug.LogError("❌ NO PLAYER FOUND! Make sure player has PlayerShipHealth component.");
        }
        
        Debug.Log("\n=== SUMMARY ===");
        bool setupComplete = hasTriggerCollider && hasSolidCollider && hasFuelBargeCollisionScript && hasCorrectTag;
        
        if (setupComplete)
        {
            Debug.Log("🎉 FUEL BARGE SETUP IS COMPLETE! Should work correctly.");
        }
        else
        {
            Debug.LogError("❌ SETUP INCOMPLETE! Fix the issues above and test again.");
        }
        
        Debug.Log("=== END DIAGNOSTIC ===\n");
    }
    
    void OnValidate()
    {
        // Update inspector values when component is selected
        if (Application.isPlaying)
        {
            RunDiagnostic();
        }
    }
}
