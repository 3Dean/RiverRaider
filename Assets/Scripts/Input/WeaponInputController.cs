using System;
using UnityEngine;

/// <summary>
/// Dedicated weapon input controller for machinegun and other weapon systems
/// Handles left mouse button for continuous firing
/// </summary>
public class WeaponInputController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private bool enableMouseInput = true;
    [SerializeField] private bool enableKeyboardInput = true;
    
    // Weapon firing events
    public static event Action OnMachinegunFireStart;
    public static event Action OnMachinegunFireEnd;
    public static event Action OnMachinegunTryFire; // Event to try firing (respects fire rate)
    
    // Secondary weapon events (for future expansion)
    public static event Action OnSecondaryWeaponFire;
    public static event Action OnWeaponSwitch;

    private bool isMachinegunFiring = false;
    private bool wasLeftMousePressed = false;
    private bool wasSpacePressed = false;
    private float lastFireEventTime = 0f;
    private const float fireEventInterval = 0.02f; // 50 times per second max (let weapon controllers handle their own rate limiting)

    void Update()
    {
        HandleMachinegunInput();
        HandleSecondaryWeaponInput();
    }

    private void HandleMachinegunInput()
    {
        bool leftMousePressed = enableMouseInput && Input.GetMouseButton(0); // Left mouse button
        bool spacePressed = enableKeyboardInput && Input.GetKey(KeyCode.Space); // Space key as backup
        
        bool shouldFire = leftMousePressed || spacePressed;

        // Handle firing start
        if (shouldFire && !isMachinegunFiring)
        {
            isMachinegunFiring = true;
            OnMachinegunFireStart?.Invoke();
        }
        // Handle firing end
        else if (!shouldFire && isMachinegunFiring)
        {
            isMachinegunFiring = false;
            OnMachinegunFireEnd?.Invoke();
        }

        // Handle continuous firing - but don't spam every frame
        // Limit fire event calls to reduce load on weapon controllers
        if (isMachinegunFiring && Time.time >= lastFireEventTime + fireEventInterval)
        {
            lastFireEventTime = Time.time;
            OnMachinegunTryFire?.Invoke();
        }

        // Update previous states
        wasLeftMousePressed = leftMousePressed;
        wasSpacePressed = spacePressed;
    }

    private void HandleSecondaryWeaponInput()
    {
        // Right mouse button for secondary weapons (rockets, missiles, etc.)
        if (Input.GetMouseButtonDown(1))
        {
            OnSecondaryWeaponFire?.Invoke();
        }

        // Weapon switching with mouse wheel or number keys
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnWeaponSwitch?.Invoke();
        }
    }

    // Public methods for external control
    public void EnableMouseInput(bool enable)
    {
        enableMouseInput = enable;
    }

    public void EnableKeyboardInput(bool enable)
    {
        enableKeyboardInput = enable;
    }

    public bool IsMachinegunFiring => isMachinegunFiring;

    void OnDestroy()
    {
        // Clear all events to prevent memory leaks
        OnMachinegunFireStart = null;
        OnMachinegunFireEnd = null;
        OnMachinegunTryFire = null;
        OnSecondaryWeaponFire = null;
        OnWeaponSwitch = null;
    }

    void OnValidate()
    {
        // Ensure at least one input method is enabled
        if (!enableMouseInput && !enableKeyboardInput)
        {
            enableKeyboardInput = true;
            Debug.LogWarning("WeaponInputController: At least one input method must be enabled. Re-enabled keyboard input.");
        }
    }
}
