using UnityEngine;

public class RailMovementController : MonoBehaviour
{
    [Header("Flight Data (assign your FlightData component)")]
    [SerializeField] private FlightData data;

    // Throttle state
    float rawThrottle, smoothThrottle, throttleVel;
    // Speed
    float currentSpeed;
    // Raw rotational inputs
    // (we’ll read X mouse here instead of events)
    // Smoothed yaw/pitch for turning the ship
    float smoothYaw, yawVel;
    float smoothPitch, pitchVel;
    // Banking
    float currentRoll, rollVel;

    void Awake()
    {
        if (data == null)
            Debug.LogError("RailMovementController needs FlightData!", this);

        FlightInputController.OnThrottleChanged += t => rawThrottle = t;
        FlightInputController.OnBoostActivated   += () => data.isBoosting = true;
        FlightInputController.OnBoostDeactivated += () => data.isBoosting = false;
    }
    
    void Start()
    {
        // Initialize speed to a reasonable starting value
        currentSpeed = (data.minSpeed + data.maxSpeed) * 0.3f; // Start at 30% of speed range
        Debug.Log($"RailMovementController initialized with speed: {currentSpeed}");
    }

    void OnDestroy()
    {
        FlightInputController.OnThrottleChanged -= t => rawThrottle = t;
        FlightInputController.OnBoostActivated   -= () => data.isBoosting = true;
        FlightInputController.OnBoostDeactivated -= () => data.isBoosting = false;
    }

    void Update()
    {
        if (data == null) return;
        float dt = Time.deltaTime;

        //── 1) THROTTLE & FORWARD ─────────────────────────────────
        // DEBUGGING: Check direct key input as fallback
        float directThrottle = 0f;
        if (Input.GetKey(KeyCode.W)) directThrottle = 1f;
        else if (Input.GetKey(KeyCode.S)) directThrottle = -1f;
        
        // Use direct input if event system isn't working
        if (rawThrottle == 0f && directThrottle != 0f)
        {
            Debug.Log($"Using direct throttle input: {directThrottle} (rawThrottle was 0)");
            rawThrottle = directThrottle;
        }
        
        smoothThrottle = Mathf.SmoothDamp(
            smoothThrottle, rawThrottle, ref throttleVel,
            Mathf.Max(data.throttleSmoothTime, 0.001f), Mathf.Infinity, dt
        );
        // Calculate speed changes step by step for debugging
        float throttleSpeedChange = data.throttleAcceleration * smoothThrottle * dt;
        float dragSpeedChange = data.dragCoefficient * currentSpeed * currentSpeed * dt;
        
        currentSpeed += throttleSpeedChange;
        currentSpeed -= dragSpeedChange;
        
        // Debug speed changes when throttle is active
        if (Mathf.Abs(smoothThrottle) > 0.1f || Mathf.Abs(throttleSpeedChange) > 0.1f)
        {
            Debug.Log($"Throttle: {smoothThrottle:F2}, ThrottleChange: +{throttleSpeedChange:F2}, DragChange: -{dragSpeedChange:F2}, ResultSpeed: {currentSpeed:F1}");
        }
        
        // Enhanced slope effect for realistic climb/dive physics
        float slope = Vector3.Dot(transform.forward, Vector3.up);
        float slopeAngle = Mathf.Asin(Mathf.Clamp(slope, -1f, 1f)) * Mathf.Rad2Deg;
        
        // Apply slope effect with angle limiting
        float normalizedSlope = Mathf.Clamp(slope, -Mathf.Sin(data.maxSlopeAngle * Mathf.Deg2Rad), 
                                                   Mathf.Sin(data.maxSlopeAngle * Mathf.Deg2Rad));
        float slopeSpeedChange = data.slopeEffect * normalizedSlope * dt;
        
        // FIXED: Climbing should slow down (positive slope = lose speed), diving should speed up (negative slope = gain speed)
        currentSpeed -= slopeSpeedChange; // This is correct: positive slope reduces speed, negative slope increases speed
        
        // Debug info for tuning (shows climb/dive effects)
        if (Mathf.Abs(slope) > 0.1f)
        {
            string direction = slope > 0 ? "CLIMBING" : "DIVING";
            string effect = slopeSpeedChange > 0 ? "LOSING SPEED" : "GAINING SPEED";
            Debug.Log($"{direction} - Angle: {slopeAngle:F1}°, {effect}, Speed Change: {-slopeSpeedChange:F2}, Airspeed: {currentSpeed:F1}");
        }
        
        currentSpeed = Mathf.Clamp(currentSpeed, data.minSpeed, data.maxSpeed);
        
        // Use the airspeed from FlightData for actual movement (so UI and movement match)
        float actualMovementSpeed = data.airspeed;
        transform.Translate(Vector3.forward * actualMovementSpeed * dt);
        
        Debug.Log($"Movement Speed: {actualMovementSpeed:F1} (from FlightData.airspeed)");

        //── 2) TURNING (YAW & PITCH) - SPEED-DEPENDENT RESPONSIVENESS ───────────────────────────────
        float rawYawInput   = Input.GetAxis("Mouse X");  // –1..+1
        float rawPitchInput = Input.GetAxis("Mouse Y");  // –1..+1

        // Use speed-adjusted control sensitivity (less responsive at high speed)
        float targetYawRate   = rawYawInput   * data.GetSpeedAdjustedYawSpeed();
        float targetPitchRate = -rawPitchInput * data.GetSpeedAdjustedPitchSpeed();

        // Use speed-adjusted smooth time (more sluggish at high speed)
        float speedAdjustedYawSmoothTime = data.GetSpeedAdjustedSmoothTime(data.yawSmoothTime);
        float speedAdjustedPitchSmoothTime = data.GetSpeedAdjustedSmoothTime(data.pitchSmoothTime);

        smoothYaw = Mathf.SmoothDamp(
            smoothYaw, targetYawRate, ref yawVel,
            Mathf.Max(speedAdjustedYawSmoothTime, 0.001f), Mathf.Infinity, dt
        );
        smoothPitch = Mathf.SmoothDamp(
            smoothPitch, targetPitchRate, ref pitchVel,
            Mathf.Max(speedAdjustedPitchSmoothTime, 0.001f), Mathf.Infinity, dt
        );
        
        // Debug responsiveness changes (can be disabled)
        if (Time.frameCount % 120 == 0) // Log every ~2 seconds at 60fps
        {
            float responsiveness = data.GetCurrentResponsiveness();
            Debug.Log($"Speed: {data.airspeed:F0} MPH | Responsiveness: {responsiveness:F2} | YawSpeed: {data.GetSpeedAdjustedYawSpeed():F1}");
        }

        transform.Rotate(
            smoothPitch * dt,
            smoothYaw   * dt,
            0f,
            Space.Self
        );

        //── 3) PHYSICS-BASED BANKING ───────────────────────────────
        // Convert smoothed yaw rate (deg/sec) → rad/sec
        float yawRateRad   = smoothYaw * Mathf.Deg2Rad;
        // Centripetal accel = ω × v
        float lateralAccel = yawRateRad * currentSpeed;
        float g            = Mathf.Abs(Physics.gravity.y);
        float bankRad      = Mathf.Atan2(lateralAccel, g);
        float targetRoll   = -bankRad * Mathf.Rad2Deg;

        // Smooth into that roll angle
        currentRoll = Mathf.SmoothDampAngle(
            currentRoll,
            targetRoll,
            ref rollVel,
            data.bankLerpSpeed   // now “time (sec) to settle into bank”
        );

        // Apply only the Z-axis on the same transform
        Vector3 e = transform.localEulerAngles;
        // Normalize X/Y so they stay in –180..+180
        e.x = (e.x > 180f ? e.x - 360f : e.x);
        e.y = (e.y > 180f ? e.y - 360f : e.y);
        e.z = currentRoll;
        transform.localEulerAngles = e;

        //── 4) UPDATE FLIGHT DATA ─────────────────────────────────
        // DISABLED: Let other scripts control airspeed
        // data.airspeed = currentSpeed;
        
        // Additional debugging every few seconds
        if (Time.time % 2f < Time.deltaTime) // Log every ~2 seconds
        {
            Debug.Log($"[RailMovementController] Speed: {currentSpeed:F1}, Throttle: {rawThrottle:F2}, SmoothThrottle: {smoothThrottle:F2}");
        }
    }
}
