using UnityEngine;
using TMPro;

public class SpeedTapeScroller : MonoBehaviour
{
    public RectTransform tickContainer;
    public TMP_Text speedLabel;
    public float unitsPerTick = 5f;
    public float tickSpacing = 30f; // spacing between ticks in pixels
    public float currentSpeed;

    void Update()
    {
        float offset = (currentSpeed % unitsPerTick) / unitsPerTick * tickSpacing;
        tickContainer.localPosition = new Vector3(0f, offset, 0f);
        speedLabel.text = currentSpeed.ToString("F0");
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
    }
}
