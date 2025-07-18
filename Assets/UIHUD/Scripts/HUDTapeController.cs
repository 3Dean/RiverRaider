using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Unified HUD tape controller for speed and altitude displays.
/// Animates tick marks based on real flight data and updates TextMeshPro labels.
/// </summary>
public class HUDTapeController : MonoBehaviour
{
    [Header("Tape Type")]
    [SerializeField] private TapeType tapeType = TapeType.Speed;
    
    [Header("UI References")]
    [SerializeField] private RectTransform tickContainer;
    [SerializeField] private TMP_Text mainValueLabel;
    [SerializeField] private GameObject tickPrefab;
    
    [Header("Flight Data References")]
    [SerializeField] private FlightData flightData;
    [SerializeField] private Transform aircraftTransform; // For altitude calculation
    
    [Header("Tape Settings")]
    [SerializeField] private float unitsPerTick = 1f; // 1 MPH or 1 foot increments
    [SerializeField] private float tickSpacing = 6f; // Pixels between ticks (smaller for 1 MPH increments)
    [SerializeField] private int visibleTicks = 75; // More ticks needed for 1 MPH increments
    [SerializeField] private float animationSmoothness = 5f; // Animation lerp speed
    [SerializeField] private int majorTickInterval = 5; // Show numbers every 5 units
    
    [Header("Display Settings")]
    [SerializeField] private string valueFormat = "F0";
    [SerializeField] private string speedUnit = "";
    [SerializeField] private string altitudeUnit = "";
    [SerializeField] private float updateInterval = 0.05f; // More frequent updates for smooth animation
    
    [Header("Masking Settings - DISABLED (Using UI Mask instead)")]
    [SerializeField] private float maskingRange = 2.5f; // Hide labels within this range of current value
    [SerializeField] private bool enableConditionalMasking = false; // DISABLED - Using UI Mask component instead
    [SerializeField] private bool enablePositionalMasking = false; // DISABLED - Using UI Mask component instead
    [SerializeField] private float maskingZoneTop = 30f; // Top boundary of masking zone (pixels from center)
    [SerializeField] private float maskingZoneBottom = -30f; // Bottom boundary of masking zone (pixels from center)
    
    // Internal state
    private List<TickMark> tickPool = new List<TickMark>();
    private float currentValue = 0f;
    private float targetValue = 0f;
    private float lastUpdateTime = 0f;
    private bool isInitialized = false;
    
    // Tick mark data structure
    private class TickMark
    {
        public GameObject gameObject;
        public RectTransform rectTransform;
        public TMP_Text label;
        public float value;
        public bool isActive;
        
        public TickMark(GameObject go)
        {
            gameObject = go;
            rectTransform = go.GetComponent<RectTransform>();
            label = go.GetComponentInChildren<TMP_Text>();
            isActive = false;
        }
    }
    
    public enum TapeType
    {
        Speed,
        Altitude
    }
    
    void Start()
    {
        Debug.Log($"HUDTapeController ({tapeType}): Start() called");
        Debug.Log($"HUDTapeController ({tapeType}): GameObject active: {gameObject.activeInHierarchy}, Component enabled: {enabled}");
        InitializeComponents();
        CreateTickPool();
        Debug.Log($"HUDTapeController ({tapeType}): Start() completed, isInitialized: {isInitialized}");
        
        // Force an immediate update to test
        if (isInitialized)
        {
            Debug.Log($"HUDTapeController ({tapeType}): Forcing immediate update test...");
            UpdateTapeDisplay();
        }
    }
    
    private void InitializeComponents()
    {
        Debug.Log($"HUDTapeController ({tapeType}): InitializeComponents() called");
        
        // Find FlightData if not assigned
        if (flightData == null)
        {
            flightData = FindObjectOfType<FlightData>();
            if (flightData == null)
            {
                Debug.LogError($"HUDTapeController ({tapeType}): No FlightData found in scene!", this);
                return;
            }
            Debug.Log($"HUDTapeController ({tapeType}): FlightData found and assigned");
        }
        
        // Find aircraft transform for altitude calculation if needed and not assigned
        if (tapeType == TapeType.Altitude && aircraftTransform == null)
        {
            // Try to find the aircraft by looking for FlightData component
            if (flightData != null)
            {
                aircraftTransform = flightData.transform;
                Debug.Log($"HUDTapeController (Altitude): Using FlightData transform for altitude calculation");
            }
            else
            {
                // Fallback: find any object with UnifiedFlightController
                UnifiedFlightController flightController = FindObjectOfType<UnifiedFlightController>();
                if (flightController != null)
                {
                    aircraftTransform = flightController.transform;
                    Debug.Log($"HUDTapeController (Altitude): Using UnifiedFlightController transform for altitude calculation");
                }
                else
                {
                    Debug.LogError("HUDTapeController (Altitude): No aircraft transform found for altitude calculation!", this);
                    return;
                }
            }
        }
        
        // Validate required components
        if (tickContainer == null)
        {
            Debug.LogError($"HUDTapeController ({tapeType}): Tick container not assigned!", this);
            return;
        }
        Debug.Log($"HUDTapeController ({tapeType}): Tick container validated");
        
        if (tickPrefab == null)
        {
            Debug.LogError($"HUDTapeController ({tapeType}): Tick prefab not assigned!", this);
            return;
        }
        Debug.Log($"HUDTapeController ({tapeType}): Tick prefab validated");
        
        isInitialized = true;
        Debug.Log($"HUDTapeController ({tapeType}): Initialization completed successfully");
    }
    
    private void CreateTickPool()
    {
        if (!isInitialized) return;
        
        Debug.Log($"HUDTapeController ({tapeType}): Creating tick pool with {visibleTicks} ticks");
        
        // Clear existing ticks
        foreach (Transform child in tickContainer)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        tickPool.Clear();
        
        // Create tick pool with simple UI rectangles instead of SVG prefab
        for (int i = 0; i < visibleTicks; i++)
        {
            GameObject tickGO = CreateSimpleTickMark(i);
            if (tickGO == null)
            {
                Debug.LogError($"HUDTapeController ({tapeType}): Failed to create tick mark!");
                return;
            }
            
            TickMark tick = new TickMark(tickGO);
            tickPool.Add(tick);
            
            // Initially hide all ticks
            tickGO.SetActive(false);
            
            Debug.Log($"HUDTapeController ({tapeType}): Created tick {i}, Label component: {(tick.label != null ? "Found" : "Missing")}");
        }
        
        Debug.Log($"HUDTapeController ({tapeType}): Tick pool created successfully with {tickPool.Count} ticks");
    }
    
    private GameObject CreateSimpleTickMark(int index)
    {
        // Create main tick GameObject
        GameObject tickGO = new GameObject($"Tick_{index}");
        tickGO.transform.SetParent(tickContainer, false);
        
        // Add RectTransform
        RectTransform tickRect = tickGO.AddComponent<RectTransform>();
        tickRect.anchorMin = new Vector2(0.5f, 0.5f);
        tickRect.anchorMax = new Vector2(0.5f, 0.5f);
        tickRect.pivot = new Vector2(0.5f, 0.5f);
        
        // All ticks start as minor ticks - will be determined dynamically based on actual value
        tickRect.sizeDelta = new Vector2(15f, 2f); // Default to minor tick size
        
        // Add Image component for the tick line
        UnityEngine.UI.Image tickImage = tickGO.AddComponent<UnityEngine.UI.Image>();
        tickImage.color = new Color(0f, 1f, 0f, 1f); // Bright green color
        
        // Create label GameObject
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(tickGO.transform, false);
        
        // Add RectTransform for label
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        // Add TextMeshPro component for label
        TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
        labelText.text = "0";
        labelText.fontSize = 12f;
        labelText.color = new Color(0f, 1f, 0f, 1f); // Bright green color
        labelText.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        // Configure positioning based on tape type
        if (tapeType == TapeType.Altitude)
        {
            // Altitude: Tick marks on left, numbers on right
            labelRect.pivot = new Vector2(0f, 0.5f); // Left-aligned pivot
            labelRect.anchoredPosition = new Vector2(15f, 0f); // Position to the right of tick
            labelText.horizontalAlignment = HorizontalAlignmentOptions.Left; // Left-align text
        }
        else
        {
            // Speed: Numbers on left, tick marks on right (original layout)
            labelRect.pivot = new Vector2(1f, 0.5f); // Right-aligned pivot
            labelRect.anchoredPosition = new Vector2(-15f, 0f); // Position to the left of tick
            labelText.horizontalAlignment = HorizontalAlignmentOptions.Right; // Right-align text
        }
        
        labelRect.sizeDelta = new Vector2(50f, 20f);
        
        Debug.Log($"HUDTapeController ({tapeType}): Created tick {index} with size {tickRect.sizeDelta}");
        
        return tickGO;
    }
    
    void Update()
    {
        if (!isInitialized) 
        {
            if (Time.frameCount % 300 == 0) // Every 5 seconds
                Debug.LogWarning($"HUDTapeController ({tapeType}): Update() called but not initialized!");
            return;
        }
        
        // Debug Update() calls
        if (Time.frameCount % 300 == 0) // Every 5 seconds
        {
            Debug.Log($"HUDTapeController ({tapeType}): Update() running, Time.time: {Time.time:F2}, lastUpdateTime: {lastUpdateTime:F2}, updateInterval: {updateInterval:F3}");
        }
        
        // Update at specified intervals for performance
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            if (Time.frameCount % 300 == 0) // Debug when UpdateTapeDisplay is called
                Debug.Log($"HUDTapeController ({tapeType}): Calling UpdateTapeDisplay()");
            UpdateTapeDisplay();
            lastUpdateTime = Time.time;
        }
    }
    
    private void UpdateTapeDisplay()
    {
        // Get current value from appropriate source
        targetValue = GetCurrentValue();
        
        // Smooth animation towards target
        currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * animationSmoothness);
        
        // Update main label with rounded value
        UpdateMainLabel();
        
        // Update tick positions and labels
        UpdateTickMarks();
    }
    
    private float GetCurrentValue()
    {
        switch (tapeType)
        {
            case TapeType.Speed:
                return flightData != null ? flightData.airspeed : 0f;
            case TapeType.Altitude:
                // Get altitude directly from aircraft transform Y position
                return aircraftTransform != null ? aircraftTransform.position.y : 0f;
            default:
                return 0f;
        }
    }
    
    private void UpdateMainLabel()
    {
        if (mainValueLabel == null) return;
        
        // Show actual current value (not rounded to increments)
        string unit = tapeType == TapeType.Speed ? speedUnit : altitudeUnit;
        mainValueLabel.text = currentValue.ToString(valueFormat) + unit;
    }
    
    private void UpdateTickMarks()
    {
        Debug.Log($"HUDTapeController ({tapeType}): UpdateTickMarks() called, currentValue: {currentValue:F1}");
        
        if (tickPool.Count == 0)
        {
            Debug.LogWarning($"HUDTapeController ({tapeType}): Tick pool is empty!");
            return;
        }
        
        // Calculate the base value for the center of the tape
        float baseValue = Mathf.Round(currentValue / unitsPerTick) * unitsPerTick;
        
        // Calculate offset for smooth scrolling
        float valueOffset = currentValue - baseValue;
        float pixelOffset = (valueOffset / unitsPerTick) * tickSpacing;
        
        // Update each tick mark
        int centerIndex = visibleTicks / 2;
        int visibleCount = 0;
        int totalProcessed = 0;
        
        // Debug container info
        float containerHeight = tickContainer.rect.height;
        float visibilityThreshold = containerHeight / 2f + tickSpacing;
        
        Debug.Log($"HUDTapeController ({tapeType}): Container height: {containerHeight:F1}, Visibility threshold: {visibilityThreshold:F1}");
        
        for (int i = 0; i < tickPool.Count; i++)
        {
            TickMark tick = tickPool[i];
            totalProcessed++;
            
            // Calculate this tick's value
            int tickOffset = i - centerIndex;
            tick.value = baseValue + (tickOffset * unitsPerTick);
            
            // Calculate position
            float yPosition = (tickOffset * tickSpacing) - pixelOffset;
            
            // Check if tick should be visible
            bool shouldBeVisible = Mathf.Abs(yPosition) <= visibilityThreshold;
            
            // Debug ALL ticks to see what's happening
            Debug.Log($"HUDTapeController ({tapeType}): Tick {i} - Value: {tick.value:F1}, YPos: {yPosition:F1}, ShouldBeVisible: {shouldBeVisible}, Threshold: {visibilityThreshold:F1}, BaseValue: {baseValue:F1}, TickOffset: {tickOffset}, CenterIndex: {centerIndex}");
            
            if (shouldBeVisible)
            {
                // Determine if this is a major tick (multiples of majorTickInterval)
                bool isMajorTick = Mathf.Abs(tick.value % majorTickInterval) < 0.1f;
                
                // Update tick size based on major/minor status
                if (isMajorTick)
                {
                    tick.rectTransform.sizeDelta = new Vector2(19f, 3f); // Major tick: reduced width by 6 pixels
                }
                else
                {
                    tick.rectTransform.sizeDelta = new Vector2(15f, 2f); // Minor tick: shorter and thinner
                }
                
                // Position the tick
                tick.rectTransform.localPosition = new Vector3(0f, yPosition, 0f);
                
                // Update label - show for major ticks with positive values, let UI mask handle clipping
                if (tick.label != null)
                {
                    if (isMajorTick && tick.value >= 0f)
                    {
                        // Always show major tick labels - UI mask will handle clipping
                        tick.label.text = tick.value.ToString(valueFormat);
                        tick.label.gameObject.SetActive(true);
                        Debug.Log($"HUDTapeController ({tapeType}): Showing MAJOR tick label {i} with value {tick.value:F1} (UI mask will clip)");
                    }
                    else
                    {
                        // Hide minor ticks and negative values (but not for masking reasons)
                        tick.label.gameObject.SetActive(false);
                        if (!isMajorTick)
                            Debug.Log($"HUDTapeController ({tapeType}): Hiding minor tick label {i} with value {tick.value:F1}");
                        else if (tick.value < 0f)
                            Debug.Log($"HUDTapeController ({tapeType}): Hiding negative value tick {i} with value {tick.value:F1}");
                    }
                }
                
                // Activate tick
                if (!tick.isActive)
                {
                    tick.gameObject.SetActive(true);
                    tick.isActive = true;
                }
                visibleCount++;
            }
            else
            {
                // Hide tick if outside visible area
                if (tick.isActive)
                {
                    tick.gameObject.SetActive(false);
                    tick.isActive = false;
                }
            }
        }
        
        // Debug info - ALWAYS show
        Debug.Log($"HUDTapeController ({tapeType}): Current: {currentValue:F1}, Base: {baseValue:F1}, Visible: {visibleCount}/{totalProcessed}, Container H: {containerHeight:F1}, Threshold: {visibilityThreshold:F1}, PixelOffset: {pixelOffset:F1}");
        
        // Emergency debug - if no ticks are visible, force show center tick
        if (visibleCount == 0)
        {
            Debug.LogWarning($"HUDTapeController ({tapeType}): NO TICKS VISIBLE! Forcing center tick to show for debugging...");
            if (tickPool.Count > centerIndex)
            {
                TickMark centerTick = tickPool[centerIndex];
                centerTick.gameObject.SetActive(true);
                centerTick.rectTransform.localPosition = Vector3.zero;
                if (centerTick.label != null)
                {
                    centerTick.label.text = baseValue.ToString(valueFormat);
                    centerTick.label.gameObject.SetActive(true);
                }
                centerTick.isActive = true;
                Debug.Log($"HUDTapeController ({tapeType}): Forced center tick active with value {baseValue:F1}");
            }
        }
    }
    
    // Public methods for external control
    public void SetAnimationSmoothness(float smoothness)
    {
        animationSmoothness = Mathf.Max(0.1f, smoothness);
    }
    
    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.01f, interval);
    }
    
    public void ForceUpdate()
    {
        if (isInitialized)
        {
            currentValue = GetCurrentValue();
            UpdateTapeDisplay();
        }
    }
    
    // Debug information
    void OnValidate()
    {
        if (Application.isPlaying && isInitialized)
        {
            CreateTickPool();
        }
    }
}
