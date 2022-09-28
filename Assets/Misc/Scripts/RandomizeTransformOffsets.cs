using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeTransformOffsets : MonoBehaviour
{
    [System.Serializable]
    public struct Range
    {
        public bool enable;
        public float min;
        public float max;
    }

    [SerializeField] private Range randomizePosition;
    [SerializeField] private Range randomizeRotation;
    [SerializeField] private Range randomizeScale;


    void Start()
    {
        if (randomizePosition.enable)
        {
            Vector3 offset = (randomizePosition.max - randomizePosition.min) * Random.insideUnitSphere + randomizePosition.min * Vector3.one;
            transform.position += offset;
        }

        if (randomizeRotation.enable)
        {
            Quaternion rotation;
            rotation    = Quaternion.AngleAxis(Random.Range(randomizeRotation.min, randomizeRotation.max), transform.right);
            rotation    *= Quaternion.AngleAxis(Random.Range(randomizeRotation.min, randomizeRotation.max), transform.forward);
            rotation    *= Quaternion.AngleAxis(Random.Range(randomizeRotation.min, randomizeRotation.max), transform.up);
            transform.rotation = rotation;
        }

        if (randomizeScale.enable)
        {
            Vector3 offset = (randomizeScale.max - randomizeScale.min) * Random.insideUnitSphere + randomizeScale.min * Vector3.one;
            transform.localScale += offset;
        }
    }
}
