using System.Collections;
using UnityEngine;

/// <summary>
/// Legacy flight controller - DEPRECATED
/// Use RailMovementController with FlightData instead for better performance and modularity
/// This script is kept for backward compatibility but should be replaced
/// </summary>
[System.Obsolete("Use RailMovementController with FlightData instead")]
public class PlayerShipController : MonoBehaviour
{
    [Header("DEPRECATED - Use RailMovementController instead")]
    [SerializeField] private bool enableLegacyController = false;
    
    [Header("Movement Settings")]
    public float pitchSpeed = 90f;
    public float yawSpeed = 90f;
    public float rollSpeed = 100f;
    public float moveSpeed = 50f;
    public float speedChangeRate = 10f;

    private float currentSpeed;
    private bool isPerformingSnapRoll = false;

    void Start()
    {
        if (!enableLegacyController)
        {
            Debug.LogWarning("PlayerShipController is deprecated. Use RailMovementController with FlightData instead.", this);
            enabled = false;
            return;
        }
        
        currentSpeed = moveSpeed;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!enableLegacyController) return;
        
        HandleSpeedControl();
        HandleMouseLook();
        HandleRollInput();
        HandleSnapRoll();
        MoveForward();
    }

    private void HandleSpeedControl()
    {
        float speedInput = 0f;
        if (Input.GetKey(KeyCode.W)) speedInput = 1f;
        else if (Input.GetKey(KeyCode.S)) speedInput = -1f;
        
        currentSpeed += speedInput * speedChangeRate * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 10f, 200f);
    }

    private void HandleMouseLook()
    {
        if (isPerformingSnapRoll) return;
        
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        transform.Rotate(
            mouseY * pitchSpeed * Time.deltaTime, 
            mouseX * yawSpeed * Time.deltaTime, 
            0f, 
            Space.Self
        );
    }

    private void HandleRollInput()
    {
        if (isPerformingSnapRoll) return;
        
        float rollInput = 0f;
        if (Input.GetKey(KeyCode.A)) rollInput = 1f;
        else if (Input.GetKey(KeyCode.D)) rollInput = -1f;
        
        if (rollInput != 0f)
        {
            transform.Rotate(0f, 0f, rollInput * rollSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void HandleSnapRoll()
    {
        if (isPerformingSnapRoll) return;
        
        if (Input.GetKeyDown(KeyCode.X))
            StartCoroutine(SnapRoll(Vector3.forward * -90f));
        else if (Input.GetKeyDown(KeyCode.Y))
            StartCoroutine(SnapRoll(Vector3.forward * 90f));
    }

    private void MoveForward()
    {
        transform.position += transform.forward * currentSpeed * Time.deltaTime;
    }

    private IEnumerator SnapRoll(Vector3 rollAmount)
    {
        isPerformingSnapRoll = true;
        
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(rollAmount);
        
        float elapsedTime = 0f;
        const float rollDuration = 0.5f;
        
        while (elapsedTime < rollDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / rollDuration;
            t = Mathf.SmoothStep(0f, 1f, t); // Smooth easing
            
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }
        
        transform.rotation = endRotation;
        isPerformingSnapRoll = false;
    }
}
