using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public GameObject hitEffectPrefab;

    public float speed = 40f;
    public float damage = 20f;
    public float maxLifetime = 3f;
    private float lifeTimer = 0f;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifetime)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
{
            if (hitEffectPrefab != null)
{
    Instantiate(hitEffectPrefab, transform.position, Quaternion.LookRotation(transform.forward));

}
    if (other.CompareTag("Player"))
        {
            Debug.Log("Hit PlayerShip");

            // new code: grab the health component
    var health = other.GetComponent<PlayerShipHealth>();
    if (health != null)
        health.TakeDamage(damage);
    Destroy(gameObject);
        }
}

}
