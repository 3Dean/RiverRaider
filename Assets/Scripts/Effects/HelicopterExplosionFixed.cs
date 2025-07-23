using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// FIXED VERSION: Realistic helicopter explosion system with proper physics
/// This version addresses the floating/blasting issues by ensuring proper force application and physics settings
/// </summary>
public class HelicopterExplosionFixed : MonoBehaviour
{
    [Header("Explosion Parameters - FIXED")]
    [SerializeField] private float baseExplosionForce = 75f; // Realistic force for natural movement
    [SerializeField] private float upwardForceMultiplier = 0.3f; // Moderate upward bias
    [SerializeField] private float directionalForceMultiplier = 0.2f; // Subtle directional force
    [SerializeField] private float randomnessMultiplier = 0.1f; // Minimal randomness
    [SerializeField] private float explosionRadius = 5f;
    
    [Header("Physics Settings - FIXED")]
    [SerializeField] private float shardMass = 2.0f; // Increased mass for stability
    [SerializeField] private float shardDrag = 1.5f; // Proper air resistance
    [SerializeField] private float shardAngularDrag = 2.0f; // Rotational damping
    [SerializeField] private bool forceGravityEnabled = true; // Ensure gravity works
    [SerializeField] private float forceVariation = 0.1f; // Minimal variation
    [SerializeField] private float colliderActivationDelay = 1.0f; // Delay before colliders activate
    
    [Header("Cleanup")]
    [SerializeField] private float shardLifetime = 15f;
    [SerializeField] private float cleanupDistance = 200f;
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
    
    [Header("Debug - FIXED")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool enableVerboseLogging = false; // Reduced logging for performance
    [SerializeField] private bool showForceGizmos = false;

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
        // Initialize all shards with FIXED physics settings
        InitializeShardsFixed();
        
        // Start explosion immediately
        if (!hasExploded)
        {
            StartCoroutine(ExecuteExplosionFixed());
        }
    }

    void Update()
    {
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

        // Handle lifetime cleanup
        if (Time.time - explosionStartTime > shardLifetime)
        {
            CleanupExplosion();
        }
    }

    /// <summary>
    /// FIXED: Initialize all explosion shards with proper physics settings
    /// </summary>
    private void InitializeShardsFixed()
    {
        // Find all rigidbodies in children (the 51 shards)
        Rigidbody[] shardRigidbodies = GetComponentsInChildren<Rigidbody>();
        
        foreach (Rigidbody rb in shardRigidbodies)
        {
            if (rb.transform == transform) continue; // Skip parent

            // CRITICAL FIX: Apply proper physics settings to each shard
            ConfigureShardPhysics(rb);

            // Add ExplosionShard component if not present
            ExplosionShard shard = rb.GetComponent<ExplosionShard>();
            if (shard == null)
            {
                shard = rb.gameObject.AddComponent<ExplosionShard>();
            }

            // Initialize the shard
            shard.Initialize(null, metalCollisionSounds); // Pass null for parent to avoid circular reference
            explosionShards.Add(shard);

            // Initially disable physics until explosion
            rb.isKinematic = true;
            
            // CRITICAL FIX: Disable all colliders initially to prevent collision gridlock
            Collider[] colliders = rb.GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
        }

        activeShards = explosionShards.Count;

        if (showDebugInfo)
        {
            Debug.Log($"HelicopterExplosionFixed: Initialized {activeShards} shards with FIXED physics settings");
        }
    }

    /// <summary>
    /// CRITICAL FIX: Configure proper physics settings for each shard
    /// </summary>
    private void ConfigureShardPhysics(Rigidbody rb)
    {
        // Set proper mass for stability
        rb.mass = shardMass;
        
        // Set proper drag for realistic air resistance
        rb.drag = shardDrag;
        rb.angularDrag = shardAngularDrag;
        
        // FORCE gravity to be enabled
        rb.useGravity = forceGravityEnabled;
        
        // Ensure no constraints
        rb.constraints = RigidbodyConstraints.None;
        
        // Set reasonable center of mass
        rb.centerOfMass = Vector3.zero;
        
        // Remove any physics materials that might cause bouncing
        Collider collider = rb.GetComponent<Collider>();
        if (collider != null)
        {
            collider.material = null; // Remove any bouncy materials
        }
        
        if (enableVerboseLogging)
        {
            Debug.Log($"PHYSICS CONFIGURED: {rb.name} - Mass:{rb.mass}, Drag:{rb.drag}, Gravity:{rb.useGravity}");
        }
    }

    /// <summary>
    /// FIXED: Execute the explosion sequence with proper force application
    /// </summary>
    private IEnumerator ExecuteExplosionFixed()
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
        
        // Apply FIXED explosion forces to all shards
        ApplyExplosionForcesFixed();

        // Start coroutine to re-enable colliders after shards have separated
        StartCoroutine(EnableCollidersAfterDelay());

        if (showDebugInfo)
        {
            Debug.Log($"HelicopterExplosionFixed: Explosion executed with {activeShards} shards using FIXED force calculation");
        }
    }

    /// <summary>
    /// Separate shards initially to prevent collision gridlock
    /// </summary>
    private void SeparateShards()
    {
        int shardsSeparated = 0;
        
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
            float separationDistance = Random.Range(0.2f, 0.8f);
            
            // Move shard away from center
            shard.transform.position += separationDirection * separationDistance;
            
            shardsSeparated++;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"HelicopterExplosionFixed: Separated {shardsSeparated} shards");
        }
    }

    /// <summary>
    /// FIXED: Apply realistic explosion forces with proper calculation
    /// </summary>
    private void ApplyExplosionForcesFixed()
    {
        int forcesApplied = 0;
        
        if (showDebugInfo)
        {
            Debug.Log($"HelicopterExplosionFixed: Applying FIXED forces (Base Force: {baseExplosionForce}) to {explosionShards.Count} shards");
        }
        
        foreach (ExplosionShard shard in explosionShards)
        {
            if (shard == null || shard.Rigidbody == null) continue;

            // Enable physics
            shard.Rigidbody.isKinematic = false;
            
            // DOUBLE-CHECK: Ensure physics settings are correct
            VerifyShardPhysics(shard.Rigidbody);

            // Calculate FIXED explosion force for this shard
            Vector3 force = CalculateExplosionForceFixed(shard.transform.position, shard.Rigidbody.mass);
            
            if (enableVerboseLogging)
            {
                Debug.Log($"HelicopterExplosionFixed: Applying force {force.magnitude:F1} to shard {shard.name}");
            }
            
            // Apply the force using ForceMode.Impulse for immediate effect
            shard.Rigidbody.AddForce(force, ForceMode.Impulse);
            
            // Add controlled random torque for realistic spinning
            Vector3 torque = Random.insideUnitSphere * (force.magnitude * 0.05f); // Reduced torque multiplier
            shard.Rigidbody.AddTorque(torque, ForceMode.Impulse);

            // Activate the shard
            shard.Activate();
            
            forcesApplied++;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"HelicopterExplosionFixed: Applied FIXED forces to {forcesApplied} shards");
        }
    }

    /// <summary>
    /// CRITICAL FIX: Verify and correct physics settings on each shard
    /// </summary>
    private void VerifyShardPhysics(Rigidbody rb)
    {
        bool needsCorrection = false;
        
        if (rb.mass != shardMass)
        {
            rb.mass = shardMass;
            needsCorrection = true;
        }
        
        if (rb.drag != shardDrag)
        {
            rb.drag = shardDrag;
            needsCorrection = true;
        }
        
        if (rb.angularDrag != shardAngularDrag)
        {
            rb.angularDrag = shardAngularDrag;
            needsCorrection = true;
        }
        
        if (!rb.useGravity)
        {
            rb.useGravity = true;
            needsCorrection = true;
        }
        
        if (needsCorrection && enableVerboseLogging)
        {
            Debug.LogWarning($"PHYSICS CORRECTED: {rb.name} had incorrect settings - now fixed");
        }
    }

    /// <summary>
    /// FIXED: Calculate realistic explosion force with proper limits
    /// </summary>
    private Vector3 CalculateExplosionForceFixed(Vector3 shardPosition, float shardMass)
    {
        // Distance from explosion center
        Vector3 directionFromCenter = (shardPosition - explosionCenter);
        float distance = Mathf.Max(directionFromCenter.magnitude, 0.5f); // Minimum distance to prevent division by zero
        
        // Normalize direction
        Vector3 radialDirection = directionFromCenter.normalized;

        // FIXED: Proper distance falloff calculation
        float distanceFactor = explosionRadius / (distance + 1f); // +1 to prevent extreme amplification
        distanceFactor = Mathf.Clamp(distanceFactor, 0.2f, 1.0f); // Reasonable limits
        
        // Base radial force
        Vector3 radialForce = radialDirection * baseExplosionForce * distanceFactor;

        // Add controlled upward bias
        Vector3 upwardForce = Vector3.up * baseExplosionForce * upwardForceMultiplier * distanceFactor;

        // Add directional force if specified
        Vector3 directionalForce = Vector3.zero;
        if (damageDirection != Vector3.zero)
        {
            directionalForce = damageDirection.normalized * baseExplosionForce * directionalForceMultiplier * distanceFactor;
        }

        // Add minimal randomness
        Vector3 randomForce = Random.insideUnitSphere * baseExplosionForce * randomnessMultiplier * distanceFactor;

        // Combine all forces
        Vector3 totalForce = radialForce + upwardForce + directionalForce + randomForce;

        // FIXED: Apply reasonable mass scaling (heavier objects get proportionally less force)
        float massScale = 1f / Mathf.Sqrt(shardMass); // Square root scaling for more realistic physics
        totalForce *= massScale;

        // Apply minimal variation
        float variation = Random.Range(1f - forceVariation, 1f + forceVariation);
        totalForce *= variation;

        // FIXED: Clamp to reasonable force magnitude
        float maxForce = baseExplosionForce * 1.5f; // Maximum 150% of base force
        if (totalForce.magnitude > maxForce)
        {
            totalForce = totalForce.normalized * maxForce;
        }

        if (enableVerboseLogging)
        {
            Debug.Log($"FIXED FORCE CALC: Distance:{distance:F1}, Factor:{distanceFactor:F2}, Mass Scale:{massScale:F2}, Final Force:{totalForce.magnitude:F1}");
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
    /// Re-enable colliders after shards have separated
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
            Debug.Log($"HelicopterExplosionFixed: Re-enabled {collidersEnabled} colliders after {colliderActivationDelay}s delay");
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
        baseExplosionForce = force;
        explosionRadius = radius;
        upwardForceMultiplier = upwardBias;
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
            Debug.Log("HelicopterExplosionFixed: Cleaning up explosion");
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
    }
}
