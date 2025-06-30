using UnityEngine;

[CreateAssetMenu(menuName = "Flight/FlightData")]
public class FlightData : ScriptableObject
{
    [Header("Speed Settings")]
    public float minSpeed = 10f;
    public float maxSpeed = 200f;
    public float throttleAcceleration = 10f;
    public float boostMultiplier = 2f;

    public float airspeed;
    [HideInInspector] public bool isBoosting;

    [Header("Drag & Throttle Inertia")]
    [Tooltip("Linear drag coefficient (~0.01 – 0.1)")]
    public float dragCoefficient      = 0.02f;
    [Tooltip("How long (sec) it takes to go from 0→full throttle")]
    public float throttleSmoothTime   = 0.3f;

    [Header("Mouse Look & Bank")]
    public float yawSpeed = 60f;    // deg/sec per unit of Mouse X  
    public float pitchSpeed = 45f;    // deg/sec per unit of Mouse Y  
    public float maxBankAngle = 30f;    // maximum roll tilt at full turn  
    public float bankLerpSpeed = 2f;     // how quickly we interpolate toward that tilt 
    
    [Header("Rotational Inertia")]
    [Tooltip("Higher = smoother (more sluggish)")]
    public float yawSmoothTime   = 0.1f;
    public float pitchSmoothTime = 0.1f;
}
