using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController : MonoBehaviour
{
    public float pitchSpeed = 90f;
    public float yawSpeed = 90f;
    public float rollSpeed = 100f;
    public float moveSpeed = 50f;
    public float speedChangeRate = 10f;

    private float currentSpeed;

    void Start()
    {
        currentSpeed = moveSpeed;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Speed control
        if (Input.GetKey(KeyCode.W)) currentSpeed += speedChangeRate * Time.deltaTime;
        if (Input.GetKey(KeyCode.S)) currentSpeed -= speedChangeRate * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 10f, 200f);

        // Mouse look control
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        transform.Rotate(mouseY * pitchSpeed * Time.deltaTime, mouseX * yawSpeed * Time.deltaTime, 0f, Space.Self);

        // Bank (roll left/right when pressing A/D)
        float rollInput = 0f;
        if (Input.GetKey(KeyCode.A)) rollInput = 1f;
        if (Input.GetKey(KeyCode.D)) rollInput = -1f;
        transform.Rotate(0f, 0f, rollInput * rollSpeed * Time.deltaTime, Space.Self);

        // Snap roll (X/Y)
        if (Input.GetKeyDown(KeyCode.X))
            StartCoroutine(SnapRoll(Vector3.forward * -90f)); // Left roll
        if (Input.GetKeyDown(KeyCode.Y))
            StartCoroutine(SnapRoll(Vector3.forward * 90f)); // Right roll

        // Move forward
        transform.position += transform.forward * currentSpeed * Time.deltaTime;
    }

    System.Collections.IEnumerator SnapRoll(Vector3 rollAmount)
    {
        Quaternion start = transform.rotation;
        Quaternion end = start * Quaternion.Euler(rollAmount);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            transform.rotation = Quaternion.Slerp(start, end, t);
            yield return null;
        }
    }
}
