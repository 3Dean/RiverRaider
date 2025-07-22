using UnityEngine;
using UnityEngine.UI;
using Unity.VectorGraphics;
using TMPro;

/// <summary>
/// Missile UI component that displays ARM-30 missile count and type from FlightData.
/// Designed to work with the arm30 UI element containing icon and two TMP text objects.
/// </summary>
public class MissileUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI currentMissileText; // First TMP text for current count
    [SerializeField] private TextMeshProUGUI maxMissileText; // Second TMP text for max capacity
    [SerializeField] private SVGImage missileIcon; // Optional: missile type SVG icon
    
    [Header("Flight Data Reference")]
    [SerializeField] private FlightData flightData;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lowAmmoColor = Color.yellow;
    [SerializeField] private Color emptyColor = Color.red;
    [SerializeField] private int lowAmmoThreshold = 2; // Show warning when 2 or fewer missiles
    
    [Header("Performance Settings")]
    [SerializeField] private float updateInterval = 0.1f; // Update every 0.1 seconds
    
    // Cached references and state
    private bool isInitialized = false;
    private float lastUpdateTime = 0f;
    private int lastDisplayedMissiles = -1;
    private string lastDisplayedType = "";
    
    void Start()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        Debug.Log("MissileUI: Starting initialization...");
        
        // Find FlightData if not assigned
        if (flightData == null)
        {
            flightData = FindObjectOfType<FlightData>();
            if (flightData == null)
            {
                Debug.LogError("MissileUI: No FlightData found in scene!", this);
                return;
            }
            else
            {
                Debug.Log($"MissileUI: Found FlightData on '{flightData.name}'");
            }
        }
        else
        {
            Debug.Log($"MissileUI: Using assigned FlightData: '{flightData.name}'");
        }
        
        // Find text components if not assigned
        if (currentMissileText == null || maxMissileText == null)
        {
            TextMeshProUGUI[] textComponents = GetComponentsInChildren<TextMeshProUGUI>();
            if (textComponents.Length >= 2)
            {
                if (currentMissileText == null) currentMissileText = textComponents[0];
                if (maxMissileText == null) maxMissileText = textComponents[1];
                Debug.Log("MissileUI: Auto-found TMP text components");
            }
            else
            {
                Debug.LogError("MissileUI: Could not find required TMP text components!", this);
                return;
            }
        }
        else
        {
            Debug.Log("MissileUI: Using assigned TMP text components");
        }
        
        // Find icon if not assigned
        if (missileIcon == null)
        {
            missileIcon = GetComponentInChildren<SVGImage>();
            if (missileIcon != null)
            {
                Debug.Log("MissileUI: Found missile SVG icon");
            }
        }
        
        // Log current missile values
        if (flightData != null)
        {
            Debug.Log($"MissileUI: Current missiles: {flightData.currentMissiles}/{flightData.maxMissiles} Type: {flightData.currentMissileType}");
        }
        
        isInitialized = true;
        
        // Initial update
        UpdateMissileDisplay();
        
        Debug.Log("MissileUI: Initialized successfully");
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Performance optimization: Only update at specified intervals
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateMissileDisplay();
            lastUpdateTime = Time.time;
        }
    }
    
    private void UpdateMissileDisplay()
    {
        if (flightData == null || currentMissileText == null || maxMissileText == null) return;
        
        int currentMissiles = flightData.currentMissiles;
        int maxMissiles = flightData.maxMissiles;
        string missileType = flightData.currentMissileType;
        
        // Performance optimization: Only update if values changed
        if (currentMissiles == lastDisplayedMissiles && missileType == lastDisplayedType) return;
        
        // Debug logging for missile changes
        Debug.Log($"MissileUI: Updating display - Missiles: {currentMissiles}/{maxMissiles}, Type: {missileType}");
        
        // Update text displays
        currentMissileText.text = currentMissiles.ToString();
        maxMissileText.text = "/ " + maxMissiles.ToString();
        
        // Update colors based on missile count and missile type
        UpdateMissileColor(currentMissiles);
        UpdateMissileTypeDisplay(missileType);
        
        lastDisplayedMissiles = currentMissiles;
        lastDisplayedType = missileType;
    }
    
    private void UpdateMissileTypeDisplay(string missileType)
    {
        // Get current missile type data for enhanced display
        var missileTypeData = flightData.GetCurrentMissileTypeData();
        if (missileTypeData != null)
        {
            // Update icon color based on missile type
            if (missileIcon != null)
            {
                missileIcon.color = missileTypeData.uiColor;
                
                // Update SVG icon if available
                if (missileTypeData.missileSVGIcon != null)
                {
                    // Copy the SVG data from the missile type data to the UI icon
                    missileIcon.sprite = missileTypeData.missileSVGIcon.sprite;
                }
                else if (missileTypeData.missileIcon != null)
                {
                    // Fallback to regular sprite if no SVG available
                    missileIcon.sprite = missileTypeData.missileIcon;
                }
            }
        }
    }
    
    private void UpdateMissileColor(int currentMissiles)
    {
        Color targetColor;
        
        if (currentMissiles == 0)
        {
            targetColor = emptyColor;
        }
        else if (currentMissiles <= lowAmmoThreshold)
        {
            targetColor = lowAmmoColor;
        }
        else
        {
            targetColor = normalColor;
        }
        
        // Apply color to text components
        if (currentMissileText != null)
            currentMissileText.color = targetColor;
        
        if (maxMissileText != null)
            maxMissileText.color = targetColor;
        
        // Apply color to icon if available
        if (missileIcon != null)
            missileIcon.color = targetColor;
    }
    
    // Public methods for external control
    public void ForceUpdate()
    {
        lastDisplayedMissiles = -1; // Force update on next call
        lastDisplayedType = "";
        UpdateMissileDisplay();
    }
    
    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.05f, interval); // Minimum 0.05 seconds
    }
    
    public void SetLowAmmoThreshold(int threshold)
    {
        lowAmmoThreshold = Mathf.Max(0, threshold);
    }
    
    // Test methods for debugging
    [ContextMenu("Test Fire Missile")]
    private void TestFireMissile()
    {
        if (flightData != null)
        {
            flightData.ConsumeMissile();
        }
    }
    
    [ContextMenu("Test Resupply Missiles")]
    private void TestResupplyMissiles()
    {
        if (flightData != null)
        {
            flightData.ResupplyMissiles();
        }
    }
    
    [ContextMenu("Test Switch Missile Type")]
    private void TestSwitchMissileType()
    {
        if (flightData != null)
        {
            // Cycle through some test missile types
            string[] testTypes = { "ARM-30", "ARM-60", "ARM-90" };
            int currentIndex = System.Array.IndexOf(testTypes, flightData.currentMissileType);
            int nextIndex = (currentIndex + 1) % testTypes.Length;
            flightData.SetMissileType(testTypes[nextIndex]);
        }
    }
    
    // Properties for external access
    public int CurrentMissiles => flightData != null ? flightData.currentMissiles : 0;
    public int MaxMissiles => flightData != null ? flightData.maxMissiles : 0;
    public string MissileType => flightData != null ? flightData.currentMissileType : "Unknown";
    public bool HasMissiles => flightData != null && flightData.HasMissiles();
}
