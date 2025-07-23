using UnityEngine;

/// <summary>
/// ScriptableObject that defines enemy type characteristics and behaviors
/// Allows for data-driven enemy configuration and easy balancing
/// </summary>
[CreateAssetMenu(fileName = "New Enemy Type", menuName = "RiverRaider/Enemy Type Data")]
public class EnemyTypeData : ScriptableObject
{
    [Header("Basic Properties")]
    [SerializeField] private string enemyName = "Basic Helicopter";
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int difficultyTier = 1; // 1=Basic, 2=Elite, 3=Boss
    
    [Header("Health & Defense")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float armor = 0f; // Damage reduction percentage (0-1)
    
    [Header("Movement")]
    [SerializeField] private float hoverSpeed = 1f; // Bobbing frequency
    [SerializeField] private float hoverHeight = 1f; // Bobbing amplitude
    [SerializeField] private float rotationSpeed = 2f; // How fast it turns to face player
    [SerializeField] private float maxDistance = 50f; // Max engagement range
    
    [Header("Weapons")]
    [SerializeField] private bool hasMachinegun = true;
    [SerializeField] private float machinegunDamage = 10f;
    [SerializeField] private float machinegunFireRate = 0.5f; // Time between shots
    [SerializeField] private float machinegunRange = 40f;
    
    [SerializeField] private bool hasMissiles = false;
    [SerializeField] private float missileDamage = 25f;
    [SerializeField] private float missileFireRate = 3f; // Time between missile shots
    [SerializeField] private float missileRange = 60f;
    [SerializeField] private int maxMissiles = 4; // Ammo limit
    
    [Header("AI Behavior")]
    [SerializeField] private float detectionRange = 50f;
    [SerializeField] private float retreatHealthThreshold = 0.3f; // Retreat when health below 30%
    [SerializeField] private float aggressionLevel = 1f; // Multiplier for attack frequency
    
    [Header("Spawning")]
    [SerializeField] private float spawnWeight = 1f; // Probability weight for spawning
    [SerializeField] private int maxSimultaneous = 2; // Max of this type active at once
    
    [Header("Audio")]
    [SerializeField] private AudioClip[] shootSounds;
    
    // Public properties for easy access
    public string EnemyName => enemyName;
    public GameObject EnemyPrefab => enemyPrefab;
    public int DifficultyTier => difficultyTier;
    
    public float MaxHealth => maxHealth;
    public float Armor => armor;
    
    public float HoverSpeed => hoverSpeed;
    public float HoverHeight => hoverHeight;
    public float RotationSpeed => rotationSpeed;
    public float MaxDistance => maxDistance;
    
    public bool HasMachinegun => hasMachinegun;
    public float MachinegunDamage => machinegunDamage;
    public float MachinegunFireRate => machinegunFireRate;
    public float MachinegunRange => machinegunRange;
    
    public bool HasMissiles => hasMissiles;
    public float MissileDamage => missileDamage;
    public float MissileFireRate => missileFireRate;
    public float MissileRange => missileRange;
    public int MaxMissiles => maxMissiles;
    
    public float DetectionRange => detectionRange;
    public float RetreatHealthThreshold => retreatHealthThreshold;
    public float AggressionLevel => aggressionLevel;
    
    public float SpawnWeight => spawnWeight;
    public int MaxSimultaneous => maxSimultaneous;
    
    public AudioClip[] ShootSounds => shootSounds;
    
    /// <summary>
    /// Calculate effective damage after armor reduction
    /// </summary>
    public float CalculateDamageReduction(float incomingDamage)
    {
        return incomingDamage * (1f - Mathf.Clamp01(armor));
    }
    
    /// <summary>
    /// Get weapon range based on weapon type
    /// </summary>
    public float GetWeaponRange(bool useMissiles)
    {
        if (useMissiles && hasMissiles)
            return missileRange;
        else if (hasMachinegun)
            return machinegunRange;
        else
            return detectionRange;
    }
    
    /// <summary>
    /// Get weapon damage based on weapon type
    /// </summary>
    public float GetWeaponDamage(bool useMissiles)
    {
        if (useMissiles && hasMissiles)
            return missileDamage;
        else if (hasMachinegun)
            return machinegunDamage;
        else
            return 0f;
    }
    
    /// <summary>
    /// Get weapon fire rate based on weapon type
    /// </summary>
    public float GetWeaponFireRate(bool useMissiles)
    {
        if (useMissiles && hasMissiles)
            return missileFireRate;
        else if (hasMachinegun)
            return machinegunFireRate;
        else
            return 1f;
    }
    
    /// <summary>
    /// Validate the enemy data configuration
    /// </summary>
    public bool IsValid()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"EnemyTypeData '{enemyName}': Enemy prefab is not assigned!");
            return false;
        }
        
        if (maxHealth <= 0)
        {
            Debug.LogError($"EnemyTypeData '{enemyName}': Max health must be greater than 0!");
            return false;
        }
        
        if (!hasMachinegun && !hasMissiles)
        {
            Debug.LogWarning($"EnemyTypeData '{enemyName}': Enemy has no weapons assigned!");
        }
        
        return true;
    }
    
    /// <summary>
    /// Create a copy of this enemy data with scaled difficulty
    /// </summary>
    public EnemyTypeData CreateScaledVersion(float healthMultiplier, float damageMultiplier, float fireRateMultiplier)
    {
        var scaledData = CreateInstance<EnemyTypeData>();
        
        // Copy all base values
        scaledData.enemyName = $"{enemyName} (Scaled)";
        scaledData.enemyPrefab = enemyPrefab;
        scaledData.difficultyTier = difficultyTier;
        
        // Scale combat values
        scaledData.maxHealth = maxHealth * healthMultiplier;
        scaledData.armor = armor;
        
        // Copy movement values
        scaledData.hoverSpeed = hoverSpeed;
        scaledData.hoverHeight = hoverHeight;
        scaledData.rotationSpeed = rotationSpeed;
        scaledData.maxDistance = maxDistance;
        
        // Scale weapon values
        scaledData.hasMachinegun = hasMachinegun;
        scaledData.machinegunDamage = machinegunDamage * damageMultiplier;
        scaledData.machinegunFireRate = machinegunFireRate / fireRateMultiplier; // Lower = faster
        scaledData.machinegunRange = machinegunRange;
        
        scaledData.hasMissiles = hasMissiles;
        scaledData.missileDamage = missileDamage * damageMultiplier;
        scaledData.missileFireRate = missileFireRate / fireRateMultiplier;
        scaledData.missileRange = missileRange;
        scaledData.maxMissiles = maxMissiles;
        
        // Copy other values
        scaledData.detectionRange = detectionRange;
        scaledData.retreatHealthThreshold = retreatHealthThreshold;
        scaledData.aggressionLevel = aggressionLevel * fireRateMultiplier;
        
        scaledData.spawnWeight = spawnWeight;
        scaledData.maxSimultaneous = maxSimultaneous;
        
        scaledData.shootSounds = shootSounds;
        
        return scaledData;
    }
}
