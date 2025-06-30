using UnityEngine;
public class FuelBargeSpawner : MonoBehaviour
{
    public GameObject fuelBargePrefab;
    public float chunkLength = 200f;
    public float distanceThreshold = 350f;
    public float chanceOfRandomSpawn = 0.2f;

    private float lastSpawnZ = 0f;

    public void TrySpawnFuelBarge(float currentChunkZ)
    {
        float distanceSinceLast = currentChunkZ - lastSpawnZ;

        bool mustSpawn = distanceSinceLast >= distanceThreshold;
        bool randomSpawn = Random.value < chanceOfRandomSpawn;

        if (mustSpawn || randomSpawn)
        {
            Vector3 spawnPos = new Vector3(0, 8f, currentChunkZ + chunkLength / 2f); // adjust Y for height
            Instantiate(fuelBargePrefab, spawnPos, Quaternion.identity);
            lastSpawnZ = currentChunkZ;
        }
    }
}
