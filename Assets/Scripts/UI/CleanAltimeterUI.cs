using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Clean Altimeter UI - Works with UnifiedFlightController
/// Follows Unity best practices with proper altitude calculation
/// </summary>
public class CleanAltimeterUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI altitudeText;
    [SerializeField] private Text legacyAltitudeText; // Fallback for legacy UI
    
    [Header("Aircraft Reference")]
    [SerializeField] private Transform aircraft;
    
    [Header("Display Settings")]
    [SerializeField] private string altitudeFormat = "F0"; // No decimals
    [SerializeField] private string altitudeUnit = " ft";
    [SerializeField] private float updateInterval = 0.1f; // Update frequency
    [SerializeField] private bool useGroundRelativeAltitude = true;
    
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayerMask = -1;
    [SerializeField] private float maxRaycastDistance = 1000f;
    [SerializeField] private float seaLevel = 0f; // Y position of sea level
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = false;
    
    // State
    private bool isInitialized = false;
    private float lastUpdateTime = 0f;
    private float lastDisplayedAltitude = -999f;
    
    void Start()
    {
        InitializeAltimeter();
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Update at specified intervals for performance
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateAltitudeDisplay();
            lastUpdateTime = Time.time;
        }
    }
    
    private void InitializeAltimeter()
    {
        // Find aircraft if not assigned
        if (aircraft == null)
        {
            // Look for UnifiedFlightController first
            UnifiedFlightController flightController = FindObjectOfType<UnifiedFlightController>();
            if (flightController != null)
            {
                aircraft = flightController.transform;
            }
            else
            {
                // Fallback to FlightData
                FlightData flightData = FindObjectOfType<FlightData>();
                if (flightData != null)
                {
                    aircraft = flightData.transform;
                }
            }
        }
        
        if (aircraft == null)
        {
            Debug.LogError("CleanAltimeterUI: No aircraft found! Looking for UnifiedFlightController or FlightData.", this);
            return;
        }
        
        // Find text component if not assigned
        if (altitudeText == null && legacyAltitudeText == null)
        {
            altitudeText = GetComponent<TextMeshProUGUI>();
            if (altitudeText == null)
            {
                legacyAltitudeText = GetComponent<Text>();
            }
        }
        
        if (altitudeText == null && legacyAltitudeText == null)
        {
            Debug.LogError("CleanAltimeterUI: No text component found! Add TextMeshProUGUI or Text component.", this);
            return;
        }
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log($"CleanAltimeterUI: Initialized with aircraft {aircraft.name}");
        }
        
        // Initial display
        UpdateAltitudeDisplay();
    }
    
    private void UpdateAltitudeDisplay()
    {
        if (aircraft == null) return;
        
        float currentAltitude = CalculateAltitude();
        
        // Only update if altitude changed significantly (performance optimization)
        if (Mathf.Abs(currentAltitude - lastDisplayedAltitude) < 1f) return;
        
        // Format and display
        string displayText = currentAltitude.ToString(altitudeFormat) + altitudeUnit;
        
        if (altitudeText != null)
        {
            altitudeText.text = displayText;
        }
        else if (legacyAltitudeText != null)
        {
            legacyAltitudeText.text = displayText;
        }
        
        lastDisplayedAltitude = currentAltitude;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Altitude updated: {currentAltitude:F1} ft (Aircraft Y: {aircraft.position.y:F1})");
        }
    }
    
    private float CalculateAltitude()
    {
        if (useGroundRelativeAltitude)
        {
            return CalculateGroundRelativeAltitude();
        }
        else
        {
            return CalculateAbsoluteAltitude();
        }
    }
    
    private float CalculateAbsoluteAltitude()
    {
        // Simple altitude above sea level
        return aircraft.position.y - seaLevel;
    }
    
    private float CalculateGroundRelativeAltitude()
    {
        // Cast ray downward to find ground
        Vector3 rayStart = aircraft.position;
        Vector3 rayDirection = Vector3.down;
        
        if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, maxRaycastDistance, groundLayerMask))
        {
            float groundAltitude = aircraft.position.y - hit.point.y;
            
            if (enableDebugLogging)
            {
                Debug.Log($"Ground hit at Y: {hit.point.y:F1}, Aircraft Y: {aircraft.position.y:F1}, Altitude: {groundAltitude:F1}");
            }
            
            return groundAltitude;
        }
        else
        {
            // No ground detected, fallback to absolute altitude
            if (enableDebugLogging)
            {
                Debug.LogWarning($"No ground detected within {maxRaycastDistance}m, using absolute altitude");
            }
            
            return CalculateAbsoluteAltitude();
        }
    }
    
    // Public methods for external control
    public void ForceUpdate()
    {
        lastDisplayedAltitude = -999f; // Force update
        UpdateAltitudeDisplay();
    }
    
    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.05f, interval);
    }
    
    public void SetGroundRelativeMode(bool useGroundRelative)
    {
        useGroundRelativeAltitude = useGroundRelative;
        ForceUpdate();
    }
    
    public float GetCurrentAltitude()
    {
        return isInitialized ? CalculateAltitude() : 0f;
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (aircraft != null && useGroundRelativeAltitude)
        {
            Gizmos.color = Color.yellow;
            Vector3 start = aircraft.position;
            Vector3 end = start + Vector3.down * maxRaycastDistance;
            Gizmos.DrawLine(start, end);
            
            // Show raycast hit if any
            if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, maxRaycastDistance, groundLayerMask))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hit.point, 2f);
            }
        }
    }
}
