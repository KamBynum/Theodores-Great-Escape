using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
public class MainMenu : MonoBehaviour
{
    public string firstScene = "LevelSelect";
    public GameObject optionsScreen;
    public Button loadDataButton;
    public GameObject startGameButton;
    public GameObject closeOptionsButton;
    public Button newGameButton;
    public Text newGameText;
    public Text newGameTextShadow;

    public void Start()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(startGameButton);
        if (GameManager.Instance.levels.Count != 0 || !File.Exists(Application.dataPath + "/SaveData/playerData.txt") || GameManager.Instance.tutorialManager.tutorialStarted)
        {
            Disable();
        }
      
        
    }
    private void Update()
    {
        GameObject[] levels = GameObject.FindGameObjectsWithTag("LevelData");
        if ((GameManager.Instance.tutorialManager.tutorialStarted || loadDataButton.interactable == false) && levels.Length != 0)
        {
            newGameText.text = "Start Game";
            ShowNewGameButton();
        }
        else
        {
            HideNewGameButton();
        }
        newGameTextShadow.text = newGameText.text;
    }

    public void StartGame()
    {
        EventManager.TriggerEvent<ClickEvent, Vector3>(transform.position);
        SceneManager.LoadScene(firstScene);
        if (GameManager.Instance.tutorialManager.tutorialStarted)
        {
            GameManager.Instance.fsm.ImmediateTransitionToState(GameManager.GameState.LevelSelect);
        }
        else
        {
            GameManager.Instance.fsm.ImmediateTransitionToState(GameManager.GameState.InitialTutorial);
        }
        
    }
    public void NewGame()
    {
        GameManager.Instance.tutorialManager.tutorialStarted = false;
        ResourceManager.Instance.totalHoney = 0;
        GameManager.Instance.levels.Clear();
        GameObject[] levels = GameObject.FindGameObjectsWithTag("LevelData");
        foreach (GameObject levelData in levels)
        {

            List<HoneyPickup.Honey> currentHoneys = levelData.transform.GetComponent<LevelData>().honeys;
            for (int i = 0; i < currentHoneys.Count; i++)
            {
                HoneyPickup honeyReference = currentHoneys[i].data.GetComponent<HoneyPickup>();
                honeyReference.pickedUpPrior = false;
            }
            Destroy(levelData.gameObject);
        }
        StartGame();
        
    }

    public void ShowOptions()
    {
        EventManager.TriggerEvent<ClickEvent, Vector3>(transform.position);
        optionsScreen.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(closeOptionsButton);
    }

    public void CloseOptions()
    {
        EventManager.TriggerEvent<ClickEvent, Vector3>(transform.position);
        optionsScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(startGameButton);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif  // UNITY_EDITOR
    }
    public void Disable()
    {

        if (File.Exists(Application.dataPath + "/SaveData/playerData.txt") || GameManager.Instance.tutorialManager.tutorialStarted)
        {
            EventManager.TriggerEvent<ClickEvent, Vector3>(transform.position);
            loadDataButton.interactable = false;
            newGameText.text = "Start Game";
            GameManager.Instance.tutorialManager.tutorialStarted = true;
        }
        else
        {
            loadDataButton.interactable = false;
        }
        
    }

    public void ShowNewGameButton()
    {
        newGameButton.gameObject.SetActive(true);
    }

    public void HideNewGameButton()
    {
        newGameButton.gameObject.SetActive(false);
    }
}
