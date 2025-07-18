using UnityEngine;

/// <summary>
/// Simple test controller to verify HUD tape animation is working correctly.
/// Attach this to any GameObject to test the tape system with simulated values.
/// </summary>
public class HUDTapeTestController : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool enableTesting = false;
    [SerializeField] private float testSpeed = 50f;
    [SerializeField] private float testAltitude = 1000f;
    [SerializeField] private float speedChangeRate = 10f; // MPH per second
    [SerializeField] private float altitudeChangeRate = 100f; // feet per second
    
    [Header("References")]
    [SerializeField] private FlightData flightData;
    [SerializeField] private HUDTapeController speedTape;
    [SerializeField] private HUDTapeController altitudeTape;
    
    private float originalSpeed;
    private bool testingActive = false;
    
    void Start()
    {
        // Auto-find references if not assigned
        if (flightData == null)
            flightData = FindObjectOfType<FlightData>();
            
        if (speedTape == null)
        {
            HUDTapeController[] tapes = FindObjectsOfType<HUDTapeController>();
            foreach (var tape in tapes)
            {
                // This is a simple way to identify speed vs altitude tapes
                // You might need to adjust this based on your setup
                if (tape.name.ToLower().Contains("speed"))
                    speedTape = tape;
                else if (tape.name.ToLower().Contains("altitude"))
                    altitudeTape = tape;
            }
        }
        
        if (flightData != null)
            originalSpeed = flightData.airspeed;
    }
    
    void Update()
    {
        if (!enableTesting || flightData == null) return;
        
        if (!testingActive)
        {
            testingActive = true;
            Debug.Log("HUD Tape Test: Starting test mode. Original speed: " + originalSpeed);
        }
        
        // Simulate speed changes
        testSpeed += Mathf.Sin(Time.time * 0.5f) * speedChangeRate * Time.deltaTime;
        testSpeed = Mathf.Clamp(testSpeed, 0f, 200f);
        
        // Simulate altitude changes
        testAltitude += Mathf.Cos(Time.time * 0.3f) * altitudeChangeRate * Time.deltaTime;
        testAltitude = Mathf.Clamp(testAltitude, 0f, 5000f);
        
        // Apply test values
        flightData.airspeed = testSpeed;
        
        // Force updates if tapes are assigned
        if (speedTape != null)
            speedTape.ForceUpdate();
        if (altitudeTape != null)
            altitudeTape.ForceUpdate();
    }
    
    void OnDisable()
    {
        // Restore original speed when disabled
        if (testingActive && flightData != null)
        {
            flightData.airspeed = originalSpeed;
            testingActive = false;
            Debug.Log("HUD Tape Test: Restored original speed: " + originalSpeed);
        }
    }
    
    // Public methods for manual testing
    [ContextMenu("Test Speed Increase")]
    public void TestSpeedIncrease()
    {
        if (flightData != null)
        {
            flightData.airspeed += 10f;
            Debug.Log("Test: Speed increased to " + flightData.airspeed);
        }
    }
    
    [ContextMenu("Test Speed Decrease")]
    public void TestSpeedDecrease()
    {
        if (flightData != null)
        {
            flightData.airspeed = Mathf.Max(0f, flightData.airspeed - 10f);
            Debug.Log("Test: Speed decreased to " + flightData.airspeed);
        }
    }
    
    [ContextMenu("Reset to Original Speed")]
    public void ResetToOriginalSpeed()
    {
        if (flightData != null)
        {
            flightData.airspeed = originalSpeed;
            Debug.Log("Test: Speed reset to " + originalSpeed);
        }
    }
    
    // Display current values in inspector
    void OnGUI()
    {
        if (!enableTesting) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label("HUD Tape Test Controller");
        GUILayout.Label($"Test Speed: {testSpeed:F1} MPH");
        GUILayout.Label($"Test Altitude: {testAltitude:F0} ft");
        
        if (flightData != null)
            GUILayout.Label($"FlightData Speed: {flightData.airspeed:F1} MPH");
        
        if (GUILayout.Button("Stop Testing"))
        {
            enableTesting = false;
            OnDisable();
        }
        
        GUILayout.EndArea();
    }
}
