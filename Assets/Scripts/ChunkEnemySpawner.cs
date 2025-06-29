using UnityEngine;

public class ChunkEnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("EnemySpawn"))
            {
                Instantiate(enemyPrefab, child.position, Quaternion.identity);

            }
        }
    }
}
