using System;
using UnityEngine;

/// <summary>
/// Centralized input controller for flight mechanics
/// Uses events to decouple input from game logic
/// </summary>
public class FlightInputController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private bool lockCursor = true;
    
    // Core flight events
    public static event Action<float> OnThrottleChanged;
    public static event Action<Vector2> OnLookChanged;
    public static event Action OnBoostActivated;
    public static event Action OnBoostDeactivated;
    public static event Action OnFireWeapon;
    
    // Individual axis events (for systems that need specific axes)
    public static event Action<float> OnYawChanged;
    public static event Action<float> OnPitchChanged;

    private float lastThrottle = 0f;
    private bool wasBoostPressed = false;
    private bool wasFirePressed = false;

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        HandleThrottleInput();
        HandleMouseInput();
        HandleBoostInput();
        HandleFireInput();
    }

    private void HandleThrottleInput()
    {
        float throttle = 0f;
        if (Input.GetKey(KeyCode.W)) throttle = 1f;
        else if (Input.GetKey(KeyCode.S)) throttle = -1f;

        // Only invoke if throttle changed to reduce event spam
        if (Mathf.Abs(throttle - lastThrottle) > 0.01f)
        {
            OnThrottleChanged?.Invoke(throttle);
            lastThrottle = throttle;
        }
    }

    private void HandleMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Combined look vector
        Vector2 lookInput = new Vector2(mouseX, -mouseY);
        OnLookChanged?.Invoke(lookInput);

        // Individual axes for systems that need them
        OnYawChanged?.Invoke(mouseX);
        OnPitchChanged?.Invoke(-mouseY);
    }

    private void HandleBoostInput()
    {
        bool isBoostPressed = Input.GetKey(KeyCode.LeftShift);
        
        if (isBoostPressed && !wasBoostPressed)
            OnBoostActivated?.Invoke();
        else if (!isBoostPressed && wasBoostPressed)
            OnBoostDeactivated?.Invoke();
            
        wasBoostPressed = isBoostPressed;
    }

    private void HandleFireInput()
    {
        bool isFirePressed = Input.GetKey(KeyCode.Space);
        
        if (isFirePressed && !wasFirePressed)
            OnFireWeapon?.Invoke();
            
        wasFirePressed = isFirePressed;
    }

    void OnDestroy()
    {
        // Clear all events to prevent memory leaks
        OnThrottleChanged = null;
        OnLookChanged = null;
        OnBoostActivated = null;
        OnBoostDeactivated = null;
        OnFireWeapon = null;
        OnYawChanged = null;
        OnPitchChanged = null;
    }
}
