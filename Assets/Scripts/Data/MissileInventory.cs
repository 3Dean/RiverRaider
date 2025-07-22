using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Manages missile inventory with separate ammunition tracking for each missile type
/// </summary>
[System.Serializable]
public class MissileInventory
{
    [System.Serializable]
    public class MissileStock
    {
        public MissileTypeData missileType;
        public int currentAmount;
        public int maxCapacity;
        
        public MissileStock(MissileTypeData type)
        {
            missileType = type;
            maxCapacity = type.maxCapacity;
            currentAmount = maxCapacity; // Start with full capacity
        }
        
        public bool CanFire(int cost = 1)
        {
            return currentAmount >= cost;
        }
        
        public bool ConsumeMissile(int cost = 1)
        {
            if (CanFire(cost))
            {
                currentAmount -= cost;
                return true;
            }
            return false;
        }
        
        public void Resupply(int amount = -1)
        {
            if (amount == -1)
                currentAmount = maxCapacity; // Full resupply
            else
                currentAmount = Mathf.Clamp(currentAmount + amount, 0, maxCapacity);
        }
        
        public float GetPercentage()
        {
            return maxCapacity > 0 ? (float)currentAmount / maxCapacity : 0f;
        }
    }
    
    [SerializeField] private List<MissileStock> missileStocks = new List<MissileStock>();
    [SerializeField] private int currentMissileTypeIndex = 0;
    
    public MissileStock CurrentMissileStock
    {
        get
        {
            if (missileStocks.Count == 0) return null;
            if (currentMissileTypeIndex >= missileStocks.Count) currentMissileTypeIndex = 0;
            return missileStocks[currentMissileTypeIndex];
        }
    }
    
    public MissileTypeData CurrentMissileType
    {
        get { return CurrentMissileStock?.missileType; }
    }
    
    public int CurrentMissileCount
    {
        get { return CurrentMissileStock?.currentAmount ?? 0; }
    }
    
    public int CurrentMaxCapacity
    {
        get { return CurrentMissileStock?.maxCapacity ?? 0; }
    }
    
    public string CurrentMissileTypeName
    {
        get { return CurrentMissileType?.displayName ?? "None"; }
    }
    
    /// <summary>
    /// Initialize inventory with missile types
    /// </summary>
    public void Initialize(MissileTypeData[] missileTypes)
    {
        missileStocks.Clear();
        
        foreach (var missileType in missileTypes)
        {
            if (missileType != null)
            {
                missileStocks.Add(new MissileStock(missileType));
            }
        }
        
        currentMissileTypeIndex = 0;
        Debug.Log($"MissileInventory initialized with {missileStocks.Count} missile types");
    }
    
    /// <summary>
    /// Add a new missile type to inventory
    /// </summary>
    public void AddMissileType(MissileTypeData missileType)
    {
        if (missileType == null) return;
        
        // Check if this missile type already exists
        var existing = missileStocks.FirstOrDefault(stock => stock.missileType == missileType);
        if (existing == null)
        {
            missileStocks.Add(new MissileStock(missileType));
            Debug.Log($"Added new missile type: {missileType.displayName}");
        }
    }
    
    /// <summary>
    /// Switch to next missile type
    /// </summary>
    public bool SwitchToNextMissileType()
    {
        if (missileStocks.Count <= 1) return false;
        
        currentMissileTypeIndex = (currentMissileTypeIndex + 1) % missileStocks.Count;
        Debug.Log($"Switched to missile type: {CurrentMissileTypeName} ({CurrentMissileCount}/{CurrentMaxCapacity})");
        return true;
    }
    
    /// <summary>
    /// Switch to previous missile type
    /// </summary>
    public bool SwitchToPreviousMissileType()
    {
        if (missileStocks.Count <= 1) return false;
        
        currentMissileTypeIndex = (currentMissileTypeIndex - 1 + missileStocks.Count) % missileStocks.Count;
        Debug.Log($"Switched to missile type: {CurrentMissileTypeName} ({CurrentMissileCount}/{CurrentMaxCapacity})");
        return true;
    }
    
    /// <summary>
    /// Switch to specific missile type by name
    /// </summary>
    public bool SwitchToMissileType(string typeName)
    {
        for (int i = 0; i < missileStocks.Count; i++)
        {
            if (missileStocks[i].missileType.missileTypeName.Equals(typeName, System.StringComparison.OrdinalIgnoreCase) ||
                missileStocks[i].missileType.displayName.Equals(typeName, System.StringComparison.OrdinalIgnoreCase))
            {
                currentMissileTypeIndex = i;
                Debug.Log($"Switched to missile type: {CurrentMissileTypeName} ({CurrentMissileCount}/{CurrentMaxCapacity})");
                return true;
            }
        }
        
        Debug.LogWarning($"Missile type '{typeName}' not found in inventory");
        return false;
    }
    
    /// <summary>
    /// Fire current missile type
    /// </summary>
    public bool FireMissile()
    {
        var currentStock = CurrentMissileStock;
        if (currentStock == null) return false;
        
        int cost = currentStock.missileType.costPerShot;
        bool success = currentStock.ConsumeMissile(cost);
        
        if (success)
        {
            Debug.Log($"Fired {CurrentMissileTypeName}! Remaining: {CurrentMissileCount}/{CurrentMaxCapacity}");
        }
        else
        {
            Debug.Log($"Cannot fire {CurrentMissileTypeName} - insufficient ammunition!");
        }
        
        return success;
    }
    
    /// <summary>
    /// Resupply current missile type
    /// </summary>
    public void ResupplyCurrentType(int amount = -1)
    {
        CurrentMissileStock?.Resupply(amount);
        Debug.Log($"Resupplied {CurrentMissileTypeName}: {CurrentMissileCount}/{CurrentMaxCapacity}");
    }
    
    /// <summary>
    /// Resupply all missile types
    /// </summary>
    public void ResupplyAll()
    {
        foreach (var stock in missileStocks)
        {
            stock.Resupply();
        }
        Debug.Log("All missile types resupplied to maximum capacity");
    }
    
    /// <summary>
    /// Get missile stock by type name
    /// </summary>
    public MissileStock GetMissileStock(string typeName)
    {
        return missileStocks.FirstOrDefault(stock => 
            stock.missileType.missileTypeName.Equals(typeName, System.StringComparison.OrdinalIgnoreCase) ||
            stock.missileType.displayName.Equals(typeName, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Get all available missile types
    /// </summary>
    public MissileTypeData[] GetAvailableMissileTypes()
    {
        return missileStocks.Select(stock => stock.missileType).ToArray();
    }
    
    /// <summary>
    /// Check if any missiles are available
    /// </summary>
    public bool HasAnyMissiles()
    {
        return missileStocks.Any(stock => stock.currentAmount > 0);
    }
    
    /// <summary>
    /// Get total missile count across all types
    /// </summary>
    public int GetTotalMissileCount()
    {
        return missileStocks.Sum(stock => stock.currentAmount);
    }
}
