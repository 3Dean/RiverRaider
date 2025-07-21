using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Machinegun cooldown UI component that displays heat level and cooling rate from MachinegunController.
/// Connects to existing "CooldownBar" slider and provides visual feedback for weapon heat state.
/// </summary>
public class MachinegunCooldownUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider cooldownSlider;
    [SerializeField] private Image fillImage; // Optional: for color changes
    [SerializeField] private Text heatPercentageText; // Optional: for heat percentage display
    [SerializeField] private Text cooldownRateText; // Optional: for cooldown rate display
    
    [Header("Machinegun Controller Reference")]
    [SerializeField] private MachinegunController machinegunController;
    [SerializeField] private WeaponManager weaponManager; // Alternative reference source
    
    [Header("Visual Settings")]
    [SerializeField] private Color coolColor = Color.green;        // 0-60% heat
    [SerializeField] private Color warmingColor = Color.yellow;    // 60-80% heat
    [SerializeField] private Color hotColor = new Color(1f, 0.5f, 0f, 1f);        // 80-95% heat (orange)
    [SerializeField] private Color overheatedColor = Color.red;    // 95-100% heat / overheated
    [SerializeField] private float warmingThreshold = 0.6f;        // 60% heat
    [SerializeField] private float hotThreshold = 0.8f;           // 80% heat
    [SerializeField] private float overheatedThreshold = 0.95f;   // 95% heat
    
    [Header("Overheated Effects")]
    [SerializeField] private bool enableOverheatedPulsing = true;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseIntensity = 0.3f;
    
    [Header("Performance Settings")]
    [SerializeField] private float updateInterval = 0.05f; // Update every 0.05 seconds (20 FPS)
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogging = false;
    [SerializeField] private bool showHeatPercentage = true;
    [SerializeField] private bool showCooldownRate = true;
    
    // Cached references and state
    private bool isInitialized = false;
    private float lastUpdateTime = 0f;
    private float lastDisplayedHeat = -1f;
    private float lastHeatValue = 0f;
    private float cooldownRate = 0f;
    private Color baseColor;
    
    void Start()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        if (enableDebugLogging)
            Debug.Log("MachinegunCooldownUI: Starting initialization...");
        
        // Find cooldown slider by name if not assigned
        if (cooldownSlider == null)
        {
            GameObject cooldownBarObj = GameObject.Find("CooldownBar");
            if (cooldownBarObj != null)
            {
                cooldownSlider = cooldownBarObj.GetComponent<Slider>();
                if (cooldownSlider != null)
                {
                    if (enableDebugLogging)
                        Debug.Log("MachinegunCooldownUI: Found CooldownBar slider by name");
                }
                else
                {
                    Debug.LogError("MachinegunCooldownUI: CooldownBar GameObject found but no Slider component!", this);
                    return;
                }
            }
            else
            {
                // Fallback: try to find any slider on this GameObject
                cooldownSlider = GetComponent<Slider>();
                if (cooldownSlider == null)
                {
                    Debug.LogError("MachinegunCooldownUI: No CooldownBar found and no Slider component on this GameObject!", this);
                    return;
                }
                else
                {
                    if (enableDebugLogging)
                        Debug.Log("MachinegunCooldownUI: Using Slider component on this GameObject");
                }
            }
        }
        else
        {
            if (enableDebugLogging)
                Debug.Log("MachinegunCooldownUI: Using assigned cooldown slider");
        }
        
        // Find MachinegunController if not assigned
        if (machinegunController == null)
        {
            // Try to find via WeaponManager first
            if (weaponManager == null)
                weaponManager = FindObjectOfType<WeaponManager>();
            
            if (weaponManager != null)
            {
                // WeaponManager doesn't expose machinegunController publicly, so find it directly
                machinegunController = FindObjectOfType<MachinegunController>();
                if (machinegunController != null)
                {
                    if (enableDebugLogging)
                        Debug.Log($"MachinegunCooldownUI: Found MachinegunController on '{machinegunController.name}'");
                }
            }
            else
            {
                // Direct search for MachinegunController
                machinegunController = FindObjectOfType<MachinegunController>();
                if (machinegunController == null)
                {
                    Debug.LogError("MachinegunCooldownUI: No MachinegunController found in scene!", this);
                    return;
                }
                else
                {
                    if (enableDebugLogging)
                        Debug.Log($"MachinegunCooldownUI: Found MachinegunController on '{machinegunController.name}'");
                }
            }
        }
        else
        {
            if (enableDebugLogging)
                Debug.Log($"MachinegunCooldownUI: Using assigned MachinegunController: '{machinegunController.name}'");
        }
        
        // Configure slider range (0 to 1 for 0% to 100% heat)
        cooldownSlider.minValue = 0f;
        cooldownSlider.maxValue = 1f;
        cooldownSlider.interactable = false; // Make it display-only
        
        // Get fill image if not assigned
        if (fillImage == null && cooldownSlider != null)
        {
            fillImage = cooldownSlider.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                baseColor = fillImage.color;
                if (enableDebugLogging)
                    Debug.Log("MachinegunCooldownUI: Found fill image");
            }
            else
            {
                Debug.LogWarning("MachinegunCooldownUI: No fill image found - colors won't change");
            }
        }
        
        // Get text components if not assigned but we want to show them
        if (heatPercentageText == null && showHeatPercentage)
        {
            Text[] textComponents = GetComponentsInChildren<Text>();
            foreach (Text text in textComponents)
            {
                if (text.name.ToLower().Contains("heat") || text.name.ToLower().Contains("percentage"))
                {
                    heatPercentageText = text;
                    if (enableDebugLogging)
                        Debug.Log($"MachinegunCooldownUI: Found heat percentage text component: {text.name}");
                    break;
                }
            }
        }
        
        if (cooldownRateText == null && showCooldownRate)
        {
            Text[] textComponents = GetComponentsInChildren<Text>();
            foreach (Text text in textComponents)
            {
                if (text.name.ToLower().Contains("cooldown") || text.name.ToLower().Contains("rate"))
                {
                    cooldownRateText = text;
                    if (enableDebugLogging)
                        Debug.Log($"MachinegunCooldownUI: Found cooldown rate text component: {text.name}");
                    break;
                }
            }
        }
        
        // Log current heat values
        if (machinegunController != null && enableDebugLogging)
        {
            Debug.Log($"MachinegunCooldownUI: Current heat level: {machinegunController.HeatLevel:P1}, Overheated: {machinegunController.IsOverheated}");
        }
        
        isInitialized = true;
        
        // Initial update
        UpdateCooldownDisplay();
        
        if (enableDebugLogging)
            Debug.Log("MachinegunCooldownUI: Initialized successfully");
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Performance optimization: Only update at specified intervals
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateCooldownDisplay();
            lastUpdateTime = Time.time;
        }
    }
    
    private void UpdateCooldownDisplay()
    {
        if (machinegunController == null || cooldownSlider == null) return;
        
        float currentHeatLevel = machinegunController.HeatLevel; // 0.0 to 1.0
        float currentHeatPercentage = currentHeatLevel * 100f; // 0 to 100
        bool isOverheated = machinegunController.IsOverheated;
        bool isFiring = machinegunController.IsFiring;
        
        // Calculate cooldown rate (heat change per second)
        float heatDelta = currentHeatLevel - lastHeatValue;
        cooldownRate = -heatDelta / updateInterval; // Negative because we want cooling rate (positive value)
        lastHeatValue = currentHeatLevel;
        
        // Performance optimization: Only update if heat changed significantly
        if (Mathf.Abs(currentHeatLevel - lastDisplayedHeat) < 0.01f && !isOverheated) return;
        
        // Debug logging for heat changes
        if (enableDebugLogging)
        {
            Debug.Log($"MachinegunCooldownUI: Heat: {currentHeatPercentage:F1}%, " +
                     $"Overheated: {isOverheated}, Firing: {isFiring}, " +
                     $"Cooldown Rate: {cooldownRate:F2}/s, Slider: {cooldownSlider.value:F2} → {currentHeatLevel:F2}");
        }
        
        // Update slider value
        cooldownSlider.value = currentHeatLevel;
        
        // Update color based on heat level and overheated state
        UpdateHeatColor(currentHeatLevel, isOverheated);
        
        // Update text displays
        UpdateHeatPercentageText(currentHeatPercentage, isOverheated, isFiring);
        UpdateCooldownRateText(cooldownRate, isFiring);
        
        lastDisplayedHeat = currentHeatLevel;
    }
    
    private void UpdateHeatColor(float heatLevel, bool isOverheated)
    {
        if (fillImage == null) return;
        
        Color targetColor;
        
        if (isOverheated || heatLevel >= overheatedThreshold)
        {
            targetColor = overheatedColor;
            
            // Add pulsing effect when overheated
            if (enableOverheatedPulsing)
            {
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
                targetColor = Color.Lerp(targetColor, Color.white, Mathf.Abs(pulse));
            }
        }
        else if (heatLevel >= hotThreshold)
        {
            targetColor = hotColor;
        }
        else if (heatLevel >= warmingThreshold)
        {
            targetColor = warmingColor;
        }
        else
        {
            targetColor = coolColor;
        }
        
        fillImage.color = targetColor;
    }
    
    private void UpdateHeatPercentageText(float heatPercentage, bool isOverheated, bool isFiring)
    {
        if (heatPercentageText == null || !showHeatPercentage) return;
        
        string statusText = "";
        if (isOverheated)
            statusText = " OVERHEAT!";
        else if (isFiring)
            statusText = " FIRING";
        
        heatPercentageText.text = $"{heatPercentage:F0}%{statusText}";
        
        // Change text color based on state
        if (isOverheated)
            heatPercentageText.color = overheatedColor;
        else if (isFiring)
            heatPercentageText.color = Color.white;
        else
            heatPercentageText.color = Color.gray;
    }
    
    private void UpdateCooldownRateText(float rate, bool isFiring)
    {
        if (cooldownRateText == null || !showCooldownRate) return;
        
        if (isFiring)
        {
            cooldownRateText.text = "HEATING";
            cooldownRateText.color = Color.red;
        }
        else if (rate > 0.01f)
        {
            cooldownRateText.text = $"COOLING {rate:F1}/s";
            cooldownRateText.color = Color.cyan;
        }
        else
        {
            cooldownRateText.text = "STABLE";
            cooldownRateText.color = Color.green;
        }
    }
    
    // Public methods for external control
    public void ForceUpdate()
    {
        lastDisplayedHeat = -1f; // Force update on next call
        UpdateCooldownDisplay();
    }
    
    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.01f, interval); // Minimum 0.01 seconds
    }
    
    public void SetDebugLogging(bool enabled)
    {
        enableDebugLogging = enabled;
    }
    
    public void SetShowHeatPercentage(bool show)
    {
        showHeatPercentage = show;
        if (heatPercentageText != null)
        {
            heatPercentageText.gameObject.SetActive(show);
        }
    }
    
    public void SetShowCooldownRate(bool show)
    {
        showCooldownRate = show;
        if (cooldownRateText != null)
        {
            cooldownRateText.gameObject.SetActive(show);
        }
    }
    
    // Getter methods for current state
    public float GetCurrentHeatLevel()
    {
        return machinegunController != null ? machinegunController.HeatLevel : 0f;
    }
    
    public float GetCurrentHeatPercentage()
    {
        return GetCurrentHeatLevel() * 100f;
    }
    
    public bool IsOverheated()
    {
        return machinegunController != null ? machinegunController.IsOverheated : false;
    }
    
    public bool IsFiring()
    {
        return machinegunController != null ? machinegunController.IsFiring : false;
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
        Debug.Log($"MachinegunCooldownUI: Force update - Current heat: {GetCurrentHeatPercentage():F1}%, Overheated: {IsOverheated()}");
    }
    
    [ContextMenu("Toggle Debug Logging")]
    private void ToggleDebugLogging()
    {
        enableDebugLogging = !enableDebugLogging;
        Debug.Log($"MachinegunCooldownUI: Debug logging {(enableDebugLogging ? "enabled" : "disabled")}");
    }
    
    [ContextMenu("Toggle Heat Percentage Text")]
    private void ToggleHeatPercentageText()
    {
        SetShowHeatPercentage(!showHeatPercentage);
        Debug.Log($"MachinegunCooldownUI: Heat percentage text {(showHeatPercentage ? "enabled" : "disabled")}");
    }
    
    [ContextMenu("Toggle Cooldown Rate Text")]
    private void ToggleCooldownRateText()
    {
        SetShowCooldownRate(!showCooldownRate);
        Debug.Log($"MachinegunCooldownUI: Cooldown rate text {(showCooldownRate ? "enabled" : "disabled")}");
    }
}
