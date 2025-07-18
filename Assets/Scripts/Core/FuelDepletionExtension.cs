using UnityEngine;

/// <summary>
/// Fuel Depletion Extension for UnifiedFlightController
/// Handles fuel consumption, engine failure, and stalling effects
/// Designed to work seamlessly with the unified flight system
/// </summary>
[RequireComponent(typeof(UnifiedFlightController))]
[RequireComponent(typeof(PlayerShipFuel))]
public class FuelDepletionExtension : MonoBehaviour
{
    [Header("Engine Failure Effects")]
    [SerializeField] private float gravityForce = 9.8f; // Downward force when engine off
    [SerializeField] private float glideDragCoefficient = 0.05f; // Extra drag when gliding
    [SerializeField] private float stallSpeed = 15f; // Speed below which aircraft stalls
    [SerializeField] private float stallGravityMultiplier = 2f; // Extra gravity during stall
    
    [Header("Control Degradation")]
    [SerializeField] private float glideControlEffectiveness = 0.7f; // Control reduction when gliding
    [SerializeField] private float stallControlEffectiveness = 0.3f; // Control reduction when stalling
    
    [Header("Recovery Settings")]
    [SerializeField] private float minimumFuelForRestart = 5f; // Fuel needed to restart engine
    [SerializeField] private float engineRestartDelay = 2f; // Time to restart engine
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    
    // Component References
    private UnifiedFlightController flightController;
    private PlayerShipFuel fuelSystem;
    private FlightData flightData;
    
    // State
    private bool isEngineRunning = true;
    private bool isStalling = false;
    private bool isGliding = false;
    private float engineRestartTimer = 0f;
    private float lastFuelLevel = 100f;
    
    void Awake()
    {
        // Get required components
        flightController = GetComponent<UnifiedFlightController>();
        fuelSystem = GetComponent<PlayerShipFuel>();
        flightData = GetComponent<FlightData>();
        
        if (flightController == null || fuelSystem == null || flightData == null)
        {
            Debug.LogError("FuelDepletionExtension: Missing required components!", this);
            enabled = false;
        }
    }
    
    void Start()
    {
        // Initialize state using FlightData (PlayerShipFuel uses flightData.currentFuel)
        isEngineRunning = flightData.currentFuel > 0f;
        flightData.isEngineRunning = isEngineRunning;
        
        if (enableDebugLogging)
        {
            Debug.Log($"FuelDepletionExtension: Initialized with {flightData.currentFuel:F1} fuel, Engine: {(isEngineRunning ? "RUNNING" : "OFF")}");
        }
    }
    
    void Update()
    {
        float deltaTime = Time.deltaTime;
        
        // Check engine status (PlayerShipFuel handles fuel consumption automatically)
        UpdateEngineStatus();
        
        // Apply fuel depletion effects
        if (!isEngineRunning)
        {
            ApplyEngineFailureEffects(deltaTime);
        }
        
        // Handle engine restart attempts
        HandleEngineRestart(deltaTime);
        
        // Update flight data
        UpdateFlightDataState();
        
        // Debug logging
        if (enableDebugLogging && Time.frameCount % 180 == 0) // Every 3 seconds
        {
            LogDebugInfo();
        }
    }
    
    private void UpdateEngineStatus()
    {
        bool shouldEngineRun = flightData.currentFuel > 0f;
        
        if (isEngineRunning && !shouldEngineRun)
        {
            // Engine failure
            isEngineRunning = false;
            engineRestartTimer = 0f;
            
            if (enableDebugLogging)
            {
                Debug.LogWarning("ENGINE FAILURE - Out of fuel!");
            }
        }
        else if (!isEngineRunning && shouldEngineRun && flightData.currentFuel >= minimumFuelForRestart)
        {
            // Engine can potentially restart
            if (engineRestartTimer <= 0f)
            {
                engineRestartTimer = engineRestartDelay;
            }
        }
        
        // Update flight data
        flightData.isEngineRunning = isEngineRunning;
        
        // Debug engine status
        if (enableDebugLogging && Time.frameCount % 300 == 0) // Every 5 seconds
        {
            Debug.Log($"Engine Status: {(isEngineRunning ? "RUNNING" : "OFF")} | Fuel: {flightData.currentFuel:F1}");
        }
    }
    
    private void ApplyEngineFailureEffects(float deltaTime)
    {
        // Apply gravity
        Vector3 gravityVector = Vector3.down * gravityForce * deltaTime;
        transform.Translate(gravityVector, Space.World);
        
        // Apply glide drag
        float glideDrag = glideDragCoefficient * flightData.airspeed * flightData.airspeed * deltaTime;
        flightData.airspeed -= glideDrag;
        
        // Check for stalling
        isStalling = flightData.airspeed < stallSpeed;
        isGliding = !isStalling;
        
        if (isStalling)
        {
            // Apply additional stall effects
            Vector3 stallForce = Vector3.down * gravityForce * stallGravityMultiplier * deltaTime;
            transform.Translate(stallForce, Space.World);
            
            // Reduce control effectiveness severely
            ApplyControlDegradation(stallControlEffectiveness);
            
            if (enableDebugLogging && Time.frameCount % 60 == 0)
            {
                Debug.LogWarning($"STALLING! Speed: {flightData.airspeed:F1} MPH (below {stallSpeed} MPH)");
            }
        }
        else if (isGliding)
        {
            // Gliding - some control but reduced
            ApplyControlDegradation(glideControlEffectiveness);
            
            if (enableDebugLogging && Time.frameCount % 240 == 0)
            {
                Debug.Log($"GLIDING - Speed: {flightData.airspeed:F1} MPH, Engine: OFF");
            }
        }
    }
    
    private void ApplyControlDegradation(float effectiveness)
    {
        // This would require modifying UnifiedFlightController to expose sensitivity settings
        // For now, we'll just log the effect
        if (enableDebugLogging && Time.frameCount % 120 == 0)
        {
            Debug.Log($"Control effectiveness reduced to {effectiveness * 100:F0}%");
        }
    }
    
    private void HandleEngineRestart(float deltaTime)
    {
        if (!isEngineRunning && engineRestartTimer > 0f)
        {
            engineRestartTimer -= deltaTime;
            
            if (engineRestartTimer <= 0f && flightData.currentFuel >= minimumFuelForRestart)
            {
                // Restart engine
                isEngineRunning = true;
                isStalling = false;
                isGliding = false;
                
                if (enableDebugLogging)
                {
                    Debug.Log("ENGINE RESTARTED!");
                }
            }
        }
    }
    
    private void UpdateFlightDataState()
    {
        flightData.isEngineRunning = isEngineRunning;
    }
    
    private void LogDebugInfo()
    {
        string status = isEngineRunning ? "RUNNING" : (isStalling ? "STALLING" : "GLIDING");
        Debug.Log($"[FuelDepletionExtension] Engine: {status} | " +
                 $"Fuel: {flightData.currentFuel:F1} | " +
                 $"Speed: {flightData.airspeed:F1} MPH");
    }
    
    // Public API for external systems
    public bool IsEngineRunning => isEngineRunning;
    public bool IsStalling => isStalling;
    public bool IsGliding => isGliding;
    public float GetFuelLevel() => flightData.currentFuel;
    
    public void ForceEngineRestart()
    {
        if (flightData.currentFuel >= minimumFuelForRestart)
        {
            engineRestartTimer = 0.1f; // Almost immediate restart
        }
    }
    
    // Events for other systems
    public static System.Action OnEngineFailure;
    public static System.Action OnEngineRestart;
    public static System.Action OnStallBegin;
    public static System.Action OnStallEnd;
    
    private void FireEngineFailureEvent()
    {
        OnEngineFailure?.Invoke();
    }
    
    private void FireEngineRestartEvent()
    {
        OnEngineRestart?.Invoke();
    }
}
