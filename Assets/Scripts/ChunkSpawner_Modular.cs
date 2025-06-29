using UnityEngine;
using System.Collections.Generic;

public class ChunkSpawner_Modular : MonoBehaviour
{
    public GameObject[] chunkPrefabs;
    private List<GameObject> activeChunks = new List<GameObject>();
    private Transform lastExit;

    void Start()
    {
        SpawnChunk(0);
        for (int i = 1; i < 3; i++)
        {
            SpawnChunk(Random.Range(0, chunkPrefabs.Length));
        }
    }

    void SpawnChunk(int index)
    {
        GameObject chunk = Instantiate(chunkPrefabs[index]);

        if (lastExit == null)
        {
            chunk.transform.position = Vector3.zero;
            chunk.transform.rotation = Quaternion.identity;
        }
        else
        {
            Transform entry = chunk.transform.Find("EntryPoint");
            Vector3 offset = lastExit.position - entry.position;
            chunk.transform.position += offset;
        }

        lastExit = chunk.transform.Find("ExitPoint");
        activeChunks.Add(chunk);
    }
}