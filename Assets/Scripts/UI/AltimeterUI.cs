using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Optimized altimeter UI that efficiently displays aircraft altitude.
/// Follows the same pattern as AirspeedIndicatorUI for consistency.
/// Supports both absolute and ground-relative altitude readings.
/// </summary>
public class AltimeterUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI altitudeText;
    [SerializeField] private Text legacyAltitudeText; // For legacy UI Text component
    
    [Header("Aircraft Reference")]
    [SerializeField] private Transform aircraft;
    
    [Header("Display Settings")]
    [SerializeField] private string altitudeFormat = "F0"; // Format for altitude display (F0 = no decimals)
    [SerializeField] private string altitudeUnit = " ft"; // Unit to display after altitude
    [SerializeField] private float updateInterval = 0.1f; // Update UI every 0.1 seconds (performance optimization)
    [SerializeField] private bool useGroundRelativeAltitude = true; // True = altitude above ground, False = absolute altitude
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayerMask = -1; // What layers count as "ground"
    [SerializeField] private float maxRaycastDistance = 1000f; // Maximum distance to check for ground
    [SerializeField] private float seaLevel = 0f; // Y position considered "sea level"
    
    // Cached references and state
    private bool isInitialized = false;
    private float lastUpdateTime = 0f;
    private float lastDisplayedAltitude = -1f; // Track last altitude to avoid unnecessary updates
    
    void Start()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        // Try to find aircraft if not assigned (look for FlightData component)
        if (aircraft == null)
        {
            FlightData flightData = FindObjectOfType<FlightData>();
            if (flightData != null)
            {
                aircraft = flightData.transform;
            }
            else
            {
                Debug.LogError("AltimeterUI: No aircraft Transform or FlightData found in scene!", this);
                return;
            }
        }
        
        // Try to find text component if not assigned
        if (altitudeText == null && legacyAltitudeText == null)
        {
            altitudeText = GetComponent<TextMeshProUGUI>();
            if (altitudeText == null)
            {
                legacyAltitudeText = GetComponent<Text>();
                if (legacyAltitudeText == null)
                {
                    Debug.LogError("AltimeterUI: No TextMeshProUGUI or Text component found!", this);
                    return;
                }
            }
        }
        
        isInitialized = true;
        
        // Initial display update
        UpdateAltitudeDisplay();
    }

    void Update()
    {
        if (!isInitialized) return;
        
        // Performance optimization: Only update UI at specified intervals
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateAltitudeDisplay();
            lastUpdateTime = Time.time;
        }
    }
    
    private void UpdateAltitudeDisplay()
    {
        if (aircraft == null) return;
        
        float currentAltitude = GetAltitude();
        
        // Performance optimization: Only update text if altitude has changed significantly
        if (Mathf.Abs(currentAltitude - lastDisplayedAltitude) < 1f) return;
        
        // Format the altitude display
        string displayText = currentAltitude.ToString(altitudeFormat) + altitudeUnit;
        
        // Update the appropriate text component
        if (altitudeText != null)
        {
            altitudeText.text = displayText;
        }
        else if (legacyAltitudeText != null)
        {
            legacyAltitudeText.text = displayText;
        }
        
        lastDisplayedAltitude = currentAltitude;
    }
    
    private float GetAltitude()
    {
        if (useGroundRelativeAltitude)
        {
            return GetGroundRelativeAltitude();
        }
        else
        {
            return GetAbsoluteAltitude();
        }
    }
    
    private float GetAbsoluteAltitude()
    {
        // Simple absolute altitude (Y position relative to sea level)
        return aircraft.position.y - seaLevel;
    }
    
    private float GetGroundRelativeAltitude()
    {
        // Cast a ray downward to find ground
        RaycastHit hit;
        Vector3 rayStart = aircraft.position;
        Vector3 rayDirection = Vector3.down;
        
        if (Physics.Raycast(rayStart, rayDirection, out hit, maxRaycastDistance, groundLayerMask))
        {
            // Return altitude above the detected ground
            return aircraft.position.y - hit.point.y;
        }
        else
        {
            // No ground detected, fallback to absolute altitude
            return GetAbsoluteAltitude();
        }
    }
    
    // Public method to force immediate update (if needed)
    public void ForceUpdate()
    {
        lastDisplayedAltitude = -1f; // Force update on next call
        UpdateAltitudeDisplay();
    }
    
    // Public method to change update frequency
    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.05f, interval); // Minimum 0.05 seconds
    }
    
    // Public method to toggle altitude mode
    public void SetGroundRelativeMode(bool useGroundRelative)
    {
        useGroundRelativeAltitude = useGroundRelative;
        ForceUpdate();
    }
    
    // Public method to get current altitude (for other systems)
    public float GetCurrentAltitude()
    {
        return GetAltitude();
    }
    
    // Debug method to visualize raycast in Scene view
    void OnDrawGizmosSelected()
    {
        if (aircraft != null && useGroundRelativeAltitude)
        {
            Gizmos.color = Color.yellow;
            Vector3 start = aircraft.position;
            Vector3 end = start + Vector3.down * maxRaycastDistance;
            Gizmos.DrawLine(start, end);
        }
    }
}
