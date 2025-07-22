using UnityEngine;

/// <summary>
/// Enhanced weapon input controller that handles missile type switching and firing
/// </summary>
public class EnhancedWeaponInputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FlightData flightData;
    [SerializeField] private MissileController missileController;
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode fireMissileKey = KeyCode.Space;
    [SerializeField] private KeyCode switchMissileTypeKey = KeyCode.Tab;
    [SerializeField] private KeyCode previousMissileTypeKey = KeyCode.LeftShift;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip missileTypeSwitchSound;
    
    void Start()
    {
        // Find references if not assigned
        if (flightData == null)
        {
            flightData = FindObjectOfType<FlightData>();
        }
        
        if (missileController == null)
        {
            missileController = FindObjectOfType<MissileController>();
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        Debug.Log("EnhancedWeaponInputController initialized");
    }
    
    void Update()
    {
        HandleMissileInput();
        HandleMissileTypeSwitching();
    }
    
    private void HandleMissileInput()
    {
        // Fire missile
        if (Input.GetKeyDown(fireMissileKey))
        {
            if (missileController != null)
            {
                bool fired = missileController.FireMissile();
                if (!fired)
                {
                    Debug.Log("Cannot fire missile - insufficient ammunition or cooldown active");
                }
            }
            else if (flightData != null)
            {
                // Fallback to FlightData missile firing
                flightData.ConsumeMissile();
            }
        }
    }
    
    private void HandleMissileTypeSwitching()
    {
        if (flightData == null) return;
        
        // Switch to next missile type
        if (Input.GetKeyDown(switchMissileTypeKey))
        {
            bool switched = flightData.SwitchToNextMissileType();
            if (switched)
            {
                PlayMissileTypeSwitchSound();
                Debug.Log($"Switched to missile type: {flightData.currentMissileType} ({flightData.currentMissiles}/{flightData.maxMissiles})");
            }
        }
        
        // Switch to previous missile type (with modifier key)
        if (Input.GetKey(previousMissileTypeKey) && Input.GetKeyDown(switchMissileTypeKey))
        {
            bool switched = flightData.SwitchToPreviousMissileType();
            if (switched)
            {
                PlayMissileTypeSwitchSound();
                Debug.Log($"Switched to missile type: {flightData.currentMissileType} ({flightData.currentMissiles}/{flightData.maxMissiles})");
            }
        }
    }
    
    private void PlayMissileTypeSwitchSound()
    {
        if (audioSource != null && missileTypeSwitchSound != null)
        {
            audioSource.PlayOneShot(missileTypeSwitchSound);
        }
    }
    
    // Public methods for external control (e.g., UI buttons)
    public void FireMissile()
    {
        if (missileController != null)
        {
            missileController.FireMissile();
        }
        else if (flightData != null)
        {
            flightData.ConsumeMissile();
        }
    }
    
    public void SwitchToNextMissileType()
    {
        if (flightData != null)
        {
            bool switched = flightData.SwitchToNextMissileType();
            if (switched)
            {
                PlayMissileTypeSwitchSound();
            }
        }
    }
    
    public void SwitchToPreviousMissileType()
    {
        if (flightData != null)
        {
            bool switched = flightData.SwitchToPreviousMissileType();
            if (switched)
            {
                PlayMissileTypeSwitchSound();
            }
        }
    }
    
    public void ResupplyMissiles()
    {
        if (flightData != null)
        {
            flightData.ResupplyMissiles();
        }
    }
    
    public void ResupplyAllMissiles()
    {
        if (flightData != null)
        {
            flightData.ResupplyAllMissiles();
        }
    }
    
    // Properties for external access
    public string CurrentMissileType => flightData != null ? flightData.currentMissileType : "Unknown";
    public int CurrentMissileCount => flightData != null ? flightData.currentMissiles : 0;
    public int MaxMissileCount => flightData != null ? flightData.maxMissiles : 0;
    public bool HasMissiles => flightData != null && flightData.HasMissiles();
}
