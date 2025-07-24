using UnityEngine;
using System.Collections;

/// <summary>
/// Individual explosion shard behavior for realistic helicopter destruction
/// Handles collision effects, lifetime management, and physics interactions
/// </summary>
public class ExplosionShard : MonoBehaviour
{
    // Basic properties that HelicopterExplosion expects
    private Rigidbody cachedRigidbody;
    private MonoBehaviour parentExplosion;
    private AudioClip[] collisionSounds;
    private bool isActive = false;

    // Public properties that HelicopterExplosion accesses
    public Rigidbody Rigidbody => cachedRigidbody;
    public bool IsActive => isActive;

    void Awake()
    {
        // Cache the rigidbody component
        cachedRigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Initialize the shard with parent explosion reference
    /// </summary>
    public void Initialize(HelicopterExplosion parent, AudioClip[] sounds)
    {
        parentExplosion = parent;
        collisionSounds = sounds;
        
        // Ensure physics is initially disabled
        if (cachedRigidbody != null)
        {
            cachedRigidbody.isKinematic = true;
        }
    }

    /// <summary>
    /// Activate the shard for physics simulation
    /// </summary>
    public void Activate()
    {
        isActive = true;
        
        // Enable physics
        if (cachedRigidbody != null)
        {
            cachedRigidbody.isKinematic = false;
        }
    }
}
