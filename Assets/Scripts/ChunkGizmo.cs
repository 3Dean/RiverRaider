using UnityEngine;

public class ChunkGizmo : MonoBehaviour
{
    public Transform exitPoint;

    void OnDrawGizmos()
    {
        if (exitPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, exitPoint.position);
            Gizmos.DrawSphere(exitPoint.position, 1f);
        }
    }
}