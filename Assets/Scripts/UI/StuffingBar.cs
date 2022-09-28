using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StuffingBar : MonoBehaviour
{
    public Slider slider;
    public GameObject lowStuffing;
    public GameObject normalStuffing;
    public GameObject highStuffing;
    public Image barHighlight;

    public float barHighlightFrequency = 0.75f;

    private bool _isHighlighted;

    private void Awake()
    {
        _isHighlighted = false;
        SetBarHighlightState(false);
    }

    private void Update()
    {
        if (barHighlight && barHighlight.enabled)
        {
            float alpha = 0.5f + 0.5f * Mathf.Sin(2f*Mathf.PI*barHighlightFrequency*Time.timeSinceLevelLoad);
            Color color = barHighlight.color;
            color.a     = alpha;

            barHighlight.color = color;
        }
    }

    public void SetMaxStuffing(float stuffing)
    {
        slider.maxValue = stuffing;
        slider.value = stuffing;
    }

    public void SetBarHighlightState(bool enable)
    {
        _isHighlighted = enable;
        if (barHighlight)
        {
            barHighlight.enabled = _isHighlighted;
        }
        UpdateIcon();
    }

    public bool GetBarHighlightState()
    {
        return _isHighlighted;
    }

    public void SetStuffing(float stuffing)
    {
        slider.value = stuffing;
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if(slider.normalizedValue < 0.35f)
        {
            // Small
            lowStuffing.SetActive(true);
            normalStuffing.SetActive(false);
            highStuffing.SetActive(false);
        }
        else
        {
            if (barHighlight && barHighlight.enabled)
            {
                // Large
                lowStuffing.SetActive(false);
                normalStuffing.SetActive(false);
                highStuffing.SetActive(true);
            }
            else
            {
                // Medium
                lowStuffing.SetActive(false);
                normalStuffing.SetActive(true);
                highStuffing.SetActive(false);
            }
        }
    }
}
