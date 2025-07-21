using UnityEngine;

/// <summary>
/// Specialized machinegun controller with rapid-fire capabilities
/// Integrates with WeaponInputController for left mouse button firing
/// </summary>
public class MachinegunController : MonoBehaviour
{
    [Header("Machinegun Settings")]
    [SerializeField] private float machinegunFireRate = 0.1f; // 10 rounds per second
    [SerializeField] private float machinegunDamage = 15f;
    [SerializeField] private float machinegunSpeed = 80f;
    [SerializeField] private Transform[] machinegunFirePoints;
    
    [Header("Velocity Inheritance")]
    [SerializeField] private bool enableVelocityInheritance = true;
    [SerializeField] private float velocityInheritanceMultiplier = 1.0f; // Adjust for gameplay balance
    
    [Header("Audio")]
    [SerializeField] private AudioSource machinegunAudioSource;
    [SerializeField] private AudioClip machinegunFireSound;
    [SerializeField] private AudioClip machinegunStartSound;
    [SerializeField] private AudioClip machinegunEndSound;
    
    [Header("Effects")]
    [SerializeField] private GameObject machinegunMuzzleFlash;
    [SerializeField] private float muzzleFlashDuration = 0.05f;
    
    [Header("Overheating (Optional)")]
    [SerializeField] private bool enableOverheating = false;
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float heatPerShot = 2f;
    [SerializeField] private float coolingRate = 10f;
    [SerializeField] private float overheatThreshold = 80f;

    private float nextFireTime;
    private int currentFirePointIndex = 0;
    private bool isFiring = false;
    private float currentHeat = 0f;
    private bool isOverheated = false;

    // Reference to the main weapon system
    private WeaponSystemController weaponSystem;

    void Awake()
    {
        weaponSystem = GetComponent<WeaponSystemController>();
    }

    // Event subscriptions removed - now controlled directly by WeaponManager

    void Update()
    {
        HandleOverheating();
    }

    private void HandleMachinegunFiring()
    {
        if (Time.time >= nextFireTime && !isOverheated)
        {
            FireMachinegun();
        }
    }

    private void OnMachinegunStart()
    {
        isFiring = true;
        
        // Play start sound
        if (machinegunAudioSource != null && machinegunStartSound != null)
        {
            machinegunAudioSource.PlayOneShot(machinegunStartSound);
        }
    }

    private void OnMachinegunEnd()
    {
        isFiring = false;
        
        // Play end sound
        if (machinegunAudioSource != null && machinegunEndSound != null)
        {
            machinegunAudioSource.PlayOneShot(machinegunEndSound);
        }
    }

    private void FireMachinegun()
    {
        if (BulletPool.Instance == null)
        {
            Debug.LogWarning("MachinegunController: BulletPool not found!", this);
            return;
        }

        // Use machinegun fire points if assigned, otherwise fall back to weapon system fire points
        Transform[] firePointsToUse = machinegunFirePoints;
        if (firePointsToUse == null || firePointsToUse.Length == 0)
        {
            if (weaponSystem != null)
            {
                firePointsToUse = weaponSystem.FirePoints;
            }
        }

        if (firePointsToUse == null || firePointsToUse.Length == 0)
        {
            Debug.LogWarning("MachinegunController: No fire points available (neither machinegun nor weapon system fire points)!", this);
            return;
        }

        nextFireTime = Time.time + machinegunFireRate;

        // Get current fire point
        Transform firePoint = GetCurrentFirePoint();
        if (firePoint == null) return;

        // Fire bullet
        FireBullet(firePoint);

        // Play effects
        PlayMachinegunEffects(firePoint);

        // Handle overheating
        if (enableOverheating)
        {
            currentHeat += heatPerShot;
            if (currentHeat >= overheatThreshold)
            {
                isOverheated = true;
            }
        }

        // Alternate fire points
        if (firePointsToUse.Length > 1)
        {
            currentFirePointIndex = (currentFirePointIndex + 1) % firePointsToUse.Length;
        }
    }

    private Transform GetCurrentFirePoint()
    {
        // Use machinegun fire points if assigned, otherwise fall back to weapon system fire points
        Transform[] firePointsToUse = machinegunFirePoints;
        if (firePointsToUse == null || firePointsToUse.Length == 0)
        {
            if (weaponSystem != null)
            {
                firePointsToUse = weaponSystem.FirePoints;
            }
        }
        
        if (firePointsToUse == null || firePointsToUse.Length == 0) return null;
        
        if (currentFirePointIndex >= firePointsToUse.Length)
            currentFirePointIndex = 0;
            
        return firePointsToUse[currentFirePointIndex];
    }

    private void FireBullet(Transform firePoint)
    {
        GameObject bullet;
        
        if (enableVelocityInheritance)
        {
            // Get aircraft velocity for realistic physics
            Vector3 aircraftVelocity = GetAircraftVelocity();
            
            // Use velocity inheritance bullet spawning
            bullet = BulletPool.Instance.GetBulletWithPlatformVelocity(
                firePoint.position,
                firePoint.rotation,
                firePoint.forward,
                aircraftVelocity,
                machinegunSpeed
            );
        }
        else
        {
            // Use traditional bullet spawning (no velocity inheritance)
            bullet = BulletPool.Instance.GetBullet(
                firePoint.position,
                firePoint.rotation,
                firePoint.forward,
                machinegunSpeed
            );
        }

        if (bullet != null)
        {
            var bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Damage = machinegunDamage;
            }
        }
    }

    private Vector3 GetAircraftVelocity()
    {
        // Try to get velocity from UnifiedFlightController
        var flightController = GetComponentInParent<UnifiedFlightController>();
        if (flightController != null)
        {
            var flightData = flightController.GetComponent<FlightData>();
            if (flightData != null)
            {
                // Convert airspeed (MPH) to Unity units per second
                // Assuming 1 Unity unit = 1 meter, and using realistic conversion
                float speedInUnitsPerSecond = flightData.airspeed * 0.44704f; // MPH to m/s conversion
                
                // Apply velocity inheritance multiplier for gameplay balance
                speedInUnitsPerSecond *= velocityInheritanceMultiplier;
                
                // Get aircraft's forward direction and calculate velocity vector
                Vector3 aircraftVelocity = transform.forward * speedInUnitsPerSecond;
                
                return aircraftVelocity;
            }
        }
        
        // Fallback: try to get velocity from rigidbody
        var rb = GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            return rb.velocity * velocityInheritanceMultiplier;
        }
        
        // Final fallback: no velocity inheritance
        Debug.LogWarning("MachinegunController: Could not find aircraft velocity source. Velocity inheritance disabled for this shot.");
        return Vector3.zero;
    }

    private void PlayMachinegunEffects(Transform firePoint)
    {
        // Play fire sound
        if (machinegunAudioSource != null && machinegunFireSound != null)
        {
            machinegunAudioSource.PlayOneShot(machinegunFireSound);
        }

        // Spawn muzzle flash
        if (machinegunMuzzleFlash != null)
        {
            GameObject muzzleFlash = Instantiate(machinegunMuzzleFlash, firePoint.position, firePoint.rotation);
            Destroy(muzzleFlash, muzzleFlashDuration);
        }
    }

    private void HandleOverheating()
    {
        if (!enableOverheating) return;

        // Cool down when not firing
        if (!isFiring && currentHeat > 0f)
        {
            currentHeat -= coolingRate * Time.deltaTime;
            currentHeat = Mathf.Max(currentHeat, 0f);
        }

        // Check if cooled down enough to stop overheating
        if (isOverheated && currentHeat <= (overheatThreshold * 0.5f))
        {
            isOverheated = false;
        }
    }

    // Public methods for WeaponManager control
    public void StartFiring()
    {
        OnMachinegunStart();
    }

    public void StopFiring()
    {
        OnMachinegunEnd();
    }

    public void TryFire()
    {
        HandleMachinegunFiring();
    }

    // Public methods for external access
    public void SetMachinegunFireRate(float newFireRate)
    {
        machinegunFireRate = Mathf.Max(0.01f, newFireRate);
    }

    public void SetMachinegunDamage(float newDamage)
    {
        machinegunDamage = Mathf.Max(0f, newDamage);
    }

    // Properties
    public float FireRate => machinegunFireRate;
    public float Damage => machinegunDamage;
    public float Speed => machinegunSpeed;
    public bool IsFiring => isFiring;
    public bool IsOverheated => isOverheated;
    public float HeatLevel => enableOverheating ? (currentHeat / maxHeat) : 0f;

    void OnValidate()
    {
        if (machinegunFireRate < 0.01f) machinegunFireRate = 0.01f;
        if (machinegunDamage < 0f) machinegunDamage = 0f;
        if (machinegunSpeed < 1f) machinegunSpeed = 1f;
        
        if (enableOverheating)
        {
            if (maxHeat <= 0f) maxHeat = 100f;
            if (heatPerShot <= 0f) heatPerShot = 1f;
            if (coolingRate <= 0f) coolingRate = 5f;
            if (overheatThreshold <= 0f || overheatThreshold > maxHeat) 
                overheatThreshold = maxHeat * 0.8f;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw fire points
        if (machinegunFirePoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform firePoint in machinegunFirePoints)
            {
                if (firePoint != null)
                {
                    Gizmos.DrawWireSphere(firePoint.position, 0.2f);
                    Gizmos.DrawRay(firePoint.position, firePoint.forward * 3f);
                }
            }
        }
    }
}
