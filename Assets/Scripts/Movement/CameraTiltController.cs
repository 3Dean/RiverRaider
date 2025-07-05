using UnityEngine;

public class CameraTiltController : MonoBehaviour
{
    [Tooltip("Drag your PlayerShip transform here")]
    public Transform ship;

    [Tooltip("How much to bank at max roll (degrees)")]
    public float maxBankAngle = 10f;

    [Tooltip("How quickly the camera banks (higher = snappier)")]
    public float smoothSpeed = 5f;

    private Quaternion initialRotation;

    void Start()
    {
        // Record starting local rotation of CameraRig
        initialRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        if (ship == null) return;

        Vector3 e = ship.localEulerAngles;
    Debug.Log($"Ship localEulerAngles → X: {e.x:F1}  Z: {e.z:F1}");

        // Get the ship's roll (Z-axis rotation) in [-180,180]
        float shipRoll = NormalizeAngle(ship.localEulerAngles.z);

        // Map ship roll to camera bank, clamped to maxBankAngle
        float targetBank = Mathf.Clamp(shipRoll, -maxBankAngle, +maxBankAngle);

        // Create desired local rotation: bank around Z
        Quaternion desired = initialRotation * Quaternion.Euler(0f, 0f, -targetBank);

        // Smoothly interpolate
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            desired,
            Time.deltaTime * smoothSpeed
        );
    }

    // Converts eulerAngles.z (0→360) to signed angle -180→+180
    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
