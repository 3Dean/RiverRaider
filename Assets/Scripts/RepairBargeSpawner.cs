using UnityEngine;
public class RepairBargeSpawner : MonoBehaviour
{
    public GameObject repairBargePrefab;
    public float chunkLength = 200f;
    public float distanceThreshold = 350f;
    public float chanceOfRandomSpawn = 0.1f;

    private float lastSpawnZ = 0f;

    public void TrySpawnRepairBarge(float currentChunkZ)
    {
        float distanceSinceLast = currentChunkZ - lastSpawnZ;

        bool mustSpawn = distanceSinceLast >= distanceThreshold;
        bool randomSpawn = Random.value < chanceOfRandomSpawn;

        if (mustSpawn || randomSpawn)
        {
            Vector3 spawnPos = new Vector3(0, 8f, currentChunkZ + chunkLength / 3f); // adjust Y for height
            Instantiate(repairBargePrefab, spawnPos, Quaternion.identity);
            lastSpawnZ = currentChunkZ;
        }
    }
}
