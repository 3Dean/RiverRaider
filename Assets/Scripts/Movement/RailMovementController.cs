using UnityEngine;

public class RailMovementController : MonoBehaviour
{
    [Header("Flight Data")]
    [SerializeField] private FlightData data;

    // Throttle state
    private float rawThrottle, smoothThrottle, throttleVel;
    // Speed & movement
    private float currentSpeed, currentRoll;
    // Raw rotational inputs
    private float rawYaw, rawPitch;
    // Smoothed rotational inputs
    private float smoothYaw, yawVel;
    private float smoothPitch, pitchVel;

    void Awake()
    {
        if (data == null)
            Debug.LogError("FlightData not assigned on " + name, this);
        currentSpeed = (data != null) ? data.minSpeed : 0f;

        FlightInputController.OnThrottleChanged += t => rawThrottle = t;
        FlightInputController.OnYawChanged      += y => rawYaw      = y;
        FlightInputController.OnPitchChanged    += p => rawPitch    = p;
        FlightInputController.OnBoostActivated   += OnBoostOn;
        FlightInputController.OnBoostDeactivated += OnBoostOff;
    }

    void OnDestroy()
    {
        FlightInputController.OnThrottleChanged -= t => rawThrottle = t;
        FlightInputController.OnYawChanged      -= y => rawYaw      = y;
        FlightInputController.OnPitchChanged    -= p => rawPitch    = p;
        FlightInputController.OnBoostActivated   -= OnBoostOn;
        FlightInputController.OnBoostDeactivated -= OnBoostOff;
    }

    private void OnBoostOn()  => data.isBoosting = true;
    private void OnBoostOff() => data.isBoosting = false;

    void Update()
    {
        if (data == null)
            return;

        float dt = Time.deltaTime;

        // Smooth throttle
        smoothThrottle = Mathf.SmoothDamp(
            smoothThrottle, rawThrottle, ref throttleVel,
            Mathf.Max(data.throttleSmoothTime, 0.001f), Mathf.Infinity, dt
        );

        // Update speed with throttle & drag
        currentSpeed += data.throttleAcceleration * smoothThrottle * dt;
        currentSpeed -= data.dragCoefficient * currentSpeed * currentSpeed * dt;

        // Slope effect
        float slope = Vector3.Dot(transform.forward, Vector3.up);
        currentSpeed -= data.slopeEffect * slope * dt;

        // Clamp and move
        currentSpeed = Mathf.Clamp(currentSpeed, data.minSpeed, data.maxSpeed);
        transform.Translate(Vector3.forward * currentSpeed * dt);

        // Smooth rotational inputs
        smoothYaw = Mathf.SmoothDamp(
            smoothYaw, rawYaw, ref yawVel,
            Mathf.Max(data.yawSmoothTime, 0.001f), Mathf.Infinity, dt
        );
        smoothPitch = Mathf.SmoothDamp(
            smoothPitch, rawPitch, ref pitchVel,
            Mathf.Max(data.pitchSmoothTime, 0.001f), Mathf.Infinity, dt
        );

        // Apply rotations
        transform.Rotate(
            smoothPitch * data.pitchSpeed * dt,
            smoothYaw   * data.yawSpeed   * dt,
            0f,
            Space.Self
        );

        // Bank tilt
        float targetRoll = -smoothYaw * data.maxBankAngle;
        currentRoll = Mathf.Lerp(currentRoll, targetRoll, data.bankLerpSpeed * dt);
        Vector3 e = transform.localEulerAngles;
        e.z = currentRoll;
        transform.localEulerAngles = e;
    }
}
