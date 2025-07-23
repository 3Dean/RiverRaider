using UnityEngine;

/// <summary>
/// Helper class for calculating realistic explosion forces
/// Provides various force patterns and physics calculations for debris
/// </summary>
public static class ExplosionForceCalculator
{
    /// <summary>
    /// Calculate explosion force with mixed patterns (radial + directional + random)
    /// </summary>
    public static Vector3 CalculateMixedExplosionForce(
        Vector3 explosionCenter,
        Vector3 shardPosition,
        float shardMass,
        float baseForce,
        float explosionRadius,
        Vector3 damageDirection = default,
        ExplosionForceSettings settings = null)
    {
        if (settings == null)
            settings = ExplosionForceSettings.Default;

        // Distance and direction calculations
        Vector3 directionFromCenter = shardPosition - explosionCenter;
        float distance = directionFromCenter.magnitude;
        Vector3 radialDirection = distance > 0.001f ? directionFromCenter.normalized : Random.insideUnitSphere.normalized;

        // Distance falloff factor
        float distanceFactor = CalculateDistanceFalloff(distance, explosionRadius, settings.falloffCurve);

        // 1. Radial Force (outward from explosion center)
        Vector3 radialForce = radialDirection * baseForce * distanceFactor * settings.radialMultiplier;

        // 2. Upward Force (for dramatic effect)
        Vector3 upwardForce = Vector3.up * baseForce * distanceFactor * settings.upwardMultiplier;

        // 3. Directional Force (from damage source)
        Vector3 directionalForce = Vector3.zero;
        if (damageDirection != Vector3.zero)
        {
            directionalForce = damageDirection.normalized * baseForce * distanceFactor * settings.directionalMultiplier;
        }

        // 4. Random Force (for natural variation)
        Vector3 randomForce = Random.insideUnitSphere * baseForce * distanceFactor * settings.randomnessMultiplier;

        // Combine all forces
        Vector3 totalForce = radialForce + upwardForce + directionalForce + randomForce;

        // Apply mass scaling (heavier objects get proportionally less force)
        totalForce = ApplyMassScaling(totalForce, shardMass, settings.massScaling);

        // Apply random variation
        totalForce = ApplyRandomVariation(totalForce, settings.forceVariation);

        // Clamp force magnitude
        totalForce = ClampForceMagnitude(totalForce, baseForce, settings.minForceMultiplier, settings.maxForceMultiplier);

        return totalForce;
    }

    /// <summary>
    /// Calculate pure radial explosion force
    /// </summary>
    public static Vector3 CalculateRadialForce(
        Vector3 explosionCenter,
        Vector3 shardPosition,
        float baseForce,
        float explosionRadius,
        AnimationCurve falloffCurve = null)
    {
        Vector3 direction = shardPosition - explosionCenter;
        float distance = direction.magnitude;
        
        if (distance < 0.001f)
            direction = Random.insideUnitSphere.normalized;
        else
            direction = direction.normalized;

        float distanceFactor = CalculateDistanceFalloff(distance, explosionRadius, falloffCurve);
        return direction * baseForce * distanceFactor;
    }

    /// <summary>
    /// Calculate directional explosion force (like from missile impact)
    /// </summary>
    public static Vector3 CalculateDirectionalForce(
        Vector3 explosionCenter,
        Vector3 shardPosition,
        Vector3 impactDirection,
        float baseForce,
        float explosionRadius,
        float directionalBias = 0.7f)
    {
        // Combine radial and directional components
        Vector3 radialDirection = (shardPosition - explosionCenter).normalized;
        Vector3 impactDir = impactDirection.normalized;

        // Blend between radial and directional based on bias
        Vector3 blendedDirection = Vector3.Lerp(radialDirection, impactDir, directionalBias).normalized;

        float distance = Vector3.Distance(explosionCenter, shardPosition);
        float distanceFactor = CalculateDistanceFalloff(distance, explosionRadius);

        return blendedDirection * baseForce * distanceFactor;
    }

    /// <summary>
    /// Calculate distance falloff factor
    /// </summary>
    private static float CalculateDistanceFalloff(float distance, float explosionRadius, AnimationCurve falloffCurve = null)
    {
        if (explosionRadius <= 0f) return 1f;

        float normalizedDistance = Mathf.Clamp01(distance / explosionRadius);

        if (falloffCurve != null && falloffCurve.keys.Length > 0)
        {
            return falloffCurve.Evaluate(normalizedDistance);
        }
        else
        {
            // Default inverse square falloff with minimum
            return Mathf.Max(0.1f, 1f / (1f + normalizedDistance * normalizedDistance));
        }
    }

    /// <summary>
    /// Apply mass scaling to force
    /// </summary>
    private static Vector3 ApplyMassScaling(Vector3 force, float mass, float massScaling)
    {
        if (massScaling <= 0f) return force;

        // Heavier objects get less force (F = ma, so for same acceleration, heavier objects need more force)
        // But for explosions, we want heavier objects to move less, so we reduce force
        float massScale = Mathf.Lerp(1f, 1f / Mathf.Max(0.1f, mass), massScaling);
        return force * massScale;
    }

    /// <summary>
    /// Apply random variation to force
    /// </summary>
    private static Vector3 ApplyRandomVariation(Vector3 force, float variation)
    {
        if (variation <= 0f) return force;

        float randomMultiplier = Random.Range(1f - variation, 1f + variation);
        return force * randomMultiplier;
    }

    /// <summary>
    /// Clamp force magnitude within reasonable bounds
    /// </summary>
    private static Vector3 ClampForceMagnitude(Vector3 force, float baseForce, float minMultiplier, float maxMultiplier)
    {
        float currentMagnitude = force.magnitude;
        if (currentMagnitude < 0.001f) return force;

        float minForce = baseForce * minMultiplier;
        float maxForce = baseForce * maxMultiplier;
        float clampedMagnitude = Mathf.Clamp(currentMagnitude, minForce, maxForce);

        return force.normalized * clampedMagnitude;
    }

    /// <summary>
    /// Calculate torque for realistic spinning
    /// </summary>
    public static Vector3 CalculateExplosionTorque(Vector3 force, float torqueMultiplier = 0.1f)
    {
        // Create torque perpendicular to force direction
        Vector3 randomAxis = Random.insideUnitSphere.normalized;
        Vector3 torqueDirection = Vector3.Cross(force.normalized, randomAxis).normalized;
        
        float torqueMagnitude = force.magnitude * torqueMultiplier;
        return torqueDirection * torqueMagnitude;
    }

    /// <summary>
    /// Calculate force based on shard size/volume
    /// </summary>
    public static float CalculateSizeBasedForceMultiplier(Bounds shardBounds)
    {
        float volume = shardBounds.size.x * shardBounds.size.y * shardBounds.size.z;
        
        // Smaller pieces get more force (they're lighter and more affected by explosion)
        // Larger pieces get less force (they're heavier and more resistant)
        return Mathf.Lerp(1.5f, 0.7f, Mathf.Clamp01(volume / 8f)); // Assuming max volume of 8 cubic units
    }

    /// <summary>
    /// Create explosion force settings for different explosion types
    /// </summary>
    public static ExplosionForceSettings CreateExplosionSettings(ExplosionType type)
    {
        switch (type)
        {
            case ExplosionType.Standard:
                return ExplosionForceSettings.Default;

            case ExplosionType.HighExplosive:
                return new ExplosionForceSettings
                {
                    radialMultiplier = 1.2f,
                    upwardMultiplier = 0.4f,
                    directionalMultiplier = 0.3f,
                    randomnessMultiplier = 0.15f,
                    massScaling = 0.9f,
                    forceVariation = 0.2f,
                    minForceMultiplier = 0.5f,
                    maxForceMultiplier = 2.0f
                };

            case ExplosionType.Directional:
                return new ExplosionForceSettings
                {
                    radialMultiplier = 0.6f,
                    upwardMultiplier = 0.2f,
                    directionalMultiplier = 0.8f,
                    randomnessMultiplier = 0.2f,
                    massScaling = 0.7f,
                    forceVariation = 0.3f,
                    minForceMultiplier = 0.4f,
                    maxForceMultiplier = 1.8f
                };

            case ExplosionType.Fragmentation:
                return new ExplosionForceSettings
                {
                    radialMultiplier = 0.8f,
                    upwardMultiplier = 0.1f,
                    directionalMultiplier = 0.4f,
                    randomnessMultiplier = 0.4f,
                    massScaling = 0.6f,
                    forceVariation = 0.4f,
                    minForceMultiplier = 0.3f,
                    maxForceMultiplier = 2.2f
                };

            default:
                return ExplosionForceSettings.Default;
        }
    }
}

/// <summary>
/// Settings for explosion force calculations
/// </summary>
[System.Serializable]
public class ExplosionForceSettings
{
    [Header("Force Components")]
    public float radialMultiplier = 1.0f;
    public float upwardMultiplier = 0.3f;
    public float directionalMultiplier = 0.4f;
    public float randomnessMultiplier = 0.25f;

    [Header("Physics")]
    public float massScaling = 0.8f;
    public float forceVariation = 0.25f;
    public float minForceMultiplier = 0.3f;
    public float maxForceMultiplier = 1.5f;

    [Header("Distance Falloff")]
    public AnimationCurve falloffCurve;

    public static ExplosionForceSettings Default => new ExplosionForceSettings();
}

/// <summary>
/// Types of explosions with different force characteristics
/// </summary>
public enum ExplosionType
{
    Standard,       // Balanced explosion
    HighExplosive,  // More radial force, less randomness
    Directional,    // More directional force (missile impact)
    Fragmentation   // More randomness, less upward force
}
