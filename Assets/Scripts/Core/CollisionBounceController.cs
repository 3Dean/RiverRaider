using UnityEngine;

/// <summary>
/// Collision Bounce Controller - Adds collision detection and bounce-back effects
/// to kinematic movement systems without modifying the core flight controller.
/// Works alongside UnifiedFlightController to provide realistic collision response.
/// </summary>
[RequireComponent(typeof(UnifiedFlightController))]
public class CollisionBounceController : MonoBehaviour
{
    [Header("Collision Detection")]
    [SerializeField] private float detectionDistance = 2f; // How far ahead to check for obstacles
    [SerializeField] private float colliderRadius = 1f; // Radius for sphere collision detection
    [SerializeField] private LayerMask obstacleLayerMask = -1; // What layers to consider obstacles
    
    [Header("Bounce Settings")]
    [SerializeField] private float bounceForce = 10f; // How strong the bounce-back is
    [SerializeField] private float bounceDuration = 0.5f; // How long the bounce effect lasts
    [SerializeField] private float speedReduction = 0.7f; // Speed multiplier after collision (0.7 = 30% speed loss)
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRays = true;
    [SerializeField] private bool enableDebugLogging = true;
    
    // Components
    private UnifiedFlightController flightController;
    private FlightData flightData;
    private Transform aircraftTransform;
    
    // Bounce state
    private bool isBouncing = false;
    private Vector3 bounceDirection = Vector3.zero;
    private float bounceTimer = 0f;
    private float originalSpeed = 0f;
    
    // Collision detection state
    private bool wasColliding = false;
    private float lastCollisionTime = 0f;
    private float collisionCooldown = 1f; // Prevent rapid collision triggers
    
    void Awake()
    {
        // Get required components
        flightController = GetComponent<UnifiedFlightController>();
        flightData = GetComponent<FlightData>();
        aircraftTransform = transform;
        
        if (flightController == null)
        {
            Debug.LogError("CollisionBounceController: UnifiedFlightController component is required!", this);
            enabled = false;
            return;
        }
        
        if (flightData == null)
        {
            Debug.LogError("CollisionBounceController: FlightData component is required!", this);
            enabled = false;
            return;
        }
    }
    
    void Start()
    {
        if (enableDebugLogging)
        {
            Debug.Log($"CollisionBounceController: Initialized on {gameObject.name}");
            Debug.Log($"Detection Distance: {detectionDistance}, Bounce Force: {bounceForce}, Speed Reduction: {speedReduction * 100}%");
        }
    }
    
    void Update()
    {
        float deltaTime = Time.deltaTime;
        
        // Update bounce effect if active
        if (isBouncing)
        {
            UpdateBounceEffect(deltaTime);
        }
        else
        {
            // Only check for collisions when not already bouncing
            CheckForCollisions(deltaTime);
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugRays || aircraftTransform == null) return;
        
        // Draw collision detection sphere
        Gizmos.color = isBouncing ? Color.red : Color.yellow;
        Vector3 checkPosition = aircraftTransform.position + aircraftTransform.forward * detectionDistance;
        Gizmos.DrawWireSphere(checkPosition, colliderRadius);
        
        // Draw forward detection ray
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(aircraftTransform.position, aircraftTransform.forward * detectionDistance);
        
        // Draw bounce direction if bouncing
        if (isBouncing)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(aircraftTransform.position, bounceDirection * 3f);
        }
    }
    
    private void CheckForCollisions(float deltaTime)
    {
        // Don't check too frequently after a collision
        if (Time.time - lastCollisionTime < collisionCooldown)
        {
            return;
        }
        
        // Calculate movement for this frame
        Vector3 forwardDirection = aircraftTransform.forward;
        float currentSpeed = flightData.airspeed;
        float movementDistance = currentSpeed * deltaTime;
        
        // Check for obstacles ahead using sphere cast
        Vector3 checkPosition = aircraftTransform.position + forwardDirection * detectionDistance;
        
        // Use OverlapSphere to detect solid colliders
        Collider[] hitColliders = Physics.OverlapSphere(checkPosition, colliderRadius, obstacleLayerMask);
        
        bool isColliding = false;
        Collider solidCollider = null;
        
        // Look for solid (non-trigger) colliders
        foreach (Collider col in hitColliders)
        {
            if (!col.isTrigger && col.gameObject != gameObject)
            {
                isColliding = true;
                solidCollider = col;
                break;
            }
        }
        
        // Trigger bounce if we hit something solid
        if (isColliding && !wasColliding)
        {
            TriggerBounce(solidCollider);
        }
        
        wasColliding = isColliding;
        
        // Debug logging
        if (enableDebugLogging && isColliding && Time.frameCount % 30 == 0) // Every 0.5 seconds
        {
            Debug.Log($"Collision detected with: {solidCollider?.name} at distance {detectionDistance}");
        }
    }
    
    private void TriggerBounce(Collider hitCollider)
    {
        if (isBouncing) return; // Already bouncing
        
        // Calculate bounce direction (away from the obstacle)
        Vector3 hitPoint = hitCollider.ClosestPoint(aircraftTransform.position);
        Vector3 hitNormal = (aircraftTransform.position - hitPoint).normalized;
        
        // If we can't determine a good normal, bounce backwards
        if (hitNormal.magnitude < 0.1f)
        {
            hitNormal = -aircraftTransform.forward;
        }
        
        // Set bounce parameters
        bounceDirection = hitNormal;
        isBouncing = true;
        bounceTimer = 0f;
        originalSpeed = flightData.airspeed;
        lastCollisionTime = Time.time;
        
        // Reduce speed on impact
        flightData.airspeed *= speedReduction;
        
        // Apply immediate bounce movement
        Vector3 immediateBounceMovemet = bounceDirection * bounceForce * Time.deltaTime;
        aircraftTransform.Translate(immediateBounceMovemet, Space.World);
        
        if (enableDebugLogging)
        {
            Debug.Log($"COLLISION BOUNCE! Hit: {hitCollider.name}, Direction: {bounceDirection}, Speed reduced to: {flightData.airspeed:F1}");
        }
        
        // Trigger damage if this is a fuel barge collision
        if (hitCollider.name.Contains("fuelbarge"))
        {
            // Find and trigger the collision script
            FuelBargeCollision fuelBargeCollision = hitCollider.GetComponent<FuelBargeCollision>();
            if (fuelBargeCollision != null)
            {
                // Manually trigger the collision damage since we're using kinematic movement
                PlayerShipHealth playerHealth = GetComponent<PlayerShipHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(25f); // Standard collision damage
                    Debug.Log("Applied collision damage from fuel barge impact!");
                }
            }
        }
    }
    
    private void UpdateBounceEffect(float deltaTime)
    {
        bounceTimer += deltaTime;
        
        // Calculate bounce intensity (starts strong, fades out)
        float bounceIntensity = Mathf.Lerp(bounceForce, 0f, bounceTimer / bounceDuration);
        
        // Apply bounce movement
        Vector3 bounceMovement = bounceDirection * bounceIntensity * deltaTime;
        aircraftTransform.Translate(bounceMovement, Space.World);
        
        // Check if bounce is complete
        if (bounceTimer >= bounceDuration)
        {
            isBouncing = false;
            bounceTimer = 0f;
            
            if (enableDebugLogging)
            {
                Debug.Log("Bounce effect complete - resuming normal flight");
            }
        }
        
        // Debug logging during bounce
        if (enableDebugLogging && Time.frameCount % 30 == 0)
        {
            Debug.Log($"Bouncing: Intensity={bounceIntensity:F2}, Timer={bounceTimer:F2}/{bounceDuration:F2}");
        }
    }
    
    /// <summary>
    /// Check if the aircraft is currently bouncing (for external systems)
    /// </summary>
    public bool IsBouncing()
    {
        return isBouncing;
    }
    
    /// <summary>
    /// Get the current bounce direction (for external systems)
    /// </summary>
    public Vector3 GetBounceDirection()
    {
        return bounceDirection;
    }
    
    /// <summary>
    /// Manually trigger a bounce effect (for external systems)
    /// </summary>
    public void TriggerManualBounce(Vector3 direction, float force = -1f)
    {
        if (force < 0f) force = bounceForce;
        
        bounceDirection = direction.normalized;
        isBouncing = true;
        bounceTimer = 0f;
        originalSpeed = flightData.airspeed;
        lastCollisionTime = Time.time;
        
        // Reduce speed on impact
        flightData.airspeed *= speedReduction;
        
        if (enableDebugLogging)
        {
            Debug.Log($"Manual bounce triggered! Direction: {bounceDirection}, Force: {force}");
        }
    }
}
