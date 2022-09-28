using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public Animator lossTransition;

    public float transitionTime = 1f;
    public float lossTransitionStartTime = 1f;
    public float delayBeforeLevelRestart = 5.5f;

    public void LoadNextLevel(string sceneName)
    {
        StartCoroutine(LoadLevel(sceneName));
    }

    IEnumerator LoadLevel(string sceneName)
    {
        
        //Plays the animation
        transition.SetTrigger("Start");
        //Wait for animation to complete
        yield return new WaitForSeconds(transitionTime);
        //Load Scene
        SceneManager.LoadScene(sceneName);
        if(GameManager.Instance.fsm.TimeInState() > 3f)
        GameManager.Instance.fsm.ImmediateTransitionToState(GameManager.Instance.State);

    }
    public void AnimEventLoseLevel()
    {
        GameManager.Instance.AnimEventLoseAnimationComplete();
    }

}
