using UnityEngine;

/// <summary>
/// Basic missile projectile component
/// Handles missile movement, damage, and collision
/// </summary>
public class Missile : MonoBehaviour
{
    [Header("Missile Settings")]
    [SerializeField] private float damage = 100f;
    [SerializeField] private float speed = 40f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private LayerMask targetLayers = (1 << 13) | (1 << 0); // Helicopter (13) + Default (0)
    
    [Header("Effects")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private AudioClip explosionSound;
    
    [Header("Trail System")]
    [SerializeField] private MonoBehaviour trailSystem; // Generic trail system reference
    
    private Vector3 direction;
    private float timeAlive = 0f;
    private bool hasExploded = false;
    private Rigidbody rb;
    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Set initial velocity if we have a rigidbody
        if (rb != null && direction != Vector3.zero)
        {
            rb.velocity = direction * speed;
        }
        
        // Initialize trail system - try to find any trail system
        if (trailSystem == null)
        {
            // Try to find trail systems by name
            MonoBehaviour[] components = GetComponentsInChildren<MonoBehaviour>();
            foreach (var component in components)
            {
                string typeName = component.GetType().Name;
                if (typeName == "HybridMissileTrail" || typeName == "RealisticMissileTrail")
                {
                    trailSystem = component;
                    break;
                }
            }
        }
    }

    void Update()
    {
        // Move missile if no rigidbody
        if (rb == null)
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }

        // Handle lifetime
        timeAlive += Time.deltaTime;
        if (timeAlive >= lifetime)
        {
            Explode();
        }
    }

    public void Initialize(float missileSpeed, float missileDamage, Vector3 moveDirection)
    {
        speed = missileSpeed;
        damage = missileDamage;
        direction = moveDirection.normalized;
        
        // Set velocity immediately if rigidbody exists
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }

    /// <summary>
    /// Initialize missile with velocity inheritance from launching platform
    /// </summary>
    public void Initialize(float missileSpeed, float missileDamage, Vector3 moveDirection, Vector3 inheritedVelocity)
    {
        speed = missileSpeed;
        damage = missileDamage;
        direction = moveDirection.normalized;
        
        // Calculate final velocity: missile's own speed + inherited velocity
        Vector3 missileVelocity = direction * speed;
        Vector3 finalVelocity = missileVelocity + inheritedVelocity;
        
        // Set velocity immediately if rigidbody exists
        if (rb != null)
        {
            rb.velocity = finalVelocity;
            Debug.Log($"Missile initialized with velocity inheritance: " +
                     $"Own={missileVelocity.magnitude:F1}, Inherited={inheritedVelocity.magnitude:F1}, " +
                     $"Final={finalVelocity.magnitude:F1}");
        }
        
        // Update speed for non-rigidbody movement
        speed = finalVelocity.magnitude;
        direction = finalVelocity.normalized;
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if we hit a valid target
        if (IsValidTarget(other))
        {
            // Deal damage to target
            DealDamage(other);
            
            // Explode
            Explode();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if we hit a valid target
        if (IsValidTarget(collision.collider))
        {
            // Deal damage to target
            DealDamage(collision.collider);
        }
        
        // Explode on any collision
        Explode();
    }

    private bool IsValidTarget(Collider target)
    {
        // Check if target is in our target layers
        return ((1 << target.gameObject.layer) & targetLayers) != 0;
    }

    private void DealDamage(Collider target)
    {
        // Try different damage interfaces
        var health = target.GetComponent<PlayerShipHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            return;
        }

        var enemyAI = target.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.TakeDamage(damage);
            return;
        }

        // Generic damage interface (if you create one later)
        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            return;
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Stop trail system using reflection to call StopTrail method
        if (trailSystem != null)
        {
            var stopTrailMethod = trailSystem.GetType().GetMethod("StopTrail");
            if (stopTrailMethod != null)
            {
                stopTrailMethod.Invoke(trailSystem, null);
            }
        }

        // Area damage
        if (explosionRadius > 0f)
        {
            DealAreaDamage();
        }

        // Spawn explosion effect
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosion, 5f); // Clean up after 5 seconds
        }

        // Play explosion sound
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
            // Delay destruction to let sound play
            Destroy(gameObject, explosionSound.length);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void DealAreaDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, targetLayers);
        
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider != null)
            {
                // Calculate distance-based damage
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                float damageMultiplier = 1f - (distance / explosionRadius);
                float finalDamage = damage * damageMultiplier;

                // Deal damage
                DealDamage(hitCollider);
            }
        }
    }

    // Properties
    public float Damage => damage;
    public float Speed => speed;
    public Vector3 Direction => direction;

    void OnDrawGizmosSelected()
    {
        // Draw explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        
        // Draw direction
        if (direction != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, direction * 3f);
        }
    }
}

/// <summary>
/// Generic damage interface for future use
/// </summary>
public interface IDamageable
{
    void TakeDamage(float damage);
}
