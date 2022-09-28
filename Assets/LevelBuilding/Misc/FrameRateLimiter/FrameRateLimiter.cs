using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameRateLimiter : MonoBehaviour
{
    public int targetFrameRate = 60;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = targetFrameRate;
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.targetFrameRate != targetFrameRate)
        {
            Application.targetFrameRate = targetFrameRate;
        }
    }
}
