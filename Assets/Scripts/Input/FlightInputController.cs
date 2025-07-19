using UnityEngine;

/// <summary>
/// FlightInputController - Centralized input handling for the aircraft
/// Part of the modular system refactor
/// </summary>
public class FlightInputController : MonoBehaviour
{
    [Header("Input Configuration")]
    public KeyCode throttleUpKey = KeyCode.W;
    public KeyCode throttleDownKey = KeyCode.S;
    public KeyCode fireKey = KeyCode.Space;
    
    [Header("Input Sensitivity")]
    public float throttleInputSensitivity = 1f;
    public float mouseInputSensitivity = 2f;
    
    [Header("Debug")]
    public bool showInputDebug = false;

    // Input state
    private float currentThrottleInput = 0f;
    private float currentMouseX = 0f;
    private float currentMouseY = 0f;
    private bool firePressed = false;

    // Events for other systems to listen to (matching existing RailMovementController interface)
    public static System.Action<float> OnThrottleChanged;
    public static System.Action OnBoostActivated;
    public static System.Action OnBoostDeactivated;
    public static System.Action<float, float> OnMouseInput;
    public static System.Action OnFirePressed;
    public static System.Action OnFireReleased;

    // References to systems that need input
    private RailMovementController movementController;

    void Start()
    {
        // DISABLED - UnifiedFlightController is now handling all input
        Debug.LogWarning("FlightInputController: DISABLED - UnifiedFlightController is handling input");
        this.enabled = false;
        return;
        
        // Find the movement controller
        movementController = FindObjectOfType<RailMovementController>();
        
        if (movementController == null)
        {
            Debug.LogWarning("FlightInputController: No RailMovementController found in scene");
        }

        if (showInputDebug)
            Debug.Log("FlightInputController: Initialized and ready for input");
    }

    void Update()
    {
        ProcessKeyboardInput();
        ProcessMouseInput();
        ProcessFireInput();
        
        // Send input to systems
        DistributeInput();
    }

    /// <summary>
    /// Process keyboard input for throttle control
    /// </summary>
    void ProcessKeyboardInput()
    {
        float throttleInput = 0f;

        // Throttle up
        if (Input.GetKey(throttleUpKey))
        {
            throttleInput = 1f;
            if (showInputDebug)
                Debug.Log("FlightInputController: Throttle UP input detected");
        }
        // Throttle down
        else if (Input.GetKey(throttleDownKey))
        {
            throttleInput = -1f;
            if (showInputDebug)
                Debug.Log("FlightInputController: Throttle DOWN input detected");
        }

        currentThrottleInput = throttleInput * throttleInputSensitivity;
    }

    /// <summary>
    /// Process mouse input for aircraft control
    /// </summary>
    void ProcessMouseInput()
    {
        currentMouseX = Input.GetAxis("Mouse X") * mouseInputSensitivity;
        currentMouseY = Input.GetAxis("Mouse Y") * mouseInputSensitivity;

        if (showInputDebug && (Mathf.Abs(currentMouseX) > 0.1f || Mathf.Abs(currentMouseY) > 0.1f))
        {
            Debug.Log($"FlightInputController: Mouse input - X: {currentMouseX:F2}, Y: {currentMouseY:F2}");
        }
    }

    /// <summary>
    /// Process fire input
    /// </summary>
    void ProcessFireInput()
    {
        if (Input.GetKeyDown(fireKey))
        {
            firePressed = true;
            if (showInputDebug)
                Debug.Log("FlightInputController: Fire button pressed");
        }
        else if (Input.GetKeyUp(fireKey))
        {
            firePressed = false;
            if (showInputDebug)
                Debug.Log("FlightInputController: Fire button released");
        }
    }

    /// <summary>
    /// Distribute input to all listening systems
    /// </summary>
    void DistributeInput()
    {
        // Send throttle input using the correct event name
        OnThrottleChanged?.Invoke(currentThrottleInput);

        // Send mouse input
        if (Mathf.Abs(currentMouseX) > 0.01f || Mathf.Abs(currentMouseY) > 0.01f)
        {
            OnMouseInput?.Invoke(currentMouseX, currentMouseY);
        }

        // Send fire input
        if (firePressed)
        {
            OnFirePressed?.Invoke();
        }
    }

    /// <summary>
    /// Get current input state for debugging
    /// </summary>
    public InputState GetCurrentInputState()
    {
        return new InputState
        {
            throttleInput = currentThrottleInput,
            mouseX = currentMouseX,
            mouseY = currentMouseY,
            firePressed = firePressed
        };
    }

    /// <summary>
    /// Enable/disable input processing
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        this.enabled = enabled;
        
        if (!enabled)
        {
            // Reset input state when disabled
            currentThrottleInput = 0f;
            currentMouseX = 0f;
            currentMouseY = 0f;
            firePressed = false;
        }

        if (showInputDebug)
            Debug.Log($"FlightInputController: Input {(enabled ? "enabled" : "disabled")}");
    }

    void OnGUI()
    {
        if (!showInputDebug) return;

        // Display input debug info
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label("=== INPUT DEBUG ===");
        GUILayout.Label($"Throttle: {currentThrottleInput:F2}");
        GUILayout.Label($"Mouse X: {currentMouseX:F2}");
        GUILayout.Label($"Mouse Y: {currentMouseY:F2}");
        GUILayout.Label($"Fire: {firePressed}");
        GUILayout.Label($"Keys: {throttleUpKey}/{throttleDownKey}/{fireKey}");
        GUILayout.EndArea();
    }
}

/// <summary>
/// Input state structure for debugging and external access
/// </summary>
[System.Serializable]
public struct InputState
{
    public float throttleInput;
    public float mouseX;
    public float mouseY;
    public bool firePressed;
}
