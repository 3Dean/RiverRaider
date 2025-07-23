using UnityEngine;

/// <summary>
/// Enhanced Helicopter Enemy AI with data-driven configuration and improved behaviors
/// Supports machinegun and missile weapons with difficulty scaling
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private EnemyTypeData enemyData;
    
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform missileFirePoint; // Optional separate missile fire point
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    // Enemy State
    public enum EnemyState
    {
        Spawning,
        Patrolling,
        Engaging,
        Retreating,
        Dead
    }
    
    private EnemyState currentState = EnemyState.Spawning;
    private float currentHealth;
    private bool isDead = false;
    
    // Movement
    private Vector3 basePosition;
    private float hoverOffset;
    private Transform playerTarget;
    
    // Combat
    private float machinegunTimer;
    private float missileTimer;
    private int currentMissileAmmo;
    private bool isInRange = false;
    
    // Performance optimization
    private Transform cachedTransform;
    private AudioSource audioSource;
    private float lastPlayerDistanceCheck;
    private float playerDistance;
    
    // Constants
    private const float PLAYER_DISTANCE_CHECK_INTERVAL = 0.2f; // Check distance 5 times per second
    private const float STATE_UPDATE_INTERVAL = 0.1f; // Update state 10 times per second
    private float lastStateUpdate;

    void Awake()
    {
        cachedTransform = transform;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        InitializeEnemy();
    }

    void Update()
    {
        if (isDead || enemyData == null) return;

        // Performance optimization - don't update every frame
        if (Time.time - lastStateUpdate < STATE_UPDATE_INTERVAL) return;
        lastStateUpdate = Time.time;

        UpdatePlayerDistance();
        UpdateState();
        HandleMovement();
        HandleCombat();
        HandleRotation();
    }

    /// <summary>
    /// Initialize enemy with data from EnemyTypeData
    /// </summary>
    private void InitializeEnemy()
    {
        if (enemyData == null)
        {
            Debug.LogError("EnemyAI: No EnemyTypeData assigned! Enemy will not function properly.", this);
            return;
        }

        if (!enemyData.IsValid())
        {
            Debug.LogError($"EnemyAI: Invalid EnemyTypeData '{enemyData.EnemyName}'!", this);
            return;
        }

        // Initialize health and combat
        currentHealth = enemyData.MaxHealth;
        currentMissileAmmo = enemyData.MaxMissiles;
        
        // Initialize position
        basePosition = cachedTransform.position;
        hoverOffset = Random.Range(0f, Mathf.PI * 2f); // Random hover phase
        
        // Find player
        FindPlayerTarget();
        
        // Initialize timers with random offset to prevent synchronized shooting
        machinegunTimer = Random.Range(0f, enemyData.MachinegunFireRate);
        missileTimer = Random.Range(0f, enemyData.MissileFireRate);
        
        // Set initial state
        currentState = EnemyState.Patrolling;
        
        if (showDebugInfo)
        {
            Debug.Log($"EnemyAI: Initialized '{enemyData.EnemyName}' with {currentHealth} health");
        }
    }

    /// <summary>
    /// Update distance to player (performance optimized)
    /// </summary>
    private void UpdatePlayerDistance()
    {
        if (Time.time - lastPlayerDistanceCheck < PLAYER_DISTANCE_CHECK_INTERVAL) return;
        lastPlayerDistanceCheck = Time.time;

        if (playerTarget != null)
        {
            playerDistance = Vector3.Distance(cachedTransform.position, playerTarget.position);
            isInRange = playerDistance <= enemyData.DetectionRange;
        }
        else
        {
            FindPlayerTarget();
            playerDistance = float.MaxValue;
            isInRange = false;
        }
    }

    /// <summary>
    /// Update enemy state based on conditions
    /// </summary>
    private void UpdateState()
    {
        EnemyState newState = currentState;

        switch (currentState)
        {
            case EnemyState.Spawning:
                newState = EnemyState.Patrolling;
                break;

            case EnemyState.Patrolling:
                if (isInRange && playerTarget != null)
                {
                    newState = EnemyState.Engaging;
                }
                break;

            case EnemyState.Engaging:
                // Check if should retreat due to low health
                if (currentHealth / enemyData.MaxHealth <= enemyData.RetreatHealthThreshold)
                {
                    newState = EnemyState.Retreating;
                }
                // Check if player is out of range
                else if (!isInRange)
                {
                    newState = EnemyState.Patrolling;
                }
                break;

            case EnemyState.Retreating:
                // Could add logic to return to patrolling after healing or time
                if (!isInRange)
                {
                    newState = EnemyState.Patrolling;
                }
                break;
        }

        if (newState != currentState)
        {
            if (showDebugInfo)
            {
                Debug.Log($"EnemyAI: State changed from {currentState} to {newState}");
            }
            currentState = newState;
        }
    }

    /// <summary>
    /// Handle helicopter hovering movement
    /// </summary>
    private void HandleMovement()
    {
        // Hovering bobbing motion
        float hoverY = basePosition.y + Mathf.Sin((Time.time * enemyData.HoverSpeed) + hoverOffset) * enemyData.HoverHeight;
        
        Vector3 targetPosition = new Vector3(basePosition.x, hoverY, basePosition.z);
        
        // Smooth movement to target position
        cachedTransform.position = Vector3.Lerp(cachedTransform.position, targetPosition, Time.deltaTime * 2f);
    }

    /// <summary>
    /// Handle rotation to face player
    /// </summary>
    private void HandleRotation()
    {
        if (playerTarget == null || currentState == EnemyState.Dead) return;

        // Calculate direction to player
        Vector3 directionToPlayer = (playerTarget.position - cachedTransform.position).normalized;
        
        // Create target rotation (only rotate around Y axis for helicopters)
        // Add 180° offset to compensate for Blender model facing backwards
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z)) * Quaternion.Euler(0, 180, 0);
        
        // Smoothly rotate towards player
        cachedTransform.rotation = Quaternion.Slerp(
            cachedTransform.rotation, 
            targetRotation, 
            enemyData.RotationSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Handle combat behavior (shooting)
    /// </summary>
    private void HandleCombat()
    {
        if (currentState != EnemyState.Engaging || playerTarget == null) return;

        // Update weapon timers
        machinegunTimer += Time.deltaTime;
        missileTimer += Time.deltaTime;

        // Try to fire machinegun
        if (enemyData.HasMachinegun && CanFireMachinegun())
        {
            FireMachinegun();
        }

        // Try to fire missiles (if available and conditions are met)
        if (enemyData.HasMissiles && CanFireMissile())
        {
            FireMissile();
        }
    }

    /// <summary>
    /// Check if can fire machinegun
    /// </summary>
    private bool CanFireMachinegun()
    {
        return machinegunTimer >= enemyData.MachinegunFireRate && 
               playerDistance <= enemyData.MachinegunRange &&
               firePoint != null;
    }

    /// <summary>
    /// Check if can fire missile
    /// </summary>
    private bool CanFireMissile()
    {
        return missileTimer >= enemyData.MissileFireRate &&
               playerDistance <= enemyData.MissileRange &&
               currentMissileAmmo > 0 &&
               (missileFirePoint != null || firePoint != null);
    }

    /// <summary>
    /// Fire machinegun at player
    /// </summary>
    private void FireMachinegun()
    {
        if (BulletPool.Instance == null)
        {
            Debug.LogWarning("EnemyAI: BulletPool not found! Cannot fire machinegun.", this);
            return;
        }

        // Calculate direction to player with some lead prediction
        Vector3 shootDirection = CalculateShootDirection();
        
        // Get bullet from pool
        GameObject bullet = BulletPool.Instance.GetBullet(
            firePoint.position,
            Quaternion.LookRotation(shootDirection),
            shootDirection,
            30f // Standard bullet speed
        );

        if (bullet != null)
        {
            // Set bullet damage
            var bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Damage = enemyData.MachinegunDamage;
            }
        }

        // Reset timer
        machinegunTimer = 0f;

        // Play sound effect
        PlayShootSound();

        if (showDebugInfo)
        {
            Debug.Log($"EnemyAI: Fired machinegun at player (Distance: {playerDistance:F1}m)");
        }
    }

    /// <summary>
    /// Fire missile at player
    /// </summary>
    private void FireMissile()
    {
        // Use missile fire point if available, otherwise use main fire point
        Transform launchPoint = missileFirePoint != null ? missileFirePoint : firePoint;
        
        // Calculate direction to player
        Vector3 shootDirection = CalculateShootDirection();
        
        // Create missile (this would need integration with your missile system)
        // For now, we'll create a more powerful bullet as a placeholder
        if (BulletPool.Instance != null)
        {
            GameObject missile = BulletPool.Instance.GetBullet(
                launchPoint.position,
                Quaternion.LookRotation(shootDirection),
                shootDirection,
                20f // Slower missile speed
            );

            if (missile != null)
            {
                var bulletComponent = missile.GetComponent<Bullet>();
                if (bulletComponent != null)
                {
                    bulletComponent.Damage = enemyData.MissileDamage;
                }
            }
        }

        // Consume ammo
        currentMissileAmmo--;
        missileTimer = 0f;

        // Play sound effect
        PlayShootSound();

        if (showDebugInfo)
        {
            Debug.Log($"EnemyAI: Fired missile at player (Ammo remaining: {currentMissileAmmo})");
        }
    }

    /// <summary>
    /// Calculate shoot direction with basic lead prediction
    /// </summary>
    private Vector3 CalculateShootDirection()
    {
        if (playerTarget == null) return cachedTransform.forward;

        // Basic lead prediction
        Vector3 targetPosition = playerTarget.position;
        
        // Try to get player velocity for lead calculation
        var playerRigidbody = playerTarget.GetComponent<Rigidbody>();
        if (playerRigidbody != null)
        {
            // Simple lead calculation
            float bulletSpeed = 30f;
            float timeToTarget = playerDistance / bulletSpeed;
            targetPosition += playerRigidbody.velocity * timeToTarget * 0.5f; // Partial lead
        }

        return (targetPosition - firePoint.position).normalized;
    }

    /// <summary>
    /// Play random shoot sound
    /// </summary>
    private void PlayShootSound()
    {
        if (audioSource != null && enemyData.ShootSounds != null && enemyData.ShootSounds.Length > 0)
        {
            AudioClip randomSound = enemyData.ShootSounds[Random.Range(0, enemyData.ShootSounds.Length)];
            audioSource.PlayOneShot(randomSound);
        }
    }

    /// <summary>
    /// Find player target in scene
    /// </summary>
    private void FindPlayerTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning("EnemyAI: No player found with 'Player' tag!");
        }
    }

    /// <summary>
    /// Take damage from player weapons
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isDead || enemyData == null) return;

        // Apply armor reduction
        float actualDamage = enemyData.CalculateDamageReduction(amount);
        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (showDebugInfo)
        {
            Debug.Log($"EnemyAI: Took {actualDamage:F1} damage (Health: {currentHealth:F1}/{enemyData.MaxHealth:F1})");
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Recover health (for future healing mechanics)
    /// </summary>
    public void RecoverHealth(float amount)
    {
        if (isDead || enemyData == null) return;

        currentHealth = Mathf.Min(currentHealth + amount, enemyData.MaxHealth);
    }

    /// <summary>
    /// Handle enemy death
    /// </summary>
    private void Die()
    {
        if (isDead) return;

        isDead = true;
        currentState = EnemyState.Dead;

        // Calculate damage direction for directional explosion
        Vector3 damageDirection = Vector3.zero;
        if (playerTarget != null)
        {
            damageDirection = (cachedTransform.position - playerTarget.position).normalized;
        }

        // Try to use realistic helicopter explosion first
        if (TryCreateRealisticExplosion(damageDirection))
        {
            // Realistic explosion created successfully
            if (showDebugInfo)
            {
                Debug.Log($"EnemyAI: '{enemyData.EnemyName}' destroyed with realistic explosion!");
            }
        }
        // Fallback to simple explosion if realistic explosion fails
        else
        {
            CreateSimpleExplosionEffect();
            
            if (showDebugInfo)
            {
                Debug.Log($"EnemyAI: '{enemyData.EnemyName}' destroyed with simple explosion fallback!");
            }
        }

        // Notify enemy manager (if exists) before destroying
        var enemyManager = FindObjectOfType<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.HandleEnemyDestroyed(this);
        }

        // Destroy after a short delay to allow explosion to initialize
        Destroy(gameObject, 0.5f);
    }

    /// <summary>
    /// Try to create realistic helicopter explosion with physics-based shards
    /// </summary>
    private bool TryCreateRealisticExplosion(Vector3 damageDirection)
    {
        // Look for HelicopterExplosionPrefab in Resources
        GameObject explosionPrefab = Resources.Load<GameObject>("HelicopterExplosionPrefab");
        
        if (showDebugInfo)
        {
            if (explosionPrefab != null)
            {
                Debug.Log($"EnemyAI: Successfully loaded HelicopterExplosionPrefab from Resources");
            }
            else
            {
                Debug.LogWarning($"EnemyAI: Could not load HelicopterExplosionPrefab from Resources!");
            }
        }

        if (explosionPrefab != null)
        {
            // Create the realistic explosion
            GameObject explosion = Instantiate(explosionPrefab, cachedTransform.position, cachedTransform.rotation);
            
            if (showDebugInfo)
            {
                Debug.Log($"EnemyAI: Created explosion GameObject: {explosion.name}");
            }
            
            // Configure the explosion
            HelicopterExplosion explosionComponent = explosion.GetComponent<HelicopterExplosion>();
            if (explosionComponent != null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"EnemyAI: Found HelicopterExplosion component, configuring...");
                }
                
                // Set damage direction for directional force
                explosionComponent.SetDamageDirection(damageDirection);
                
                // Customize explosion parameters based on enemy type
                float explosionForce = 1000f;
                float explosionRadius = 8f;
                
                if (enemyData != null)
                {
                    // Scale explosion based on enemy size/type
                    explosionForce *= enemyData.MaxHealth / 100f; // Scale with health
                    explosionRadius *= Mathf.Clamp(enemyData.MaxHealth / 100f, 0.5f, 2f); // Scale radius
                }
                
                explosionComponent.SetExplosionParameters(explosionForce, explosionRadius);
                
                if (showDebugInfo)
                {
                    Debug.Log($"EnemyAI: Configured explosion with force={explosionForce}, radius={explosionRadius}");
                }
                
                return true;
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.LogError($"EnemyAI: Explosion prefab has no HelicopterExplosion component!");
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Create a simple explosion effect when no explosion prefab is available
    /// </summary>
    private void CreateSimpleExplosionEffect()
    {
        // Try to use existing HitEffect prefab as explosion
        GameObject hitEffectPrefab = Resources.Load<GameObject>("HitEffect");
        if (hitEffectPrefab == null)
        {
            // Look for HitEffect in the scene or try to find it by name
            hitEffectPrefab = GameObject.Find("HitEffect");
            if (hitEffectPrefab != null)
            {
                hitEffectPrefab = hitEffectPrefab.GetComponent<Transform>().gameObject;
            }
        }

        if (hitEffectPrefab != null)
        {
            // Create multiple hit effects for explosion
            for (int i = 0; i < 3; i++)
            {
                Vector3 explosionPos = cachedTransform.position + Random.insideUnitSphere * 2f;
                GameObject effect = Instantiate(hitEffectPrefab, explosionPos, Random.rotation);
                Destroy(effect, 2f);
            }
        }
        else
        {
            // Fallback: Create a simple particle effect using Unity's built-in system
            GameObject simpleExplosion = new GameObject("SimpleExplosion");
            simpleExplosion.transform.position = cachedTransform.position;
            
            // Add a simple particle system
            var particles = simpleExplosion.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 1f;
            main.startSpeed = 5f;
            main.startSize = 0.5f;
            main.startColor = new Color(1f, 0.5f, 0f, 1f); // Orange color
            main.maxParticles = 50;
            
            var emission = particles.emission;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 50)
            });
            emission.enabled = true;
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 1f;
            
            // Auto-destroy
            Destroy(simpleExplosion, 2f);
        }
    }

    /// <summary>
    /// Initialize with specific enemy data (for pooling system)
    /// </summary>
    public void Initialize(EnemyTypeData data, Vector3 spawnPosition)
    {
        enemyData = data;
        cachedTransform.position = spawnPosition;
        basePosition = spawnPosition;
        
        InitializeEnemy();
    }

    /// <summary>
    /// Reset enemy for pooling system
    /// </summary>
    public void ResetEnemy()
    {
        isDead = false;
        currentState = EnemyState.Spawning;
        currentHealth = enemyData != null ? enemyData.MaxHealth : 100f;
        currentMissileAmmo = enemyData != null ? enemyData.MaxMissiles : 0;
        machinegunTimer = 0f;
        missileTimer = 0f;
    }

    // Public properties for external access
    public float Health => currentHealth;
    public float MaxHealth => enemyData != null ? enemyData.MaxHealth : 100f;
    public bool IsDead => isDead;
    public EnemyState CurrentState => currentState;
    public EnemyTypeData EnemyData => enemyData;

    void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;

        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.DetectionRange);

        // Draw machinegun range
        if (enemyData.HasMachinegun)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyData.MachinegunRange);
        }

        // Draw missile range
        if (enemyData.HasMissiles)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, enemyData.MissileRange);
        }

        // Draw fire point
        if (firePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, 0.5f);
            Gizmos.DrawRay(firePoint.position, firePoint.forward * 5f);
        }

        // Draw missile fire point
        if (missileFirePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(missileFirePoint.position, 0.3f);
            Gizmos.DrawRay(missileFirePoint.position, missileFirePoint.forward * 3f);
        }
    }
}
