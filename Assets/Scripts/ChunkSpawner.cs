using UnityEngine;
using System.Collections.Generic;

public class ChunkSpawner : MonoBehaviour
{
    [Header("Pickups")]
    public FuelBargeSpawner    fuelSpawner;   // assign in Inspector
    public RepairBargeSpawner  healthSpawner; // assign in Inspector

    [Header("Chunks")]
    public Transform  player;
    public GameObject[] chunkPrefabs;
    public float      chunkLength   = 50f;
    public int        chunksVisible = 5;

    private List<GameObject> activeChunks = new List<GameObject>();
    private float            zSpawn        = 0f; 

    void Start()
    {
        // Start one chunk behind the player
        zSpawn = -chunkLength;
        SpawnChunk(0);

        // Fill out the rest
        for (int i = 0; i < chunksVisible; i++)
            SpawnChunk(Random.Range(1, chunkPrefabs.Length));
    }

    void Update()
    {
        // Keep spawning ahead of the player
        while (zSpawn < player.position.z + chunksVisible * chunkLength)
            SpawnChunk(Random.Range(1, chunkPrefabs.Length));

        // Delete chunks that fell too far behind
        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            if (activeChunks[i].transform.position.z < player.position.z - chunkLength * 2)
            {
                Destroy(activeChunks[i]);
                activeChunks.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Spawns one chunk at the current zSpawn, then advances zSpawn.
    /// Also tries to drop fuel & repair barges at that spot.
    /// </summary>
    void SpawnChunk(int prefabIndex)
    {
        // 1) Terrain
        GameObject chunk = Instantiate(
            chunkPrefabs[prefabIndex],
            Vector3.forward * zSpawn,
            Quaternion.identity
        );
        activeChunks.Add(chunk);

        // 2) Pickups
        if (fuelSpawner  != null) fuelSpawner.TrySpawnFuelBarge(zSpawn);
        if (healthSpawner != null) healthSpawner.TrySpawnRepairBarge(zSpawn);

        // 3) Advance
        zSpawn += chunkLength;
    }
}
