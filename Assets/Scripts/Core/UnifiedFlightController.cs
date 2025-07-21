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
    [SerializeField] private KeyCode levelFlightKey = KeyCode.L;
    
    [Header("Control Settings")]
    [SerializeField] private float liftForce = 15f; // Upward force to maintain altitude
    [SerializeField] private float minSpeedForLift = 5f; // Minimum speed needed for lift
    
    [Header("Note: Flight sensitivity settings are now controlled by FlightData component")]
    [SerializeField] private bool useFlightDataSettings = true; // Always use FlightData settings
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showOnScreenDebug = true;
    
    // Core Components
    private FlightData flightData;
    private Transform aircraftTransform;
    private Rigidbody aircraftRigidbody;
    
    // Input State
    private float currentThrottleInput = 0f;
    private float currentMouseX = 0f;
    private float currentMouseY = 0f;
    private bool firePressed = false;
    
    // Discrete Throttle Position System (0-1 range)
    private float throttlePosition = 0.0f; // Start at 0% throttle (idle)
    private float targetSpeed = 0f; // Speed the throttle position is trying to achieve
    
    // Discrete Throttle Step System
    [Header("Throttle Step Settings")]
    [SerializeField] private float throttleStepSize = 0.05f; // 5% per step
    [SerializeField] private float keyRepeatDelay = 0.5f; // Initial delay before repeat starts
    [SerializeField] private float keyRepeatRate = 0.2f; // Time between repeats (5 per second)
    
    // Key repeat timing
    private float throttleUpTimer = 0f;
    private float throttleDownTimer = 0f;
    private bool throttleUpRepeating = false;
    private bool throttleDownRepeating = false;
    
    // Control State - Absolute rotation tracking for kinematic rigidbody
    private float currentPitch = 0f;
    private float currentYaw = 0f;
    private float currentRoll = 0f;
    private float smoothYaw = 0f;
    private float smoothPitch = 0f;
    private float yawVelocity = 0f;
    private float pitchVelocity = 0f;
    private float rollVelocity = 0f;
    
    // Realistic flight - maintain attitude
    private float targetRoll = 0f; // Track desired roll independently
    
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
        aircraftRigidbody = GetComponent<Rigidbody>();
        
        if (flightData == null)
        {
            Debug.LogError("UnifiedFlightController: FlightData component is required!", this);
            enabled = false;
            return;
        }
        
        if (aircraftRigidbody == null)
        {
            Debug.LogError("UnifiedFlightController: Rigidbody component is required!", this);
            enabled = false;
            return;
        }
        
        // Ensure Rigidbody is kinematic for manual control
        if (!aircraftRigidbody.isKinematic)
        {
            Debug.LogWarning("UnifiedFlightController: Setting Rigidbody to kinematic for manual flight control");
            aircraftRigidbody.isKinematic = true;
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
        
        // Update fuel system and engine power
        UpdateFuelSystem(deltaTime);
        
        // Update flight physics
        UpdateThrottle(deltaTime);
        UpdateFlightRotation(deltaTime); // Combined rotation system
        UpdateMovement(deltaTime);
        ApplyLift(deltaTime); // Add lift to maintain altitude
        
        // Apply physics effects
        ApplyDrag(deltaTime);
        ApplySlopeEffects(deltaTime);
        
        // Apply fuel depletion effects (gravity, stall, etc.)
        ApplyFuelDepletionEffects(deltaTime);
        
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
        // DISCRETE THROTTLE STEP SYSTEM - 5% increments with key repeat
        ProcessDiscreteThrottleInput();
        
        // Cursor management for flight game - hide cursor but allow mouse input
        SetupCursorForGameplay();
        
        // Get raw mouse input first
        float rawMouseX = Input.GetAxis("Mouse X");
        float rawMouseY = Input.GetAxis("Mouse Y");
        
        currentMouseX = rawMouseX * flightData.GetSpeedAdjustedYawSpeed();
        currentMouseY = rawMouseY * flightData.GetSpeedAdjustedPitchSpeed();
        
        // Fire input
        firePressed = Input.GetKeyDown(fireKey);
        
        // Emergency level flight key
        if (Input.GetKeyDown(levelFlightKey))
        {
            Debug.Log("EMERGENCY LEVEL FLIGHT ACTIVATED!");
            // Force level flight immediately
            targetRoll = 0f;
            currentRoll = 0f;
            currentPitch = 0f;
            // Keep current yaw but level the wings and nose
        }
        
        // AGGRESSIVE DEBUG - Always log mouse input to see what's happening
        //if (enableDebugLogging && Time.frameCount % 10 == 0) // Every 10 frames
        //{
        //    Debug.Log($"RAW Mouse: X={rawMouseX:F4}, Y={rawMouseY:F4} | SCALED: X={currentMouseX:F2}, Y={currentMouseY:F2}");
        //}
        
        // Also log when we detect ANY mouse movement
        //if (enableDebugLogging && (Mathf.Abs(rawMouseX) > 0.0001f || Mathf.Abs(rawMouseY) > 0.0001f))
        //{
        //    Debug.Log($"MOUSE DETECTED! Raw: X={rawMouseX:F4}, Y={rawMouseY:F4} | Scaled: X={currentMouseX:F2}, Y={currentMouseY:F2}");
        //}
    }
    
    private void ProcessDiscreteThrottleInput()
    {
        // DISCRETE THROTTLE STEP SYSTEM - 5% increments with key repeat
        bool wPressed = Input.GetKey(throttleUpKey);
        bool sPressed = Input.GetKey(throttleDownKey);
        bool wKeyDown = Input.GetKeyDown(throttleUpKey);
        bool sKeyDown = Input.GetKeyDown(throttleDownKey);
        
        float deltaTime = Time.deltaTime;
        
        // Handle W key (throttle up)
        if (wKeyDown)
        {
            // Initial press - immediate 5% increase
            AdjustThrottleStep(1);
            throttleUpTimer = 0f;
            throttleUpRepeating = false;
        }
        else if (wPressed)
        {
            // Key held down - handle repeat
            throttleUpTimer += deltaTime;
            
            if (!throttleUpRepeating && throttleUpTimer >= keyRepeatDelay)
            {
                // Start repeating after initial delay
                throttleUpRepeating = true;
                throttleUpTimer = 0f;
            }
            else if (throttleUpRepeating && throttleUpTimer >= keyRepeatRate)
            {
                // Repeat at regular intervals
                AdjustThrottleStep(1);
                throttleUpTimer = 0f;
            }
        }
        else
        {
            // Key released - reset timers
            throttleUpTimer = 0f;
            throttleUpRepeating = false;
        }
        
        // Handle S key (throttle down)
        if (sKeyDown)
        {
            // Initial press - immediate 5% decrease
            AdjustThrottleStep(-1);
            throttleDownTimer = 0f;
            throttleDownRepeating = false;
        }
        else if (sPressed)
        {
            // Key held down - handle repeat
            throttleDownTimer += deltaTime;
            
            if (!throttleDownRepeating && throttleDownTimer >= keyRepeatDelay)
            {
                // Start repeating after initial delay
                throttleDownRepeating = true;
                throttleDownTimer = 0f;
            }
            else if (throttleDownRepeating && throttleDownTimer >= keyRepeatRate)
            {
                // Repeat at regular intervals
                AdjustThrottleStep(-1);
                throttleDownTimer = 0f;
            }
        }
        else
        {
            // Key released - reset timers
            throttleDownTimer = 0f;
            throttleDownRepeating = false;
        }
        
        // Debug logging
        if (enableDebugLogging && (wPressed || sPressed))
        {
            Debug.Log($"DISCRETE THROTTLE: W={wPressed}, S={sPressed}, Position={throttlePosition*100:F0}%, " +
                     $"W_Timer={throttleUpTimer:F2}, S_Timer={throttleDownTimer:F2}");
        }
    }
    
    private void AdjustThrottleStep(int direction)
    {
        float oldPosition = throttlePosition;
        throttlePosition += direction * throttleStepSize;
        throttlePosition = Mathf.Clamp01(throttlePosition);
        
        if (enableDebugLogging)
        {
            string action = direction > 0 ? "INCREASE" : "DECREASE";
            Debug.Log($"THROTTLE STEP {action}: {oldPosition*100:F0}% → {throttlePosition*100:F0}%");
        }
        
        // Set currentThrottleInput for compatibility with existing systems
        currentThrottleInput = direction;
    }
    
    private void SetupCursorForGameplay()
    {
        // PROPER CURSOR MANAGEMENT: Lock cursor to center during gameplay
        // This prevents accidental UI clicks while preserving mouse input for flight controls
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked; // Lock cursor to center of screen
        
        // ESC key to toggle between locked (gameplay) and free (menu) cursor modes
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                // Switch to menu mode - show cursor and unlock
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                Debug.Log("CURSOR UNLOCKED - Menu mode (ESC to return to gameplay)");
            }
            else
            {
                // Switch back to gameplay mode - hide cursor and lock
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Debug.Log("CURSOR LOCKED - Gameplay mode (ESC for menu)");
            }
        }
    }
    
    #endregion
    
    #region Fuel System
    
    private void UpdateFuelSystem(float deltaTime)
    {
        // Consume fuel based on current speed and throttle usage
        if (flightData.HasFuel())
        {
            flightData.ConsumeFuel(deltaTime);
            
            // Update engine power based on fuel availability
            if (!flightData.HasFuel())
            {
                // Fuel just ran out - start engine power fade
                Debug.LogWarning("FUEL DEPLETED! Engine power fading...");
            }
        }
        
        // Update engine power multiplier based on fuel status
        if (!flightData.HasFuel())
        {
            // Gradually reduce engine power when out of fuel
            float powerReduction = deltaTime / flightData.enginePowerFadeTime;
            flightData.SetEnginePower(Mathf.Max(0f, flightData.enginePowerMultiplier - powerReduction));
        }
        else
        {
            // Restore full power when fuel is available
            flightData.SetEnginePower(1f);
        }
    }
    
    private void ApplyFuelDepletionEffects(float deltaTime)
    {
        // Only apply effects when engine is not running (no fuel)
        if (flightData.isEngineRunning) return;
        
        // Apply gravity - plane falls when no engine power
        Vector3 gravityForce = Vector3.down * flightData.gravityForce * deltaTime;
        aircraftTransform.Translate(gravityForce, Space.World);
        
        // Apply glide drag (higher drag when unpowered)
        float glideDrag = flightData.glideDragCoefficient * flightData.airspeed * flightData.airspeed * deltaTime;
        flightData.airspeed -= glideDrag;
        
        // Check for stall conditions
        if (flightData.IsStalling())
        {
            // Reduce control effectiveness during stall
            currentMouseX *= 0.3f; // Severely reduced control
            currentMouseY *= 0.3f;
            
            // Apply additional downward force during stall
            Vector3 stallForce = Vector3.down * flightData.gravityForce * 0.5f * deltaTime;
            aircraftTransform.Translate(stallForce, Space.World);
            
            // Log stall warning
            if (enableDebugLogging && Time.frameCount % 120 == 0) // Every 2 seconds
            {
                Debug.LogWarning($"AIRCRAFT STALLING! Speed: {flightData.airspeed:F1} MPH (below {flightData.stallSpeed} MPH) - CONTROLS REDUCED!");
            }
        }
        else if (flightData.IsGliding())
        {
            // Gliding - some control but no thrust
            if (enableDebugLogging && Time.frameCount % 180 == 0) // Every 3 seconds
            {
                Debug.Log($"GLIDING - Speed: {flightData.airspeed:F1} MPH, Engine: OFF, Fuel: {flightData.GetFuelPercentage()*100:F0}%");
            }
        }
        
        // Emergency landing detection
        if (flightData.airspeed < flightData.minSpeed * 0.8f)
        {
            if (enableDebugLogging && Time.frameCount % 60 == 0) // Every second
            {
                Debug.LogError($"EMERGENCY! Aircraft below minimum safe speed: {flightData.airspeed:F1} MPH");
            }
        }
    }
    
    #endregion
    
    #region Flight Physics
    
    private void UpdateThrottle(float deltaTime)
    {
        // DISCRETE THROTTLE SYSTEM - Throttle position is now set by discrete steps
        // No continuous input processing needed here - handled in ProcessDiscreteThrottleInput()
        
        // Calculate target speed based on current throttle position
        targetSpeed = Mathf.Lerp(flightData.minSpeed, flightData.maxSpeed, throttlePosition);
        
        // Apply engine power multiplier for fuel depletion effects
        float effectiveTargetSpeed = targetSpeed * flightData.enginePowerMultiplier;
        
        // SLOPE-AWARE THROTTLE SYSTEM - Allow physics effects to influence speed
        // Calculate how much slope is affecting us right now
        float slope = Vector3.Dot(aircraftTransform.forward, Vector3.up);
        float slopeInfluence = Mathf.Abs(slope);
        
        // Increase tolerance for speed deviation when slope effects are significant
        float speedTolerance = 0.5f + (slopeInfluence * 3f); // More tolerance when diving/climbing
        
        // Only apply throttle correction if we're significantly off target AND slope influence is low
        if (Mathf.Abs(flightData.airspeed - effectiveTargetSpeed) > speedTolerance)
        {
            // Reduce throttle aggressiveness when slope effects are active
            float slopeReduction = 1f - (slopeInfluence * 0.6f); // Reduce by up to 60% when steep
            float speedChangeRate = flightData.throttleAcceleration * 0.5f * slopeReduction; // Less aggressive overall
            
            float speedDifference = effectiveTargetSpeed - flightData.airspeed;
            float speedChange = Mathf.Sign(speedDifference) * speedChangeRate * deltaTime;
            
            // Don't overshoot the target
            if (Mathf.Abs(speedChange) > Mathf.Abs(speedDifference))
                speedChange = speedDifference;
                
            flightData.airspeed += speedChange;
            
            if (enableDebugLogging && Time.frameCount % 60 == 0) // Every second
            {
                Debug.Log($"Throttle Correction: Current={flightData.airspeed:F1}, Target={effectiveTargetSpeed:F1}, " +
                         $"Change={speedChange:F1}, Slope={slope:F2}, Tolerance={speedTolerance:F1}");
            }
        }
        else if (enableDebugLogging && Time.frameCount % 120 == 0 && slopeInfluence > 0.1f)
        {
            Debug.Log($"SLOPE PHYSICS ACTIVE - Throttle correction reduced. Slope={slope:F2}, Speed deviation allowed");
        }
        
        // Log fuel depletion effects
        if (flightData.enginePowerMultiplier < 1f && enableDebugLogging && Time.frameCount % 120 == 0)
        {
            Debug.LogWarning($"REDUCED ENGINE POWER: {flightData.enginePowerMultiplier*100:F0}% - Throttle: {throttlePosition*100:F0}% - Fuel: {flightData.GetFuelPercentage()*100:F0}%");
        }
    }
    
    private void UpdateFlightRotation(float deltaTime)
    {
        // REALISTIC FLIGHT - Only update rotation when there's active mouse input
        bool hasInput = Mathf.Abs(currentMouseX) > 0.01f || Mathf.Abs(currentMouseY) > 0.01f;
        
        if (hasInput)
        {
            // KINEMATIC RIGIDBODY ROTATION - Use absolute Euler angle tracking
            
            // Update absolute rotation values based on mouse input
            currentYaw += currentMouseX * deltaTime;
            currentPitch += -currentMouseY * deltaTime; // Invert Y for natural feel
            
            // ENHANCED BANKING SYSTEM - More responsive banking calculation
            if (Mathf.Abs(currentMouseX) > 0.01f)
            {
                // Calculate target roll based on mouse input intensity - much more responsive
                float bankIntensity = Mathf.Clamp(Mathf.Abs(currentMouseX) / 3f, 0f, 1f); // Reduced divisor for more sensitivity
                targetRoll = -Mathf.Sign(currentMouseX) * bankIntensity * flightData.maxBankAngle;
                
                // Clamp to prevent extreme banking
                targetRoll = Mathf.Clamp(targetRoll, -flightData.maxBankAngle, flightData.maxBankAngle);
            }
            else
            {
                // Gradually level wings when no input (more realistic)
                targetRoll = Mathf.MoveTowards(targetRoll, 0f, 15f * deltaTime); // 15 degrees per second leveling
            }
            
            // Smooth banking towards target with limits
            currentRoll = Mathf.SmoothDampAngle(currentRoll, targetRoll, ref rollVelocity, 1f / flightData.bankLerpSpeed, Mathf.Infinity, deltaTime);
            
            // Emergency clamp to prevent extreme banking
            currentRoll = Mathf.Clamp(currentRoll, -flightData.maxBankAngle * 1.5f, flightData.maxBankAngle * 1.5f);
            
            // Clamp pitch to prevent flipping
            currentPitch = Mathf.Clamp(currentPitch, -45f, 45f);
            
            if (enableDebugLogging && Time.frameCount % 60 == 0)
            {
                Debug.Log($"REALISTIC Flight: Pitch={currentPitch:F1}°, Yaw={currentYaw:F1}°, Roll={currentRoll:F1}° | Target Roll={targetRoll:F1}° | Mouse: X={currentMouseX:F2}, Y={currentMouseY:F2}");
            }
        }
        else
        {
            // NO INPUT - Maintain current attitude (realistic flight behavior)
            // Only apply very gentle roll leveling if the plane is extremely banked
            if (Mathf.Abs(currentRoll) > 60f) // Only level if severely banked
            {
                float levelingRate = 10f; // Very slow leveling
                targetRoll = Mathf.MoveTowards(targetRoll, 0f, levelingRate * deltaTime);
                currentRoll = Mathf.SmoothDampAngle(currentRoll, targetRoll, ref rollVelocity, (1f / flightData.bankLerpSpeed) * 3f, Mathf.Infinity, deltaTime);
                
                if (enableDebugLogging && Time.frameCount % 120 == 0)
                {
                    Debug.Log($"EMERGENCY LEVELING: Severe bank angle {currentRoll:F1}°, slowly leveling...");
                }
            }
        }
        
        // Always apply the current rotation to the aircraft
        Quaternion targetRotation = Quaternion.Euler(currentPitch, currentYaw, currentRoll);
        aircraftTransform.rotation = targetRotation;
        
        // Update FlightData with current attitude for UI systems
        flightData.pitch = currentPitch;
        flightData.yaw = currentYaw;
        flightData.roll = currentRoll;
    }
    
    private void UpdateMovement(float deltaTime)
    {
        // Move forward based on current speed
        Vector3 forwardMovement = aircraftTransform.forward * flightData.airspeed * deltaTime;
        aircraftTransform.Translate(forwardMovement, Space.World);
    }
    
    
    private void ApplyDrag(float deltaTime)
    {
        // FIXED DRAG LOGIC - Only apply drag when throttle position is at idle (0%)
        // This allows throttle position to maintain speed properly
        if (throttlePosition < 0.05f) // Only apply drag when throttle is essentially at idle
        {
            float dragForce = flightData.dragCoefficient * flightData.airspeed * flightData.airspeed * deltaTime;
            flightData.airspeed -= dragForce;
            
            if (dragForce > 0.1f && enableDebugLogging && Time.frameCount % 120 == 0)
            {
                Debug.Log($"Applying Drag (Throttle Idle): -{dragForce:F2} MPH, New Speed: {flightData.airspeed:F1}");
            }
        }
        else if (enableDebugLogging && Time.frameCount % 180 == 0) // Less frequent logging
        {
            Debug.Log($"DRAG DISABLED - Throttle Position: {throttlePosition*100:F0}%");
        }
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
        float slopeSpeedChange = flightData.slopeEffect * slope * deltaTime;
        
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
        GUILayout.BeginArea(new Rect(10, 10, 450, 250));
        GUILayout.Label("=== UNIFIED FLIGHT CONTROLLER ===");
        GUILayout.Label($"Speed: {flightData.airspeed:F1} MPH ({flightData.minSpeed}-{flightData.maxSpeed})");
        GUILayout.Label($"Throttle Position: {throttlePosition*100:F0}% (Target: {targetSpeed:F1} MPH)");
        GUILayout.Label($"Throttle Input: {currentThrottleInput:F2}");
        GUILayout.Label($"Engine Power: {flightData.enginePowerMultiplier*100:F0}%");
        GUILayout.Label($"Mouse: X={currentMouseX:F2}, Y={currentMouseY:F2}");
        GUILayout.Label($"Banking: {currentRoll:F1}° (Target: {targetRoll:F1}°)");
        GUILayout.Label($"Attitude: Pitch={currentPitch:F1}°, Yaw={currentYaw:F1}°");
        GUILayout.Label($"Fuel: {flightData.GetFuelPercentage()*100:F0}%");
        GUILayout.Label($"Position: {aircraftTransform.position}");
        GUILayout.Label($"Keys: {throttleUpKey}/{throttleDownKey} (Throttle), {fireKey} (Fire), {levelFlightKey} (Level)");
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
    
    /// <summary>
    /// Get current throttle position as percentage (0.0 to 1.0)
    /// </summary>
    public float GetThrottlePosition()
    {
        return throttlePosition;
    }
    
    /// <summary>
    /// Get current throttle position as percentage (0 to 100)
    /// </summary>
    public float GetThrottlePercentage()
    {
        return throttlePosition * 100f;
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
