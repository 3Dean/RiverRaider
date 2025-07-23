using UnityEngine;

/// <summary>
/// Simple script to test helicopter explosions
/// Attach to any GameObject and use the keyboard shortcuts
/// </summary>
public class ExplosionTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private GameObject helicopterExplosionPrefab;
    [SerializeField] private Vector3 testPosition = Vector3.zero;
    [SerializeField] private bool usePlayerPosition = true;
    
    [Header("Controls")]
    [SerializeField] private KeyCode testExplosionKey = KeyCode.E;
    [SerializeField] private KeyCode testAtPositionKey = KeyCode.R;
    
    private Transform playerTransform;

    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Try to find explosion prefab if not assigned
        if (helicopterExplosionPrefab == null)
        {
            helicopterExplosionPrefab = Resources.Load<GameObject>("HelicopterExplosionPrefab");
        }
    }

    void Update()
    {
        // Test explosion at player position
        if (Input.GetKeyDown(testExplosionKey))
        {
            TestExplosion();
        }

        // Test explosion at fixed position
        if (Input.GetKeyDown(testAtPositionKey))
        {
            TestExplosionAtPosition();
        }
    }

    /// <summary>
    /// Test explosion at player position or camera position
    /// </summary>
    public void TestExplosion()
    {
        Vector3 spawnPosition;
        
        if (usePlayerPosition && playerTransform != null)
        {
            spawnPosition = playerTransform.position + playerTransform.forward * 10f + Vector3.up * 5f;
        }
        else
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                spawnPosition = cam.transform.position + cam.transform.forward * 10f;
            }
            else
            {
                spawnPosition = Vector3.up * 5f;
            }
        }

        CreateExplosion(spawnPosition);
    }

    /// <summary>
    /// Test explosion at fixed position
    /// </summary>
    public void TestExplosionAtPosition()
    {
        CreateExplosion(testPosition);
    }

    /// <summary>
    /// Create explosion at specified position
    /// </summary>
    private void CreateExplosion(Vector3 position)
    {
        if (helicopterExplosionPrefab == null)
        {
            Debug.LogError("ExplosionTester: No helicopter explosion prefab assigned!");
            return;
        }

        GameObject explosion = Instantiate(helicopterExplosionPrefab, position, Quaternion.identity);
        
        // Optional: Set damage direction (simulate being hit from player direction)
        HelicopterExplosion explosionComponent = explosion.GetComponent<HelicopterExplosion>();
        if (explosionComponent != null && playerTransform != null)
        {
            Vector3 damageDirection = (position - playerTransform.position).normalized;
            explosionComponent.SetDamageDirection(damageDirection);
        }

        Debug.Log($"ExplosionTester: Created explosion at {position}");
    }

    void OnGUI()
    {
        if (helicopterExplosionPrefab == null)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(10, 10, 400, 20), "ExplosionTester: No explosion prefab found!");
            GUI.color = Color.white;
            return;
        }

        GUI.Label(new Rect(10, 10, 300, 20), $"Press {testExplosionKey} to test explosion");
        GUI.Label(new Rect(10, 30, 300, 20), $"Press {testAtPositionKey} to test at fixed position");
        
        if (GUI.Button(new Rect(10, 60, 150, 30), "Test Explosion"))
        {
            TestExplosion();
        }
        
        if (GUI.Button(new Rect(170, 60, 150, 30), "Test at Position"))
        {
            TestExplosionAtPosition();
        }
    }
}
