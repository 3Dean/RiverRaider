using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float hoverSpeed = 1f;
    public float hoverHeight = 1f;
    public float shootInterval = 2f;
    public GameObject enemyBullet;
    public Transform firePoint;

    private float baseY;
    private float shootTimer;
    public float maxHealth = 100f;
private float currentHealth;

void Start()
{
    baseY = transform.position.y;
    currentHealth = maxHealth;
}

public void TakeDamage(float amount)
{
    currentHealth -= amount;
    if (currentHealth <= 0f)
    {
        Die();
    }
}

void Die()
{
    // Optional: Trigger explosion effect
    Destroy(gameObject);
}

    void Update()
    {
        // Hovering effect
        float newY = baseY + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Simple timed shooting
        shootTimer += Time.deltaTime;
        if (shootTimer >= shootInterval)
        {
            shootTimer = 0;
            if (enemyBullet && firePoint)
{
    GameObject bullet = Instantiate(enemyBullet, firePoint.position, firePoint.rotation);
}

        }
    }
}
