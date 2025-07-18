using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enhanced fuel system that integrates with FlightData for speed-dependent fuel consumption.
/// Fuel consumption now increases with speed and boosting for realistic flight mechanics.
/// </summary>
public class PlayerShipFuel : MonoBehaviour
{
    [Header("Flight Data Reference")]
    [SerializeField] private FlightData flightData;
    
    [Header("UI References")]
    [SerializeField] private Slider fuelSlider;
    
    [Header("Refueling Settings")]
    [SerializeField] private float refuelRate = 50f; // Fuel per second when refueling
    
    // State tracking
    private bool isRefueling = false;
    private bool isInitialized = false;
    private bool wasOutOfFuel = false;
    private float enginePowerFadeTimer = 0f;
    
    // Fuel depletion states
    public enum FuelState
    {
        Normal,
        Low,
        Depleted,
        Refueling
    }
    
    private FuelState currentFuelState = FuelState.Normal;

    void Start()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        // Find FlightData if not assigned
        if (flightData == null)
        {
            flightData = FindObjectOfType<FlightData>();
            if (flightData == null)
            {
                Debug.LogError("PlayerShipFuel: No FlightData found in scene!", this);
                return;
            }
        }
        
        // Find fuel slider if not assigned
        if (fuelSlider == null)
        {
            fuelSlider = GetComponent<Slider>();
            if (fuelSlider == null)
            {
                Debug.LogError("PlayerShipFuel: No Slider component found!", this);
                return;
            }
        }
        
        isInitialized = true;
        
        // Initial UI update
        UpdateFuelUI();
        
        Debug.Log("PlayerShipFuel: Initialized with speed-dependent fuel consumption");
    }

    void Update()
    {
        if (!isInitialized) return;
        
        // Update fuel state
        UpdateFuelState();
        
        // Handle fuel consumption and refueling
        if (isRefueling)
        {
            HandleRefueling();
        }
        else if (flightData.HasFuel())
        {
            // Use FlightData's speed-dependent fuel consumption
            flightData.ConsumeFuel(Time.deltaTime);
        }
        
        // Handle fuel depletion mechanics
        HandleFuelDepletion();
        
        // Update UI
        UpdateFuelUI();
    }
    
    private void HandleRefueling()
    {
        if (flightData.currentFuel < flightData.maxFuel)
        {
            flightData.AddFuel(refuelRate * Time.deltaTime);
        }
    }
    
    private void UpdateFuelState()
    {
        float fuelPercentage = flightData.GetFuelPercentage();
        
        if (isRefueling)
        {
            currentFuelState = FuelState.Refueling;
        }
        else if (fuelPercentage <= 0f)
        {
            currentFuelState = FuelState.Depleted;
        }
        else if (fuelPercentage <= 0.2f) // Low fuel warning at 20%
        {
            currentFuelState = FuelState.Low;
        }
        else
        {
            currentFuelState = FuelState.Normal;
        }
    }
    
    private void HandleFuelDepletion()
    {
        bool isOutOfFuel = !flightData.HasFuel();
        
        if (isOutOfFuel)
        {
            // Handle transition to out of fuel state
            if (!wasOutOfFuel)
            {
                wasOutOfFuel = true;
                enginePowerFadeTimer = 0f;
                Debug.LogWarning("FUEL DEPLETED! Engine power failing...");
            }
            
            // Gradually reduce engine power over time
            enginePowerFadeTimer += Time.deltaTime;
            float fadeProgress = enginePowerFadeTimer / flightData.enginePowerFadeTime;
            float enginePower = Mathf.Lerp(1f, 0f, fadeProgress);
            
            flightData.SetEnginePower(enginePower);
            
            // Log warnings periodically
            if (Time.frameCount % 300 == 0) // Every ~5 seconds
            {
                if (flightData.isEngineRunning)
                {
                    Debug.LogWarning($"ENGINE FAILING! Power at {(enginePower * 100f):F0}% - Find fuel barge!");
                }
                else
                {
                    Debug.LogWarning("ENGINE DEAD! Gliding without power - Find fuel barge!");
                }
            }
        }
        else
        {
            // Handle recovery from out of fuel state
            if (wasOutOfFuel)
            {
                wasOutOfFuel = false;
                enginePowerFadeTimer = 0f;
                flightData.RestartEngine();
            }
        }
    }

    // Note: Boost functionality removed - W key acceleration now directly affects speed, which affects fuel consumption

    void UpdateFuelUI()
    {
        if (fuelSlider != null && isInitialized)
        {
            fuelSlider.value = flightData.GetFuelPercentage();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isInitialized) return;
        
        if (other.CompareTag("FuelBarge"))
        {
            isRefueling = true;
            Debug.Log("Refueling started at fuel barge");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!isInitialized) return;
        
        if (other.CompareTag("FuelBarge"))
        {
            isRefueling = false;
            Debug.Log("Refueling stopped - left fuel barge");
        }
    }
    
    // Public methods for external systems
    public bool HasFuel()
    {
        return isInitialized && flightData.HasFuel();
    }
    
    public float GetFuelPercentage()
    {
        return isInitialized ? flightData.GetFuelPercentage() : 0f;
    }
    
    public float GetCurrentFuelConsumptionRate()
    {
        return isInitialized ? flightData.GetCurrentFuelConsumptionRate() : 0f;
    }
    
    public bool IsRefueling()
    {
        return isRefueling;
    }
    
    // Test methods for debugging
    [ContextMenu("Test Consume Fuel (20)")]
    private void TestConsumeFuel()
    {
        if (isInitialized)
        {
            flightData.currentFuel = Mathf.Clamp(flightData.currentFuel - 20f, 0f, flightData.maxFuel);
        }
    }
    
    [ContextMenu("Test Refuel to Full")]
    private void TestRefuel()
    {
        if (isInitialized)
        {
            flightData.RefuelToFull();
        }
    }
    
    [ContextMenu("Show Fuel Consumption Rate")]
    private void ShowFuelConsumptionRate()
    {
        if (isInitialized)
        {
            float rate = flightData.GetCurrentFuelConsumptionRate();
            Debug.Log($"Current fuel consumption: {rate:F2} fuel/sec at {flightData.airspeed:F0} MPH");
        }
    }
}
