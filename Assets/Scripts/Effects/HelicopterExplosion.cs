using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Realistic helicopter explosion system with physics-based shards
/// Handles 51 individual pieces with mixed explosion forces and terrain interaction
/// </summary>
public class HelicopterExplosion : MonoBehaviour
{
    [Header("Explosion Parameters")]
    [SerializeField] private float baseExplosionForce = 80f; // Reduced force for realistic separation
    [SerializeField] private float upwardForceMultiplier = 0.3f; // Moderate upward force
    [SerializeField] private float directionalForceMultiplier = 0.2f; // Reduced directional force
    [SerializeField] private float randomnessMultiplier = 0.15f; // Reduced randomness for natural look
    [SerializeField] private float explosionRadius = 5f; // Reasonable explosion radius
    
    [Header("Physics Settings")]
    [SerializeField] private float forceVariation = 0.1f; // ±10% force variation - minimal
    [SerializeField] private float massScaling = 1.0f; // No mass scaling - keep forces consistent
    [SerializeField] private float minForceMultiplier = 0.8f; // Reduced minimum clamp
    [SerializeField] private float maxForceMultiplier = 1.2f; // Reduced maximum clamp
    [SerializeField] private float colliderActivationDelay = 1.5f; // Increased delay before enabling colliders
    
    [Header("Cleanup")]
    [SerializeField] private float shardLifetime = 15f;
    [SerializeField] private float cleanupDistance = 200f; // Increased from 100f
    [SerializeField] private bool enableDistanceCleanup = true;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject explosionFlashPrefab;
    [SerializeField] private GameObject fireAndSmokePrefab;
    [SerializeField] private GameObject sparksPrefab;
    [SerializeField] private bool createGroundScorch = true;
    
    [Header("Audio")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip[] metalCollisionSounds;
    [SerializeField] private AudioClip fireCracklingSound;
    [SerializeField] private float audioVolume = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showForceGizmos = false;
    [SerializeField] private bool enableVerboseLogging = true;
    [SerializeField] private bool useInspectorValuesOnly = false; // Override EnemyAI parameters

    // Internal components
    private List<ExplosionShard> explosionShards = new List<ExplosionShard>();
    private AudioSource audioSource;
    private Transform playerTransform;
    private Vector3 explosionCenter;
    private Vector3 damageDirection = Vector3.zero;
    private bool hasExploded = false;

    // Performance tracking
    private int activeShards = 0;
    private float explosionStartTime;

    void Awake()
    {
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = audioVolume;
        }

        // Find player for distance calculations
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Cache explosion center
        explosionCenter = transform.position;
    }

    void Start()
    {
        // Initialize all shards
        InitializeShards();
        
        // Start explosion immediately
        if (!hasExploded)
        {
            StartCoroutine(ExecuteExplosion());
        }
    }

    void Update()
    {
        // Don't cleanup until explosion has actually started
        if (!hasExploded) return;

        // Handle distance-based cleanup
        if (enableDistanceCleanup && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer > cleanupDistance)
            {
                CleanupExplosion();
                return;
            }
        }

        // Handle lifetime cleanup - but only after explosion has been running for a while
        if (Time.time - explosionStartTime > shardLifetime)
        {
            CleanupExplosion();
        }
    }

    /// <summary>
    /// Initialize all explosion shards
    /// </summary>
    private void InitializeShards()
    {
        // Find all rigidbodies in children (the 51 shards)
        Rigidbody[] shardRigidbodies = GetComponentsInChildren<Rigidbody>();
        
        foreach (Rigidbody rb in shardRigidbodies)
        {
            if (rb.transform == transform) continue; // Skip parent

            // Add ExplosionShard component if not present
            ExplosionShard shard = rb.GetComponent<ExplosionShard>();
            if (shard == null)
            {
                shard = rb.gameObject.AddComponent<ExplosionShard>();
            }

            // Initialize the shard
            shard.Initialize(this, metalCollisionSounds);
            explosionShards.Add(shard);

            // Initially disable physics until explosion
            rb.isKinematic = true;
            
            // CRITICAL FIX: Disable all colliders initially to prevent overlapping collision gridlock
            Collider[] colliders = rb.GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
        }

        activeShards = explosionShards.Count;

        if (showDebugInfo)
        {
            Debug.Log($"HelicopterExplosion: Initialized {activeShards} shards with colliders disabled");
        }
    }

    /// <summary>
    /// Execute the explosion sequence
    /// </summary>
    private IEnumerator ExecuteExplosion()
    {
        if (hasExploded) yield break;

        hasExploded = true;
        explosionStartTime = Time.time;

        // Play explosion sound
        PlayExplosionSound();

        // Create visual effects
        CreateVisualEffects();

        // Small delay for dramatic effect
        yield return new WaitForSeconds(0.1f);

        // First, separate shards to prevent collision gridlock
        SeparateShards();
        
        // Small delay to let separation take effect
        yield return new WaitForSeconds(0.05f);
        
        // Apply explosion forces to all shards
        ApplyExplosionForces();

        // Start coroutine to re-enable colliders after shards have separated
        StartCoroutine(EnableCollidersAfterDelay());

        if (showDebugInfo)
        {
            Debug.Log($"HelicopterExplosion: Explosion executed with {activeShards} shards");
        }
    }

    /// <summary>
    /// Separate shards initially to prevent collision gridlock
    /// </summary>
    private void SeparateShards()
    {
        int shardsSeparated = 0;
        
        if (showDebugInfo)
        {
            Debug.Log($"HelicopterExplosion: Separating {explosionShards.Count} shards to prevent collision gridlock");
        }
        
        foreach (ExplosionShard shard in explosionShards)
        {
            if (shard == null || shard.transform == null) continue;
            
            // Calculate direction from explosion center
            Vector3 directionFromCenter = (shard.transform.position - explosionCenter);
            if (directionFromCenter.magnitude < 0.1f)
            {
                // If too close to center, use random direction
                directionFromCenter = Random.insideUnitSphere;
                directionFromCenter.y = Mathf.Abs(directionFromCenter.y); // Keep upward bias
            }
            
            // Normalize and apply small separation distance
            Vector3 separationDirection = directionFromCenter.normalized;
            float separationDistance = Random.Range(0.2f, 0.8f); // Small random separation
            
            // Move shard away from center
            shard.transform.position += separationDirection * separationDistance;
            
            shardsSeparated++;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"HelicopterExplosion: Separated {shardsSeparated} shards");
        }
    }

    /// <summary>
    /// Apply realistic explosion forces to all shards
    /// </summary>
    private void ApplyExplosionForces()
    {
        int forcesApplied = 0;
        int nullShards = 0;
        int nullRigidbodies = 0;
        
        if (enableVerboseLogging)
        {
            Debug.Log($"HelicopterExplosion: Starting to apply forces to {explosionShards.Count} shards");
        }
        
        foreach (ExplosionShard shard in explosionShards)
        {
            if (shard == null)
            {
                nullShards++;
                continue;
            }
            
            if (shard.Rigidbody == null)
            {
                nullRigidbodies++;
                if (enableVerboseLogging)
                {
                    Debug.LogWarning($"HelicopterExplosion: Shard {shard.name} has no Rigidbody!");
                }
                continue;
            }

            // Enable physics
            bool wasKinematic = shard.Rigidbody.isKinematic;
            shard.Rigidbody.isKinematic = false;
            
            // CRITICAL FIX: Ensure proper physics settings with balanced drag
            shard.Rigidbody.useGravity = true;
            shard.Rigidbody.drag = 1.5f; // Moderate air resistance - allows gravity to work but slows pieces
            shard.Rigidbody.angularDrag = 2.0f; // Moderate rotational damping for controlled spinning
            
            // GRAVITY VERIFICATION: Force gravity to be enabled and log it
            if (enableVerboseLogging)
            {
                Debug.Log($"HelicopterExplosion: Shard {shard.name} - Gravity: {shard.Rigidbody.useGravity}, Drag: {shard.Rigidbody.drag}, Mass: {shard.Rigidbody.mass}");
            }

            // Calculate explosion force for this shard
            Vector3 force = CalculateExplosionForce(shard.transform.position, shard.Rigidbody.mass);
            
            if (enableVerboseLogging)
            {
                Debug.Log($"HelicopterExplosion: Applying force {force.magnitude:F1} to shard {shard.name} (was kinematic: {wasKinematic})");
            }
            
            // Apply the force
            shard.Rigidbody.AddForce(force, ForceMode.Impulse);
            
            // Add random torque for realistic spinning
            Vector3 torque = Random.insideUnitSphere * (force.magnitude * 0.1f);
            shard.Rigidbody.AddTorque(torque, ForceMode.Impulse);

            // Activate the shard
            shard.Activate();
            
            forcesApplied++;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"HelicopterExplosion: Applied forces to {forcesApplied} shards (Null shards: {nullShards}, Missing Rigidbodies: {nullRigidbodies})");
        }
        
        // Check if any forces were actually applied
        if (forcesApplied == 0)
        {
            Debug.LogError("HelicopterExplosion: NO FORCES WERE APPLIED! This is why shards aren't moving!");
        }
    }

    /// <summary>
    /// Calculate realistic explosion force for a shard at given position
    /// </summary>
    private Vector3 CalculateExplosionForce(Vector3 shardPosition, float shardMass)
    {
        // Distance from explosion center
        Vector3 directionFromCenter = (shardPosition - explosionCenter);
        float distance = directionFromCenter.magnitude;
        
        // Normalize direction
        Vector3 radialDirection = directionFromCenter.normalized;

        // Improved distance factor with smoother falloff
        float distanceFactor = Mathf.Clamp01(explosionRadius / Mathf.Max(distance, 0.5f));
        // Use a curve for more natural falloff
        distanceFactor = Mathf.Pow(distanceFactor, 0.7f); // Gentler falloff curve
        distanceFactor = Mathf.Clamp(distanceFactor, 0.3f, 1.0f); // Minimum 30% force

        // FIXED: Use base force as the total budget, not per-component
        float totalForceAmount = baseExplosionForce * distanceFactor;

        // Calculate direction components (these are normalized directions, not forces)
        Vector3 primaryDirection = radialDirection;
        
        // Add upward bias to the direction
        primaryDirection += Vector3.up * upwardForceMultiplier;
        
        // Add directional bias if damage direction is specified
        if (damageDirection != Vector3.zero)
        {
            primaryDirection += damageDirection.normalized * directionalForceMultiplier;
        }

        // Add small random component for natural variation
        Vector3 randomDirection = Random.insideUnitSphere * randomnessMultiplier;
        primaryDirection += randomDirection;

        // Normalize the combined direction
        primaryDirection = primaryDirection.normalized;

        // Apply the total force amount in the calculated direction
        Vector3 totalForce = primaryDirection * totalForceAmount;

        // Apply mass scaling (heavier objects get less force)
        float massScale = Mathf.Lerp(1f, massScaling, Mathf.Clamp01(shardMass / 2f));
        totalForce *= massScale;

        // Apply random variation
        float variation = Random.Range(1f - forceVariation, 1f + forceVariation);
        totalForce *= variation;

        // Absolute force limits to prevent excessive forces
        float forceMagnitude = totalForce.magnitude;
        
        // Adjust limits based on whether using inspector values
        float maxAbsoluteForce = useInspectorValuesOnly ? 1000f : 150f; // Allow higher max when testing
        float minAbsoluteForce = useInspectorValuesOnly ? 0.01f : 20f;  // Allow much lower min when testing
        
        float clampedMagnitude = Mathf.Clamp(forceMagnitude, minAbsoluteForce, maxAbsoluteForce);
        
        if (forceMagnitude > 0)
        {
            totalForce = totalForce.normalized * clampedMagnitude;
        }

        // DETAILED DEBUG LOGGING
        if (enableVerboseLogging)
        {
            Debug.Log($"FORCE CALCULATION DEBUG (FIXED):" +
                $"\n  Base Force Budget: {baseExplosionForce}" +
                $"\n  Distance: {distance:F2}, Factor: {distanceFactor:F2}" +
                $"\n  Force Amount: {totalForceAmount:F2}" +
                $"\n  Direction: {primaryDirection}" +
                $"\n  Mass Scale: {massScale:F2}, Variation: {variation:F2}" +
                $"\n  Before Clamp: {forceMagnitude:F2}" +
                $"\n  FINAL FORCE: {totalForce.magnitude:F2}");
        }

        return totalForce;
    }

    /// <summary>
    /// Create visual effects for the explosion
    /// </summary>
    private void CreateVisualEffects()
    {
        // Main explosion flash
        if (explosionFlashPrefab != null)
        {
            GameObject flash = Instantiate(explosionFlashPrefab, explosionCenter, Quaternion.identity);
            Destroy(flash, 2f);
        }

        // Fire and smoke
        if (fireAndSmokePrefab != null)
        {
            GameObject fireSmoke = Instantiate(fireAndSmokePrefab, explosionCenter, Quaternion.identity);
            Destroy(fireSmoke, 10f);
        }

        // Sparks
        if (sparksPrefab != null)
        {
            GameObject sparks = Instantiate(sparksPrefab, explosionCenter, Quaternion.identity);
            Destroy(sparks, 3f);
        }

        // Ground scorch mark
        if (createGroundScorch)
        {
            CreateGroundScorch();
        }
    }

    /// <summary>
    /// Create a scorch mark on the ground
    /// </summary>
    private void CreateGroundScorch()
    {
        // Raycast down to find ground
        RaycastHit hit;
        if (Physics.Raycast(explosionCenter, Vector3.down, out hit, 50f))
        {
            // Create a simple dark circle on the ground
            GameObject scorch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            scorch.name = "ExplosionScorch";
            scorch.transform.position = hit.point + Vector3.up * 0.01f;
            scorch.transform.localScale = new Vector3(explosionRadius * 0.8f, 0.01f, explosionRadius * 0.8f);
            
            // Make it dark
            Renderer renderer = scorch.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.2f, 0.1f, 0.1f, 0.8f);
            }

            // Remove collider
            Destroy(scorch.GetComponent<Collider>());

            // Auto-cleanup
            Destroy(scorch, 30f);
        }
    }

    /// <summary>
    /// Play explosion sound effects
    /// </summary>
    private void PlayExplosionSound()
    {
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound, audioVolume);
        }

        // Play fire crackling sound with delay
        if (fireCracklingSound != null)
        {
            StartCoroutine(PlayDelayedFireSound());
        }
    }

    /// <summary>
    /// Play fire crackling sound with delay
    /// </summary>
    private IEnumerator PlayDelayedFireSound()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (audioSource != null && fireCracklingSound != null)
        {
            audioSource.PlayOneShot(fireCracklingSound, audioVolume * 0.6f);
        }
    }

    /// <summary>
    /// Re-enable colliders after shards have separated to prevent collision gridlock
    /// </summary>
    private IEnumerator EnableCollidersAfterDelay()
    {
        yield return new WaitForSeconds(colliderActivationDelay);
        
        int collidersEnabled = 0;
        
        foreach (ExplosionShard shard in explosionShards)
        {
            if (shard != null && shard.gameObject != null)
            {
                Collider[] colliders = shard.GetComponents<Collider>();
                foreach (Collider col in colliders)
                {
                    col.enabled = true;
                    collidersEnabled++;
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"HelicopterExplosion: Re-enabled {collidersEnabled} colliders after {colliderActivationDelay}s delay");
        }
    }

    /// <summary>
    /// Set damage direction for directional explosion force
    /// </summary>
    public void SetDamageDirection(Vector3 direction)
    {
        damageDirection = direction.normalized;
    }

    /// <summary>
    /// Set custom explosion parameters
    /// </summary>
    public void SetExplosionParameters(float force, float radius, float upwardBias = 0.3f)
    {
        // Only apply if not using inspector values only
        if (!useInspectorValuesOnly)
        {
            baseExplosionForce = force;
            explosionRadius = radius;
            upwardForceMultiplier = upwardBias;
            
            if (showDebugInfo)
            {
                Debug.Log($"HelicopterExplosion: Parameters set by script - Force: {force}, Radius: {radius}, Upward: {upwardBias}");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"HelicopterExplosion: Ignoring script parameters, using inspector values - Force: {baseExplosionForce}, Radius: {explosionRadius}, Upward: {upwardForceMultiplier}");
            }
        }
    }

    /// <summary>
    /// Called by shards when they're destroyed
    /// </summary>
    public void OnShardDestroyed()
    {
        activeShards--;
        
        if (activeShards <= 0)
        {
            CleanupExplosion();
        }
    }

    /// <summary>
    /// Clean up the entire explosion
    /// </summary>
    private void CleanupExplosion()
    {
        if (showDebugInfo)
        {
            Debug.Log("HelicopterExplosion: Cleaning up explosion");
        }

        // Destroy all remaining shards
        foreach (ExplosionShard shard in explosionShards)
        {
            if (shard != null && shard.gameObject != null)
            {
                Destroy(shard.gameObject);
            }
        }

        // Destroy the explosion object
        Destroy(gameObject);
    }

    /// <summary>
    /// Get explosion statistics for debugging
    /// </summary>
    public ExplosionStats GetExplosionStats()
    {
        return new ExplosionStats
        {
            totalShards = explosionShards.Count,
            activeShards = activeShards,
            explosionTime = hasExploded ? Time.time - explosionStartTime : 0f,
            explosionForce = baseExplosionForce,
            explosionRadius = explosionRadius
        };
    }

    void OnDrawGizmosSelected()
    {
        if (!showForceGizmos) return;

        // Draw explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        // Draw damage direction
        if (damageDirection != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, damageDirection * 5f);
        }

        // Draw force vectors for each shard (if exploded)
        if (hasExploded && showDebugInfo)
        {
            Gizmos.color = Color.green;
            foreach (ExplosionShard shard in explosionShards)
            {
                if (shard != null && shard.Rigidbody != null)
                {
                    Vector3 velocity = shard.Rigidbody.velocity;
                    Gizmos.DrawRay(shard.transform.position, velocity * 0.1f);
                }
            }
        }
    }
}

/// <summary>
/// Statistics structure for explosion debugging
/// </summary>
[System.Serializable]
public struct ExplosionStats
{
    public int totalShards;
    public int activeShards;
    public float explosionTime;
    public float explosionForce;
    public float explosionRadius;
}
