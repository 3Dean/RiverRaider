using UnityEngine;

/// <summary>
/// Enemy AI with improved performance, pooled bullets, and better state management
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float hoverSpeed = 1f;
    [SerializeField] private float hoverHeight = 1f;
    
    [Header("Combat")]
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float bulletSpeed = 30f;
    [SerializeField] private float bulletDamage = 10f;
    
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject explosionEffect;
    
    [Header("Targeting")]
    [SerializeField] private float detectionRange = 50f;
    [SerializeField] private LayerMask playerLayer = 1;

    private float baseY;
    private float shootTimer;
    private float currentHealth;
    private Transform playerTarget;
    private bool isDead = false;

    // Cached components for performance
    private Transform cachedTransform;

    void Awake()
    {
        cachedTransform = transform;
    }

    void Start()
    {
        baseY = cachedTransform.position.y;
        currentHealth = maxHealth;
        shootTimer = Random.Range(0f, shootInterval); // Randomize initial shoot timer
        
        // Find player target
        FindPlayerTarget();
    }

    void Update()
    {
        if (isDead) return;

        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        // Hovering effect with cached transform
        float newY = baseY + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        Vector3 currentPos = cachedTransform.position;
        cachedTransform.position = new Vector3(currentPos.x, newY, currentPos.z);
    }

    private void HandleShooting()
    {
        shootTimer += Time.deltaTime;
        
        if (shootTimer >= shootInterval && CanShoot())
        {
            shootTimer = 0f;
            Shoot();
        }
    }

    private bool CanShoot()
    {
        if (firePoint == null) return false;
        if (playerTarget == null) 
        {
            FindPlayerTarget();
            return playerTarget != null;
        }

        // Check if player is in range
        float distanceToPlayer = Vector3.Distance(cachedTransform.position, playerTarget.position);
        return distanceToPlayer <= detectionRange;
    }

    private void Shoot()
    {
        if (BulletPool.Instance == null)
        {
            Debug.LogWarning("EnemyAI: BulletPool not found! Cannot shoot.", this);
            return;
        }

        // Calculate direction to player
        Vector3 shootDirection = firePoint.forward;
        if (playerTarget != null)
        {
            shootDirection = (playerTarget.position - firePoint.position).normalized;
        }

        // Get bullet from pool
        GameObject bullet = BulletPool.Instance.GetBullet(
            firePoint.position, 
            Quaternion.LookRotation(shootDirection), 
            shootDirection, 
            bulletSpeed
        );

        if (bullet != null)
        {
            // Set bullet damage if it has the Bullet component
            var bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Damage = bulletDamage;
            }
        }
    }

    private void FindPlayerTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);
        
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void RecoverHealth(float amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    private void Die()
    {
        if (isDead) return;
        
        isDead = true;

        // Spawn explosion effect
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, cachedTransform.position, cachedTransform.rotation);
            
            // Auto-destroy explosion if it doesn't have AutoDestroy component
            var autoDestroy = explosion.GetComponent<AutoDestroy>();
            if (autoDestroy == null)
            {
                Destroy(explosion, 3f);
            }
        }

        // Destroy enemy
        Destroy(gameObject);
    }

    // Properties for external access
    public float Health => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw fire point
        if (firePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(firePoint.position, 0.5f);
            Gizmos.DrawRay(firePoint.position, firePoint.forward * 5f);
        }
    }
}
