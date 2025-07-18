using UnityEngine;

/// <summary>
/// Flight System Cleanup - Disables conflicting flight controllers
/// Ensures only UnifiedFlightController is active
/// Run this script once to clean up the system
/// </summary>
public class FLIGHT_SYSTEM_CLEANUP : MonoBehaviour
{
    [Header("Cleanup Actions")]
    [SerializeField] private bool disableSimpleFlightController = true;
    [SerializeField] private bool disableFlightSpeedController = true;
    [SerializeField] private bool disableOldInputSystems = true;
    [SerializeField] private bool enableUnifiedFlightController = true;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    
    void Start()
    {
        CleanupFlightSystems();
        
        // Disable this script after cleanup
        enabled = false;
    }
    
    private void CleanupFlightSystems()
    {
        if (enableDebugLogging)
        {
            Debug.Log("=== FLIGHT SYSTEM CLEANUP STARTING ===");
        }
        
        // Find and disable SimpleFlightController
        if (disableSimpleFlightController)
        {
            SimpleFlightController simpleController = GetComponent<SimpleFlightController>();
            if (simpleController != null)
            {
                simpleController.enabled = false;
                if (enableDebugLogging)
                {
                    Debug.Log("✅ DISABLED: SimpleFlightController");
                }
            }
        }
        
        // Find and disable FlightSpeedController
        if (disableFlightSpeedController)
        {
            FlightSpeedController speedController = GetComponent<FlightSpeedController>();
            if (speedController != null)
            {
                speedController.enabled = false;
                if (enableDebugLogging)
                {
                    Debug.Log("✅ DISABLED: FlightSpeedController");
                }
            }
        }
        
        // Find and disable old input systems
        if (disableOldInputSystems)
        {
            // Check for FlightInputController
            FlightInputController inputController = GetComponent<FlightInputController>();
            if (inputController != null)
            {
                inputController.enabled = false;
                if (enableDebugLogging)
                {
                    Debug.Log("✅ DISABLED: FlightInputController");
                }
            }
            
            // Check for RailMovementController
            RailMovementController railController = GetComponent<RailMovementController>();
            if (railController != null)
            {
                railController.enabled = false;
                if (enableDebugLogging)
                {
                    Debug.Log("✅ DISABLED: RailMovementController");
                }
            }
        }
        
        // Ensure UnifiedFlightController is enabled
        if (enableUnifiedFlightController)
        {
            UnifiedFlightController unifiedController = GetComponent<UnifiedFlightController>();
            if (unifiedController != null)
            {
                unifiedController.enabled = true;
                if (enableDebugLogging)
                {
                    Debug.Log("✅ ENABLED: UnifiedFlightController");
                }
            }
            else
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning("⚠️ UnifiedFlightController not found on this GameObject!");
                }
            }
        }
        
        // Initialize FlightData properly
        FlightData flightData = GetComponent<FlightData>();
        if (flightData != null)
        {
            // Set reasonable starting values
            flightData.airspeed = Mathf.Clamp(flightData.airspeed, flightData.minSpeed, 50f);
            flightData.isEngineRunning = flightData.HasFuel();
            flightData.enginePowerMultiplier = flightData.HasFuel() ? 1f : 0f;
            
            if (enableDebugLogging)
            {
                Debug.Log($"✅ INITIALIZED: FlightData - Speed: {flightData.airspeed:F1}, Engine: {(flightData.isEngineRunning ? "ON" : "OFF")}, Fuel: {flightData.currentFuel:F1}");
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log("=== FLIGHT SYSTEM CLEANUP COMPLETE ===");
            Debug.Log("🚀 READY FOR FLIGHT - Only UnifiedFlightController should be active now!");
        }
    }
    
    // Public method to run cleanup manually
    [ContextMenu("Run Flight System Cleanup")]
    public void RunCleanup()
    {
        CleanupFlightSystems();
    }
}
