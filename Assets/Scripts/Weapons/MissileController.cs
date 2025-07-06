using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Missile controller for single-shot projectile weapons
/// Handles different missile types and ammunition management
/// </summary>
public class MissileController : MonoBehaviour
{
    [Header("Missile Settings")]
    [SerializeField] private MissileType currentMissileType = MissileType.Standard;
    [SerializeField] private Transform[] missileFirePoints;
    [SerializeField] private int maxMissiles = 20;
    [SerializeField] private int currentMissiles = 20;
    
    [Header("Missile Types")]
    [SerializeField] private MissileData[] missileTypes;
    
    [Header("Audio")]
    [SerializeField] private AudioSource missileAudioSource;
    [SerializeField] private AudioClip missileLaunchSound;
    [SerializeField] private AudioClip missileEmptySound;
    
    [Header("Effects")]
    [SerializeField] private GameObject missileLaunchEffect;
    [SerializeField] private float launchEffectDuration = 1f;

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
        // Initialize missile count
        if (currentMissiles > maxMissiles)
            currentMissiles = maxMissiles;
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

        // Check ammunition
        if (currentMissiles < missileData.cost)
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

        // Update state
        currentMissiles -= missileData.cost;
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
        
        // Configure missile (assuming it has a Missile component)
        var missileComponent = missile.GetComponent<Missile>();
        if (missileComponent != null)
        {
            missileComponent.Initialize(missileData.speed, missileData.damage, firePoint.forward);
        }
        else
        {
            // Fallback: just give it velocity if it has a Rigidbody
            var rb = missile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = firePoint.forward * missileData.speed;
            }
        }

        // Play effects
        PlayLaunchEffects(firePoint);
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
    }

    public void AddMissiles(int amount)
    {
        currentMissiles = Mathf.Min(currentMissiles + amount, maxMissiles);
    }

    public void SetMaxMissiles(int newMax)
    {
        maxMissiles = newMax;
        if (currentMissiles > maxMissiles)
            currentMissiles = maxMissiles;
    }

    // Properties
    public int CurrentMissiles => currentMissiles;
    public int MaxMissiles => maxMissiles;
    public MissileType CurrentMissileType => currentMissileType;
    public bool HasMissiles => currentMissiles > 0;
    public float MissilePercentage => maxMissiles > 0 ? (float)currentMissiles / maxMissiles : 0f;

    void OnValidate()
    {
        if (maxMissiles < 0) maxMissiles = 0;
        if (currentMissiles < 0) currentMissiles = 0;
        if (currentMissiles > maxMissiles) currentMissiles = maxMissiles;
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
                }
            }
        }
    }
}
