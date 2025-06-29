using UnityEngine;
using UnityEngine.UI;

public class PlayerShipHealth : MonoBehaviour
{
    [Tooltip("How much health to restore on pickup")]
    public float healAmount = 25f;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"HealthPack hit {other.name} (tag={other.tag})");
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerShipController>();
            if (player != null)
            {
                player.RecoverHealth(healAmount);
                Destroy(gameObject);
            }
        }
    }
}
