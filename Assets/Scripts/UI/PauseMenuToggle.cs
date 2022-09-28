using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class PauseMenuToggle : MonoBehaviour
{
    [SerializeField] GameObject playButton;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject restartButton;
    [SerializeField] GameObject exitLevelButton;
    [SerializeField] GameObject optionsButton;
    [SerializeField] GameObject options;
    [SerializeField] GameObject saveButton;
    [SerializeField] GameObject dontSaveButton;
    [SerializeField] GameObject VictoryRestartButton;
    [SerializeField] GameObject VictoryExitLevelButton;
    [SerializeField] GameObject tutorialButton;
    [SerializeField] GameObject confirmQuit;
    [SerializeField] GameObject confirmQuitGame;
    [SerializeField] Text quitButtonText;
    [SerializeField] Text quitButtonTextShadow;
    [SerializeField] GameObject optionsCloseButton;
    [SerializeField] GameObject exitCancelButton;
    [SerializeField] GameObject quitCancelButton;
    private bool _saveState;
    private bool _restartState;

    private LevelLoader GetLevelLoader()
    {
        var levelLoaderGO = GameObject.FindGameObjectWithTag("LevelLoader");
        if (null != levelLoaderGO)
        {
            return levelLoaderGO.GetComponentInChildren<LevelLoader>();
        }
        else
        {
            Debug.Log("No level loader found");
            return null;
        }
    }

    private void Update()
    {
        if (GameManager.Instance.fsm.GetStateCurrent() == GameManager.GameState.LevelSelect)
        {
            restartButton.SetActive(false);
            exitLevelButton.SetActive(false);
            tutorialButton.SetActive(true);
            optionsButton.SetActive(true);
        }
        else
        {
            restartButton.SetActive(true);
            exitLevelButton.SetActive(true);
            tutorialButton.SetActive(false);
            optionsButton.SetActive(false);
        }
        if (GameManager.Instance.fsm.GetStateCurrent() == GameManager.GameState.Victory && saveButton.activeSelf && !_saveState)
        {
            _saveState = true;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(saveButton);
        }
        else if(GameManager.Instance.fsm.GetStateCurrent() == GameManager.GameState.Victory && VictoryExitLevelButton.activeSelf && !_restartState)
        {
            _restartState = true;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(VictoryExitLevelButton);
        }

    }
   
    public void Pause()
    {
        if (GameManager.Instance.State != GameManager.GameState.Lose && GameManager.Instance.State !=GameManager.GameState.InitialTutorial && GameManager.Instance.State != GameManager.GameState.Victory)
        {
            EventManager.TriggerEvent<ClickEvent, Vector3>(transform.position);
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            pauseMenu.SetActive(true);
            TimeManager.Instance.Pause();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(playButton);
        }

    }

    public void Resume()
    {
        EventManager.TriggerEvent<ClickEvent, Vector3>(transform.position);
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        pauseMenu.SetActive(false);
        confirmQuit.SetActive(false);
        confirmQuitGame.SetActive(false);
        TimeManager.Instance.Resume();
    }

    public void MainMenu()
    {
        EventManager.TriggerEvent<ClickEvent, Vector3>(transform.position);
        LevelLoader levelLoader = GetLevelLoader();
        if (null != levelLoader)
        {
            GameManager.Instance.EraseCurrentLevelHoneyProgress();
            levelLoader.LoadNextLevel("MainMenu");
            GameManager.Instance.fsm.ImmediateTransitionToState(GameManager.GameState.MainMenu);
            Debug.Log("Returning to Main Menu");
            Resume();
            GameManager.Instance.transform.Find("HUD").gameObject.transform.Find("Current Level Honey Tracker").gameObject.SetActive(false);
            GameManager.Instance.transform.Find("HUD").gameObject.transform.Find("Honey Bar").gameObject.SetActive(false);
            GameManager.Instance.transform.Find("HUD").gameObject.transform.Find("Stuffing Bar").gameObject.SetActive(false);
            GameManager.Instance.transform.Find("HUD").gameObject.transform.Find("Timer").gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("No level loader found");
        }
    }
    public void QuitLevel()
    {
        pauseMenu.SetActive(false);
        confirmQuit.SetActive(true);
        quitButtonText.text = "Exit Level";
        quitButtonTextShadow.text = "Exit Level";
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(exitCancelButton);
    }
    public void QuitLevelToMain()
    {
        pauseMenu.SetActive(false);
        if(GameManager.Instance.fsm.GetStateCurrent() != GameManager.GameState.LevelSelect)
        {
            confirmQuit.SetActive(true);
            quitButtonText.text = "Main Menu";
            quitButtonTextShadow.text = "Main Menu";
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(exitCancelButton);
        }
        else
        {
            MainMenu();
        }
        
    }
    public void QuitGameWindow()
    {
        pauseMenu.SetActive(false);
        confirmQuitGame.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(quitCancelButton);
    }
    public void ShowOptions()
    {
        options.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsCloseButton);
    }
    public void CloseOptions()
    {
        options.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(playButton);
    }
    public void LevelSelect()
    {

        EventManager.TriggerEvent<ClickEvent, Vector3>(transform.position);
        if (GameManager.Instance.State == GameManager.GameState.LevelSelect)
        {
            Debug.Log("Not in a level");
        }
        else
        {
            GameManager.Instance.fsm.ImmediateTransitionToState(GameManager.GameState.Exit);
            Debug.Log("Returning to Level Select");
        }
        
        Resume();
        _restartState = false;
        _saveState = false;
    }

    public void QuitGame()
    {
        EventManager.TriggerEvent<ClickEvent, Vector3>(transform.position);
        Debug.Log("Quitting Game");
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                    Application.Quit();
        #endif  // UNITY_EDITOR
    }

    public void Restart()
    {
        EventManager.TriggerEvent<ClickEvent, Vector3>(transform.position);
        if (GameManager.Instance.State == GameManager.GameState.LevelSelect)
        {
            Debug.Log("Cannot restart level select.");
        }
        else
        {
            LevelLoader levelLoader = GetLevelLoader();
            if (null != levelLoader)
            {
                GameManager.Instance.fsm.ImmediateTransitionToState(GameManager.GameState.Restart);
                Debug.Log("Restarting Level");
                Resume();
                _restartState = false;
                _saveState = false;
            }
            else
            {
                Debug.Log("No level loader found");
            }
            
        }
        
    }
    public void DisableSaveButtons()
    {
        saveButton.SetActive(false);
        dontSaveButton.SetActive(false);
        VictoryRestartButton.SetActive(true);
        VictoryExitLevelButton.SetActive(true);
    }
    public void EnableSaveButtons()
    {
        saveButton.SetActive(true);
        dontSaveButton.SetActive(true);
        VictoryRestartButton.SetActive(false);
        VictoryExitLevelButton.SetActive(false);
    }
    public void RestartTutorial()
    {
        GameManager.Instance.fsm.ImmediateTransitionToState(GameManager.GameState.InitialTutorial);
        GameManager.Instance.tutorialManager.StartInitialTutorial();
        Resume();
    }
    public void LoseProgress()
    {
        if(quitButtonText.text == "Main Menu")
        {
            MainMenu(); 
        }
        else
        {
            LevelSelect();
        }
    }
}
