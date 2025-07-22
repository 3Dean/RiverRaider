using UnityEngine;

/// <summary>
/// Advanced missile trail system that creates realistic smoke effects
/// Optimized for viewing from behind with natural smoke behavior
/// </summary>
public class RealisticMissileTrail : MonoBehaviour
{
    [Header("Trail Systems")]
    [SerializeField] private ParticleSystem primaryTrail;
    [SerializeField] private ParticleSystem secondarySmoke;
    [SerializeField] private ParticleSystem detailParticles;
    [SerializeField] private TrailRenderer trailRenderer;
    
    [Header("Trail Settings")]
    [SerializeField] private float baseEmissionRate = 50f;
    [SerializeField] private float maxEmissionRate = 150f;
    [SerializeField] private float velocityThreshold = 10f;
    [SerializeField] private bool adaptToSpeed = true;
    
    [Header("Visual Quality")]
    [SerializeField] private float maxViewDistance = 200f;
    [SerializeField] private AnimationCurve qualityByDistance = AnimationCurve.Linear(0, 1, 1, 0.3f);
    
    private Rigidbody missileRigidbody;
    private Vector3 lastPosition;
    private float currentSpeed;
    private Camera mainCamera;
    private float distanceToCamera;
    
    // Emission modules for dynamic control
    private ParticleSystem.EmissionModule primaryEmission;
    private ParticleSystem.EmissionModule secondaryEmission;
    private ParticleSystem.EmissionModule detailEmission;
    
    void Start()
    {
        // Get components
        missileRigidbody = GetComponentInParent<Rigidbody>();
        mainCamera = Camera.main;
        lastPosition = transform.position;
        
        // Cache emission modules
        if (primaryTrail != null)
            primaryEmission = primaryTrail.emission;
        if (secondarySmoke != null)
            secondaryEmission = secondarySmoke.emission;
        if (detailParticles != null)
            detailEmission = detailParticles.emission;
        
        // Initialize trail renderer
        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
        }
    }
    
    void Update()
    {
        UpdateTrailParameters();
        UpdateQualityBasedOnDistance();
    }
    
    private void UpdateTrailParameters()
    {
        // Calculate current speed
        if (missileRigidbody != null)
        {
            currentSpeed = missileRigidbody.velocity.magnitude;
        }
        else
        {
            currentSpeed = (transform.position - lastPosition).magnitude / Time.deltaTime;
            lastPosition = transform.position;
        }
        
        if (!adaptToSpeed) return;
        
        // Adjust emission rates based on speed
        float speedFactor = Mathf.Clamp01(currentSpeed / velocityThreshold);
        float targetEmissionRate = Mathf.Lerp(baseEmissionRate, maxEmissionRate, speedFactor);
        
        // Apply to primary trail
        if (primaryTrail != null)
        {
            var rateOverDistance = primaryEmission.rateOverDistance;
            rateOverDistance.constant = targetEmissionRate;
            primaryEmission.rateOverDistance = rateOverDistance;
        }
        
        // Apply to secondary smoke (less responsive)
        if (secondarySmoke != null)
        {
            var rateOverDistance = secondaryEmission.rateOverDistance;
            rateOverDistance.constant = targetEmissionRate * 0.6f;
            secondaryEmission.rateOverDistance = rateOverDistance;
        }
        
        // Detail particles only at high speeds
        if (detailParticles != null)
        {
            bool shouldEmitDetails = currentSpeed > velocityThreshold * 0.7f;
            detailEmission.enabled = shouldEmitDetails;
            
            if (shouldEmitDetails)
            {
                var rateOverDistance = detailEmission.rateOverDistance;
                rateOverDistance.constant = targetEmissionRate * 0.3f;
                detailEmission.rateOverDistance = rateOverDistance;
            }
        }
    }
    
    private void UpdateQualityBasedOnDistance()
    {
        if (mainCamera == null) return;
        
        distanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);
        float normalizedDistance = Mathf.Clamp01(distanceToCamera / maxViewDistance);
        float qualityFactor = qualityByDistance.Evaluate(normalizedDistance);
        
        // Adjust particle counts based on distance
        AdjustParticleSystemQuality(primaryTrail, qualityFactor);
        AdjustParticleSystemQuality(secondarySmoke, qualityFactor * 0.8f);
        AdjustParticleSystemQuality(detailParticles, qualityFactor * 0.5f);
        
        // Disable very distant trails
        if (distanceToCamera > maxViewDistance)
        {
            SetTrailEnabled(false);
        }
        else
        {
            SetTrailEnabled(true);
        }
    }
    
    private void AdjustParticleSystemQuality(ParticleSystem ps, float qualityFactor)
    {
        if (ps == null) return;
        
        var main = ps.main;
        var emission = ps.emission;
        
        // Adjust max particles
        int baseMaxParticles = 1000; // You can make this configurable
        main.maxParticles = Mathf.RoundToInt(baseMaxParticles * qualityFactor);
        
        // Adjust emission rate
        if (emission.rateOverDistance.mode == ParticleSystemCurveMode.Constant)
        {
            var rateOverDistance = emission.rateOverDistance;
            float baseRate = adaptToSpeed ? baseEmissionRate : rateOverDistance.constant;
            rateOverDistance.constant = baseRate * qualityFactor;
            emission.rateOverDistance = rateOverDistance;
        }
    }
    
    private void SetTrailEnabled(bool enabled)
    {
        if (primaryTrail != null) primaryTrail.gameObject.SetActive(enabled);
        if (secondarySmoke != null) secondarySmoke.gameObject.SetActive(enabled);
        if (detailParticles != null) detailParticles.gameObject.SetActive(enabled);
        if (trailRenderer != null) trailRenderer.enabled = enabled;
    }
    
    public void SetTrailIntensity(float intensity)
    {
        intensity = Mathf.Clamp01(intensity);
        
        if (primaryTrail != null)
        {
            var emission = primaryTrail.emission;
            var rateOverDistance = emission.rateOverDistance;
            rateOverDistance.constant = baseEmissionRate * intensity;
            emission.rateOverDistance = rateOverDistance;
        }
        
        if (secondarySmoke != null)
        {
            var emission = secondarySmoke.emission;
            var rateOverDistance = emission.rateOverDistance;
            rateOverDistance.constant = baseEmissionRate * 0.6f * intensity;
            emission.rateOverDistance = rateOverDistance;
        }
    }
    
    public void StopTrail()
    {
        if (primaryTrail != null) primaryTrail.Stop();
        if (secondarySmoke != null) secondarySmoke.Stop();
        if (detailParticles != null) detailParticles.Stop();
        if (trailRenderer != null) trailRenderer.enabled = false;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw quality distance threshold
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxViewDistance);
        
        // Draw velocity vector
        if (Application.isPlaying && missileRigidbody != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, missileRigidbody.velocity.normalized * 5f);
        }
    }
}
