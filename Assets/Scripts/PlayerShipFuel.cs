using UnityEngine;
using UnityEngine.UI;

public class PlayerShipFuel : MonoBehaviour
{
    public float maxFuel = 100f;
    public float currentFuel;
    public float idleFuelDrainRate = 1f;
    public float boostFuelDrainRate = 5f;
    public Slider fuelSlider;

    private bool isBoosting = false;
    private bool isRefueling = false;

    void Start()
    {
        currentFuel = maxFuel;
        UpdateFuelUI();
    }

    void Update()
    {
        if (currentFuel > 0 && !isRefueling)
        {
            float drainRate = isBoosting ? boostFuelDrainRate : idleFuelDrainRate;
            currentFuel -= drainRate * Time.deltaTime;
            currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
        }

        UpdateFuelUI();
    }

    public void SetBoosting(bool boosting)
    {
        isBoosting = boosting;
    }

    void UpdateFuelUI()
    {
        if (fuelSlider != null)
        {
            fuelSlider.value = currentFuel / maxFuel;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FuelBarge"))
        {
            isRefueling = true;
            currentFuel = maxFuel;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("FuelBarge"))
        {
            isRefueling = false;
        }
    }
}
