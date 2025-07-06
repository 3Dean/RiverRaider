using UnityEngine;

/// <summary>
/// Main weapon system controller for player ship
/// Handles firing, weapon switching, and ammunition management
/// </summary>
public class WeaponSystemController : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform[] firePoints;
    
    // Public access to fire points for other weapon systems
    public Transform[] FirePoints => firePoints;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float bulletSpeed = 60f;
    [SerializeField] private float bulletDamage = 25f;
    [SerializeField] private bool alternatingFirePoints = true;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSound;
    
    [Header("Effects")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private float muzzleFlashDuration = 0.1f;

    private float nextFireTime;
    private int currentFirePointIndex = 0;
    private bool isFiring = false;

    void Start()
    {
        // Check if MachinegunController is present on this GameObject - if so, completely disable this component
        var machinegunController = GetComponent<MachinegunController>();
        if (machinegunController != null)
        {
            Debug.Log("WeaponSystemController: MachinegunController detected on same GameObject - disabling WeaponSystemController");
            enabled = false;
            return;
        }
        
        Debug.Log("WeaponSystemController: No MachinegunController found - WeaponSystemController is active");
        
        // Subscribe to weapon input events only if no MachinegunController
        WeaponInputController.OnMachinegunTryFire += TryFire;
        WeaponInputController.OnMachinegunFireStart += OnMachinegunStart;
        WeaponInputController.OnMachinegunFireEnd += OnMachinegunEnd;
        
        // Keep legacy support for FlightInputController
        FlightInputController.OnFireWeapon += StartFiring;
    }

    void OnDisable()
    {
        // Unsubscribe from weapon input events
        WeaponInputController.OnMachinegunTryFire -= TryFire;
        WeaponInputController.OnMachinegunFireStart -= OnMachinegunStart;
        WeaponInputController.OnMachinegunFireEnd -= OnMachinegunEnd;
        
        // Legacy support cleanup
        FlightInputController.OnFireWeapon -= StartFiring;
    }

    void Update()
    {
        // Legacy support: Handle continuous firing while space is held
        // Only if no MachinegunController is present to avoid conflicts
        var machinegunController = FindObjectOfType<MachinegunController>();
        if (machinegunController == null && Input.GetKey(KeyCode.Space))
        {
            if (Time.time >= nextFireTime)
            {
                Debug.Log("WeaponSystemController: Firing from Update (legacy Space key support)");
                TryFire();
            }
        }
    }

    private void OnMachinegunStart()
    {
        // Called when machinegun firing starts
        // Could be used for audio/visual effects, muzzle flash start, etc.
        Debug.Log("Machinegun firing started");
    }

    private void OnMachinegunEnd()
    {
        // Called when machinegun firing ends
        // Could be used to stop continuous effects
        Debug.Log("Machinegun firing ended");
    }

    private void StartFiring()
    {
        if (Time.time >= nextFireTime)
        {
            TryFire();
        }
    }

    private void TryFire()
    {
        if (BulletPool.Instance == null)
        {
            Debug.LogWarning("WeaponSystemController: BulletPool not found!", this);
            return;
        }

        if (firePoints == null || firePoints.Length == 0)
        {
            Debug.LogWarning("WeaponSystemController: No fire points assigned!", this);
            return;
        }

        nextFireTime = Time.time + fireRate;

        // Get the current fire point
        Transform currentFirePoint = GetCurrentFirePoint();
        if (currentFirePoint == null) return;

        // Fire bullet
        FireBullet(currentFirePoint);

        // Play effects
        PlayFireEffects(currentFirePoint);

        // Update fire point for alternating fire
        if (alternatingFirePoints && firePoints.Length > 1)
        {
            currentFirePointIndex = (currentFirePointIndex + 1) % firePoints.Length;
        }
    }

    private Transform GetCurrentFirePoint()
    {
        if (firePoints == null || firePoints.Length == 0) return null;
        
        // Ensure index is valid
        if (currentFirePointIndex >= firePoints.Length)
            currentFirePointIndex = 0;
            
        return firePoints[currentFirePointIndex];
    }

    private void FireBullet(Transform firePoint)
    {
        // Get bullet from pool with proper initialization
        GameObject bullet = BulletPool.Instance.GetBullet(
            firePoint.position,
            firePoint.rotation,
            firePoint.forward,
            bulletSpeed
        );

        if (bullet != null)
        {
            // Set bullet damage
            var bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Damage = bulletDamage;
            }
        }
    }

    private void PlayFireEffects(Transform firePoint)
    {
        // Play fire sound
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        // Spawn muzzle flash
        if (muzzleFlashPrefab != null)
        {
            GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(muzzleFlash, muzzleFlashDuration);
        }
    }

    // Public methods for external control
    public void SetFireRate(float newFireRate)
    {
        fireRate = Mathf.Max(0.01f, newFireRate);
    }

    public void SetBulletDamage(float newDamage)
    {
        bulletDamage = Mathf.Max(0f, newDamage);
    }

    public void SetBulletSpeed(float newSpeed)
    {
        bulletSpeed = Mathf.Max(1f, newSpeed);
    }

    // Properties for external access
    public float FireRate => fireRate;
    public float BulletDamage => bulletDamage;
    public float BulletSpeed => bulletSpeed;
    public bool CanFire => Time.time >= nextFireTime;

    void OnValidate()
    {
        // Ensure fire rate is reasonable
        if (fireRate < 0.01f) fireRate = 0.01f;
        if (bulletSpeed < 1f) bulletSpeed = 1f;
        if (bulletDamage < 0f) bulletDamage = 0f;
    }
}
