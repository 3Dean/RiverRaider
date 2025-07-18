using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// PickupManager - Coordinates all pickup spawning (repair barges)
/// Respects manual fuel barge placement in terrain prefabs
/// Part of the modular spawning system refactor
/// </summary>
public class PickupManager : MonoBehaviour
{
    [Header("Repair Barge Configuration")]
    public GameObject repairBargePrefab;
    public float minDistanceBetweenPickups = 150f;
    public float maxDistanceBetweenRepairBarges = 400f;
    public float repairBargeSpawnChance = 0.15f;
    
    [Header("Spawn Settings")]
    public float spawnHeight = 8f;
    public float chunkLength = 300f;
    
    [Header("Debug")]
    public bool showDebugLogs = false;
    public bool showGizmos = true;

    private List<Vector3> allPickupPositions = new List<Vector3>();
    private List<GameObject> activeRepairBarges = new List<GameObject>();
    private float lastRepairBargeZ = -1000f;

    void OnEnable()
    {
        // Listen to terrain spawning events
        ChunkSpawner.OnChunkSpawned += OnChunkSpawned;
        ChunkSpawner.OnChunkDestroyed += OnChunkDestroyed;
    }

    void OnDisable()
    {
        // Unsubscribe from events
        ChunkSpawner.OnChunkSpawned -= OnChunkSpawned;
        ChunkSpawner.OnChunkDestroyed -= OnChunkDestroyed;
    }

    void Start()
    {
        if (repairBargePrefab == null)
        {
            Debug.LogWarning("PickupManager: repairBargePrefab is not assigned. Repair barges will not spawn.");
        }

        if (showDebugLogs)
            Debug.Log("PickupManager: Initialized and listening for terrain events");
    }

    /// <summary>
    /// Called when a new terrain chunk is spawned
    /// </summary>
    void OnChunkSpawned(float chunkZ, int prefabIndex)
    {
        // Scan for existing fuel barges in the chunk (manual placement)
        ScanChunkForFuelBarges(chunkZ);
        
        // Try to spawn repair barge if conditions are met
        TrySpawnRepairBarge(chunkZ);
    }

    /// <summary>
    /// Called when a terrain chunk is destroyed
    /// </summary>
    void OnChunkDestroyed(GameObject chunk)
    {
        // Clean up pickup position tracking for destroyed chunks
        CleanupPickupPositions(chunk.transform.position.z);
        
        // Clean up destroyed repair barges
        CleanupDestroyedRepairBarges();
    }

    /// <summary>
    /// Scan a newly spawned chunk for manually placed fuel barges
    /// </summary>
    void ScanChunkForFuelBarges(float chunkZ)
    {
        // Find all fuel barges in the scene within this chunk's range
        GameObject[] allFuelBarges = GameObject.FindGameObjectsWithTag("FuelBarge");
        
        foreach (GameObject fuelBarge in allFuelBarges)
        {
            float bargeZ = fuelBarge.transform.position.z;
            
            // If this fuel barge is in the current chunk range
            if (bargeZ >= chunkZ && bargeZ < chunkZ + chunkLength)
            {
                allPickupPositions.Add(fuelBarge.transform.position);
                
                if (showDebugLogs)
                    Debug.Log($"PickupManager: Found manual fuel barge at {fuelBarge.transform.position}");
            }
        }
    }

    /// <summary>
    /// Try to spawn a repair barge in the given chunk
    /// </summary>
    void TrySpawnRepairBarge(float chunkZ)
    {
        if (repairBargePrefab == null) return;

        // Check distance from last repair barge
        float distanceFromLastRepair = chunkZ - lastRepairBargeZ;
        bool shouldSpawn = false;

        // Force spawn if too far from last repair barge
        if (distanceFromLastRepair >= maxDistanceBetweenRepairBarges)
        {
            shouldSpawn = true;
            if (showDebugLogs)
                Debug.Log($"PickupManager: Forcing repair barge spawn (distance: {distanceFromLastRepair})");
        }
        // Random chance spawn
        else if (Random.value < repairBargeSpawnChance)
        {
            shouldSpawn = true;
            if (showDebugLogs)
                Debug.Log($"PickupManager: Random repair barge spawn triggered");
        }

        if (shouldSpawn)
        {
            Vector3 spawnPosition = FindValidSpawnPosition(chunkZ);
            
            if (spawnPosition != Vector3.zero)
            {
                SpawnRepairBarge(spawnPosition);
                lastRepairBargeZ = chunkZ;
            }
        }
    }

    /// <summary>
    /// Find a valid spawn position that doesn't conflict with other pickups
    /// </summary>
    Vector3 FindValidSpawnPosition(float chunkZ)
    {
        int maxAttempts = 10;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Random position within the chunk
            Vector3 candidatePosition = new Vector3(
                Random.Range(-20f, 20f), // X range within river
                spawnHeight,
                chunkZ + Random.Range(chunkLength * 0.2f, chunkLength * 0.8f) // Z within chunk
            );

            // Check if this position is too close to existing pickups
            bool validPosition = true;
            
            foreach (Vector3 existingPickup in allPickupPositions)
            {
                float distance = Vector3.Distance(candidatePosition, existingPickup);
                if (distance < minDistanceBetweenPickups)
                {
                    validPosition = false;
                    break;
                }
            }

            if (validPosition)
            {
                if (showDebugLogs)
                    Debug.Log($"PickupManager: Found valid spawn position at {candidatePosition} (attempt {attempt + 1})");
                return candidatePosition;
            }
        }

        if (showDebugLogs)
            Debug.LogWarning($"PickupManager: Could not find valid spawn position in chunk {chunkZ} after {maxAttempts} attempts");
        
        return Vector3.zero; // No valid position found
    }

    /// <summary>
    /// Spawn a repair barge at the specified position
    /// </summary>
    void SpawnRepairBarge(Vector3 position)
    {
        GameObject repairBarge = Instantiate(repairBargePrefab, position, Quaternion.identity);
        repairBarge.transform.SetParent(transform);
        repairBarge.name = $"RepairBarge_{activeRepairBarges.Count:D3}";
        
        activeRepairBarges.Add(repairBarge);
        allPickupPositions.Add(position);

        if (showDebugLogs)
            Debug.Log($"PickupManager: Spawned repair barge at {position}");
    }

    /// <summary>
    /// Clean up pickup positions for destroyed chunks
    /// </summary>
    void CleanupPickupPositions(float destroyedChunkZ)
    {
        // Remove pickup positions that were in the destroyed chunk
        allPickupPositions.RemoveAll(pos => 
            pos.z >= destroyedChunkZ && pos.z < destroyedChunkZ + chunkLength);
    }

    /// <summary>
    /// Clean up destroyed repair barges from tracking
    /// </summary>
    void CleanupDestroyedRepairBarges()
    {
        activeRepairBarges.RemoveAll(barge => barge == null);
    }

    /// <summary>
    /// Get all active pickup positions for debugging
    /// </summary>
    public List<Vector3> GetAllPickupPositions()
    {
        return new List<Vector3>(allPickupPositions);
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw pickup positions
        Gizmos.color = Color.green;
        foreach (Vector3 pos in allPickupPositions)
        {
            Gizmos.DrawWireSphere(pos, minDistanceBetweenPickups * 0.5f);
        }

        // Draw active repair barges
        Gizmos.color = Color.blue;
        foreach (GameObject repairBarge in activeRepairBarges)
        {
            if (repairBarge != null)
            {
                Gizmos.DrawWireCube(repairBarge.transform.position, Vector3.one * 5f);
            }
        }
    }
}
