using UnityEngine;

/// <summary>
/// Speed-dependent aircraft shake system that adds realistic turbulence effects.
/// Shake intensity increases with airspeed for immersive flight experience.
/// IMPORTANT: This component should be attached to a child object of the aircraft,
/// not the main aircraft transform to avoid interfering with movement systems.
/// </summary>
public class AircraftShakeController : MonoBehaviour
{
    [Header("Flight Data Reference")]
    [SerializeField] private FlightData flightData;
    
    [Header("Shake Target (Optional)")]
    [SerializeField] private Transform shakeTarget; // Leave empty to shake this object, or assign a specific child object
    [Tooltip("If checked, will automatically find the first child with a MeshRenderer to shake")]
    [SerializeField] private bool autoFindShakeTarget = false; // Changed default to false for simplicity
    
    [Header("Shake Settings")]
    [SerializeField] private float minShakeSpeed = 20f; // Speed where shake starts
    [SerializeField] private float maxShakeSpeed = 200f; // Speed where shake is maximum
    [SerializeField] private float maxShakeIntensity = 0.15f; // Maximum shake amplitude
    [SerializeField] private float shakeFrequency = 15f; // Shake frequency (Hz)
    
    [Header("Shake Components")]
    [SerializeField] private bool enablePositionShake = true;
    [SerializeField] private bool enableRotationShake = true;
    [SerializeField] private float rotationShakeMultiplier = 2f; // Rotation shake relative to position
    
    [Header("Smoothing")]
    [SerializeField] private float shakeSmoothTime = 0.1f; // How quickly shake intensity changes
    
    // Internal state
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float currentShakeIntensity = 0f;
    private float shakeIntensityVelocity = 0f;
    private bool isInitialized = false;
    
    // Noise offsets for smooth shake
    private float noiseOffsetX;
    private float noiseOffsetY;
    private float noiseOffsetZ;
    private float rotNoiseOffsetX;
    private float rotNoiseOffsetY;
    private float rotNoiseOffsetZ;

    void Start()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        // Find FlightData if not assigned
        if (flightData == null)
        {
            flightData = FindObjectOfType<FlightData>();
            if (flightData == null)
            {
                Debug.LogError("AircraftShakeController: No FlightData found in scene!", this);
                return;
            }
        }
        
        // Store original transform
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        
        // Initialize noise offsets for smooth randomness
        noiseOffsetX = Random.Range(0f, 100f);
        noiseOffsetY = Random.Range(0f, 100f);
        noiseOffsetZ = Random.Range(0f, 100f);
        rotNoiseOffsetX = Random.Range(0f, 100f);
        rotNoiseOffsetY = Random.Range(0f, 100f);
        rotNoiseOffsetZ = Random.Range(0f, 100f);
        
        isInitialized = true;
        
        Debug.Log("AircraftShakeController: Initialized speed-dependent shake system");
    }

    void Update()
    {
        if (!isInitialized) return;
        
        UpdateShakeIntensity();
        ApplyShake();
    }
    
    private void UpdateShakeIntensity()
    {
        // Calculate target shake intensity based on speed
        float speedRatio = Mathf.InverseLerp(minShakeSpeed, maxShakeSpeed, flightData.airspeed);
        float targetIntensity = speedRatio * maxShakeIntensity;
        
        // Smooth the shake intensity changes
        currentShakeIntensity = Mathf.SmoothDamp(
            currentShakeIntensity, 
            targetIntensity, 
            ref shakeIntensityVelocity, 
            shakeSmoothTime
        );
    }
    
    private void ApplyShake()
    {
        if (currentShakeIntensity <= 0.001f)
        {
            // No shake needed - return to original position/rotation
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            return;
        }
        
        float time = Time.time * shakeFrequency;
        
        // Generate smooth shake using Perlin noise
        Vector3 shakeOffset = Vector3.zero;
        Vector3 rotationShake = Vector3.zero;
        
        if (enablePositionShake)
        {
            shakeOffset = new Vector3(
                (Mathf.PerlinNoise(time + noiseOffsetX, 0f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(time + noiseOffsetY, 0f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(time + noiseOffsetZ, 0f) - 0.5f) * 2f
            ) * currentShakeIntensity;
        }
        
        if (enableRotationShake)
        {
            rotationShake = new Vector3(
                (Mathf.PerlinNoise(time + rotNoiseOffsetX, 0f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(time + rotNoiseOffsetY, 0f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(time + rotNoiseOffsetZ, 0f) - 0.5f) * 2f
            ) * currentShakeIntensity * rotationShakeMultiplier;
        }
        
        // Apply shake to transform
        transform.localPosition = originalPosition + shakeOffset;
        transform.localRotation = originalRotation * Quaternion.Euler(rotationShake);
    }
    
    // Public methods for external control
    public void SetShakeEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            // Reset to original transform when disabled
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
        }
    }
    
    public float GetCurrentShakeIntensity()
    {
        return currentShakeIntensity;
    }
    
    public void SetShakeIntensityMultiplier(float multiplier)
    {
        maxShakeIntensity *= multiplier;
    }
    
    // Test methods for debugging
    [ContextMenu("Test Light Shake")]
    private void TestLightShake()
    {
        if (isInitialized)
        {
            currentShakeIntensity = maxShakeIntensity * 0.3f;
            Debug.Log($"Testing light shake: {currentShakeIntensity:F3} intensity");
        }
    }
    
    [ContextMenu("Test Heavy Shake")]
    private void TestHeavyShake()
    {
        if (isInitialized)
        {
            currentShakeIntensity = maxShakeIntensity;
            Debug.Log($"Testing heavy shake: {currentShakeIntensity:F3} intensity");
        }
    }
    
    [ContextMenu("Reset Shake")]
    private void ResetShake()
    {
        if (isInitialized)
        {
            currentShakeIntensity = 0f;
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            Debug.Log("Shake reset to zero");
        }
    }
    
    [ContextMenu("Show Current Shake Info")]
    private void ShowShakeInfo()
    {
        if (isInitialized)
        {
            float speedRatio = Mathf.InverseLerp(minShakeSpeed, maxShakeSpeed, flightData.airspeed);
            Debug.Log($"Speed: {flightData.airspeed:F0} MPH | Speed Ratio: {speedRatio:F2} | Shake Intensity: {currentShakeIntensity:F3}");
        }
    }
    
    void OnValidate()
    {
        // Ensure valid settings in editor
        minShakeSpeed = Mathf.Max(0f, minShakeSpeed);
        maxShakeSpeed = Mathf.Max(minShakeSpeed + 1f, maxShakeSpeed);
        maxShakeIntensity = Mathf.Max(0f, maxShakeIntensity);
        shakeFrequency = Mathf.Max(0.1f, shakeFrequency);
        shakeSmoothTime = Mathf.Max(0.01f, shakeSmoothTime);
        rotationShakeMultiplier = Mathf.Max(0f, rotationShakeMultiplier);
    }
}
