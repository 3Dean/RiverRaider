using UnityEngine;

/// <summary>
/// Poolable bullet with improved performance and better collision handling
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 50f;
    [SerializeField] private float maxLifetime = 3f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private LayerMask hitLayers = -1;
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    
    private float lifeTimer = 0f;
    private Vector3 velocity;
    private bool isActive = false;

    // Properties for external access
    public float Speed { get => speed; set => speed = value; }
    public float Damage { get => damage; set => damage = value; }

    void OnEnable()
    {
        ResetBullet();
    }

    void Update()
    {
        if (!isActive) return;

        // Move bullet using cached velocity for better performance
        transform.position += velocity * Time.deltaTime;

        // Update lifetime
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifetime)
        {
            DeactivateBullet();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        // Check if the collider is on a valid hit layer
        if (((1 << other.gameObject.layer) & hitLayers) == 0) return;

        // Handle hit effects
        SpawnHitEffect();

        // Handle damage to enemies
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        // Handle damage to player (for enemy bullets)
        else if (other.CompareTag("Player"))
        {
            var playerHealth = other.GetComponent<PlayerShipHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }

        DeactivateBullet();
    }

    /// <summary>
    /// Initialize bullet with direction and speed
    /// </summary>
    public void Initialize(Vector3 direction, float bulletSpeed = -1f)
    {
        if (bulletSpeed > 0f) speed = bulletSpeed;
        velocity = direction.normalized * speed;
        isActive = true;
    }

    private void ResetBullet()
    {
        lifeTimer = 0f;
        velocity = transform.forward * speed;
        isActive = true;
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            // Use object pooling for hit effects if available
            var hitEffect = Instantiate(hitEffectPrefab, transform.position, 
                Quaternion.LookRotation(-transform.forward));
            
            // Auto-destroy hit effect if it doesn't have AutoDestroy component
            var autoDestroy = hitEffect.GetComponent<AutoDestroy>();
            if (autoDestroy == null)
            {
                Destroy(hitEffect, 2f);
            }
        }
    }

    private void DeactivateBullet()
    {
        isActive = false;
        
        // Return to pool if pool exists
        if (BulletPool.Instance != null)
        {
            BulletPool.Instance.ReturnBullet(gameObject);
        }
        else
        {
            // Fallback: just deactivate
            gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        isActive = false;
    }
}
