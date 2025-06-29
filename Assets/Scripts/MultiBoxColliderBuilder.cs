using UnityEngine;

public class MultiBoxColliderBuilder : MonoBehaviour
{
    [System.Serializable]
    public class BoxColliderConfig
    {
        public string name = "HitBox";
        public Vector3 localPosition = Vector3.zero;
        public Vector3 size = Vector3.one;
    }

    public BoxColliderConfig[] colliders = new BoxColliderConfig[]
    {
        new BoxColliderConfig { name = "Body", localPosition = new Vector3(0f, 0f, 0f), size = new Vector3(2f, 1f, 6f) },
        new BoxColliderConfig { name = "Left Wing", localPosition = new Vector3(-2.5f, 0f, 0f), size = new Vector3(2f, 0.5f, 2f) },
        new BoxColliderConfig { name = "Right Wing", localPosition = new Vector3(2.5f, 0f, 0f), size = new Vector3(2f, 0.5f, 2f) },
        new BoxColliderConfig { name = "Tail", localPosition = new Vector3(0f, 0.5f, -3f), size = new Vector3(1f, 1f, 1f) },
    };

    public bool showDebugColliders = false;
    public Material debugMaterial;

    void Start()
    {
        foreach (var config in colliders)
        {
            GameObject child = new GameObject(config.name);
            child.transform.parent = transform;
            child.transform.localPosition = config.localPosition;
            child.transform.localRotation = Quaternion.identity;

            BoxCollider box = child.AddComponent<BoxCollider>();
            box.size = config.size;
            box.isTrigger = false;

            if (showDebugColliders && debugMaterial != null)
{
    MeshFilter mf = child.AddComponent<MeshFilter>();
    mf.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

    MeshRenderer mr = child.AddComponent<MeshRenderer>();
    mr.material = debugMaterial;

    // 🔧 Match mesh scale to collider size
    child.transform.localScale = config.size;
}
        }

        // Destroy(this); // Comment this out to keep the script in play mode
    }
}
