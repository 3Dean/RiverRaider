using UnityEngine;

public class ChunkScroller : MonoBehaviour
{
    public float scrollSpeed = 10f;

    void Update()
    {
        transform.Translate(Vector3.back * scrollSpeed * Time.deltaTime);
    }
}
