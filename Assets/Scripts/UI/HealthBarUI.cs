using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public  static HealthBarUI Instance { get; private set; }
    [SerializeField] private Slider slider;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetValue(float normalized)
    {
        slider.value = normalized;
    }
}
