using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClimbDiveIndicator : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI indicatorText;
    [SerializeField] private Text legacyIndicatorText;
    
    [Header("Flight Data Reference")]
    [SerializeField] private FlightData flightData;
    [SerializeField] private Transform aircraft;
    
    [Header("Display Settings")]
    [SerializeField] private Color climbColor = Color.red;
    [SerializeField] private Color diveColor = Color.green;
    [SerializeField] private Color levelColor = Color.white;
    [SerializeField] private float minAngleToShow = 5f; // Minimum angle to show indicator
    
    void Start()
    {
        // Auto-find components if not assigned
        if (flightData == null)
            flightData = FindObjectOfType<FlightData>();
            
        if (aircraft == null && flightData != null)
            aircraft = flightData.transform;
            
        if (indicatorText == null && legacyIndicatorText == null)
        {
            indicatorText = GetComponent<TextMeshProUGUI>();
            if (indicatorText == null)
                legacyIndicatorText = GetComponent<Text>();
        }
    }
    
    void Update()
    {
        if (aircraft == null) return;
        
        // Calculate slope and angle
        float slope = Vector3.Dot(aircraft.forward, Vector3.up);
        float angle = Mathf.Asin(Mathf.Clamp(slope, -1f, 1f)) * Mathf.Rad2Deg;
        
        string displayText = "";
        Color textColor = levelColor;
        
        if (Mathf.Abs(angle) > minAngleToShow)
        {
            if (angle > 0)
            {
                displayText = $"CLIMBING {angle:F0}°";
                textColor = climbColor;
            }
            else
            {
                displayText = $"DIVING {Mathf.Abs(angle):F0}°";
                textColor = diveColor;
            }
        }
        else
        {
            displayText = "LEVEL FLIGHT";
            textColor = levelColor;
        }
        
        // Update text component
        if (indicatorText != null)
        {
            indicatorText.text = displayText;
            indicatorText.color = textColor;
        }
        else if (legacyIndicatorText != null)
        {
            legacyIndicatorText.text = displayText;
            legacyIndicatorText.color = textColor;
        }
    }
}
