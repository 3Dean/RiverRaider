using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simplified missile controller that works directly with FlightData
/// Handles missile firing with velocity inheritance - no more complex type management
/// </summary>
public class MissileController : MonoBehaviour
{
    [Header("Missile Settings")]
    [SerializeField] private Transform[] missileFirePoints;
    [SerializeField] private FlightData flightData; // Single source of truth
    
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

        MissileTypeData missileTypeData = GetCurrentMissileTypeData();
        if (missileTypeData == null)
        {
            Debug.LogWarning("MissileController: No missile data found for current type!", this);
            return false;
        }

        // Check ammunition using FlightData
        if (flightData == null || !flightData.HasMissiles() || flightData.currentMissiles < missileTypeData.costPerShot)
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
        LaunchMissile(missileTypeData, firePoint);

        // Update state using FlightData
        flightData.ConsumeMissile(missileTypeData.costPerShot);
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
        MissileTypeData missileTypeData = GetCurrentMissileTypeData();
        if (missileTypeData == null) return false;
        
        return Time.time >= lastFireTime + missileTypeData.cooldown;
    }

    private MissileTypeData GetCurrentMissileTypeData()
    {
        // Get missile type data directly from FlightData (single source of truth)
        return flightData?.GetCurrentMissileTypeData();
    }

    private Transform GetCurrentFirePoint()
    {
        if (missileFirePoints == null || missileFirePoints.Length == 0) return transform;
        
        if (currentFirePointIndex >= missileFirePoints.Length)
            currentFirePointIndex = 0;
            
        return missileFirePoints[currentFirePointIndex];
    }

    private void LaunchMissile(MissileTypeData missileTypeData, Transform firePoint)
    {
        if (missileTypeData.missilePrefab == null)
        {
            Debug.LogWarning($"MissileController: No prefab assigned for missile type {missileTypeData.missileTypeName}!", this);
            return;
        }

        // Instantiate missile
        GameObject missile = Instantiate(missileTypeData.missilePrefab, firePoint.position, firePoint.rotation);
        
        // Calculate player's velocity for inheritance
        Vector3 playerVelocity = enableVelocityInheritance ? CalculatePlayerVelocity() : Vector3.zero;
        
        // Configure missile (assuming it has a Missile component)
        var missileComponent = missile.GetComponent<Missile>();
        if (missileComponent != null)
        {
            // Use the new velocity inheritance initialization
            missileComponent.Initialize(missileTypeData.speed, missileTypeData.damage, firePoint.forward, playerVelocity);
        }
        else
        {
            // Fallback: just give it velocity if it has a Rigidbody
            var rb = missile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 missileVelocity = firePoint.forward * missileTypeData.speed;
                Vector3 finalVelocity = missileVelocity + playerVelocity;
                rb.velocity = finalVelocity;
                
                Debug.Log($"Missile fallback velocity: Own={missileVelocity.magnitude:F1}, " +
                         $"Inherited={playerVelocity.magnitude:F1}, Final={finalVelocity.magnitude:F1}");
            }
        }

        // Play effects (use missile type's launch sound if available)
        PlayLaunchEffects(firePoint, missileTypeData);
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

    private void PlayLaunchEffects(Transform firePoint, MissileTypeData missileTypeData = null)
    {
        // Play launch sound - prefer missile type's sound, fallback to default
        AudioClip soundToPlay = (missileTypeData != null && missileTypeData.launchSound != null) 
            ? missileTypeData.launchSound 
            : missileLaunchSound;
            
        if (missileAudioSource != null && soundToPlay != null)
        {
            missileAudioSource.PlayOneShot(soundToPlay);
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
    public void SwitchMissileType(string newTypeName)
    {
        if (flightData != null)
        {
            flightData.SetMissileType(newTypeName);
            Debug.Log($"Switched to missile type: {newTypeName}");
        }
    }
    
    public void SwitchToNextMissileType()
    {
        if (flightData != null)
        {
            flightData.SwitchToNextMissileType();
        }
    }
    
    public void SwitchToPreviousMissileType()
    {
        if (flightData != null)
        {
            flightData.SwitchToPreviousMissileType();
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
        // This method is now handled by the missile inventory system
        // Individual missile types have their own capacities
        Debug.Log($"SetMaxMissiles called with {newMax} - now handled by missile inventory system");
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

    // Properties using FlightData as single source of truth
    public int CurrentMissiles => flightData != null ? flightData.currentMissiles : 0;
    public int MaxMissiles => flightData != null ? flightData.maxMissiles : 0;
    public string CurrentMissileTypeName => flightData != null ? flightData.currentMissileType : "None";
    public MissileTypeData CurrentMissileTypeData => GetCurrentMissileTypeData();
    public bool HasMissiles => flightData != null && flightData.HasMissiles();
    public float MissilePercentage => MaxMissiles > 0 ? (float)CurrentMissiles / MaxMissiles : 0f;
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
