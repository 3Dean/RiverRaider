// Assets/Scripts/Input/FlightInputController.cs
using System;
using UnityEngine;

public class FlightInputController : MonoBehaviour {
    public static event Action<float> OnThrottleChanged;
    public static event Action<Vector2> OnLookChanged;
    public static event Action OnBoostActivated;
    public static event Action OnBoostDeactivated;
    public static event Action OnFireWeapon;
    public static event Action<float> OnYawChanged;
public static event Action<float> OnPitchChanged;


    void Update() {
        // Throttle (W/S)
        float throttle = Input.GetKey(KeyCode.W)? 1f : Input.GetKey(KeyCode.S)? -1f : 0f;
        OnThrottleChanged?.Invoke(throttle);

        // Mouse look
        /*         Vector2 look = new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
                OnLookChanged?.Invoke(look); */
        
         Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible   = false;

    // read raw mouse axes
    float mx = Input.GetAxis("Mouse X");
    float my = Input.GetAxis("Mouse Y");

    OnYawChanged?.Invoke(mx);
    OnPitchChanged?.Invoke(-my);

        // Boost
        if (Input.GetKeyDown(KeyCode.LeftShift)) OnBoostActivated?.Invoke();
        if (Input.GetKeyUp(KeyCode.LeftShift))   OnBoostDeactivated?.Invoke();

        // Fire
        if (Input.GetKey(KeyCode.Space)) OnFireWeapon?.Invoke();
    }
}
