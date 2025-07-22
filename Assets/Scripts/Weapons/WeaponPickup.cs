using UnityEngine;

/// <summary>
/// Simple weapon pickup that switches missile types when collected
/// Perfect for future weapon pickup system integration
/// </summary>
public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Pickup Settings")]
    [SerializeField] private string missileTypeName = "ARM-60";
    [SerializeField] private bool resupplyOnPickup = true;
    [SerializeField] private bool destroyOnPickup = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private AudioClip pickupSound;
    
    [Header("Auto-Detection")]
    [SerializeField] private string playerTag = "Player";
    
    private FlightData flightData;
    private AudioSource audioSource;
    
    void Start()
    {
        // Find FlightData in scene
        flightData = FindObjectOfType<FlightData>();
        if (flightData == null)
        {
            Debug.LogError("WeaponPickup: No FlightData found in scene!", this);
        }
        
        // Get or add AudioSource for pickup sound
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if player collided with pickup
        if (other.CompareTag(playerTag))
        {
            CollectWeapon();
        }
    }
    
    /// <summary>
    /// Collect the weapon pickup - switches missile type and optionally resupplies
    /// </summary>
    public void CollectWeapon()
    {
        if (flightData == null)
        {
            Debug.LogWarning("WeaponPickup: No FlightData available for weapon collection!", this);
            return;
        }
        
        // Switch to new missile type
        flightData.SetMissileType(missileTypeName);
        Debug.Log($"Weapon Pickup: Switched to {missileTypeName}");
        
        // Resupply missiles if enabled
        if (resupplyOnPickup)
        {
            flightData.ResupplyMissiles();
            Debug.Log($"Weapon Pickup: Resupplied {missileTypeName} missiles");
        }
        
        // Play pickup effects
        PlayPickupEffects();
        
        // Destroy pickup if enabled
        if (destroyOnPickup)
        {
            Destroy(gameObject, 0.1f); // Small delay to allow sound to play
        }
    }
    
    private void PlayPickupEffects()
    {
        // Play pickup sound
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        
        // Spawn pickup effect
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, transform.rotation);
            Destroy(effect, 3f); // Clean up effect after 3 seconds
        }
    }
    
    /// <summary>
    /// Set the missile type this pickup will provide
    /// </summary>
    public void SetMissileType(string newMissileType)
    {
        missileTypeName = newMissileType;
    }
    
    /// <summary>
    /// Configure pickup behavior
    /// </summary>
    public void ConfigurePickup(string missileType, bool resupply = true, bool destroyAfter = true)
    {
        missileTypeName = missileType;
        resupplyOnPickup = resupply;
        destroyOnPickup = destroyAfter;
    }
    
    // Properties for external access
    public string MissileTypeName => missileTypeName;
    public bool ResupplyOnPickup => resupplyOnPickup;
    public bool DestroyOnPickup => destroyOnPickup;
    
    void OnDrawGizmosSelected()
    {
        // Draw pickup detection area
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = Color.yellow;
            if (col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(transform.position, sphere.radius * transform.localScale.x);
            }
            else if (col is BoxCollider box)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
                Gizmos.DrawWireCube(Vector3.zero, box.size);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
        
        // Draw missile type label
        Gizmos.color = Color.white;
        Vector3 labelPos = transform.position + Vector3.up * 2f;
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(labelPos, $"Weapon: {missileTypeName}");
        #endif
    }
}
