using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Missile controller for single-shot projectile weapons
/// Handles different missile types and ammunition management with velocity inheritance
/// </summary>
public class MissileController : MonoBehaviour
{
    [Header("Missile Settings")]
    [SerializeField] private MissileType currentMissileType = MissileType.Standard;
    [SerializeField] private Transform[] missileFirePoints;
    [SerializeField] private FlightData flightData; // Reference to centralized data
    
    [Header("Missile Types")]
    [SerializeField] private MissileData[] missileTypes;
    
    [Header("Audio")]
    [SerializeField] private AudioSource missileAudioSource;
    [SerializeField] private AudioClip missileLaunchSound;
    [SerializeField] private AudioClip missileEmptySound;
    
    [Header("Effects")]
    [SerializeField] private GameObject missileLaunchEffect;
    [SerializeField] private float launchEffectDuration = 1f;

    [Header("Velocity Inheritance")]
    [SerializeField] private float velocityInheritanceMultiplier = 1f; // How much player velocity to inherit (0-1)
    [SerializeField] private bool enableVelocityInheritance = true;

    private int currentFirePointIndex = 0;
    private float lastFireTime = 0f;

    public enum MissileType
    {
        Standard,
        HeavyDamage,
        FastSpeed,
        Homing,
        Cluster
    }

    [System.Serializable]
    public class MissileData
    {
        public MissileType type;
        public string name;
        public GameObject missilePrefab;
        public float damage = 100f;
        public float speed = 40f;
        public float cooldown = 1f;
        public int cost = 1; // How many missiles this type consumes
    }

    void Start()
    {
        // Find FlightData if not assigned
        if (flightData == null)
        {
            flightData = FindObjectOfType<FlightData>();
            if (flightData == null)
            {
                Debug.LogError("MissileController: No FlightData found in scene!", this);
            }
            else
            {
                Debug.Log($"MissileController: Found FlightData on '{flightData.name}'");
            }
        }
    }

    public bool FireMissile()
    {
        // Check if we can fire
        if (!CanFire())
            return false;

        MissileData missileData = GetCurrentMissileData();
        if (missileData == null)
        {
            Debug.LogWarning("MissileController: No missile data found for current type!", this);
            return false;
        }

        // Check ammunition using FlightData
        if (flightData == null || !flightData.HasMissiles() || flightData.currentMissiles < missileData.cost)
        {
            PlayEmptySound();
            return false;
        }

        // Get fire point
        Transform firePoint = GetCurrentFirePoint();
        if (firePoint == null)
        {
            Debug.LogWarning("MissileController: No fire points assigned!", this);
            return false;
        }

        // Fire the missile
        LaunchMissile(missileData, firePoint);

        // Update state using FlightData
        flightData.ConsumeMissile(missileData.cost);
        lastFireTime = Time.time;
        
        // Alternate fire points
        if (missileFirePoints.Length > 1)
        {
            currentFirePointIndex = (currentFirePointIndex + 1) % missileFirePoints.Length;
        }

        return true;
    }

    private bool CanFire()
    {
        MissileData missileData = GetCurrentMissileData();
        if (missileData == null) return false;
        
        return Time.time >= lastFireTime + missileData.cooldown;
    }

    private MissileData GetCurrentMissileData()
    {
        if (missileTypes == null || missileTypes.Length == 0) return null;
        
        foreach (var missile in missileTypes)
        {
            if (missile.type == currentMissileType)
                return missile;
        }
        
        return missileTypes[0]; // Fallback to first missile type
    }

    private Transform GetCurrentFirePoint()
    {
        if (missileFirePoints == null || missileFirePoints.Length == 0) return transform;
        
        if (currentFirePointIndex >= missileFirePoints.Length)
            currentFirePointIndex = 0;
            
        return missileFirePoints[currentFirePointIndex];
    }

    private void LaunchMissile(MissileData missileData, Transform firePoint)
    {
        if (missileData.missilePrefab == null)
        {
            Debug.LogWarning($"MissileController: No prefab assigned for missile type {missileData.type}!", this);
            return;
        }

        // Instantiate missile
        GameObject missile = Instantiate(missileData.missilePrefab, firePoint.position, firePoint.rotation);
        
        // Calculate player's velocity for inheritance
        Vector3 playerVelocity = enableVelocityInheritance ? CalculatePlayerVelocity() : Vector3.zero;
        
        // Configure missile (assuming it has a Missile component)
        var missileComponent = missile.GetComponent<Missile>();
        if (missileComponent != null)
        {
            // Use the new velocity inheritance initialization
            missileComponent.Initialize(missileData.speed, missileData.damage, firePoint.forward, playerVelocity);
        }
        else
        {
            // Fallback: just give it velocity if it has a Rigidbody
            var rb = missile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 missileVelocity = firePoint.forward * missileData.speed;
                Vector3 finalVelocity = missileVelocity + playerVelocity;
                rb.velocity = finalVelocity;
                
                Debug.Log($"Missile fallback velocity: Own={missileVelocity.magnitude:F1}, " +
                         $"Inherited={playerVelocity.magnitude:F1}, Final={finalVelocity.magnitude:F1}");
            }
        }

        // Play effects
        PlayLaunchEffects(firePoint);
    }

    /// <summary>
    /// Calculate the player's current velocity for missile inheritance
    /// </summary>
    private Vector3 CalculatePlayerVelocity()
    {
        if (flightData == null)
        {
            Debug.LogWarning("MissileController: No FlightData available for velocity calculation!");
            return Vector3.zero;
        }

        // Get the player's current speed and direction
        float playerSpeed = flightData.airspeed;
        Vector3 playerDirection = transform.forward; // Aircraft's forward direction
        
        // Convert speed from MPH to Unity units per second
        // The airspeed is already in Unity units per second based on the flight controller
        Vector3 playerVelocity = playerDirection * playerSpeed * velocityInheritanceMultiplier;
        
        Debug.Log($"Player velocity calculated: Speed={playerSpeed:F1}, Direction={playerDirection}, " +
                 $"Inheritance={velocityInheritanceMultiplier:F1}, Final={playerVelocity.magnitude:F1}");
        
        return playerVelocity;
    }

    private void PlayLaunchEffects(Transform firePoint)
    {
        // Play launch sound
        if (missileAudioSource != null && missileLaunchSound != null)
        {
            missileAudioSource.PlayOneShot(missileLaunchSound);
        }

        // Spawn launch effect
        if (missileLaunchEffect != null)
        {
            GameObject effect = Instantiate(missileLaunchEffect, firePoint.position, firePoint.rotation);
            Destroy(effect, launchEffectDuration);
        }
    }

    private void PlayEmptySound()
    {
        if (missileAudioSource != null && missileEmptySound != null)
        {
            missileAudioSource.PlayOneShot(missileEmptySound);
        }
    }

    // Public methods for external control
    public void SwitchMissileType(MissileType newType)
    {
        currentMissileType = newType;
        if (flightData != null)
        {
            flightData.SetMissileType(newType.ToString());
        }
    }

    public void AddMissiles(int amount)
    {
        if (flightData != null)
        {
            flightData.AddMissiles(amount);
        }
    }

    public void SetMaxMissiles(int newMax)
    {
        if (flightData != null)
        {
            flightData.maxMissiles = newMax;
            if (flightData.currentMissiles > newMax)
                flightData.currentMissiles = newMax;
        }
    }

    // Velocity inheritance controls
    public void SetVelocityInheritance(bool enabled)
    {
        enableVelocityInheritance = enabled;
        Debug.Log($"Missile velocity inheritance {(enabled ? "enabled" : "disabled")}");
    }

    public void SetVelocityInheritanceMultiplier(float multiplier)
    {
        velocityInheritanceMultiplier = Mathf.Clamp01(multiplier);
        Debug.Log($"Missile velocity inheritance multiplier set to {velocityInheritanceMultiplier:F1}");
    }

    // Properties using FlightData
    public int CurrentMissiles => flightData != null ? flightData.currentMissiles : 0;
    public int MaxMissiles => flightData != null ? flightData.maxMissiles : 0;
    public MissileType CurrentMissileType => currentMissileType;
    public bool HasMissiles => flightData != null && flightData.HasMissiles();
    public float MissilePercentage => flightData != null ? flightData.GetMissilePercentage() : 0f;
    public bool VelocityInheritanceEnabled => enableVelocityInheritance;
    public float VelocityInheritanceMultiplier => velocityInheritanceMultiplier;

    void OnValidate()
    {
        // Clamp velocity inheritance multiplier
        velocityInheritanceMultiplier = Mathf.Clamp01(velocityInheritanceMultiplier);
    }

    void OnDrawGizmosSelected()
    {
        // Draw missile fire points
        if (missileFirePoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform firePoint in missileFirePoints)
            {
                if (firePoint != null)
                {
                    Gizmos.DrawWireCube(firePoint.position, Vector3.one * 0.3f);
                    Gizmos.DrawRay(firePoint.position, firePoint.forward * 5f);
                    
                    // Draw velocity inheritance visualization
                    if (enableVelocityInheritance && Application.isPlaying && flightData != null)
                    {
                        Vector3 playerVel = CalculatePlayerVelocity();
                        Gizmos.color = Color.green;
                        Gizmos.DrawRay(firePoint.position, playerVel.normalized * 3f);
                    }
                }
            }
        }
    }
}
