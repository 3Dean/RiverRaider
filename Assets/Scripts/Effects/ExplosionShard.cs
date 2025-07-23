using UnityEngine;
using System.Collections;

/// <summary>
/// Individual explosion shard behavior for realistic helicopter destruction
/// Handles collision effects, lifetime management, and physics interactions
/// </summary>
public class ExplosionShard : MonoBehaviour
{
    [Header("Shard Settings")]
    [SerializeField] private float individualLifetime = 12f;
    [SerializeField] private float minimumVelocityForSparks = 5f;
    [SerializeField] private float collisionSoundThreshold = 3f;
    [SerializeField] private bool canCreateFire = false;
    [SerializeField] private float fireChance = 0.1f;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject sparkEffectPrefab;
    [SerializeField] private GameObject fireTrailPrefab;
    [SerializeField] private bool fadeOutBeforeDestroy = true;
    [SerializeField] private float fadeOutDuration = 2f;
    
    [Header("Physics")]
    [SerializeField] private float bounceThreshold = 2f;
    [SerializeField] private float stickingVelocityThreshold = 0.2f; // Much lower threshold
    [SerializeField] private bool canStickToTerrain = true;
    [SerializeField] private float minimumFallTime = 2f; // Don't stick until after falling for a while
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    // Components
    private Rigidbody cachedRigidbody;
    private Collider cachedCollider;
    private Renderer cachedRenderer;
    private AudioSource audioSource;
    
    // References
    private HelicopterExplosion parentExplosion;
    private AudioClip[] collisionSounds;
    
    // State
    private bool isActive = false;
    private bool isStuck = false;
    private bool isFadingOut = false;
    private float activationTime;
    private int collisionCount = 0;
    private Vector3 lastVelocity;
    
    // Effects
    private GameObject activeFireTrail;
    private Material originalMaterial;
    private Color originalColor;

    public Rigidbody Rigidbody => cachedRigidbody;
    public bool IsActive => isActive;
    public bool IsStuck => isStuck;

    void Awake()
    {
        // Cache components
        cachedRigidbody = GetComponent<Rigidbody>();
        cachedCollider = GetComponent<Collider>();
        cachedRenderer = GetComponent<Renderer>();
        
        // Create audio source for collision sounds
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.5f;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = 50f;
        }

        // Store original material properties for fading
        if (cachedRenderer != null && cachedRenderer.material != null)
        {
            originalMaterial = cachedRenderer.material;
            originalColor = originalMaterial.color;
        }
    }

    void Update()
    {
        if (!isActive) return;

        // Handle lifetime
        if (Time.time - activationTime > individualLifetime)
        {
            StartDestruction();
        }

        // Track velocity for collision detection
        if (cachedRigidbody != null)
        {
            lastVelocity = cachedRigidbody.velocity;
        }

        // Handle sticking logic
        if (!isStuck && cachedRigidbody != null)
        {
            CheckForSticking();
        }
    }

    /// <summary>
    /// Initialize the shard with parent explosion reference
    /// </summary>
    public void Initialize(HelicopterExplosion parent, AudioClip[] sounds)
    {
        parentExplosion = parent;
        collisionSounds = sounds;
        
        // Ensure physics is initially disabled
        if (cachedRigidbody != null)
        {
            cachedRigidbody.isKinematic = true;
        }
    }

    /// <summary>
    /// Activate the shard for physics simulation
    /// </summary>
    public void Activate()
    {
        isActive = true;
        activationTime = Time.time;
        
        // Enable physics
        if (cachedRigidbody != null)
        {
            cachedRigidbody.isKinematic = false;
        }

        // Randomly create fire trail for some shards
        if (canCreateFire && Random.value < fireChance && fireTrailPrefab != null)
        {
            CreateFireTrail();
        }

        if (showDebugInfo)
        {
            Debug.Log($"ExplosionShard: {gameObject.name} activated");
        }
    }

    /// <summary>
    /// Handle collision with other objects
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;

        collisionCount++;
        float impactVelocity = lastVelocity.magnitude;

        if (showDebugInfo)
        {
            Debug.Log($"ExplosionShard: {gameObject.name} collided with {collision.gameObject.name} at velocity {impactVelocity:F1}");
        }

        // Create spark effects for high-velocity impacts
        if (impactVelocity > minimumVelocityForSparks)
        {
            CreateSparkEffect(collision.contacts[0].point, collision.contacts[0].normal);
        }

        // Play collision sound
        if (impactVelocity > collisionSoundThreshold)
        {
            PlayCollisionSound(impactVelocity);
        }

        // Handle bouncing vs sticking
        HandleCollisionPhysics(collision, impactVelocity);

        // Check for terrain sticking
        if (canStickToTerrain && IsTerrainCollision(collision))
        {
            HandleTerrainCollision(collision, impactVelocity);
        }
    }

    /// <summary>
    /// Check if shard should stick to surface due to low velocity
    /// </summary>
    private void CheckForSticking()
    {
        if (cachedRigidbody == null || isStuck) return;

        // Don't allow sticking until minimum fall time has passed
        float timeSinceActivation = Time.time - activationTime;
        if (timeSinceActivation < minimumFallTime)
        {
            return;
        }

        float velocity = cachedRigidbody.velocity.magnitude;
        float angularVelocity = cachedRigidbody.angularVelocity.magnitude;

        // Only stick if moving VERY slowly AND has had collisions (indicating it's on ground)
        if (velocity < stickingVelocityThreshold && angularVelocity < 0.5f && collisionCount > 0)
        {
            StickToSurface();
        }
    }

    /// <summary>
    /// Make the shard stick to the current surface
    /// </summary>
    private void StickToSurface()
    {
        if (isStuck) return;

        isStuck = true;
        
        if (cachedRigidbody != null)
        {
            // CRITICAL FIX: Set velocities to zero BEFORE making kinematic
            // Setting velocity/angularVelocity on kinematic rigidbodies is not supported
            cachedRigidbody.velocity = Vector3.zero;
            cachedRigidbody.angularVelocity = Vector3.zero;
            cachedRigidbody.isKinematic = true;
        }

        // Destroy fire trail if active
        if (activeFireTrail != null)
        {
            Destroy(activeFireTrail);
            activeFireTrail = null;
        }

        if (showDebugInfo)
        {
            Debug.Log($"ExplosionShard: {gameObject.name} stuck to surface");
        }
    }

    /// <summary>
    /// Handle collision physics (bouncing, energy loss)
    /// </summary>
    private void HandleCollisionPhysics(Collision collision, float impactVelocity)
    {
        if (cachedRigidbody == null) return;

        // Reduce velocity on each collision (energy loss)
        float energyLoss = Mathf.Clamp01(impactVelocity / 20f) * 0.3f; // Lose up to 30% energy
        cachedRigidbody.velocity *= (1f - energyLoss);
        cachedRigidbody.angularVelocity *= (1f - energyLoss * 0.5f);

        // Add some randomness to prevent predictable bouncing
        Vector3 randomForce = Random.insideUnitSphere * impactVelocity * 0.1f;
        cachedRigidbody.AddForce(randomForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Handle collision with terrain specifically
    /// </summary>
    private void HandleTerrainCollision(Collision collision, float impactVelocity)
    {
        // Stick to terrain if impact is not too violent
        if (impactVelocity < bounceThreshold * 2f)
        {
            // Chance to stick increases with more collisions
            float stickChance = Mathf.Clamp01(collisionCount * 0.2f);
            if (Random.value < stickChance)
            {
                StickToSurface();
            }
        }
    }

    /// <summary>
    /// Check if collision is with terrain
    /// </summary>
    private bool IsTerrainCollision(Collision collision)
    {
        // Check for terrain layer or terrain component
        return collision.gameObject.layer == LayerMask.NameToLayer("Terrain") ||
               collision.gameObject.GetComponent<Terrain>() != null ||
               collision.gameObject.name.ToLower().Contains("terrain") ||
               collision.gameObject.name.ToLower().Contains("ground");
    }

    /// <summary>
    /// Create spark effect at collision point
    /// </summary>
    private void CreateSparkEffect(Vector3 position, Vector3 normal)
    {
        if (sparkEffectPrefab != null)
        {
            GameObject sparks = Instantiate(sparkEffectPrefab, position, Quaternion.LookRotation(normal));
            Destroy(sparks, 2f);
        }
        else
        {
            // Create simple particle sparks
            CreateSimpleSparks(position, normal);
        }
    }

    /// <summary>
    /// Create simple spark particles if no prefab is available
    /// </summary>
    private void CreateSimpleSparks(Vector3 position, Vector3 normal)
    {
        GameObject sparkObject = new GameObject("Sparks");
        sparkObject.transform.position = position;
        
        ParticleSystem particles = sparkObject.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 8f;
        main.startSize = 0.1f;
        main.startColor = new Color(1f, 0.8f, 0.2f, 1f); // Yellow-orange sparks
        main.maxParticles = 15;
        
        var emission = particles.emission;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 15)
        });
        
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.2f;
        
        // CRITICAL FIX: Assign material to prevent pink particles
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        // Try to use existing Sparks material, fallback to Default-Particle
        Material sparksMaterial = Resources.Load<Material>("Materials/Sparks");
        if (sparksMaterial == null)
        {
            // Use Unity's default particle material
            sparksMaterial = Resources.GetBuiltinResource<Material>("Default-Particle.mat");
        }
        if (sparksMaterial != null)
        {
            renderer.material = sparksMaterial;
        }
        
        // Orient towards collision normal
        sparkObject.transform.rotation = Quaternion.LookRotation(normal);
        
        Destroy(sparkObject, 2f);
    }

    /// <summary>
    /// Create fire trail effect for this shard
    /// </summary>
    private void CreateFireTrail()
    {
        if (fireTrailPrefab != null)
        {
            activeFireTrail = Instantiate(fireTrailPrefab, transform);
            activeFireTrail.transform.localPosition = Vector3.zero;
        }
        else
        {
            // Create simple fire trail
            CreateSimpleFireTrail();
        }
    }

    /// <summary>
    /// Create simple fire trail if no prefab available
    /// </summary>
    private void CreateSimpleFireTrail()
    {
        GameObject fireTrail = new GameObject("FireTrail");
        fireTrail.transform.SetParent(transform);
        fireTrail.transform.localPosition = Vector3.zero;
        
        ParticleSystem particles = fireTrail.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 1f;
        main.startSpeed = 2f;
        main.startSize = 0.3f;
        main.startColor = new Color(1f, 0.4f, 0.1f, 0.8f); // Orange fire
        main.maxParticles = 20;
        
        var emission = particles.emission;
        emission.rateOverTime = 20f;
        
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        
        // CRITICAL FIX: Assign material to prevent pink particles
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        // Try to use existing Fire material, fallback to Default-Particle
        Material fireMaterial = Resources.Load<Material>("Materials/Fire");
        if (fireMaterial == null)
        {
            // Use Unity's default particle material
            fireMaterial = Resources.GetBuiltinResource<Material>("Default-Particle.mat");
        }
        if (fireMaterial != null)
        {
            renderer.material = fireMaterial;
        }
        
        activeFireTrail = fireTrail;
    }

    /// <summary>
    /// Play collision sound based on impact velocity
    /// </summary>
    private void PlayCollisionSound(float impactVelocity)
    {
        if (audioSource == null || collisionSounds == null || collisionSounds.Length == 0) return;

        // Select random collision sound
        AudioClip sound = collisionSounds[Random.Range(0, collisionSounds.Length)];
        if (sound == null) return;

        // Volume based on impact velocity
        float volume = Mathf.Clamp01(impactVelocity / 15f) * 0.8f;
        
        // Pitch variation for variety
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        
        audioSource.PlayOneShot(sound, volume);
    }

    /// <summary>
    /// Start the destruction process
    /// </summary>
    private void StartDestruction()
    {
        if (isFadingOut) return;

        if (fadeOutBeforeDestroy && cachedRenderer != null)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
        else
        {
            DestroyImmediate();
        }
    }

    /// <summary>
    /// Fade out the shard before destroying
    /// </summary>
    private IEnumerator FadeOutAndDestroy()
    {
        isFadingOut = true;
        float fadeTimer = 0f;
        
        while (fadeTimer < fadeOutDuration)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, fadeTimer / fadeOutDuration);
            
            if (cachedRenderer != null && cachedRenderer.material != null)
            {
                Color newColor = originalColor;
                newColor.a = alpha;
                cachedRenderer.material.color = newColor;
            }
            
            yield return null;
        }
        
        DestroyImmediate();
    }

    /// <summary>
    /// Immediately destroy the shard
    /// </summary>
    private void DestroyImmediate()
    {
        // Destroy fire trail
        if (activeFireTrail != null)
        {
            Destroy(activeFireTrail);
        }

        // Notify parent explosion
        if (parentExplosion != null)
        {
            parentExplosion.OnShardDestroyed();
        }

        if (showDebugInfo)
        {
            Debug.Log($"ExplosionShard: {gameObject.name} destroyed after {Time.time - activationTime:F1} seconds");
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Force destroy this shard (called by parent explosion)
    /// </summary>
    public void ForceDestroy()
    {
        StopAllCoroutines();
        DestroyImmediate();
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugInfo || !isActive) return;

        // Draw velocity vector
        if (cachedRigidbody != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, cachedRigidbody.velocity * 0.5f);
        }

        // Draw collision count
        Gizmos.color = Color.white;
        Vector3 textPos = transform.position + Vector3.up * 2f;
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(textPos, $"Collisions: {collisionCount}");
        #endif
    }
}
