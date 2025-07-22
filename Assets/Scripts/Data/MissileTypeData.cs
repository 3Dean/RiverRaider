using UnityEngine;
using Unity.VectorGraphics;

/// <summary>
/// ScriptableObject that defines missile type properties including capacity, damage, and visual settings
/// </summary>
[CreateAssetMenu(fileName = "New Missile Type", menuName = "Weapons/Missile Type Data")]
public class MissileTypeData : ScriptableObject
{
    [Header("Basic Properties")]
    public string missileTypeName = "ARM-30";
    public string displayName = "ARM-30";
    public int maxCapacity = 5;
    public int costPerShot = 1;
    
    [Header("Combat Properties")]
    public float damage = 100f;
    public float speed = 40f;
    public float cooldown = 1f;
    public float explosionRadius = 5f;
    
    [Header("Visual & Audio")]
    public GameObject missilePrefab;
    public Sprite missileIcon; // Legacy sprite support
    public SVGImage missileSVGIcon; // New SVG icon support
    public Color uiColor = Color.white;
    public AudioClip launchSound;
    
    [Header("Description")]
    [TextArea(3, 5)]
    public string description = "Standard anti-radar missile with balanced performance.";
    
    /// <summary>
    /// Get a normalized missile type name for consistency
    /// </summary>
    public string GetNormalizedTypeName()
    {
        return missileTypeName.ToUpper().Replace("-", "");
    }
    
    /// <summary>
    /// Check if this missile type matches a given name (case-insensitive)
    /// </summary>
    public bool MatchesTypeName(string typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return false;
        
        string normalizedInput = typeName.ToUpper().Replace("-", "");
        string normalizedThis = GetNormalizedTypeName();
        
        return normalizedThis == normalizedInput || 
               missileTypeName.Equals(typeName, System.StringComparison.OrdinalIgnoreCase) ||
               displayName.Equals(typeName, System.StringComparison.OrdinalIgnoreCase);
    }
}
