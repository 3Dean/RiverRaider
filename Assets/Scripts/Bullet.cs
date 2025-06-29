using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject hitEffectPrefab;

    public float speed = 50f;
    public float maxLifetime = 3f;
    private float lifeTimer = 0f;
    public float damage = 25f;

    void OnEnable()
    {
        lifeTimer = 0f; // Reset when bullet is reused
    }

    void Update()
    {
         transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // Increase timer
        lifeTimer += Time.deltaTime;

        // Deactivate if lifetime exceeded
        if (lifeTimer >= maxLifetime)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hitEffectPrefab != null)
{
    Instantiate(hitEffectPrefab, transform.position, Quaternion.LookRotation(-transform.forward));

}

        if (other.CompareTag("Enemy"))
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            gameObject.SetActive(false);
        }
    }
}
