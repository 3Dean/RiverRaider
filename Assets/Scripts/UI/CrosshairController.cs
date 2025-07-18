using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Advanced crosshair controller with dynamic features for machine gun targeting.
/// Shows where fire points are actually aiming with visual feedback.
/// </summary>
public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair UI")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private RectTransform crosshairTransform;
    [SerializeField] private bool useSVGImage = false; // Set to true if using Vector Graphics SVGImage
    
    [Header("Weapon System References")]
    [SerializeField] private MachinegunController machinegunController;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform[] firePoints; // Will auto-find from machinegun if not set
    
    [Header("Targeting Settings")]
    [SerializeField] private float maxTargetingRange = 100f;
    [SerializeField] private LayerMask enemyLayers = 1 << 8; // Layer 8 for enemies
    [SerializeField] private LayerMask obstacleLayer = 1; // Default layer for obstacles
    [SerializeField] private bool showDebugRays = true;
    
    [Header("Dynamic Color Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color enemyTargetColor = Color.red;
    [SerializeField] private Color noTargetColor = Color.gray;
    [SerializeField] private float colorTransitionSpeed = 5f;
    
    [Header("Dynamic Size Settings")]
    [SerializeField] private Vector2 baseSize = new Vector2(50f, 50f);
    [SerializeField] private Vector2 maxSize = new Vector2(80f, 80f);
    [SerializeField] private Vector2 minSize = new Vector2(30f, 30f);
    [SerializeField] private float sizeTransitionSpeed = 3f;
    [SerializeField] private float accuracyRange = 50f; // Range for maximum accuracy
    
    [Header("Recoil Animation")]
    [SerializeField] private float recoilIntensity = 10f;
    [SerializeField] private float recoilDuration = 0.1f;
    [SerializeField] private float recoilRecoverySpeed = 8f;
    [SerializeField] private AnimationCurve recoilCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    // Private state variables
    private Canvas parentCanvas;
    private bool isTargetingEnemy = false;
    private bool hasValidTarget = false;
    private float currentTargetDistance = 0f;
    private Vector3 currentAimPoint = Vector3.zero;
    private Color targetColor;
    private Vector2 targetSize;
    private Vector2 recoilOffset = Vector2.zero;
    private bool isRecoiling = false;
    private Coroutine recoilCoroutine;
    
    // Cached components
    private WeaponManager weaponManager;
    
    void Start()
    {
        InitializeComponents();
        SetupEventListeners();
    }
    
    private void InitializeComponents()
    {
        // Get canvas reference
        parentCanvas = GetComponentInParent<Canvas>();
        
        // Auto-find crosshair image if not assigned
        if (crosshairImage == null)
        {
            crosshairImage = GetComponent<Image>();
            
            // Note: SVGImage from Vector Graphics package works with regular Image component
            if (crosshairImage == null)
            {
                Debug.LogWarning("CrosshairController: No Image component found. Make sure crosshair GameObject has an Image or SVGImage component.");
            }
        }
        
        if (crosshairTransform == null)
            crosshairTransform = GetComponent<RectTransform>();
        
        // Auto-find camera if not assigned
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        // Auto-find weapon components
        if (machinegunController == null)
            machinegunController = FindObjectOfType<MachinegunController>();
        
        weaponManager = FindObjectOfType<WeaponManager>();
        
        // Get fire points from machinegun controller
        if (firePoints == null || firePoints.Length == 0)
        {
            if (machinegunController != null)
            {
                // Access fire points through reflection or public property
                var weaponSystem = machinegunController.GetComponent<WeaponSystemController>();
                if (weaponSystem != null && weaponSystem.FirePoints != null)
                {
                    firePoints = weaponSystem.FirePoints;
                }
            }
        }
        
        // Initialize target values
        targetColor = normalColor;
        targetSize = baseSize;
        
        // Set initial crosshair properties
        if (crosshairImage != null)
        {
            crosshairImage.color = normalColor;
        }
        
        if (crosshairTransform != null)
        {
            crosshairTransform.sizeDelta = baseSize;
        }
    }
    
    private void SetupEventListeners()
    {
        // Listen for weapon firing events if using event system
        if (weaponManager != null)
        {
            // We'll check firing state in Update instead of events for simplicity
        }
    }
    
    void Update()
    {
        UpdateTargeting();
        UpdateCrosshairAppearance();
        UpdateCrosshairPosition();
        CheckForRecoil();
    }
    
    private void UpdateTargeting()
    {
        if (playerCamera == null || firePoints == null || firePoints.Length == 0)
        {
            hasValidTarget = false;
            isTargetingEnemy = false;
            return;
        }
        
        // Use the current fire point for targeting
        Transform currentFirePoint = GetCurrentFirePoint();
        if (currentFirePoint == null)
        {
            hasValidTarget = false;
            isTargetingEnemy = false;
            return;
        }
        
        // Cast ray from current fire point forward
        Ray aimRay = new Ray(currentFirePoint.position, currentFirePoint.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(aimRay, out hit, maxTargetingRange, enemyLayers | obstacleLayer))
        {
            hasValidTarget = true;
            currentTargetDistance = hit.distance;
            currentAimPoint = hit.point;
            
            // Check if we're targeting an enemy
            isTargetingEnemy = ((1 << hit.collider.gameObject.layer) & enemyLayers) != 0;
            
            // Debug ray
            if (showDebugRays)
            {
                Debug.DrawRay(aimRay.origin, aimRay.direction * hit.distance, 
                    isTargetingEnemy ? Color.red : Color.yellow);
            }
        }
        else
        {
            hasValidTarget = false;
            isTargetingEnemy = false;
            currentTargetDistance = maxTargetingRange;
            currentAimPoint = aimRay.GetPoint(maxTargetingRange);
            
            // Debug ray for no hit
            if (showDebugRays)
            {
                Debug.DrawRay(aimRay.origin, aimRay.direction * maxTargetingRange, Color.white);
            }
        }
    }
    
    private Transform GetCurrentFirePoint()
    {
        if (firePoints == null || firePoints.Length == 0) return null;
        
        // For now, use the first fire point. Could be enhanced to track current fire point index
        return firePoints[0];
    }
    
    private void UpdateCrosshairAppearance()
    {
        // Update target color based on targeting state
        if (isTargetingEnemy)
        {
            targetColor = enemyTargetColor;
        }
        else if (hasValidTarget)
        {
            targetColor = normalColor;
        }
        else
        {
            targetColor = noTargetColor;
        }
        
        // Update target size based on accuracy/range
        float accuracyFactor = Mathf.Clamp01(accuracyRange / Mathf.Max(currentTargetDistance, 1f));
        targetSize = Vector2.Lerp(maxSize, minSize, accuracyFactor);
        
        // Apply smooth transitions
        if (crosshairImage != null)
        {
            crosshairImage.color = Color.Lerp(crosshairImage.color, targetColor, 
                colorTransitionSpeed * Time.deltaTime);
        }
        
        if (crosshairTransform != null)
        {
            Vector2 currentSize = crosshairTransform.sizeDelta;
            Vector2 newSize = Vector2.Lerp(currentSize, targetSize, 
                sizeTransitionSpeed * Time.deltaTime);
            crosshairTransform.sizeDelta = newSize;
        }
    }
    
    private void UpdateCrosshairPosition()
    {
        if (crosshairTransform == null || playerCamera == null) return;
        
        // Project the aim point to screen space
        Vector3 screenPoint = playerCamera.WorldToScreenPoint(currentAimPoint);
        
        // Convert to canvas space
        if (parentCanvas != null)
        {
            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPoint,
                parentCanvas.worldCamera,
                out canvasPosition
            );
            
            // Apply recoil offset
            canvasPosition += recoilOffset;
            
            crosshairTransform.localPosition = canvasPosition;
        }
    }
    
    private void CheckForRecoil()
    {
        // Check if machinegun is firing
        bool isFiring = false;
        if (machinegunController != null)
        {
            isFiring = machinegunController.IsFiring;
        }
        else if (weaponManager != null)
        {
            isFiring = weaponManager.IsMachinegunFiring;
        }
        
        // Trigger recoil if firing and not already recoiling
        if (isFiring && !isRecoiling)
        {
            TriggerRecoil();
        }
    }
    
    private void TriggerRecoil()
    {
        if (recoilCoroutine != null)
        {
            StopCoroutine(recoilCoroutine);
        }
        
        recoilCoroutine = StartCoroutine(RecoilAnimation());
    }
    
    private IEnumerator RecoilAnimation()
    {
        isRecoiling = true;
        float elapsedTime = 0f;
        
        while (elapsedTime < recoilDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / recoilDuration;
            float curveValue = recoilCurve.Evaluate(normalizedTime);
            
            // Generate random recoil direction
            Vector2 recoilDirection = Random.insideUnitCircle.normalized;
            recoilOffset = recoilDirection * (curveValue * recoilIntensity);
            
            yield return null;
        }
        
        // Recovery phase
        while (recoilOffset.magnitude > 0.1f)
        {
            recoilOffset = Vector2.Lerp(recoilOffset, Vector2.zero, 
                recoilRecoverySpeed * Time.deltaTime);
            yield return null;
        }
        
        recoilOffset = Vector2.zero;
        isRecoiling = false;
        recoilCoroutine = null;
    }
    
    // Public methods for external control
    public void SetCrosshairVisibility(bool visible)
    {
        if (crosshairImage != null)
        {
            crosshairImage.enabled = visible;
        }
    }
    
    public void SetRecoilIntensity(float intensity)
    {
        recoilIntensity = Mathf.Max(0f, intensity);
    }
    
    public void SetTargetingRange(float range)
    {
        maxTargetingRange = Mathf.Max(1f, range);
    }
    
    // Properties for external access
    public bool IsTargetingEnemy => isTargetingEnemy;
    public bool HasValidTarget => hasValidTarget;
    public float CurrentTargetDistance => currentTargetDistance;
    public Vector3 CurrentAimPoint => currentAimPoint;
    
    void OnDestroy()
    {
        if (recoilCoroutine != null)
        {
            StopCoroutine(recoilCoroutine);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw targeting information in Scene view
        if (hasValidTarget)
        {
            Gizmos.color = isTargetingEnemy ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(currentAimPoint, 1f);
        }
        
        // Draw fire points
        if (firePoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform firePoint in firePoints)
            {
                if (firePoint != null)
                {
                    Gizmos.DrawWireSphere(firePoint.position, 0.5f);
                    Gizmos.DrawRay(firePoint.position, firePoint.forward * 10f);
                }
            }
        }
    }
}
