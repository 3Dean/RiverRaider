using UnityEngine;

public class Shooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;

    private float nextFire = 0f;

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            GameObject bullet = BulletPool.Instance.GetBullet();
bullet.transform.position = firePoint.position;
bullet.transform.rotation = firePoint.rotation;
bullet.SetActive(true);

        }
    }
}
