using UnityEngine;

/// <summary>
/// Mouse Input Fix - Disables all conflicting flight controllers
/// Ensures only UnifiedFlightController receives mouse input
/// This fixes the mouse input being intercepted by multiple systems
/// </summary>
public class MOUSE_INPUT_FIX : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    
    void Start()
    {
        FixMouseInputConflicts();
        
        // Disable this script after running once
        enabled = false;
    }
    
    private void FixMouseInputConflicts()
    {
        if (enableDebugLogging)
        {
            Debug.Log("=== MOUSE INPUT FIX STARTING ===");
        }
        
        // Disable the legacy PlayerShipController (in FlightMovementController.cs)
        PlayerShipController legacyController = GetComponent<PlayerShipController>();
        if (legacyController != null)
        {
            legacyController.enabled = false;
            if (enableDebugLogging)
            {
                Debug.Log("✅ DISABLED: PlayerShipController (legacy)");
            }
        }
        
        // Disable RailMovementController
        RailMovementController railController = GetComponent<RailMovementController>();
        if (railController != null)
        {
            railController.enabled = false;
            if (enableDebugLogging)
            {
                Debug.Log("✅ DISABLED: RailMovementController");
            }
        }
        
        // Disable FlightInputController
        FlightInputController inputController = GetComponent<FlightInputController>();
        if (inputController != null)
        {
            inputController.enabled = false;
            if (enableDebugLogging)
            {
                Debug.Log("✅ DISABLED: FlightInputController");
            }
        }
        
        // Disable SimpleFlightController
        SimpleFlightController simpleController = GetComponent<SimpleFlightController>();
        if (simpleController != null)
        {
            simpleController.enabled = false;
            if (enableDebugLogging)
            {
                Debug.Log("✅ DISABLED: SimpleFlightController");
            }
        }
        
        // Disable FlightSpeedController
        FlightSpeedController speedController = GetComponent<FlightSpeedController>();
        if (speedController != null)
        {
            speedController.enabled = false;
            if (enableDebugLogging)
            {
                Debug.Log("✅ DISABLED: FlightSpeedController");
            }
        }
        
        // Ensure UnifiedFlightController is enabled (this has the working lift system)
        UnifiedFlightController unifiedController = GetComponent<UnifiedFlightController>();
        if (unifiedController != null)
        {
            unifiedController.enabled = true;
            if (enableDebugLogging)
            {
                Debug.Log("✅ ENABLED: UnifiedFlightController (with lift system)");
            }
        }
        else
        {
            if (enableDebugLogging)
            {
                Debug.LogError("❌ UnifiedFlightController NOT FOUND! This is required for mouse input.");
            }
        }
        
        // Ensure FlightData is available
        FlightData flightData = GetComponent<FlightData>();
        if (flightData != null)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"✅ FlightData found - Current speed: {flightData.airspeed:F1} MPH");
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log("=== MOUSE INPUT FIX COMPLETE ===");
            Debug.Log("🎮 Mouse input should now work for pitch/yaw/banking!");
            Debug.Log("🚀 Lift system preserved - plane will drop below ~15 MPH");
        }
    }
    
    // Manual execution for testing
    [ContextMenu("Fix Mouse Input")]
    public void ManualFix()
    {
        FixMouseInputConflicts();
    }
}
