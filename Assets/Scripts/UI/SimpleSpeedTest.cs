using UnityEngine;

public class SimpleSpeedTest : MonoBehaviour
{
    [Header("Configurable Settings")]
    [SerializeField] private float throttleRate = 30f; // MPH per second (adjustable)
    [SerializeField] private float slopeMultiplier = 50f; // Slope effect strength (adjustable)
    [SerializeField] private float startingSpeed = 20f; // Starting speed
    
    private bool initialized = false;
    
    void Update()
    {
        // DISABLED - UnifiedFlightController is now handling all flight control
        if (!initialized)
        {
            Debug.LogWarning("SimpleSpeedTest: DISABLED - UnifiedFlightController is handling flight control");
            initialized = true;
        }
        return;
        
        // Find the FlightData component
        FlightData flightData = FindObjectOfType<FlightData>();
        
        if (flightData != null)
        {
            // Initialize starting speed once
            if (!initialized)
            {
                flightData.airspeed = startingSpeed;
                initialized = true;
                Debug.Log($"Starting speed set to {startingSpeed} MPH");
            }
            
            // Direct speed control - now configurable
            if (Input.GetKey(KeyCode.W))
            {
                flightData.airspeed += throttleRate * Time.deltaTime;
                Debug.Log("W PRESSED - Speed now: " + flightData.airspeed);
            }
            
            if (Input.GetKey(KeyCode.S))
            {
                flightData.airspeed -= throttleRate * Time.deltaTime;
                Debug.Log("S PRESSED - Speed now: " + flightData.airspeed);
            }
            
            // Configurable slope effects
            float slope = Vector3.Dot(flightData.transform.forward, Vector3.up);
            if (Mathf.Abs(slope) > 0.05f)
            {
                float slopeEffect = slopeMultiplier * slope * Time.deltaTime;
                flightData.airspeed -= slopeEffect; // Climbing slows down, diving speeds up
                
                string direction = slope > 0 ? "CLIMBING" : "DIVING";
                string effect = slopeEffect > 0 ? "SLOWING DOWN" : "SPEEDING UP";
                Debug.Log($"{direction} - {effect} - Speed change: {-slopeEffect:F1}, New speed: {flightData.airspeed:F1}");
            }
            
            // Use FlightData settings for min/max speed
            flightData.airspeed = Mathf.Clamp(flightData.airspeed, flightData.minSpeed, flightData.maxSpeed);
        }
        else
        {
            Debug.LogError("Cannot find FlightData component!");
        }
        
        // Show status every 3 seconds
        if (Time.time % 3f < Time.deltaTime)
        {
            Debug.Log($"SimpleSpeedTest: W/S = ±{throttleRate} MPH/sec, Slope = {slopeMultiplier}x");
        }
    }
}
