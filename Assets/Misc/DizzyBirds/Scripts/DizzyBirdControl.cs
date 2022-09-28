using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DizzyBirdControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AnimEventOnTweet()
    {
        EventManager.TriggerEvent<DizzyBirdTweetEvent, Vector3>(transform.position);
    }
}
