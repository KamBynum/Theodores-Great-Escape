using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelData : MonoBehaviour
{
    public string level;
    public bool isComplete;
    public float completionTime;
    public float allotedTime;

    private bool _saveFileLoaded;
    private bool _levelSelectLoaded;


    [Tooltip("List of honeys on this levels")]
    public List<HoneyPickup.Honey> honeys = new List<HoneyPickup.Honey>();

    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.levels.Count != 0 && GameManager.Instance.levels.Exists(x => x.name == SceneManager.GetActiveScene().name))
                    {
                        Destroy(this.gameObject);
                    }
        }
        
        DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {
        if (GameManager.Instance.State == GameManager.GameState.Level)
        {
            _saveFileLoaded = true;
            this.level = SceneManager.GetActiveScene().name;
            this.name = SceneManager.GetActiveScene().name + " Data";

            if (honeys.Count == 0)
            {
                GameObject[] temp = GameObject.FindGameObjectsWithTag("HoneyPickup");
                foreach (GameObject honey in temp)
                {
                    honeys.Add(new HoneyPickup.Honey(false, honey.transform, false));
                }
            }
            Debug.Log("Number of honeys on " + this.level + ": " + honeys.Count);
        }

        
            
        
    }
    private void Update()
    {
        CaptureCurrentLevel();
        UpdateHoneyListUponPickup();
        CheckForLoad();
        UpdateGameManagerForSave();
    }

    private void CaptureCurrentLevel()
    {
        if (GameManager.Instance.State == GameManager.GameState.Level && this.level == SceneManager.GetActiveScene().name)
        {
            GameManager.Instance.currentLevelData = this.gameObject;
        }
    }
    private void CheckForLoad()
    {
        //Reinitialize honeys upon loading from main menu
        if (GameManager.Instance.State == GameManager.GameState.MainMenu && !_saveFileLoaded)
        {
            Debug.Log("Loading Previous Data");

            GameObject[] temp = GameObject.FindGameObjectsWithTag("HoneyPickup");
            foreach (GameObject honey in temp)
            {
                if (isComplete && honey.transform.IsChildOf(this.transform) && honey.GetComponent<HoneyPickup>().pickedUpPrior)
                {
                    this.honeys.Add(new HoneyPickup.Honey(true, honey.transform, true));
                }
                else if (honey.transform.IsChildOf(this.transform))
                {
                    this.honeys.Add(new HoneyPickup.Honey(false, honey.transform, false));
                }
                honey.SetActive(false);

            }
            Debug.Log("Number of honeys on " + this.level + ": " + this.honeys.Count);
            foreach (GameManager.Level level in GameManager.Instance.levels)
            {
                if (level.name == this.level)
                {
                    this.isComplete = level.isComplete;
                    this.completionTime = level.completionTime;
                    this.allotedTime = level.allotedTime;
                }
            }
            _saveFileLoaded = true;
        }
    }

    private void UpdateGameManagerForSave()
    {
        if (GameManager.Instance.State == GameManager.GameState.LevelSelect && !_levelSelectLoaded)
        {
            for (int i = 0; i < GameManager.Instance.levels.Count; i++)
            {
                LevelData currentData = GameManager.Instance.levels[i].data.GetComponent<LevelData>();
                if (currentData.level == this.level)
                {
                    GameManager.Level currentLevel = GameManager.Instance.levels[i];
                    currentLevel.isComplete = this.isComplete;
                    currentLevel.completionTime = this.completionTime;
                    GameManager.Instance.levels[i] = currentLevel;

                }
            }
            _levelSelectLoaded = true;
        }
        else
        {
            _levelSelectLoaded = false;
        }
    }

    private void UpdateHoneyListUponPickup()
    {
        //Initial capture of honeys
        if (this.level == SceneManager.GetActiveScene().name)
        {
            for (int i = 0; i < honeys.Count; i++)
            {
                HoneyPickup honeyReference = honeys[i].data.GetComponent<HoneyPickup>();
                if (honeyReference.pickedUpPrior)
                {
                    HoneyPickup.Honey honey = honeys[i];
                    honey.pickedUpPrior = true;
                    honeys[i] = honey;
                }

            }
        }
    }
}

  
