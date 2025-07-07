using UnityEngine;

/// <summary>
/// Player health system that integrates with FlightData.
/// Handles damage, healing, and death logic while using centralized health data.
/// </summary>
[DisallowMultipleComponent]
public class PlayerShipHealth : MonoBehaviour
{
    [Header("Flight Data Reference")]
    [SerializeField] private FlightData flightData;
    
    [Header("Death Settings")]
    [SerializeField] private bool disableOnDeath = true;
    [SerializeField] private GameObject deathEffect; // Optional explosion effect
    
    // Cached references
    private bool isInitialized = false;
    
    void Start()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        // Find FlightData if not assigned
        if (flightData == null)
        {
            flightData = FindObjectOfType<FlightData>();
            if (flightData == null)
            {
                Debug.LogError("PlayerShipHealth: No FlightData found in scene!", this);
                return;
            }
        }
        
        isInitialized = true;
        Debug.Log("PlayerShipHealth: Initialized and connected to FlightData");
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Check for death condition
        if (!flightData.IsAlive())
        {
            Die();
        }
    }

    /// <summary>
    /// Called by bullets, explosions, etc.
    /// Now uses FlightData for centralized health management.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (!isInitialized || !flightData.IsAlive()) return;
        
        flightData.TakeDamage(amount);
        
        // Death is handled in Update() to avoid multiple calls
    }

    /// <summary>
    /// Called by health packs or repair systems.
    /// Now uses FlightData for centralized health management.
    /// </summary>
    public void RecoverHealth(float amount)
    {
        if (!isInitialized) return;
        
        flightData.RecoverHealth(amount);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isInitialized) return;
        
        if (other.CompareTag("HealthPack"))
        {
            RecoverHealth(flightData.healAmount);
            Destroy(other.gameObject);
            Debug.Log("Health pack collected!");
        }
    }

    private void Die()
    {
        Debug.Log("Player ship destroyed!");
        
        // Spawn death effect if assigned
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }
        
        // Disable ship or trigger game over
        if (disableOnDeath)
        {
            gameObject.SetActive(false);
        }
        
        // You can add more death logic here:
        // - Game over screen
        // - Respawn logic
        // - Score penalties
        // - etc.
    }
    
    // Public methods for external systems
    public bool IsAlive()
    {
        return isInitialized && flightData.IsAlive();
    }
    
    public float GetHealthPercentage()
    {
        return isInitialized ? flightData.GetHealthPercentage() : 0f;
    }
    
    public float GetCurrentHealth()
    {
        return isInitialized ? flightData.currentHealth : 0f;
    }
    
    public float GetMaxHealth()
    {
        return isInitialized ? flightData.maxHealth : 100f;
    }
    
    // Test methods for debugging
    [ContextMenu("Test Damage (20)")]
    private void TestDamage()
    {
        TakeDamage(20f);
    }
    
    [ContextMenu("Test Heal (25)")]
    private void TestHeal()
    {
        RecoverHealth(25f);
    }
    
    [ContextMenu("Test Kill")]
    private void TestKill()
    {
        if (isInitialized)
        {
            flightData.TakeDamage(flightData.currentHealth);
        }
    }
}
