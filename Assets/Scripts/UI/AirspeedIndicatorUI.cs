using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Optimized airspeed indicator UI that efficiently displays flight speed.
/// Caches references and minimizes string allocations.
/// </summary>
public class AirspeedIndicatorUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Text legacySpeedText; // For legacy UI Text component
    
    [Header("Flight Data Reference")]
    [SerializeField] private FlightData flightData;
    
    [Header("Display Settings")]
    [SerializeField] private string speedFormat = "F0"; // Format for speed display (F0 = no decimals)
    [SerializeField] private string speedUnit = " mph"; // Unit to display after speed
    [SerializeField] private float updateInterval = 0.1f; // Update UI every 0.1 seconds (performance optimization)
    
    // Cached references and state
    private bool isInitialized = false;
    private float lastUpdateTime = 0f;
    private float lastDisplayedSpeed = -1f; // Track last speed to avoid unnecessary updates
    
    void Start()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        // Try to find FlightData if not assigned (only once at start)
        if (flightData == null)
        {
            flightData = FindObjectOfType<FlightData>();
            if (flightData == null)
            {
                Debug.LogError("AirspeedIndicatorUI: No FlightData found in scene!", this);
                return;
            }
        }
        
        // Try to find text component if not assigned
        if (speedText == null && legacySpeedText == null)
        {
            speedText = GetComponent<TextMeshProUGUI>();
            if (speedText == null)
            {
                legacySpeedText = GetComponent<Text>();
                if (legacySpeedText == null)
                {
                    Debug.LogError("AirspeedIndicatorUI: No TextMeshProUGUI or Text component found!", this);
                    return;
                }
            }
        }
        
        isInitialized = true;
        
        // Initial display update
        UpdateSpeedDisplay();
    }

    void Update()
    {
        if (!isInitialized) return;
        
        // Performance optimization: Only update UI at specified intervals
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateSpeedDisplay();
            lastUpdateTime = Time.time;
        }
    }
    
    private void UpdateSpeedDisplay()
    {
        if (flightData == null) return;
        
        float currentSpeed = flightData.airspeed;
        
        // Performance optimization: Only update text if speed has changed significantly
        if (Mathf.Abs(currentSpeed - lastDisplayedSpeed) < 0.5f) return;
        
        // Get the current airspeed and format it
        string displayText = currentSpeed.ToString(speedFormat) + speedUnit;
        
        // Update the appropriate text component
        if (speedText != null)
        {
            speedText.text = displayText;
        }
        else if (legacySpeedText != null)
        {
            legacySpeedText.text = displayText;
        }
        
        lastDisplayedSpeed = currentSpeed;
    }
    
    // Public method to force immediate update (if needed)
    public void ForceUpdate()
    {
        lastDisplayedSpeed = -1f; // Force update on next call
        UpdateSpeedDisplay();
    }
    
    // Public method to change update frequency
    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.05f, interval); // Minimum 0.05 seconds
    }
}
