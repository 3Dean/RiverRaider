using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player health system that integrates with FlightData.
/// Handles damage, healing, death logic, and UI updates using centralized health data.
/// Follows the same pattern as PlayerShipFuel for consistency.
/// </summary>
[DisallowMultipleComponent]
public class PlayerShipHealth : MonoBehaviour
{
    [Header("Flight Data Reference")]
    [SerializeField] private FlightData flightData;
    
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    
    [Header("Death Settings")]
    [SerializeField] private bool disableOnDeath = true;
    [SerializeField] private GameObject deathEffect; // Optional explosion effect
    
    [Header("Visual Settings")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color damagedColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private float criticalThreshold = 0.25f; // 25% health
    [SerializeField] private float damagedThreshold = 0.6f;   // 60% health
    
    // Cached references and state
    private bool isInitialized = false;
    private Image healthFillImage;
    
    // Health states for visual feedback
    public enum HealthState
    {
        Healthy,
        Damaged,
        Critical,
        Dead
    }
    
    private HealthState currentHealthState = HealthState.Healthy;
    
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
        
        // Find health slider if not assigned
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
            if (healthSlider == null)
            {
                Debug.LogError("PlayerShipHealth: No Slider component found!", this);
                return;
            }
        }
        
        // Get fill image for color changes
        if (healthSlider != null && healthSlider.fillRect != null)
        {
            healthFillImage = healthSlider.fillRect.GetComponent<Image>();
        }
        
        isInitialized = true;
        
        // Initial UI update
        UpdateHealthUI();
        
        Debug.Log("PlayerShipHealth: Initialized with UI integration following PlayerShipFuel pattern");
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Update health state and UI
        UpdateHealthState();
        UpdateHealthUI();
        
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
    
    private void UpdateHealthState()
    {
        float healthPercentage = flightData.GetHealthPercentage();
        
        if (healthPercentage <= 0f)
        {
            currentHealthState = HealthState.Dead;
        }
        else if (healthPercentage <= criticalThreshold)
        {
            currentHealthState = HealthState.Critical;
        }
        else if (healthPercentage <= damagedThreshold)
        {
            currentHealthState = HealthState.Damaged;
        }
        else
        {
            currentHealthState = HealthState.Healthy;
        }
    }
    
    void UpdateHealthUI()
    {
        if (healthSlider != null && isInitialized)
        {
            healthSlider.value = flightData.GetHealthPercentage();
            
            // Update color based on health state
            if (healthFillImage != null)
            {
                Color targetColor = healthyColor;
                
                switch (currentHealthState)
                {
                    case HealthState.Critical:
                        targetColor = criticalColor;
                        break;
                    case HealthState.Damaged:
                        targetColor = damagedColor;
                        break;
                    case HealthState.Healthy:
                        targetColor = healthyColor;
                        break;
                    case HealthState.Dead:
                        targetColor = criticalColor;
                        break;
                }
                
                healthFillImage.color = targetColor;
            }
        }
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
