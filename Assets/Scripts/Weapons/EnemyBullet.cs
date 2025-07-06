using UnityEngine;

/// <summary>
/// DEPRECATED - Use the unified Bullet class instead
/// EnemyBullet functionality has been merged into the main Bullet class
/// </summary>
[System.Obsolete("Use the unified Bullet class instead")]
public class EnemyBullet : MonoBehaviour
{
    [Header("DEPRECATED - Use unified Bullet class instead")]
    [SerializeField] private bool enableLegacyEnemyBullet = false;
    
    public GameObject hitEffectPrefab;
    public float speed = 40f;
    public float damage = 20f;
    public float maxLifetime = 3f;
    private float lifeTimer = 0f;

    void Start()
    {
        if (!enableLegacyEnemyBullet)
        {
            Debug.LogWarning("EnemyBullet is deprecated. Use the unified Bullet class instead.", this);
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (!enableLegacyEnemyBullet) return;
        
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifetime)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!enableLegacyEnemyBullet) return;
        
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.LookRotation(transform.forward));
        }
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("Hit PlayerShip");

            // Grab the health component
            var health = other.GetComponent<PlayerShipHealth>();
            if (health != null)
                health.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
