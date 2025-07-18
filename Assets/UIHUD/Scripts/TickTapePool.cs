// C# script for tick pooling
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TickTapePool : MonoBehaviour
{
    public RectTransform container;
    public GameObject tickPrefab;
    public int poolSize = 20;
    public float tickSpacing = 30f;

    private List<GameObject> pool = new List<GameObject>();
    private float currentValue;
    private float scrollOffset;

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var tick = Instantiate(tickPrefab, container);
            tick.transform.localPosition = new Vector3(0, i * tickSpacing, 0);
            pool.Add(tick);
        }
    }

    void Update()
    {
        scrollOffset += Time.deltaTime * 20f; // scrolling speed
        if (scrollOffset >= tickSpacing)
        {
            scrollOffset -= tickSpacing;

            // Recycle the bottom tick
            GameObject recycled = pool[0];
            pool.RemoveAt(0);
            pool.Add(recycled);

            // Move it to the top
            float newY = pool[pool.Count - 2].transform.localPosition.y + tickSpacing;
            recycled.transform.localPosition = new Vector3(0, newY, 0);

            // Update label (if it has one)
            TMP_Text label = recycled.GetComponentInChildren<TMP_Text>();
            if (label) label.text = (currentValue += 5f).ToString("F0");
        }

        // Scroll the container
        container.localPosition = new Vector3(0, -scrollOffset, 0);
    }
}
