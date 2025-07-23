using UnityEngine;
using System.Collections;

/// <summary>
/// Non-physics explosion shard that uses direct transform animation
/// Provides predictable, controllable movement without physics complications
/// </summary>
public class ExplosionShardAnimated : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float separationDuration = 0.8f;
    [SerializeField] private float fallDuration = 2.5f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private AnimationCurve separationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 2f), new Keyframe(1f, 1f, 0f, 0f));
    [SerializeField] private AnimationCurve fallCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Physics Simulation")]
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float airResistance = 0.5f;
    [SerializeField] private float bounceHeight = 0.2f;
    [SerializeField] private float bounceDamping = 0.7f;

    // Private variables
    private Vector3 startPosition;
    private Vector3 separationTarget;
    private Vector3 currentVelocity;
    private Vector3 rotationAxis;
    private float rotationSpeed_internal;
    private bool isAnimating = false;
    private bool hasLanded = false;
    private float groundY;
    
    // Animation state
    private float separationProgress = 0f;
    private float fallTime = 0f;
    private Vector3 separationStartPos;
    private Vector3 fallStartPos;
    
    // References
    private HelicopterExplosionAnimated explosionController;
    private AudioSource audioSource;
    private AudioClip[] collisionSounds;

    /// <summary>
    /// Initialize the shard with explosion parameters
    /// </summary>
    public void Initialize(HelicopterExplosionAnimated controller, Vector3 separationDirection, 
                          float separationDistance, AudioClip[] metalSounds)
    {
        explosionController = controller;
        collisionSounds = metalSounds;
        
        // Store starting position
        startPosition = transform.position;
        separationStartPos = startPosition;
        
        // Calculate separation target
        separationTarget = startPosition + separationDirection * separationDistance;
        
        // DEBUG: Log initialization
        Debug.Log($"ExplosionShardAnimated [{gameObject.name}]: Initialized at {startPosition}, target: {separationTarget}, distance: {separationDistance}");
        
        // Random rotation axis and speed
        rotationAxis = Random.insideUnitSphere.normalized;
        rotationSpeed_internal = Random.Range(-rotationSpeed, rotationSpeed);
        
        // Find ground level (raycast down)
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f))
        {
            groundY = hit.point.y;
        }
        else
        {
            groundY = transform.position.y - 20f; // Fallback
        }
        
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.3f;
        }
        
        // Disable any rigidbodies and colliders initially
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }

    /// <summary>
    /// Start the explosion animation
    /// </summary>
    public void StartExplosion()
    {
        if (isAnimating) return;
        
        // DEBUG: Log explosion start
        Debug.Log($"ExplosionShardAnimated [{gameObject.name}]: Starting explosion animation at {transform.position}");
        
        isAnimating = true;
        StartCoroutine(AnimateExplosion());
    }

    /// <summary>
    /// Main explosion animation coroutine
    /// </summary>
    private IEnumerator AnimateExplosion()
    {
        // DEBUG: Log animation start
        Debug.Log($"ExplosionShardAnimated [{gameObject.name}]: Starting animation phases");
        
        // Phase 1: Separation (gentle outward movement)
        Debug.Log($"ExplosionShardAnimated [{gameObject.name}]: Phase 1 - Separation");
        yield return StartCoroutine(AnimateSeparation());
        
        // Phase 2: Falling (gravity simulation)
        Debug.Log($"ExplosionShardAnimated [{gameObject.name}]: Phase 2 - Falling");
        yield return StartCoroutine(AnimateFalling());
        
        // Phase 3: Landing and settling
        Debug.Log($"ExplosionShardAnimated [{gameObject.name}]: Phase 3 - Landing");
        yield return StartCoroutine(AnimateLanding());
        
        // Animation complete
        Debug.Log($"ExplosionShardAnimated [{gameObject.name}]: Animation complete at {transform.position}");
        isAnimating = false;
        
        // Notify controller
        if (explosionController != null)
        {
            explosionController.OnShardAnimationComplete();
        }
    }

    /// <summary>
    /// Animate the initial separation phase
    /// </summary>
    private IEnumerator AnimateSeparation()
    {
        separationProgress = 0f;
        
        while (separationProgress < 1f)
        {
            separationProgress += Time.deltaTime / separationDuration;
            separationProgress = Mathf.Clamp01(separationProgress);
            
            // Smooth separation using animation curve
            float curveValue = separationCurve.Evaluate(separationProgress);
            Vector3 currentPos = Vector3.Lerp(separationStartPos, separationTarget, curveValue);
            
            // Apply position
            transform.position = currentPos;
            
            // Apply rotation
            transform.Rotate(rotationAxis, rotationSpeed_internal * Time.deltaTime);
            
            yield return null;
        }
        
        // Store position for fall phase
        fallStartPos = transform.position;
        currentVelocity = Vector3.zero;
        fallTime = 0f;
    }

    /// <summary>
    /// Animate the falling phase with gravity simulation
    /// </summary>
    private IEnumerator AnimateFalling()
    {
        while (transform.position.y > groundY + 0.1f && fallTime < fallDuration)
        {
            fallTime += Time.deltaTime;
            
            // Apply gravity
            currentVelocity.y -= gravity * Time.deltaTime;
            
            // Apply air resistance
            currentVelocity *= (1f - airResistance * Time.deltaTime);
            
            // Update position
            transform.position += currentVelocity * Time.deltaTime;
            
            // Continue rotation (slower during fall)
            transform.Rotate(rotationAxis, rotationSpeed_internal * 0.5f * Time.deltaTime);
            
            yield return null;
        }
    }

    /// <summary>
    /// Animate the landing and settling
    /// </summary>
    private IEnumerator AnimateLanding()
    {
        if (!hasLanded)
        {
            hasLanded = true;
            
            // Play collision sound
            PlayCollisionSound();
            
            // Simple bounce effect
            if (bounceHeight > 0f)
            {
                Vector3 landingPos = transform.position;
                landingPos.y = groundY;
                
                // Small bounce
                float bounceTime = 0f;
                float bounceDuration = 0.3f;
                
                while (bounceTime < bounceDuration)
                {
                    bounceTime += Time.deltaTime;
                    float bounceProgress = bounceTime / bounceDuration;
                    
                    // Bounce curve (up then down)
                    float bounceY = Mathf.Sin(bounceProgress * Mathf.PI) * bounceHeight * bounceDamping;
                    
                    Vector3 bouncePos = landingPos;
                    bouncePos.y += bounceY;
                    transform.position = bouncePos;
                    
                    // Slow down rotation
                    transform.Rotate(rotationAxis, rotationSpeed_internal * 0.2f * Time.deltaTime);
                    
                    yield return null;
                }
            }
            
            // Final position
            Vector3 finalPos = transform.position;
            finalPos.y = groundY;
            transform.position = finalPos;
        }
        
        // Enable colliders for terrain interaction
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
    }

    /// <summary>
    /// Play collision sound when landing
    /// </summary>
    private void PlayCollisionSound()
    {
        if (audioSource != null && collisionSounds != null && collisionSounds.Length > 0)
        {
            AudioClip randomSound = collisionSounds[Random.Range(0, collisionSounds.Length)];
            if (randomSound != null)
            {
                audioSource.PlayOneShot(randomSound, Random.Range(0.1f, 0.3f));
            }
        }
    }

    /// <summary>
    /// Get current animation progress (0-1)
    /// </summary>
    public float GetAnimationProgress()
    {
        if (!isAnimating) return hasLanded ? 1f : 0f;
        
        float totalDuration = separationDuration + fallDuration;
        float currentTime = separationProgress * separationDuration + fallTime;
        return Mathf.Clamp01(currentTime / totalDuration);
    }

    /// <summary>
    /// Check if shard has finished animating
    /// </summary>
    public bool IsAnimationComplete()
    {
        return !isAnimating && hasLanded;
    }

    /// <summary>
    /// Force stop animation and cleanup
    /// </summary>
    public void StopAnimation()
    {
        StopAllCoroutines();
        isAnimating = false;
        hasLanded = true;
        
        // Final position on ground
        Vector3 finalPos = transform.position;
        finalPos.y = groundY;
        transform.position = finalPos;
    }

    void OnDrawGizmosSelected()
    {
        // Draw separation target
        if (Application.isPlaying && isAnimating)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(separationTarget, 0.1f);
            Gizmos.DrawLine(startPosition, separationTarget);
            
            // Draw ground level
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector3(transform.position.x, groundY, transform.position.z), 
                               new Vector3(0.5f, 0.02f, 0.5f));
        }
    }
}
