using UnityEngine;

/// <summary>
/// Simple, robust flight controller that combines input and speed control
/// This is a backup solution if the modular system has issues
/// </summary>
public class SimpleFlightController : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float throttleRate = 30f; // Speed change per second
    [SerializeField] private float startingSpeed = 20f;
    [SerializeField] private float minSpeed = 10f;
    [SerializeField] private float maxSpeed = 100f;
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode throttleUpKey = KeyCode.W;
    [SerializeField] private KeyCode throttleDownKey = KeyCode.S;
    
    [Header("Debug")]
    [SerializeField] private bool showDebug = true;
    
    // References
    private FlightData flightData;
    
    // State
    private bool initialized = false;
    
    void Start()
    {
        // Find or create FlightData
        flightData = GetComponent<FlightData>();
        if (flightData == null)
        {
            flightData = FindObjectOfType<FlightData>();
        }
        
        if (flightData != null)
        {
            flightData.airspeed = startingSpeed;
            initialized = true;
            
            if (showDebug)
                Debug.Log($"SimpleFlightController: Initialized on {gameObject.name} with speed {startingSpeed}");
        }
        else
        {
            Debug.LogError("SimpleFlightController: No FlightData found!", this);
        }
    }
    
    void Update()
    {
        if (!initialized) return;
        
        HandleInput();
        ClampSpeed();
        
        if (showDebug)
        {
            // Show current speed in debug
            if (Time.frameCount % 60 == 0) // Every 60 frames (roughly 1 second)
            {
                Debug.Log($"SimpleFlightController - Current Speed: {flightData.airspeed:F1} MPH");
            }
        }
    }
    
    void LateUpdate()
    {
        // Run in LateUpdate to override any other systems that might modify airspeed
        if (!initialized) return;
        
        // Ensure our speed value persists (in case other systems override it)
        ClampSpeed();
    }
    
    void HandleInput()
    {
        float speedChange = 0f;
        
        if (Input.GetKey(throttleUpKey))
        {
            speedChange = throttleRate * Time.deltaTime;
            flightData.airspeed += speedChange;
            
            if (showDebug)
                Debug.Log($"W Key - Speed UP: {flightData.airspeed:F1} MPH (+{speedChange:F1})");
        }
        else if (Input.GetKey(throttleDownKey))
        {
            speedChange = -throttleRate * Time.deltaTime;
            flightData.airspeed += speedChange;
            
            if (showDebug)
                Debug.Log($"S Key - Speed DOWN: {flightData.airspeed:F1} MPH ({speedChange:F1})");
        }
    }
    
    void ClampSpeed()
    {
        float oldSpeed = flightData.airspeed;
        flightData.airspeed = Mathf.Clamp(flightData.airspeed, minSpeed, maxSpeed);
        
        if (oldSpeed != flightData.airspeed && showDebug)
        {
            Debug.Log($"Speed clamped from {oldSpeed:F1} to {flightData.airspeed:F1}");
        }
    }
    
    // Public methods for external control
    public void SetSpeed(float newSpeed)
    {
        if (flightData != null)
        {
            flightData.airspeed = Mathf.Clamp(newSpeed, minSpeed, maxSpeed);
        }
    }
    
    public float GetCurrentSpeed()
    {
        return flightData != null ? flightData.airspeed : 0f;
    }
    
    void OnGUI()
    {
        if (!showDebug || !initialized) return;
        
        // Simple on-screen debug display
        GUILayout.BeginArea(new Rect(10, 200, 300, 100));
        GUILayout.Label("=== SIMPLE FLIGHT CONTROLLER ===");
        GUILayout.Label($"Speed: {flightData.airspeed:F1} MPH");
        GUILayout.Label($"W/S Keys: {throttleUpKey}/{throttleDownKey}");
        GUILayout.Label($"Range: {minSpeed}-{maxSpeed} MPH");
        GUILayout.EndArea();
    }
}
