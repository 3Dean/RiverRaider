using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Thrust bar UI component that displays throttle position from UnifiedFlightController.
/// Shows the discrete throttle setting (0-100%) controlled by W/S keys.
/// </summary>
public class ThrustBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider thrustSlider;
    [SerializeField] private Image fillImage; // Optional: for color changes
    [SerializeField] private Text percentageText; // Optional: for text display
    
    [Header("Flight Controller Reference")]
    [SerializeField] private UnifiedFlightController flightController;
    
    [Header("Visual Settings")]
    [SerializeField] private Color idleColor = Color.red;        // 0% throttle
    [SerializeField] private Color partialColor = Color.yellow;  // 1-99% throttle
    [SerializeField] private Color fullColor = Color.green;      // 100% throttle
    [SerializeField] private float partialThreshold = 0.99f;     // 99% threshold for full color
    
    [Header("Performance Settings")]
    [SerializeField] private float updateInterval = 0.05f; // Update every 0.05 seconds (20 FPS)
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogging = false;
    [SerializeField] private bool showPercentageText = true;
    
    // Cached references and state
    private bool isInitialized = false;
    private float lastUpdateTime = 0f;
    private float lastDisplayedThrottle = -1f;
    
    void Start()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        if (enableDebugLogging)
            Debug.Log("ThrustBarUI: Starting initialization...");
        
        // Find UnifiedFlightController if not assigned
        if (flightController == null)
        {
            flightController = FindObjectOfType<UnifiedFlightController>();
            if (flightController == null)
            {
                Debug.LogError("ThrustBarUI: No UnifiedFlightController found in scene!", this);
                return;
            }
            else
            {
                if (enableDebugLogging)
                    Debug.Log($"ThrustBarUI: Found UnifiedFlightController on '{flightController.name}'");
            }
        }
        else
        {
            if (enableDebugLogging)
                Debug.Log($"ThrustBarUI: Using assigned UnifiedFlightController: '{flightController.name}'");
        }
        
        // Find slider if not assigned
        if (thrustSlider == null)
        {
            thrustSlider = GetComponent<Slider>();
            if (thrustSlider == null)
            {
                Debug.LogError("ThrustBarUI: No Slider component found!", this);
                return;
            }
            else
            {
                if (enableDebugLogging)
                    Debug.Log("ThrustBarUI: Found Slider component");
            }
        }
        else
        {
            if (enableDebugLogging)
                Debug.Log("ThrustBarUI: Using assigned Slider");
        }
        
        // Configure slider range (0 to 1 for 0% to 100%)
        thrustSlider.minValue = 0f;
        thrustSlider.maxValue = 1f;
        thrustSlider.interactable = false; // Make it display-only
        
        // Get fill image if not assigned
        if (fillImage == null && thrustSlider != null)
        {
            fillImage = thrustSlider.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                if (enableDebugLogging)
                    Debug.Log("ThrustBarUI: Found fill image");
            }
            else
            {
                Debug.LogWarning("ThrustBarUI: No fill image found - colors won't change");
            }
        }
        
        // Get percentage text if not assigned but we want to show it
        if (percentageText == null && showPercentageText)
        {
            percentageText = GetComponentInChildren<Text>();
            if (percentageText != null)
            {
                if (enableDebugLogging)
                    Debug.Log("ThrustBarUI: Found percentage text component");
            }
            else
            {
                Debug.LogWarning("ThrustBarUI: No Text component found - percentage won't be displayed");
            }
        }
        
        // Log current throttle values
        if (flightController != null && enableDebugLogging)
        {
            Debug.Log($"ThrustBarUI: Current throttle: {flightController.GetThrottlePercentage():F0}%");
        }
        
        isInitialized = true;
        
        // Initial update
        UpdateThrustDisplay();
        
        if (enableDebugLogging)
            Debug.Log("ThrustBarUI: Initialized successfully");
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Performance optimization: Only update at specified intervals
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateThrustDisplay();
            lastUpdateTime = Time.time;
        }
    }
    
    private void UpdateThrustDisplay()
    {
        if (flightController == null || thrustSlider == null) return;
        
        float currentThrottlePosition = flightController.GetThrottlePosition(); // 0.0 to 1.0
        float currentThrottlePercentage = flightController.GetThrottlePercentage(); // 0 to 100
        
        // Performance optimization: Only update if throttle changed significantly
        if (Mathf.Abs(currentThrottlePosition - lastDisplayedThrottle) < 0.01f) return;
        
        // Debug logging for throttle changes
        if (enableDebugLogging)
        {
            Debug.Log($"ThrustBarUI: Updating display - Throttle: {currentThrottlePercentage:F0}%, " +
                     $"Position: {currentThrottlePosition:F2}, Slider: {thrustSlider.value:F2} → {currentThrottlePosition:F2}");
        }
        
        // Update slider value
        thrustSlider.value = currentThrottlePosition;
        
        // Update color based on throttle level
        UpdateThrustColor(currentThrottlePosition);
        
        // Update percentage text if available
        UpdatePercentageText(currentThrottlePercentage);
        
        lastDisplayedThrottle = currentThrottlePosition;
    }
    
    private void UpdateThrustColor(float throttlePosition)
    {
        if (fillImage == null) return;
        
        Color targetColor;
        
        if (throttlePosition <= 0.01f) // Essentially idle (0-1%)
        {
            targetColor = idleColor;
        }
        else if (throttlePosition >= partialThreshold) // Full throttle (99-100%)
        {
            targetColor = fullColor;
        }
        else // Partial throttle (1-99%)
        {
            targetColor = partialColor;
        }
        
        fillImage.color = targetColor;
    }
    
    private void UpdatePercentageText(float throttlePercentage)
    {
        if (percentageText == null || !showPercentageText) return;
        
        percentageText.text = $"{throttlePercentage:F0}%";
    }
    
    // Public methods for external control
    public void ForceUpdate()
    {
        lastDisplayedThrottle = -1f; // Force update on next call
        UpdateThrustDisplay();
    }
    
    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.01f, interval); // Minimum 0.01 seconds
    }
    
    public void SetDebugLogging(bool enabled)
    {
        enableDebugLogging = enabled;
    }
    
    public void SetShowPercentageText(bool show)
    {
        showPercentageText = show;
        if (percentageText != null)
        {
            percentageText.gameObject.SetActive(show);
        }
    }
    
    // Getter methods for current state
    public float GetCurrentThrottlePosition()
    {
        return flightController != null ? flightController.GetThrottlePosition() : 0f;
    }
    
    public float GetCurrentThrottlePercentage()
    {
        return flightController != null ? flightController.GetThrottlePercentage() : 0f;
    }
    
    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    // Test methods for debugging
    [ContextMenu("Test Force Update")]
    private void TestForceUpdate()
    {
        ForceUpdate();
        Debug.Log($"ThrustBarUI: Force update - Current throttle: {GetCurrentThrottlePercentage():F0}%");
    }
    
    [ContextMenu("Toggle Debug Logging")]
    private void ToggleDebugLogging()
    {
        enableDebugLogging = !enableDebugLogging;
        Debug.Log($"ThrustBarUI: Debug logging {(enableDebugLogging ? "enabled" : "disabled")}");
    }
    
    [ContextMenu("Toggle Percentage Text")]
    private void TogglePercentageText()
    {
        SetShowPercentageText(!showPercentageText);
        Debug.Log($"ThrustBarUI: Percentage text {(showPercentageText ? "enabled" : "disabled")}");
    }
}
