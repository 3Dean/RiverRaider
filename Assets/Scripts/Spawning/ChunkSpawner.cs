using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TerrainManager - Handles only terrain chunk spawning and management
/// Part of the modular spawning system refactor
/// </summary>
public class ChunkSpawner : MonoBehaviour
{
    [Header("Terrain Configuration")]
    public Transform  player;
    public GameObject[] chunkPrefabs;
    public float      chunkLength   = 300f;  // Updated to match actual terrain size
    public int        chunksVisible = 5;

    [Header("Debug")]
    public bool showDebugLogs = false;

    private List<GameObject> activeChunks = new List<GameObject>();
    private float            zSpawn        = 0f;

    // Events for other systems to listen to
    public static System.Action<float, int> OnChunkSpawned;
    public static System.Action<GameObject> OnChunkDestroyed;

    void Start()
    {
        // Validate setup
        if (chunkPrefabs == null || chunkPrefabs.Length == 0)
        {
            Debug.LogError("TerrainManager: chunkPrefabs array is not assigned or empty! Please assign terrain prefabs in the inspector.");
            return;
        }
        
        if (player == null)
        {
            Debug.LogError("TerrainManager: Player transform is not assigned! Please assign the player in the inspector.");
            return;
        }

        // Start one chunk behind the player
        zSpawn = -chunkLength;
        SpawnChunk(0);

        // Fill out the rest
        for (int i = 0; i < chunksVisible; i++)
        {
            int randomIndex = chunkPrefabs.Length > 1 ? Random.Range(1, chunkPrefabs.Length) : 0;
            SpawnChunk(randomIndex);
        }

        if (showDebugLogs)
            Debug.Log($"TerrainManager: Initialized with {activeChunks.Count} chunks");
    }

    void Update()
    {
        // Skip if not properly initialized
        if (chunkPrefabs == null || chunkPrefabs.Length == 0 || player == null)
            return;

        // Keep spawning ahead of the player
        while (zSpawn < player.position.z + chunksVisible * chunkLength)
        {
            int randomIndex = chunkPrefabs.Length > 1 ? Random.Range(1, chunkPrefabs.Length) : 0;
            SpawnChunk(randomIndex);
        }

        // Delete chunks that fell too far behind
        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            if (activeChunks[i] != null && activeChunks[i].transform.position.z < player.position.z - chunkLength * 2)
            {
                GameObject chunkToDestroy = activeChunks[i];
                OnChunkDestroyed?.Invoke(chunkToDestroy);
                Destroy(chunkToDestroy);
                activeChunks.RemoveAt(i);
                
                if (showDebugLogs)
                    Debug.Log($"TerrainManager: Destroyed chunk behind player");
            }
        }
    }

    /// <summary>
    /// Spawns one terrain chunk at the current zSpawn position
    /// Notifies other systems via events
    /// </summary>
    void SpawnChunk(int prefabIndex)
    {
        // Validate prefab index
        if (chunkPrefabs == null || prefabIndex < 0 || prefabIndex >= chunkPrefabs.Length)
        {
            Debug.LogError($"TerrainManager: Invalid prefab index {prefabIndex}. Array length: {(chunkPrefabs?.Length ?? 0)}");
            return;
        }

        // Validate prefab is not null
        if (chunkPrefabs[prefabIndex] == null)
        {
            Debug.LogError($"TerrainManager: Prefab at index {prefabIndex} is null!");
            return;
        }

        // Spawn terrain chunk
        GameObject chunk = Instantiate(
            chunkPrefabs[prefabIndex],
            Vector3.forward * zSpawn,
            Quaternion.identity
        );
        
        // Set parent for organization
        chunk.transform.SetParent(transform);
        chunk.name = $"TerrainChunk_{activeChunks.Count:D3}_{chunkPrefabs[prefabIndex].name}";
        
        activeChunks.Add(chunk);

        // Notify other systems that a chunk was spawned
        OnChunkSpawned?.Invoke(zSpawn, prefabIndex);

        if (showDebugLogs)
            Debug.Log($"TerrainManager: Spawned {chunk.name} at Z={zSpawn}");

        // Advance spawn position
        zSpawn += chunkLength;
    }

    /// <summary>
    /// Get information about active chunks for other systems
    /// </summary>
    public List<GameObject> GetActiveChunks()
    {
        return new List<GameObject>(activeChunks);
    }

    /// <summary>
    /// Get the current spawn position
    /// </summary>
    public float GetCurrentSpawnZ()
    {
        return zSpawn;
    }
}
