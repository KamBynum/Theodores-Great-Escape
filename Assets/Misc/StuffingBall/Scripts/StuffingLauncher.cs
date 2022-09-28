using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StuffingLauncher : MonoBehaviour
{
    public GameObject   stuffingBall;
    public string       ballLayerName = "PlayerAttack";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Launch(float baseSpeed, Vector3 direction)
    {
        if (GameManager.Instance.fsm.GetStateCurrent() != GameManager.GameState.LevelSelect)
        {
            // FIXME TODO Need to add the baseSpeed to the initial launch speed of the ball!

            GameObject newBall = Instantiate(stuffingBall);
            newBall.layer = LayerMask.NameToLayer(ballLayerName);

            StuffingBallController ballController = newBall.GetComponent<StuffingBallController>();
            if (ballController)
            {
                ballController.speed += baseSpeed;
            }

            // Orient the ball to our forward direction
            newBall.transform.position = transform.position;
            //newBall.transform.up        = Vector3.up;
            //newBall.transform.forward   = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            newBall.transform.forward = direction;
        }
    }
}
