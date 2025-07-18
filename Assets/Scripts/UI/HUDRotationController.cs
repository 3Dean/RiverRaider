using UnityEngine;

/// <summary>
/// Controls the rotation of HUD elements to match aircraft yaw/banking motion.
/// Creates an immersive cockpit effect where UI elements tilt with the aircraft.
/// </summary>
public class HUDRotationController : MonoBehaviour
{
    [Header("Aircraft Reference")]
    [SerializeField] private Transform aircraft;
    [SerializeField] private FlightData flightData; // For accessing roll data directly
    
    [Header("Rotation Settings")]
    [SerializeField] private bool useRollRotation = true; // Use aircraft roll (banking)
    [SerializeField] private bool useYawRotation = false; // Use aircraft yaw (turning)
    [SerializeField] private bool usePitchRotation = false; // Use aircraft pitch (climbing/diving)
    
    [Header("Rotation Limits and Scaling")]
    [SerializeField] private float rollMultiplier = 1f; // How much to rotate relative to aircraft roll
    [SerializeField] private float yawMultiplier = 0.3f; // How much to rotate relative to aircraft yaw
    [SerializeField] private float pitchMultiplier = 0.2f; // How much to rotate relative to aircraft pitch
    [SerializeField] private float maxRotationAngle = 45f; // Maximum rotation angle in degrees
    
    [Header("Smoothing")]
    [SerializeField] private bool useSmoothRotation = true;
    [SerializeField] private float rotationSmoothTime = 0.2f; // How quickly to interpolate rotation
    
    // Private variables for smooth rotation
    private Vector3 targetRotation = Vector3.zero;
    private Vector3 currentRotationVelocity = Vector3.zero;
    private RectTransform rectTransform;
    
    void Start()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        // Get RectTransform component
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("HUDRotationController: No RectTransform found! This script should be on a UI element.", this);
            enabled = false;
            return;
        }
        
        // Try to find aircraft and FlightData if not assigned
        if (aircraft == null || flightData == null)
        {
            FlightData foundFlightData = FindObjectOfType<FlightData>();
            if (foundFlightData != null)
            {
                flightData = foundFlightData;
                aircraft = foundFlightData.transform;
            }
            else
            {
                Debug.LogError("HUDRotationController: No aircraft Transform or FlightData found in scene!", this);
                enabled = false;
                return;
            }
        }
    }

    void Update()
    {
        if (aircraft == null || flightData == null) return;
        
        UpdateRotation();
    }
    
    private void UpdateRotation()
    {
        // Calculate target rotation based on aircraft orientation
        Vector3 newTargetRotation = Vector3.zero;
        
        if (useRollRotation)
        {
            // Use roll from FlightData (banking motion)
            float rollAngle = flightData.roll * rollMultiplier;
            newTargetRotation.z = Mathf.Clamp(rollAngle, -maxRotationAngle, maxRotationAngle);
        }
        
        if (useYawRotation)
        {
            // Use yaw from FlightData (turning motion)
            float yawAngle = flightData.yaw * yawMultiplier;
            newTargetRotation.y = Mathf.Clamp(yawAngle, -maxRotationAngle, maxRotationAngle);
        }
        
        if (usePitchRotation)
        {
            // Use pitch from FlightData (climbing/diving motion)
            float pitchAngle = flightData.pitch * pitchMultiplier;
            newTargetRotation.x = Mathf.Clamp(pitchAngle, -maxRotationAngle, maxRotationAngle);
        }
        
        targetRotation = newTargetRotation;
        
        // Apply rotation (smooth or immediate)
        if (useSmoothRotation)
        {
            // Smooth rotation using SmoothDamp
            Vector3 currentEuler = rectTransform.localEulerAngles;
            
            // Handle angle wrapping for smooth interpolation
            currentEuler = NormalizeAngles(currentEuler);
            targetRotation = NormalizeAngles(targetRotation);
            
            Vector3 smoothedRotation = Vector3.SmoothDamp(
                currentEuler, 
                targetRotation, 
                ref currentRotationVelocity, 
                rotationSmoothTime
            );
            
            rectTransform.localEulerAngles = smoothedRotation;
        }
        else
        {
            // Immediate rotation
            rectTransform.localEulerAngles = targetRotation;
        }
    }
    
    private Vector3 NormalizeAngles(Vector3 angles)
    {
        // Normalize angles to -180 to 180 range for smooth interpolation
        angles.x = NormalizeAngle(angles.x);
        angles.y = NormalizeAngle(angles.y);
        angles.z = NormalizeAngle(angles.z);
        return angles;
    }
    
    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
    
    // Public methods for runtime control
    public void SetRollRotation(bool enabled)
    {
        useRollRotation = enabled;
    }
    
    public void SetYawRotation(bool enabled)
    {
        useYawRotation = enabled;
    }
    
    public void SetPitchRotation(bool enabled)
    {
        usePitchRotation = enabled;
    }
    
    public void SetRotationMultiplier(float roll, float yaw, float pitch)
    {
        rollMultiplier = roll;
        yawMultiplier = yaw;
        pitchMultiplier = pitch;
    }
    
    public void SetSmoothRotation(bool enabled, float smoothTime = 0.2f)
    {
        useSmoothRotation = enabled;
        rotationSmoothTime = smoothTime;
    }
    
    // Reset rotation to zero
    public void ResetRotation()
    {
        targetRotation = Vector3.zero;
        rectTransform.localEulerAngles = Vector3.zero;
        currentRotationVelocity = Vector3.zero;
    }
}
