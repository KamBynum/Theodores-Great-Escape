using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public string sceneName;
    LevelLoader levelLoader;

    void Awake()
    {
        var levelLoaderGO = GameObject.FindGameObjectWithTag("LevelLoader");
        if (null != levelLoaderGO)
        {
            levelLoader = levelLoaderGO.GetComponentInChildren<LevelLoader>();
        }
        else
        {
            Debug.Log("No level loader found");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Player":
                HandleTriggerEnterPortal(other);
                break;
            default:
                break;
        }
    }

    private void HandleTriggerEnterPortal(Collider other)
    {
        //If player walks through a door, change scene
        if (this.sceneName == "LevelSelect" && GameManager.Instance.State != GameManager.GameState.LevelSelect && GameManager.Instance.State != GameManager.GameState.Victory)
        {
            GameManager.Instance.fsm.ImmediateTransitionToState(GameManager.GameState.Victory);
            Destroy(this);
        }

        else if(this.sceneName != "LevelSelect")
        {   
            levelLoader.LoadNextLevel(this.sceneName);
            GameManager.Instance.fsm.ImmediateTransitionToState(GameManager.GameState.Level);
            Destroy(this);
        }
        
    }


}
