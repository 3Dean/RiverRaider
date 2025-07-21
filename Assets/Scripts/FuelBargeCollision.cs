using UnityEngine;

/// <summary>
/// Handles collision damage for fuel barge solid collider.
/// Works alongside the trigger collider for refueling functionality.
/// Prevents bullets from hitting the refueling trigger zone while allowing them to hit the solid structure.
/// </summary>
[DisallowMultipleComponent]
public class FuelBargeCollision : MonoBehaviour
{
    [Header("Collision Damage")]
    [SerializeField] private float crashDamage = 35f;
    [SerializeField] private bool enableBulletDamage = true;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject crashEffect; // Optional explosion/sparks effect
    [SerializeField] private GameObject bulletHitEffect; // Optional bullet impact effect
    
    [Header("Audio")]
    [SerializeField] private AudioClip crashSound;
    [SerializeField] private AudioClip bulletHitSound;
    
    private AudioSource audioSource;
    
    void Start()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (crashSound != null || bulletHitSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Verify setup
        var colliders = GetComponents<Collider>();
        var solidColliders = 0;
        var triggerColliders = 0;
        
        foreach (var col in colliders)
        {
            if (col.isTrigger)
            {
                triggerColliders++;
                Debug.Log($"FuelBargeCollision: TRIGGER collider - Size: {col.bounds.size}, Center: {col.bounds.center}");
            }
            else
            {
                solidColliders++;
                Debug.Log($"FuelBargeCollision: SOLID collider - Size: {col.bounds.size}, Center: {col.bounds.center}");
            }
        }
        
        Debug.Log($"FuelBargeCollision: Initialized - Found {solidColliders} solid colliders, {triggerColliders} trigger colliders");
        Debug.Log($"FuelBargeCollision: GameObject tag = '{gameObject.tag}', layer = {gameObject.layer}");
        
        // Check for player in scene
        var player = FindObjectOfType<PlayerShipHealth>();
        if (player != null)
        {
            Debug.Log($"FuelBargeCollision: Found player '{player.name}' with tag '{player.tag}' on layer {player.gameObject.layer}");
            var playerColliders = player.GetComponents<Collider>();
            Debug.Log($"FuelBargeCollision: Player has {playerColliders.Length} colliders");
            var playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Debug.Log($"FuelBargeCollision: Player rigidbody - IsKinematic: {playerRb.isKinematic}, Mass: {playerRb.mass}");
            }
            else
            {
                Debug.LogWarning("FuelBargeCollision: Player has NO RIGIDBODY!");
            }
        }
        else
        {
            Debug.LogError("FuelBargeCollision: NO PLAYER WITH PlayerShipHealth FOUND IN SCENE!");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"FuelBargeCollision: OnCollisionEnter triggered by '{collision.gameObject.name}' with tag '{collision.gameObject.tag}'");
        
        // Handle player crashes
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("FuelBargeCollision: Player collision detected - calling HandlePlayerCrash");
            HandlePlayerCrash(collision);
        }
        // Handle bullet hits on solid collider only
        else if (collision.gameObject.CompareTag("Bullet") && enableBulletDamage)
        {
            Debug.Log("FuelBargeCollision: Bullet collision detected - calling HandleBulletHit");
            HandleBulletHit(collision);
        }
        else
        {
            Debug.Log($"FuelBargeCollision: Unhandled collision with '{collision.gameObject.name}' (tag: '{collision.gameObject.tag}')");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"FuelBargeCollision: OnTriggerEnter triggered by '{other.gameObject.name}' with tag '{other.gameObject.tag}'");
        
        // HYBRID COLLISION SYSTEM: Handle damage via trigger if OnCollisionEnter doesn't work
        // This is needed because kinematic rigidbodies don't trigger OnCollisionEnter properly
        if (other.CompareTag("Player"))
        {
            Debug.Log("FuelBargeCollision: Player entered trigger zone");
            
            // CRITICAL FIX: Only apply damage if this is the DAMAGE collider, not the refueling collider
            // We need to identify which collider triggered this event
            Collider triggeringCollider = GetTriggeringCollider(other);
            
            if (triggeringCollider != null && IsDamageCollider(triggeringCollider))
            {
                Debug.Log("FuelBargeCollision: Player entered DAMAGE trigger zone");
                
                // Check if player has kinematic rigidbody (which won't trigger OnCollisionEnter)
                var playerRb = other.GetComponent<Rigidbody>();
                if (playerRb != null && playerRb.isKinematic)
                {
                    Debug.Log("FuelBargeCollision: KINEMATIC PLAYER DETECTED - Using trigger-based damage system");
                    HandlePlayerCrashViaTrigger(other);
                }
                else
                {
                    Debug.Log("FuelBargeCollision: Non-kinematic player - damage should be handled by OnCollisionEnter");
                }
            }
            else
            {
                Debug.Log("FuelBargeCollision: Player entered REFUELING trigger zone - NO DAMAGE applied");
            }
        }
    }
    
    /// <summary>
    /// Determines which collider triggered the OnTriggerEnter event.
    /// Since Unity doesn't tell us which specific collider was triggered, we use bounds checking.
    /// </summary>
    private Collider GetTriggeringCollider(Collider playerCollider)
    {
        var colliders = GetComponents<Collider>();
        Vector3 playerPosition = playerCollider.bounds.center;
        
        foreach (var col in colliders)
        {
            if (col.isTrigger && col.bounds.Contains(playerPosition))
            {
                return col;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Determines if a collider is the damage collider using improved detection logic.
    /// Uses size, position, and bounds analysis to distinguish between refuel and damage zones.
    /// </summary>
    private bool IsDamageCollider(Collider collider)
    {
        Debug.Log($"FuelBargeCollision: Analyzing collider for damage classification");
        Debug.Log($"FuelBargeCollision: Collider bounds - Size: {collider.bounds.size}, Center: {collider.bounds.center}");
        
        // Get all trigger colliders on this GameObject
        var allColliders = GetComponents<Collider>();
        var triggerColliders = new System.Collections.Generic.List<Collider>();
        
        foreach (var col in allColliders)
        {
            if (col.isTrigger)
                triggerColliders.Add(col);
        }
        
        Debug.Log($"FuelBargeCollision: Found {triggerColliders.Count} trigger colliders total");
        
        // If we only have one trigger collider, we need to determine its purpose
        if (triggerColliders.Count == 1)
        {
            // Check if this is a large collider (likely refuel zone)
            float volume = collider.bounds.size.x * collider.bounds.size.y * collider.bounds.size.z;
            bool isLarge = volume > 10f; // Adjust threshold as needed
            
            Debug.Log($"FuelBargeCollision: Single trigger collider detected. Volume: {volume:F2}, IsLarge: {isLarge}");
            
            // Large single collider = refuel zone, small = damage zone
            if (isLarge)
            {
                Debug.Log("FuelBargeCollision: Single large trigger identified as REFUEL collider");
                return false;
            }
            else
            {
                Debug.Log("FuelBargeCollision: Single small trigger identified as DAMAGE collider");
                return true;
            }
        }
        
        // If we have exactly 2 trigger colliders, compare them
        if (triggerColliders.Count == 2)
        {
            var col1 = triggerColliders[0];
            var col2 = triggerColliders[1];
            
            // Calculate volumes
            float vol1 = col1.bounds.size.x * col1.bounds.size.y * col1.bounds.size.z;
            float vol2 = col2.bounds.size.x * col2.bounds.size.y * col2.bounds.size.z;
            
            Debug.Log($"FuelBargeCollision: Collider 1 volume: {vol1:F2}, Collider 2 volume: {vol2:F2}");
            
            // The damage collider should be the smaller one
            Collider damageCollider = (vol1 < vol2) ? col1 : col2;
            Collider refuelCollider = (vol1 < vol2) ? col2 : col1;
            
            bool isDamage = (collider == damageCollider);
            
            Debug.Log($"FuelBargeCollision: Damage collider volume: {(vol1 < vol2 ? vol1 : vol2):F2}");
            Debug.Log($"FuelBargeCollision: Refuel collider volume: {(vol1 < vol2 ? vol2 : vol1):F2}");
            Debug.Log($"FuelBargeCollision: Current collider identified as {(isDamage ? "DAMAGE" : "REFUEL")} collider");
            
            return isDamage;
        }
        
        // If we have more than 2 trigger colliders, use position-based logic
        if (triggerColliders.Count > 2)
        {
            Debug.LogWarning($"FuelBargeCollision: Found {triggerColliders.Count} trigger colliders - using position-based detection");
            
            // Sort by Y position (lower colliders are more likely to be damage zones)
            triggerColliders.Sort((a, b) => a.bounds.center.y.CompareTo(b.bounds.center.y));
            
            // Bottom half = damage colliders, top half = refuel colliders
            int damageCount = Mathf.CeilToInt(triggerColliders.Count / 2f);
            bool isDamage = false;
            
            for (int i = 0; i < damageCount; i++)
            {
                if (triggerColliders[i] == collider)
                {
                    isDamage = true;
                    break;
                }
            }
            
            Debug.Log($"FuelBargeCollision: Position-based detection - collider is {(isDamage ? "DAMAGE" : "REFUEL")}");
            return isDamage;
        }
        
        // Final fallback - assume damage for safety
        Debug.LogWarning("FuelBargeCollision: Cannot determine collider type - defaulting to DAMAGE for safety");
        return true;
    }
    
    private void HandlePlayerCrashViaTrigger(Collider playerCollider)
    {
        var playerHealth = playerCollider.GetComponent<PlayerShipHealth>();
        if (playerHealth != null && playerHealth.IsAlive())
        {
            // For trigger-based damage, we can't get collision force, so use base damage
            float finalDamage = crashDamage;
            
            // Optional: Scale damage based on player speed
            var playerRb = playerCollider.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                float playerSpeed = playerRb.velocity.magnitude;
                if (playerSpeed > 10f)
                {
                    finalDamage *= Mathf.Clamp(playerSpeed / 20f, 1f, 2f);
                }
                Debug.Log($"FuelBargeCollision: Player speed: {playerSpeed:F1}, damage multiplier applied");
            }
            
            playerHealth.TakeDamage(finalDamage);
            
            // Visual effects at player position
            if (crashEffect != null)
            {
                Instantiate(crashEffect, playerCollider.transform.position, playerCollider.transform.rotation);
            }
            
            // Audio effects
            if (audioSource != null && crashSound != null)
            {
                audioSource.PlayOneShot(crashSound);
            }
            
            Debug.Log($"TRIGGER-BASED DAMAGE: Player crashed into fuel barge! Took {finalDamage:F1} damage");
        }
        else if (playerHealth == null)
        {
            Debug.LogError("FuelBargeCollision: Player has no PlayerShipHealth component!");
        }
        else
        {
            Debug.Log("FuelBargeCollision: Player is already dead, no damage applied");
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        Debug.Log($"FuelBargeCollision: OnCollisionExit - '{collision.gameObject.name}' left collision");
    }
    
    void OnTriggerExit(Collider other)
    {
        Debug.Log($"FuelBargeCollision: OnTriggerExit - '{other.gameObject.name}' left trigger");
    }
    
    private void HandlePlayerCrash(Collision collision)
    {
        var playerHealth = collision.gameObject.GetComponent<PlayerShipHealth>();
        if (playerHealth != null && playerHealth.IsAlive())
        {
            // Calculate damage based on collision force (optional)
            float impactForce = collision.relativeVelocity.magnitude;
            float finalDamage = crashDamage;
            
            // Optional: Scale damage based on impact speed
            if (impactForce > 10f)
            {
                finalDamage *= Mathf.Clamp(impactForce / 20f, 1f, 2f);
            }
            
            playerHealth.TakeDamage(finalDamage);
            
            // Visual effects
            if (crashEffect != null)
            {
                Instantiate(crashEffect, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
            }
            
            // Audio effects
            if (audioSource != null && crashSound != null)
            {
                audioSource.PlayOneShot(crashSound);
            }
            
            Debug.Log($"Player crashed into fuel barge! Took {finalDamage:F1} damage (Impact force: {impactForce:F1})");
        }
    }
    
    private void HandleBulletHit(Collision collision)
    {
        // Visual effects
        if (bulletHitEffect != null)
        {
            Instantiate(bulletHitEffect, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
        }
        
        // Audio effects
        if (audioSource != null && bulletHitSound != null)
        {
            audioSource.PlayOneShot(bulletHitSound);
        }
        
        Debug.Log("Bullet hit fuel barge solid structure!");
        
        // Note: Bullet destruction is handled by the bullet's own collision logic
        // You could add barge damage/destruction logic here if desired
    }
    
    // Public methods for external systems
    public void SetCrashDamage(float damage)
    {
        crashDamage = damage;
    }
    
    public float GetCrashDamage()
    {
        return crashDamage;
    }
    
    // Test methods for debugging
    [ContextMenu("Test Crash Damage")]
    private void TestCrashDamage()
    {
        var player = FindObjectOfType<PlayerShipHealth>();
        if (player != null)
        {
            player.TakeDamage(crashDamage);
            Debug.Log($"Test: Applied {crashDamage} crash damage to player");
        }
        else
        {
            Debug.LogWarning("No PlayerShipHealth found for testing");
        }
    }
}
