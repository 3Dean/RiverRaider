using UnityEngine;

/// <summary>
/// Test controller specifically for altitude tape tick marks.
/// Provides manual testing capabilities and simulated altitude changes.
/// </summary>
public class AltitudeTapeTestController : MonoBehaviour
{
    [Header("Testing Configuration")]
    [SerializeField] private bool enableTesting = false;
    [SerializeField] private float testAltitudeMin = 0f;
    [SerializeField] private float testAltitudeMax = 500f;
    [SerializeField] private float altitudeChangeSpeed = 20f; // feet per second
    
    [Header("Manual Testing")]
    [SerializeField] private float manualAltitudeIncrement = 25f;
    
    [Header("References")]
    [SerializeField] private AltimeterUI altimeterUI;
    [SerializeField] private HUDTapeController altitudeTapeController;
    [SerializeField] private Transform aircraft;
    
    // Internal state
    private float currentTestAltitude = 100f;
    private bool isIncreasing = true;
    private Vector3 originalAircraftPosition;
    private bool testingActive = false;
    
    void Start()
    {
        InitializeComponents();
        
        if (aircraft != null)
        {
            originalAircraftPosition = aircraft.position;
        }
        
        if (enableTesting)
        {
            Debug.Log("AltitudeTapeTestController: Testing enabled - altitude will cycle automatically");
            testingActive = true;
        }
    }
    
    private void InitializeComponents()
    {
        // Find AltimeterUI if not assigned
        if (altimeterUI == null)
        {
            altimeterUI = FindObjectOfType<AltimeterUI>();
            if (altimeterUI == null)
            {
                Debug.LogError("AltitudeTapeTestController: No AltimeterUI found in scene!", this);
            }
        }
        
        // Find HUDTapeController configured for altitude
        if (altitudeTapeController == null)
        {
            HUDTapeController[] controllers = FindObjectsOfType<HUDTapeController>();
            foreach (var controller in controllers)
            {
                // We need to check if it's configured for altitude
                // Since TapeType is private, we'll find the one that has AltimeterUI reference
                if (controller.name.ToLower().Contains("altitude") || 
                    controller.transform.parent.name.ToLower().Contains("altitude"))
                {
                    altitudeTapeController = controller;
                    break;
                }
            }
            
            if (altitudeTapeController == null)
            {
                Debug.LogWarning("AltitudeTapeTestController: No altitude HUDTapeController found. Make sure to set up altitude tape first.");
            }
        }
        
        // Find aircraft if not assigned
        if (aircraft == null)
        {
            FlightData flightData = FindObjectOfType<FlightData>();
            if (flightData != null)
            {
                aircraft = flightData.transform;
            }
            else
            {
                Debug.LogError("AltitudeTapeTestController: No aircraft Transform or FlightData found!", this);
            }
        }
    }
    
    void Update()
    {
        if (!testingActive || !enableTesting || aircraft == null) return;
        
        // Cycle altitude up and down
        if (isIncreasing)
        {
            currentTestAltitude += altitudeChangeSpeed * Time.deltaTime;
            if (currentTestAltitude >= testAltitudeMax)
            {
                currentTestAltitude = testAltitudeMax;
                isIncreasing = false;
            }
        }
        else
        {
            currentTestAltitude -= altitudeChangeSpeed * Time.deltaTime;
            if (currentTestAltitude <= testAltitudeMin)
            {
                currentTestAltitude = testAltitudeMin;
                isIncreasing = true;
            }
        }
        
        // Apply test altitude to aircraft
        Vector3 newPosition = originalAircraftPosition;
        newPosition.y = originalAircraftPosition.y + currentTestAltitude;
        aircraft.position = newPosition;
    }
    
    // Manual testing methods (can be called from Inspector context menu)
    [ContextMenu("Test Altitude Increase")]
    public void TestAltitudeIncrease()
    {
        if (aircraft == null) return;
        
        Vector3 newPosition = aircraft.position;
        newPosition.y += manualAltitudeIncrement;
        aircraft.position = newPosition;
        
        Debug.Log($"AltitudeTapeTestController: Increased altitude by {manualAltitudeIncrement} ft. New altitude: {GetCurrentAltitude():F1} ft");
        
        ForceUpdateSystems();
    }
    
    [ContextMenu("Test Altitude Decrease")]
    public void TestAltitudeDecrease()
    {
        if (aircraft == null) return;
        
        Vector3 newPosition = aircraft.position;
        newPosition.y -= manualAltitudeIncrement;
        aircraft.position = newPosition;
        
        Debug.Log($"AltitudeTapeTestController: Decreased altitude by {manualAltitudeIncrement} ft. New altitude: {GetCurrentAltitude():F1} ft");
        
        ForceUpdateSystems();
    }
    
    [ContextMenu("Set Test Altitude to 100 ft")]
    public void SetTestAltitude100()
    {
        SetTestAltitude(100f);
    }
    
    [ContextMenu("Set Test Altitude to 250 ft")]
    public void SetTestAltitude250()
    {
        SetTestAltitude(250f);
    }
    
    [ContextMenu("Set Test Altitude to 500 ft")]
    public void SetTestAltitude500()
    {
        SetTestAltitude(500f);
    }
    
    public void SetTestAltitude(float altitude)
    {
        if (aircraft == null) return;
        
        Vector3 newPosition = originalAircraftPosition;
        newPosition.y = originalAircraftPosition.y + altitude;
        aircraft.position = newPosition;
        
        Debug.Log($"AltitudeTapeTestController: Set test altitude to {altitude} ft. Actual altitude: {GetCurrentAltitude():F1} ft");
        
        ForceUpdateSystems();
    }
    
    [ContextMenu("Toggle Automatic Testing")]
    public void ToggleAutomaticTesting()
    {
        enableTesting = !enableTesting;
        testingActive = enableTesting;
        
        if (enableTesting)
        {
            Debug.Log("AltitudeTapeTestController: Automatic testing enabled");
            if (aircraft != null)
            {
                originalAircraftPosition = aircraft.position;
            }
        }
        else
        {
            Debug.Log("AltitudeTapeTestController: Automatic testing disabled");
        }
    }
    
    [ContextMenu("Reset Aircraft Position")]
    public void ResetAircraftPosition()
    {
        if (aircraft == null) return;
        
        aircraft.position = originalAircraftPosition;
        Debug.Log("AltitudeTapeTestController: Reset aircraft to original position");
        
        ForceUpdateSystems();
    }
    
    private void ForceUpdateSystems()
    {
        // Force update altimeter
        if (altimeterUI != null)
        {
            altimeterUI.ForceUpdate();
        }
        
        // Force update tape controller
        if (altitudeTapeController != null)
        {
            altitudeTapeController.ForceUpdate();
        }
    }
    
    private float GetCurrentAltitude()
    {
        if (altimeterUI != null)
        {
            return altimeterUI.GetCurrentAltitude();
        }
        return 0f;
    }
    
    // Debug information
    void OnGUI()
    {
        if (!enableTesting) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label("Altitude Tape Test Controller", GUI.skin.box);
        GUILayout.Label($"Current Test Altitude: {currentTestAltitude:F1} ft");
        GUILayout.Label($"Actual Altitude: {GetCurrentAltitude():F1} ft");
        GUILayout.Label($"Direction: {(isIncreasing ? "Increasing" : "Decreasing")}");
        
        if (GUILayout.Button("Manual +25 ft"))
        {
            TestAltitudeIncrease();
        }
        
        if (GUILayout.Button("Manual -25 ft"))
        {
            TestAltitudeDecrease();
        }
        
        GUILayout.EndArea();
    }
}
