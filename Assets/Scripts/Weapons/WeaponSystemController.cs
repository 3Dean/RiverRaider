// Assets/Scripts/Weapons/WeaponSystemController.cs
using UnityEngine;

public class WeaponSystemController : MonoBehaviour {
    [SerializeField] Transform firePoint;
    [SerializeField] float fireRate = 0.2f;
    private float nextFire;

    void OnEnable() => FlightInputController.OnFireWeapon += TryFire;
    void OnDisable() => FlightInputController.OnFireWeapon -= TryFire;

    void TryFire() {
        if (Time.time < nextFire) return;
        nextFire = Time.time + fireRate;
        var bullet = BulletPool.Instance.GetBullet();
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        bullet.SetActive(true);
    }
}
