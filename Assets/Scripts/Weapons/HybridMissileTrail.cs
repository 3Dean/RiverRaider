using UnityEngine;

/// <summary>
/// Hybrid missile trail system that combines particle effects with mesh-based trails
/// Optimized for rear-view visibility and realistic smoke behavior
/// </summary>
public class HybridMissileTrail : MonoBehaviour
{
    [Header("Trail Configuration")]
    [SerializeField] private bool useHybridSystem = true;
    [SerializeField] private float trailLifetime = 3f;
    [SerializeField] private float trailWidth = 0.5f;

    [Header("Particle System")]
    [SerializeField] private ParticleSystem smokeParticles;
    [SerializeField] private bool createParticleSystem = true;

    [Header("Mesh Trail")]
    [SerializeField] private TrailRenderer meshTrail;
    [SerializeField] private bool createMeshTrail = true;

    [Header("Rear-View Optimization")]
    [SerializeField] private float rearViewParticleScale = 1.5f;
    [SerializeField] private float rearViewEmissionRate = 50f;
    [SerializeField] private float sideViewEmissionRate = 30f;
    [SerializeField] private AnimationCurve sizeOverLifetime = AnimationCurve.Linear(0f, 0.2f, 1f, 1.2f);

    [Header("Velocity-Based Effects")]
    [SerializeField] private bool useVelocityStretching;
}
