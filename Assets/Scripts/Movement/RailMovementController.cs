using UnityEngine;

public class RailMovementController : MonoBehaviour
{
    [SerializeField] private FlightData data;    // ← assign your SO here
    public float baseSpeed = 50f;
    private float currentSpeed;
    
    // smoothing state
    float smoothYawInput = 0f, yawInputVelocity = 0f;
    float smoothPitchInput = 0f, pitchInputVelocity = 0f;
    float currentRoll = 0f;
    

    void Awake()
    {
        currentSpeed = baseSpeed;
        FlightInputController.OnThrottleChanged   += HandleThrottle;
        FlightInputController.OnBoostActivated    += () => data.isBoosting = true;
        FlightInputController.OnBoostDeactivated  += () => data.isBoosting = false;
    }

void OnEnable()
{
    FlightInputController.OnYawChanged   += HandleRawYaw;
    FlightInputController.OnPitchChanged += HandleRawPitch;
    FlightInputController.OnThrottleChanged   += HandleThrottle;
    FlightInputController.OnBoostActivated    += () => data.isBoosting = true;
    FlightInputController.OnBoostDeactivated  += () => data.isBoosting = false;
}

void OnDisable()
{
    FlightInputController.OnYawChanged   -= HandleRawYaw;
    FlightInputController.OnPitchChanged -= HandleRawPitch;
    FlightInputController.OnThrottleChanged   -= HandleThrottle;
    FlightInputController.OnBoostActivated    -= () => data.isBoosting = true;
    FlightInputController.OnBoostDeactivated  -= () => data.isBoosting = false;
}


   // these store the latest raw  -1..1  input
    private float rawYaw, rawPitch;

    void HandleRawYaw(float amount)
    {
        rawYaw = amount;
    }

    void HandleRawPitch(float amount)
    {
        rawPitch = amount;
    }

    void OnDestroy()
    {
        FlightInputController.OnThrottleChanged -= HandleThrottle;
    }

    void HandleThrottle(float amount)
    {
        currentSpeed += data.throttleAcceleration * amount * Time.deltaTime;
        currentSpeed  = Mathf.Clamp(currentSpeed, data.minSpeed, data.maxSpeed);
        data.airspeed = currentSpeed;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // 1) smooth the raw inputs
        smoothYawInput = Mathf.SmoothDamp(
            smoothYawInput, rawYaw,
            ref yawInputVelocity,
            data.yawSmoothTime,
            Mathf.Infinity, dt
        );
        smoothPitchInput = Mathf.SmoothDamp(
            smoothPitchInput, rawPitch,
            ref pitchInputVelocity,
            data.pitchSmoothTime,
            Mathf.Infinity, dt
        );

        // 2) apply yaw & bank off of the smoothed input
        float yDelta = smoothYawInput * data.yawSpeed * dt;
        transform.Rotate(0f, yDelta, 0f, Space.Self);

        float targetRoll = -smoothYawInput * data.maxBankAngle;
        currentRoll = Mathf.Lerp(currentRoll, targetRoll, data.bankLerpSpeed * dt);

        // preserve X & Y, only override Z for bank
        Vector3 e = transform.localEulerAngles;
        e.z = currentRoll;
        transform.localEulerAngles = e;

        // 3) apply pitch off of the smoothed input
        float xDelta = smoothPitchInput * data.pitchSpeed * dt;
        transform.Rotate(xDelta, 0f, 0f, Space.Self);

        float speed = data.isBoosting 
            ? currentSpeed * data.boostMultiplier 
            : currentSpeed;

        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
