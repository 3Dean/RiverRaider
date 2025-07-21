using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Efficient bullet pool manager with automatic expansion and cleanup
/// </summary>
public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int initialPoolSize = 50;
    [SerializeField] private int maxPoolSize = 200;
    [SerializeField] private bool allowExpansion = true;
    
    [Header("Organization")]
    [SerializeField] private Transform poolParent;

    private Queue<GameObject> availableBullets;
    private List<GameObject> allBullets;
    private int currentPoolSize;

    void Awake()
    {
        // Singleton pattern with null check
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        InitializePool();
    }

    void Start()
    {
        // Create pool parent if not assigned
        if (poolParent == null)
        {
            GameObject poolContainer = new GameObject("BulletPool");
            poolContainer.transform.SetParent(transform);
            poolParent = poolContainer.transform;
        }
    }

    private void InitializePool()
    {
        availableBullets = new Queue<GameObject>(initialPoolSize);
        allBullets = new List<GameObject>(initialPoolSize);
        currentPoolSize = 0;

        // Pre-populate the pool
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewBullet();
        }
    }

    private GameObject CreateNewBullet()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("BulletPool: bulletPrefab is not assigned!", this);
            return null;
        }

        GameObject bullet = Instantiate(bulletPrefab, poolParent);
        bullet.SetActive(false);
        bullet.name = $"Bullet_{currentPoolSize:D3}";
        
        availableBullets.Enqueue(bullet);
        allBullets.Add(bullet);
        currentPoolSize++;
        
        return bullet;
    }

    /// <summary>
    /// Get a bullet from the pool. Returns null if pool is exhausted and expansion is disabled.
    /// </summary>
    public GameObject GetBullet()
    {
        // Return available bullet if any
        if (availableBullets.Count > 0)
        {
            return availableBullets.Dequeue();
        }

        // Try to expand pool if allowed
        if (allowExpansion && currentPoolSize < maxPoolSize)
        {
            return CreateNewBullet();
        }

        // Pool exhausted - try to find an inactive bullet
        foreach (GameObject bullet in allBullets)
        {
            if (bullet != null && !bullet.activeInHierarchy)
            {
                return bullet;
            }
        }

        Debug.LogWarning("BulletPool: No bullets available and pool expansion is disabled or at max capacity!");
        return null;
    }

    /// <summary>
    /// Get a bullet and initialize it with position, rotation, and direction
    /// </summary>
    public GameObject GetBullet(Vector3 position, Quaternion rotation, Vector3 direction, float speed = -1f)
    {
        GameObject bullet = GetBullet();
        if (bullet != null)
        {
            bullet.transform.position = position;
            bullet.transform.rotation = rotation;
            bullet.SetActive(true);
            
            // Initialize bullet if it has the Bullet component
            var bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Initialize(direction, speed);
            }
        }
        return bullet;
    }

    /// <summary>
    /// Get a bullet and initialize it with platform velocity inheritance (realistic physics)
    /// </summary>
    public GameObject GetBulletWithPlatformVelocity(Vector3 position, Quaternion rotation, Vector3 direction, Vector3 platformVelocity, float speed = -1f)
    {
        GameObject bullet = GetBullet();
        if (bullet != null)
        {
            bullet.transform.position = position;
            bullet.transform.rotation = rotation;
            bullet.SetActive(true);
            
            // Initialize bullet with platform velocity inheritance
            var bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.InitializeWithPlatformVelocity(direction, platformVelocity, speed);
            }
        }
        return bullet;
    }

    /// <summary>
    /// Return a bullet to the pool (called automatically when bullet is deactivated)
    /// </summary>
    public void ReturnBullet(GameObject bullet)
    {
        if (bullet != null && allBullets.Contains(bullet))
        {
            bullet.SetActive(false);
            availableBullets.Enqueue(bullet);
        }
    }

    /// <summary>
    /// Get pool statistics for debugging
    /// </summary>
    public void GetPoolStats(out int total, out int available, out int active)
    {
        total = currentPoolSize;
        available = availableBullets.Count;
        active = total - available;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Debug info in inspector
    void OnValidate()
    {
        if (maxPoolSize < initialPoolSize)
            maxPoolSize = initialPoolSize;
    }

#if UNITY_EDITOR
    [Header("Debug Info (Runtime Only)")]
    [SerializeField, HideInInspector] private int debugTotalBullets;
    [SerializeField, HideInInspector] private int debugAvailableBullets;
    [SerializeField, HideInInspector] private int debugActiveBullets;

    void Update()
    {
        if (Application.isPlaying)
        {
            GetPoolStats(out debugTotalBullets, out debugAvailableBullets, out debugActiveBullets);
        }
    }
#endif
}
