using UnityEngine;
using UnityEngine.UI;  // if you need to update a UI slider here

[DisallowMultipleComponent]
public class PlayerShipHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    [HideInInspector] public float currentHealth;

    [Tooltip("How much health to restore on pickup")]
    public float healAmount = 25f;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Called by bullets, explosions, etc.
    /// </summary>
    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        UpdateHealthUI();
        if (currentHealth <= 0f)
            Die();
    }

    /// <summary>
    /// Called by health‐packs (or you can keep your existing OnTriggerEnter).
    /// </summary>
    public void RecoverHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        UpdateHealthUI();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HealthPack"))
        {
            RecoverHealth(healAmount);
            Destroy(other.gameObject);
        }
    }

    void UpdateHealthUI()
    {
        // TODO: Implement UI update when HealthBarUI is created
        // Example: HealthBarUI.Instance?.SetValue(currentHealth / maxHealth);
        
        // For now, just log the health change for debugging
        Debug.Log($"Player Health: {currentHealth:F1}/{maxHealth:F1} ({(currentHealth/maxHealth)*100:F0}%)");
    }

    void Die()
    {
        // explosion, game‐over logic, disable ship, etc.
        gameObject.SetActive(false);
    }
}
