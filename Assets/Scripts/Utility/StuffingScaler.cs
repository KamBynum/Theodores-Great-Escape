using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StuffingScaler : MonoBehaviour
{
    [Header("Stuffing Scale Limits")] 
    [SerializeField] private AnimationCurve scaleEnemyDmg   = AnimationCurve.EaseInOut(0f, 2.0f,    1.0f, 0.25f);
    [SerializeField] private AnimationCurve scalePunchPower = AnimationCurve.EaseInOut(0f, 0.5f,    1.0f, 2.0f);
    [SerializeField] private AnimationCurve scaleSize       = AnimationCurve.EaseInOut(0f, 0.33f,   1.0f, 1.0f);
    [SerializeField] private AnimationCurve scaleMass       = AnimationCurve.EaseInOut(0f, 1.0f,    1.0f, 1.0f);
    [SerializeField] private AnimationCurve scaleJumpHeight = AnimationCurve.EaseInOut(0f, 4f,      1.0f, 0.5f);
    [SerializeField] private AnimationCurve scaleJumpSpan   = AnimationCurve.EaseInOut(0f, 8f,      1.0f, 2f);
    public float GetScale()
    {
        if (ResourceManager.Instance)
        {
            return ResourceManager.Instance.GetCurrentStuffing() / ResourceManager.Instance.GetMaxStuffing();
        }
        else
        {
            return 1f;
        }
    }

    public float GetScaleEnemyDmg()
    {
        return scaleEnemyDmg.Evaluate(GetScale());
    }
    public float GetScalePunchPower()
    {
        return scalePunchPower.Evaluate(GetScale());
    }
    public float GetScaleSize()
    {
        return scaleSize.Evaluate(GetScale());
    }
    public float GetScaleMass()
    {
        return scaleMass.Evaluate(GetScale());
    }
    public float GetScaleJumpHeight()
    {
        return scaleJumpHeight.Evaluate(GetScale());
    }
    public float GetScaleJumpSpan()
    {
        return scaleJumpSpan.Evaluate(GetScale());
    }
}
