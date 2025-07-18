using UnityEngine;

/// <summary>
/// Unified Flight Controller - Single responsibility for all aircraft control
/// Follows Unity best practices with clean separation of concerns
/// Designed for easy expansion and maintenance
/// </summary>
[RequireComponent(typeof(FlightData))]
public class UnifiedFlightController : MonoBehaviour
{
    [Header("Input Configuration")]
    [SerializeField] private KeyCode throttleUpKey = KeyCode.W;
    [SerializeField] private KeyCode throttleDownKey = KeyCode.S;
    [SerializeField] private KeyCode fireKey = KeyCode.Space;
    
    [Header("Control Settings")]
    [SerializeField] private float throttleRate = 30f; // Speed change per second
    [SerializeField] private float mouseYawSensitivity = 2f;
    [SerializeField] private float mousePitchSensitivity = 2f;
    [SerializeField] private float controlSmoothTime = 0.1f;
    
    [Header("Physics Settings")]
    [SerializeField] private float dragCoefficient = 0.02f;
    [SerializeField] private float slopeEffect = 20f;
    [SerializeField] private float bankingStrength = 30f;
    [SerializeField] private float bankingSmoothTime = 0.2f;
    [SerializeField] private float liftForce = 15f; // Upward force to maintain altitude
    [SerializeField] private float minSpeedForLift = 5f; // Minimum speed needed for lift
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showOnScreenDebug = true;
    
    // Core Components
    private FlightData flightData;
    private Transform aircraftTransform;
    
    // Input State
    private float currentThrottleInput = 0f;
    private float currentMouseX = 0f;
    private float currentMouseY = 0f;
    private bool firePressed = false;
    
    // Control State
    private float smoothYaw = 0f;
    private float smoothPitch = 0f;
    private float currentRoll = 0f;
    private float yawVelocity = 0f;
    private float pitchVelocity = 0f;
    private float rollVelocity = 0f;
    
    // System State
    private bool isInitialized = false;
    
    // Events for other systems
    public static System.Action<float> OnSpeedChanged;
    public static System.Action<Vector3> OnPositionChanged;
    public static System.Action OnFirePressed;
    
    #region Unity Lifecycle
    
    void Awake()
    {
        // Get required components
        flightData = GetComponent<FlightData>();
        aircraftTransform = transform;
        
        if (flightData == null)
        {
            Debug.LogError("UnifiedFlightController: FlightData component is required!", this);
            enabled = false;
            return;
        }
    }
    
    void Start()
    {
        InitializeSystem();
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        float deltaTime = Time.deltaTime;
        
        // Process input
        ProcessInput();
        
        // Update flight physics
        UpdateThrottle(deltaTime);
        UpdateTurning(deltaTime);
        UpdateMovement(deltaTime);
        ApplyLift(deltaTime); // Add lift to maintain altitude
        UpdateBanking(deltaTime);
        
        // Apply physics effects
        ApplyDrag(deltaTime);
        ApplySlopeEffects(deltaTime);
        
        // Clamp values to safe ranges
        ClampFlightValues();
        
        // Fire events for other systems
        FireEvents();
        
        // Debug output
        if (enableDebugLogging && Time.frameCount % 60 == 0) // Every second
        {
            LogDebugInfo();
        }
    }
    
    void OnGUI()
    {
        if (!showOnScreenDebug || !isInitialized) return;
        
        DrawDebugGUI();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeSystem()
    {
        // Set initial values
        flightData.airspeed = Mathf.Clamp(flightData.airspeed, flightData.minSpeed, flightData.maxSpeed);
        
        // Reset control state
        smoothYaw = 0f;
        smoothPitch = 0f;
        currentRoll = 0f;
        
        isInitialized = true;
        
        if (enableDebugLogging)
        {
            Debug.Log($"UnifiedFlightController: Initialized on {gameObject.name} with speed {flightData.airspeed:F1} MPH");
        }
    }
    
    #endregion
    
    #region Input Processing
    
    private void ProcessInput()
    {
        // Throttle input
        currentThrottleInput = 0f;
        if (Input.GetKey(throttleUpKey))
            currentThrottleInput = 1f;
        else if (Input.GetKey(throttleDownKey))
            currentThrottleInput = -1f;
        
        // Cursor management for flight game - hide cursor but allow mouse input
        SetupCursorForGameplay();
        
        currentMouseX = Input.GetAxis("Mouse X") * mouseYawSensitivity;
        currentMouseY = Input.GetAxis("Mouse Y") * mousePitchSensitivity;
        
        // Fire input
        firePressed = Input.GetKeyDown(fireKey);
        
        // Debug mouse input
        if (enableDebugLogging && (Mathf.Abs(currentMouseX) > 0.01f || Mathf.Abs(currentMouseY) > 0.01f))
        {
            Debug.Log($"Mouse Input: X={currentMouseX:F2}, Y={currentMouseY:F2}");
        }
    }
    
    private void SetupCursorForGameplay()
    {
        // Hide cursor during gameplay to prevent clicking other windows
        Cursor.visible = false;
        
        // Confine cursor to game window but don't lock it completely
        // This allows mouse movement for flight control while preventing window switching
        Cursor.lockState = CursorLockMode.Confined;
        
        // ESC key to show cursor temporarily (common game pattern)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.visible)
            {
                // Hide cursor again
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                // Show cursor temporarily
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
    
    #endregion
    
    #region Flight Physics
    
    private void UpdateThrottle(float deltaTime)
    {
        if (Mathf.Abs(currentThrottleInput) > 0.01f)
        {
            float speedChange = throttleRate * currentThrottleInput * deltaTime;
            flightData.airspeed += speedChange;
            
            if (enableDebugLogging && Mathf.Abs(speedChange) > 0.1f)
            {
                string direction = currentThrottleInput > 0 ? "UP" : "DOWN";
                Debug.Log($"Throttle {direction}: Speed {flightData.airspeed:F1} MPH (Δ{speedChange:F1})");
            }
        }
    }
    
    private void UpdateTurning(float deltaTime)
    {
        // Smooth the turning inputs
        float targetYaw = currentMouseX;
        float targetPitch = -currentMouseY; // Invert Y for natural feel
        
        smoothYaw = Mathf.SmoothDamp(smoothYaw, targetYaw, ref yawVelocity, controlSmoothTime, Mathf.Infinity, deltaTime);
        smoothPitch = Mathf.SmoothDamp(smoothPitch, targetPitch, ref pitchVelocity, controlSmoothTime, Mathf.Infinity, deltaTime);
        
        // Apply rotation
        aircraftTransform.Rotate(smoothPitch * deltaTime, smoothYaw * deltaTime, 0f, Space.Self);
    }
    
    private void UpdateMovement(float deltaTime)
    {
        // Move forward based on current speed
        Vector3 forwardMovement = aircraftTransform.forward * flightData.airspeed * deltaTime;
        aircraftTransform.Translate(forwardMovement, Space.World);
    }
    
    private void UpdateBanking(float deltaTime)
    {
        // Calculate banking angle based on turn rate
        float targetRoll = -smoothYaw * bankingStrength;
        
        // Smooth the banking
        currentRoll = Mathf.SmoothDampAngle(currentRoll, targetRoll, ref rollVelocity, bankingSmoothTime, Mathf.Infinity, deltaTime);
        
        // Apply banking rotation (only roll, don't interfere with pitch/yaw)
        Vector3 currentEuler = aircraftTransform.localEulerAngles;
        currentEuler.z = currentRoll;
        aircraftTransform.localEulerAngles = currentEuler;
        
        if (enableDebugLogging && Mathf.Abs(targetRoll) > 1f && Time.frameCount % 60 == 0)
        {
            Debug.Log($"Banking: Target={targetRoll:F1}°, Current={currentRoll:F1}°, YawInput={smoothYaw:F2}");
        }
    }
    
    private void ApplyDrag(float deltaTime)
    {
        // Simple drag calculation
        float dragForce = dragCoefficient * flightData.airspeed * flightData.airspeed * deltaTime;
        flightData.airspeed -= dragForce;
    }
    
    private void ApplyLift(float deltaTime)
    {
        // Apply upward lift force when engine is running and speed is sufficient
        if (flightData.isEngineRunning && flightData.airspeed >= minSpeedForLift)
        {
            // Calculate lift based on speed (more speed = more lift)
            float speedRatio = flightData.airspeed / flightData.maxSpeed;
            float currentLift = liftForce * speedRatio;
            
            // Apply upward force to counteract gravity
            Vector3 liftVector = Vector3.up * currentLift * deltaTime;
            aircraftTransform.Translate(liftVector, Space.World);
            
            if (enableDebugLogging && Time.frameCount % 180 == 0) // Every 3 seconds
            {
                Debug.Log($"Applying Lift: {currentLift:F1} (Speed: {flightData.airspeed:F1}, Ratio: {speedRatio:F2})");
            }
        }
        else if (enableDebugLogging && Time.frameCount % 180 == 0)
        {
            string reason = !flightData.isEngineRunning ? "Engine Off" : "Speed Too Low";
            Debug.Log($"No Lift Applied: {reason} (Speed: {flightData.airspeed:F1})");
        }
    }
    
    private void ApplySlopeEffects(float deltaTime)
    {
        // Calculate slope effect (climbing slows down, diving speeds up)
        float slope = Vector3.Dot(aircraftTransform.forward, Vector3.up);
        float slopeSpeedChange = slopeEffect * slope * deltaTime;
        
        // Apply slope effect (positive slope = climbing = lose speed)
        flightData.airspeed -= slopeSpeedChange;
        
        if (enableDebugLogging && Mathf.Abs(slope) > 0.1f && Time.frameCount % 120 == 0)
        {
            string direction = slope > 0 ? "CLIMBING" : "DIVING";
            Debug.Log($"{direction}: Slope {slope:F2}, Speed change {-slopeSpeedChange:F1}");
        }
    }
    
    #endregion
    
    #region Value Management
    
    private void ClampFlightValues()
    {
        // Clamp speed to safe ranges
        float oldSpeed = flightData.airspeed;
        flightData.airspeed = Mathf.Clamp(flightData.airspeed, flightData.minSpeed, flightData.maxSpeed);
        
        if (enableDebugLogging && Mathf.Abs(oldSpeed - flightData.airspeed) > 0.1f)
        {
            Debug.Log($"Speed clamped from {oldSpeed:F1} to {flightData.airspeed:F1}");
        }
    }
    
    #endregion
    
    #region Events
    
    private void FireEvents()
    {
        // Fire speed change event
        OnSpeedChanged?.Invoke(flightData.airspeed);
        
        // Fire position change event
        OnPositionChanged?.Invoke(aircraftTransform.position);
        
        // Fire weapon event
        if (firePressed)
        {
            OnFirePressed?.Invoke();
        }
    }
    
    #endregion
    
    #region Debug
    
    private void LogDebugInfo()
    {
        Debug.Log($"[UnifiedFlightController] Speed: {flightData.airspeed:F1} MPH | " +
                 $"Throttle: {currentThrottleInput:F2} | " +
                 $"Yaw: {smoothYaw:F2} | Pitch: {smoothPitch:F2} | Roll: {currentRoll:F1}°");
    }
    
    private void DrawDebugGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Label("=== UNIFIED FLIGHT CONTROLLER ===");
        GUILayout.Label($"Speed: {flightData.airspeed:F1} MPH ({flightData.minSpeed}-{flightData.maxSpeed})");
        GUILayout.Label($"Throttle Input: {currentThrottleInput:F2}");
        GUILayout.Label($"Mouse: X={currentMouseX:F2}, Y={currentMouseY:F2}");
        GUILayout.Label($"Controls: Yaw={smoothYaw:F2}, Pitch={smoothPitch:F2}");
        GUILayout.Label($"Banking: {currentRoll:F1}°");
        GUILayout.Label($"Position: {aircraftTransform.position}");
        GUILayout.Label($"Keys: {throttleUpKey}/{throttleDownKey} (Throttle), {fireKey} (Fire)");
        GUILayout.EndArea();
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Get current flight state for external systems
    /// </summary>
    public FlightState GetFlightState()
    {
        return new FlightState
        {
            speed = flightData.airspeed,
            position = aircraftTransform.position,
            rotation = aircraftTransform.rotation,
            throttleInput = currentThrottleInput,
            isEngineRunning = flightData.isEngineRunning
        };
    }
    
    /// <summary>
    /// Set speed directly (for external systems like fuel depletion)
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        flightData.airspeed = Mathf.Clamp(newSpeed, flightData.minSpeed, flightData.maxSpeed);
    }
    
    /// <summary>
    /// Enable/disable the flight controller
    /// </summary>
    public void SetControlEnabled(bool enabled)
    {
        this.enabled = enabled;
        
        if (!enabled)
        {
            // Reset input state when disabled
            currentThrottleInput = 0f;
            currentMouseX = 0f;
            currentMouseY = 0f;
        }
    }
    
    #endregion
}

/// <summary>
/// Flight state data structure for external systems
/// </summary>
[System.Serializable]
public struct FlightState
{
    public float speed;
    public Vector3 position;
    public Quaternion rotation;
    public float throttleInput;
    public bool isEngineRunning;
}
