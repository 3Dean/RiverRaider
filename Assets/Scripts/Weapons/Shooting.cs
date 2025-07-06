using UnityEngine;

/// <summary>
/// DEPRECATED - Use WeaponSystemController instead
/// This script is kept for backward compatibility but should be replaced
/// </summary>
[System.Obsolete("Use WeaponSystemController instead")]
public class Shooting : MonoBehaviour
{
    [Header("DEPRECATED - Use WeaponSystemController instead")]
    [SerializeField] private bool enableLegacyShooter = false;
    
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;

    private float nextFire = 0f;

    void Start()
    {
        if (!enableLegacyShooter)
        {
            Debug.LogWarning("Shooting script is deprecated. Use WeaponSystemController instead.", this);
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (!enableLegacyShooter) return;
        
        if (Input.GetKey(KeyCode.Space) && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            GameObject bullet = BulletPool.Instance.GetBullet();
            if (bullet != null)
            {
                bullet.transform.position = firePoint.position;
                bullet.transform.rotation = firePoint.rotation;
                bullet.SetActive(true);
            }
        }
    }
}
