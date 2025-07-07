using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Health bar UI component that displays player health from FlightData.
/// Works similar to the fuel system but for health management.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage; // Optional: for color changes
    
    [Header("Flight Data Reference")]
    [SerializeField] private FlightData flightData;
    
    [Header("Visual Settings")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color damagedColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private float criticalThreshold = 0.25f; // 25% health
    [SerializeField] private float damagedThreshold = 0.6f;   // 60% health
    
    [Header("Performance Settings")]
    [SerializeField] private float updateInterval = 0.1f; // Update every 0.1 seconds
    
    // Cached references and state
    private bool isInitialized = false;
    private float lastUpdateTime = 0f;
    private float lastDisplayedHealth = -1f;
    
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
                Debug.LogError("HealthBarUI: No FlightData found in scene!", this);
                return;
            }
        }
        
        // Find slider if not assigned
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
            if (healthSlider == null)
            {
                Debug.LogError("HealthBarUI: No Slider component found!", this);
                return;
            }
        }
        
        // Get fill image if not assigned
        if (fillImage == null && healthSlider != null)
        {
            fillImage = healthSlider.fillRect?.GetComponent<Image>();
        }
        
        isInitialized = true;
        
        // Initial update
        UpdateHealthDisplay();
        
        Debug.Log("HealthBarUI: Initialized successfully");
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Performance optimization: Only update at specified intervals
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateHealthDisplay();
            lastUpdateTime = Time.time;
        }
    }
    
    private void UpdateHealthDisplay()
    {
        if (flightData == null || healthSlider == null) return;
        
        float currentHealth = flightData.currentHealth;
        float healthPercentage = flightData.GetHealthPercentage();
        
        // Performance optimization: Only update if health changed significantly
        if (Mathf.Abs(currentHealth - lastDisplayedHealth) < 1f) return;
        
        // Update slider value
        healthSlider.value = healthPercentage;
        
        // Update color based on health level
        UpdateHealthColor(healthPercentage);
        
        lastDisplayedHealth = currentHealth;
    }
    
    private void UpdateHealthColor(float healthPercentage)
    {
        if (fillImage == null) return;
        
        Color targetColor;
        
        if (healthPercentage <= criticalThreshold)
        {
            targetColor = criticalColor;
        }
        else if (healthPercentage <= damagedThreshold)
        {
            targetColor = damagedColor;
        }
        else
        {
            targetColor = healthyColor;
        }
        
        fillImage.color = targetColor;
    }
    
    // Public methods for external control
    public void ForceUpdate()
    {
        lastDisplayedHealth = -1f; // Force update on next call
        UpdateHealthDisplay();
    }
    
    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.05f, interval); // Minimum 0.05 seconds
    }
    
    // Test methods for debugging
    [ContextMenu("Test Damage")]
    private void TestDamage()
    {
        if (flightData != null)
        {
            flightData.TakeDamage(20f);
        }
    }
    
    [ContextMenu("Test Heal")]
    private void TestHeal()
    {
        if (flightData != null)
        {
            flightData.RecoverHealth(25f);
        }
    }
}
