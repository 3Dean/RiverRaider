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
        smoothThrottle = Mathf.SmoothDamp(
            smoothThrottle, rawThrottle, ref throttleVel,
            Mathf.Max(data.throttleSmoothTime, 0.001f), Mathf.Infinity, dt
        );
        currentSpeed += data.throttleAcceleration * smoothThrottle * dt;
        currentSpeed -= data.dragCoefficient * currentSpeed * currentSpeed * dt;
        float slope = Vector3.Dot(transform.forward, Vector3.up);
        currentSpeed -= data.slopeEffect * slope * dt;
        currentSpeed = Mathf.Clamp(currentSpeed, data.minSpeed, data.maxSpeed);
        transform.Translate(Vector3.forward * currentSpeed * dt);

        //── 2) TURNING (YAW & PITCH) ───────────────────────────────
        float rawYawInput   = Input.GetAxis("Mouse X");  // –1..+1
        float rawPitchInput = Input.GetAxis("Mouse Y");  // –1..+1

        float targetYawRate   = rawYawInput   * data.yawSpeed;
        float targetPitchRate = -rawPitchInput * data.pitchSpeed;

        smoothYaw = Mathf.SmoothDamp(
            smoothYaw, targetYawRate, ref yawVel,
            Mathf.Max(data.yawSmoothTime, 0.001f), Mathf.Infinity, dt
        );
        smoothPitch = Mathf.SmoothDamp(
            smoothPitch, targetPitchRate, ref pitchVel,
            Mathf.Max(data.pitchSmoothTime, 0.001f), Mathf.Infinity, dt
        );

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

        //── 4) DEBUG LOG (optional) ───────────────────────────────
        Debug.Log($"YawRate={smoothYaw:F1}°/s  LatAccel={lateralAccel:F1}m/s²  Bank={currentRoll:F1}°");
    }
}
