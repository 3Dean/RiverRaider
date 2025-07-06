using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central weapon management system for the player ship
/// Handles multiple weapon types: machinegun (continuous), missiles (single-shot)
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Systems")]
    [SerializeField] private MachinegunController machinegunController;
    [SerializeField] private MissileController missileController;
    
    [Header("Input Settings")]
    [SerializeField] private bool enableMouseInput = true;
    [SerializeField] private bool enableKeyboardInput = true;
    
    // Weapon states
    private bool isMachinegunFiring = false;
    private bool wasLeftMousePressed = false;
    private bool wasSpacePressed = false;

    void Start()
    {
        // Auto-find weapon controllers if not assigned
        if (machinegunController == null)
            machinegunController = GetComponent<MachinegunController>();
        
        if (missileController == null)
            missileController = GetComponent<MissileController>();
    }

    void Update()
    {
        HandleMachinegunInput();
        HandleMissileInput();
    }

    private void HandleMachinegunInput()
    {
        if (!enableMouseInput) return;
        
        bool leftMousePressed = Input.GetMouseButton(0); // Left mouse = continuous machinegun

        // Handle machinegun firing start/stop
        if (leftMousePressed && !isMachinegunFiring)
        {
            StartMachinegunFiring();
        }
        else if (!leftMousePressed && isMachinegunFiring)
        {
            StopMachinegunFiring();
        }

        // Continuous firing while held
        if (isMachinegunFiring && machinegunController != null)
        {
            machinegunController.TryFire();
        }

        wasLeftMousePressed = leftMousePressed;
    }

    private void HandleMissileInput()
    {
        bool spacePressed = enableKeyboardInput && Input.GetKeyDown(KeyCode.Space); // Space = single missile

        if (spacePressed && missileController != null)
        {
            missileController.FireMissile();
        }
    }

    private void StartMachinegunFiring()
    {
        isMachinegunFiring = true;
        if (machinegunController != null)
        {
            machinegunController.StartFiring();
        }
    }

    private void StopMachinegunFiring()
    {
        isMachinegunFiring = false;
        if (machinegunController != null)
        {
            machinegunController.StopFiring();
        }
    }

    // Public methods for external control
    public void EnableMouseInput(bool enable) => enableMouseInput = enable;
    public void EnableKeyboardInput(bool enable) => enableKeyboardInput = enable;
    
    // Properties
    public bool IsMachinegunFiring => isMachinegunFiring;
    public bool HasMachinegun => machinegunController != null;
    public bool HasMissiles => missileController != null;
}
