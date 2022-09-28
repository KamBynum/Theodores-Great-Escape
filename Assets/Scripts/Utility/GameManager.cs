using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance
    {
        get; private set;
    }
    public enum GameState
    {
        None,
        MainMenu,
        LevelSelect,
        Level,
        Victory,
        Lose,
        Restart,
        Exit,
        InitialTutorial
    }
    [Serializable]
    public struct Level
    {
        public string name;
        public GameObject data;
        public bool isComplete;
        public float completionTime;
        public float allotedTime;
        public List<HoneyPickup.Honey> honeys ;
        public Level(string newName, GameObject newData, List<HoneyPickup.Honey> newHoneys, bool newCompleted, float newCompletionTime, float newAllotedTime)
        {
            name = newName;
            data = newData;
            isComplete = newCompleted;
            completionTime = newCompletionTime;
            allotedTime = newAllotedTime;
            honeys = new List<HoneyPickup.Honey>();
            foreach (HoneyPickup.Honey honey in newHoneys)
                honeys.Add(honey);
        }
    }
    [Tooltip("State of the game")]
    public GameState State;
    public static event Action<GameState> GameStateChanged;
    [Tooltip("List of all levels and data for those levels")]
    public List<Level> levels = new List<Level>();
    [Tooltip("Current level the player is on")]
    public GameObject currentLevelData;
    [Tooltip("Reference to LevelLoader")]
    [SerializeField]LevelLoader levelLoader;
    [Tooltip("Reference to tutorialManager")]
    public TutorialManager tutorialManager;
    [Tooltip("Reference to tutorialManager")]
    public SaveManager saveManager;
    public FSM<GameState> fsm;
    public Animator victoryAnim;

    [Header("Level State")]
    private float _levelDataSearchDelay = 2f;
    private bool _levelInit;
    private bool _newLevel;

    [Header("Tutorial State")]
    private bool _playerDisabled;
    [Header("Lose State")]
    public float loseTransitionTime = 1f;
    private bool _lossAnimationComplete;
    private bool _levelLost;

    [Header("Player")]
    private MonoBehaviour _playerController;
    private MonoBehaviour _cameraController;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
        tutorialManager = transform.Find("HUD").gameObject.transform.Find("Tutorial Popup").gameObject.GetComponent<TutorialManager>();
        saveManager = GetComponent<SaveManager>();
        victoryAnim = transform.Find("HUD").gameObject.GetComponent<Animator>();
        fsm = new FSM<GameState>("GameManager",
                            GameState.None,
                            Time.timeSinceLevelLoad,
                            null,
                            Debug.Log);
        fsm.RegisterState(GameState.None, "None", null, null, null);
        fsm.RegisterState(GameState.MainMenu, "MainMenu", MainMenuStateEnter, null, null);
        fsm.RegisterState(GameState.LevelSelect, "LevelSelect", LevelSelectStateEnter, null, null);
        fsm.RegisterState(GameState.InitialTutorial, "InitialTutorial", InitialTutorialStateEnter, InitialTutorialStateActive, InitialTutorialStateExit);
        fsm.RegisterState(GameState.Level, "Level", LevelStateEnter, LevelStateActive, LevelStateExit);
        fsm.RegisterState(GameState.Victory, "Victory", VictoryStateEnter, null, null);

        fsm.RegisterState(GameState.Lose, "Lose", LoseStateEnter, LoseStateActive, LoseStateExit);
        fsm.RegisterState(GameState.Restart, "Restart", RestartStateEnter, RestartStateActive, null);

        fsm.RegisterState(GameState.Exit, "Exit", ExitLevelStateEnter, ExitLevelStateActive, null);
    
    }

    void Start()
    {
        EventManager.TriggerEvent<MusicStartEvent, Vector3>(transform.position);
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            fsm.ImmediateTransitionToState(GameState.MainMenu);
        }
        else if (SceneManager.GetActiveScene().name == "LevelSelect")
        {
            fsm.ImmediateTransitionToState(GameState.LevelSelect);
        }
        else
        {
            fsm.ImmediateTransitionToState(GameState.Level);
        }
    }

    private void Update()
    {
        State = fsm.GetStateCurrent();
        fsm.Update(Time.unscaledTime);
    }

    public void RegisterPlayerInScene(MonoBehaviour playerController, MonoBehaviour cameraController)
    {
        _playerController = playerController;
        _cameraController = cameraController;
    }

    public PlayerControllerV3_3 Player()
    {
        return _playerController.GetComponent<PlayerControllerV3_3>();
    }
    public CapsuleCollider PlayerCollider()
    {
        return _playerController.GetComponent<CapsuleCollider>();
    }
    public Transform PlayerTransform()
    {
        return _playerController.gameObject.transform;
    }
    public TakesFallDamage FallDamage()
    {
        return _playerController.GetComponent<TakesFallDamage>();
    }
    ////////////////////////////////////////////////////////////////////////////
    // Main Menu State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void MainMenuStateEnter()
    {
        Debug.Log("Main Menu");
    }

    ////////////////////////////////////////////////////////////////////////////
    // Tutorial State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void InitialTutorialStateEnter()
    {
        transform.Find("HUD").gameObject.transform.Find("Total Honey Tracker").gameObject.SetActive(false);
       
    }
    private void InitialTutorialStateActive()
    {
        if(tutorialManager != null && !_playerDisabled && _playerController)
        {
            _playerDisabled = SetPlayerEnableState(false);
        }
    }
    private void InitialTutorialStateExit()
    {
        if (tutorialManager != null)
        {
            _playerDisabled = SetPlayerEnableState(true);
            transform.Find("HUD").gameObject.transform.Find("Total Honey Tracker").gameObject.SetActive(true);
        }

    }
    private bool SetPlayerColliderEnableState(bool isEnabled)
    {
        if (_playerController)
        {
            PlayerCollider().enabled = isEnabled;
            FallDamage().enabled = isEnabled;
        }
        else
        {
            Debug.LogError("No player controller is registered with the GameManager. Cannot set enabled state.");
            return false;
        }
        if (_cameraController)
        {
            _cameraController.enabled = isEnabled;
        }
        else
        {
            Debug.LogError("No camera controller is registered with the GameManager. Cannot set enabled state.");
            return false;
        }
        return true;
    }
    public bool EnablePlayer()
    {
        if (tutorialManager != null)
        {
            return SetPlayerEnableState(true);
        }
        return false;
    }
    public bool DisablePlayer()
    {
        if (tutorialManager != null)
        {
            return SetPlayerEnableState(false);
        }
        return false;
    }
    private bool SetPlayerEnableState(bool isEnabled)
    {
        if (_playerController)
        {
            _playerController.enabled = isEnabled;
        }
        else
        {
            Debug.LogError("No player controller is registered with the GameManager. Cannot set enabled state.");
            return false;
        }
        if (_cameraController)
        {
            _cameraController.enabled = isEnabled;
        }
        else
        {
            Debug.LogError("No camera controller is registered with the GameManager. Cannot set enabled state.");
            return false;
        }
        return true;
        
    }

    ////////////////////////////////////////////////////////////////////////////
    //Level Select State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void LevelSelectStateEnter()
    {
        SetPlayerColliderEnableState(true);
        transform.Find("HUD").gameObject.transform.Find("Total Honey Tracker").gameObject.SetActive(true);
        Debug.Log("There are " + levels.Count + " visited levels.");
        //Hide HUD and remove current level obj
        transform.Find("HUD").gameObject.transform.Find("Current Level Honey Tracker").gameObject.SetActive(false);
        transform.Find("HUD").gameObject.transform.Find("Honey Bar").gameObject.SetActive(false); 
        transform.Find("HUD").gameObject.transform.Find("Stuffing Bar").gameObject.SetActive(false);
        transform.Find("HUD").gameObject.transform.Find("Timer").gameObject.SetActive(false);
        transform.Find("HUD").gameObject.transform.Find("VictoryPopup").gameObject.SetActive(false);
        if (currentLevelData != null)
        {   //Set honey pickups inactive if not in a level
            foreach (Transform honeyPickup in currentLevelData.transform)
            {
                honeyPickup.gameObject.SetActive(false);
            }
        }
        TimeManager.Instance.ClearTimer();
        currentLevelData = null;
    }

    ////////////////////////////////////////////////////////////////////////////
    //Level State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void LevelStateEnter()
    {
        SetPlayerColliderEnableState(true);
        ResourceManager.Instance.CollectHoney(ResourceManager.Instance.maxHoney);
        ResourceManager.Instance.currentStuffing = 0;
        ResourceManager.Instance.CollectCotton(ResourceManager.Instance.maxStuffing / 2);
        transform.Find("HUD").gameObject.transform.Find("Total Honey Tracker").gameObject.SetActive(false);
        //Initiate HUD
        transform.Find("HUD").gameObject.transform.Find("Current Level Honey Tracker").gameObject.SetActive(true);
        transform.Find("HUD").gameObject.transform.Find("Honey Bar").gameObject.SetActive(true);
        transform.Find("HUD").gameObject.transform.Find("Stuffing Bar").gameObject.SetActive(true);
        transform.Find("HUD").gameObject.transform.Find("Timer").gameObject.SetActive(true);
        //Restart Timer
        TimeManager.Instance.ClearTimer();


        

        LevelLoaderInit();

    }
    private void LevelStateActive()
    {   //Check to see if scene data is captured in the game manager
        if(fsm.TimeInState() > _levelDataSearchDelay && !_levelInit)
        {
            _levelInit= true;
            if (FindLevelData())
            {
                _newLevel = true;
                //Invoke("NewLevelInit", _levelDataInitDelay);
            }
        }
        if(_newLevel && currentLevelData != null)
        {
            _newLevel = false;
            NewLevelInit();
        }
    }

    private void LevelStateExit()
    {
        LevelLoaderInit();
        _levelInit = false;
        _newLevel = false;

    }

    private bool FindLevelData()
    {
        Level level = levels.Find(x => x.name == SceneManager.GetActiveScene().name);
        if (level.name != null)
        {
            Debug.Log("Level has already been visited.");
            //Set honey pickups on current level active 
            foreach (Transform honeyPickup in currentLevelData.transform)
            {
                honeyPickup.gameObject.SetActive(true);
            }
            return false;
        }
        else
        {
            return true;
        }
    }

    private void NewLevelInit()
    {
        Debug.Log("Level has not been visited.");
        //get reference to honeys
        if (currentLevelData != null)
        {
            List<HoneyPickup.Honey> honeyReferences = currentLevelData.gameObject.GetComponent<LevelData>().honeys;
            LevelData levelData = currentLevelData.GetComponent<LevelData>();
            levels.Add(new Level(SceneManager.GetActiveScene().name, currentLevelData, honeyReferences, levelData.isComplete, levelData.completionTime, levelData.allotedTime));

        }
        Debug.Log("New number of levels: " + levels.Count);
    }

    ////////////////////////////////////////////////////////////////////////////
    //Exit Level State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void ExitLevelStateEnter()
    {
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        transform.Find("HUD").gameObject.transform.Find("VictoryPopup").gameObject.transform.Find("AllHoneyCollected").gameObject.SetActive(false);
        transform.Find("HUD").gameObject.transform.Find("VictoryPopup").gameObject.SetActive(false);
        victoryAnim.SetBool("Victory", false);
        EraseCurrentLevelHoneyProgress();

    }
    private void ExitLevelStateActive()
    {
        if (levelLoader == null)
        {
            LevelLoaderInit();
        }
        else { 
            levelLoader.LoadNextLevel("LevelSelect");
            fsm.ImmediateTransitionToState(GameState.LevelSelect);

        }
    }
    ////////////////////////////////////////////////////////////////////////////
    //Restart Level State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void RestartStateEnter()
    {
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        transform.Find("HUD").gameObject.transform.Find("VictoryPopup").gameObject.transform.Find("AllHoneyCollected").gameObject.SetActive(false);
        transform.Find("HUD").gameObject.transform.Find("VictoryPopup").gameObject.SetActive(false);
        victoryAnim.SetBool("Victory", false);
        
        
       
    }
    private void RestartStateActive()
    {
        if(levelLoader == null)
        {
            LevelLoaderInit();
        }
        else
        {
            levelLoader.LoadNextLevel(SceneManager.GetActiveScene().name);
            ResourceManager.Instance.CollectHoney(ResourceManager.Instance.maxHoney);
            ResourceManager.Instance.currentStuffing = 0;
            ResourceManager.Instance.CollectCotton(ResourceManager.Instance.maxStuffing / 2);
            EraseCurrentLevelHoneyProgress();
            fsm.ImmediateTransitionToState(GameState.Level);

        }
    }
        
////////////////////////////////////////////////////////////////////////////
//Lose State Handlers
////////////////////////////////////////////////////////////////////////////
private void LoseStateEnter()
    {
        SetPlayerColliderEnableState(false);
        if(ResourceManager.Instance.currentHoney <= 0)
        {
           // TimeManager.Instance.LossSlowDown();
        }
        SetPlayerEnableState(false);
        LevelLoaderInit();
    }
    private void LoseStateActive()
    {
        //Wait a few seconds after entering loss state before playing loss animation
        if(fsm.TimeInState() > loseTransitionTime && !_levelLost)
        {
            _levelLost = true;
            LoseLevel();
        }
        if (_lossAnimationComplete)
        {
            fsm.SetNextState(GameState.Restart);
        }
    }

    private void LoseStateExit()
    {
        _lossAnimationComplete = false;
        _levelLost = false;
    }

    private void LoseLevel()
    {
        EventManager.TriggerEvent<GameOverEvent, Vector3>(transform.position);
        //Plays the animation
        LevelLoaderInit();
        levelLoader.transition.SetTrigger("Lose");
    }

    public void AnimEventLoseAnimationComplete()
    {
        _lossAnimationComplete = true;
    }
  

    ////////////////////////////////////////////////////////////////////////////
    //Victory State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void VictoryStateEnter()
    {
        
        Debug.Log("Handling Victory");
        if(currentLevelData != null)
        {
            int pickedCount = 0;
            LevelData levelData = currentLevelData.GetComponent<LevelData>();
            if (levelData.completionTime > (levelData.allotedTime - TimeManager.Instance.levelRemainingTime) || !levelData.isComplete)
            {
                levelData.completionTime = levelData.allotedTime - TimeManager.Instance.levelRemainingTime;
                Debug.Log("New Fastest Completion Time.");
            }
            //Set level data to complete
            levelData.isComplete = true;
            for (int i = 0; i < levelData.honeys.Count; i++)
            {
                HoneyPickup honeyReference = levelData.honeys[i].data.GetComponent<HoneyPickup>();
                if (honeyReference.pickedUpPrior)
                {
                    HoneyPickup.Honey honey = levelData.honeys[i];
                    honey.completed = true;
                    levelData.honeys[i] = honey;
                    ++pickedCount;
                }

            }
            //foreach (Level level in levels)
            for (int i = 0; i < levels.Count; i++)
            {   //Save picked up honey data to game manager
                if (levels[i].name == levelData.level)
                {
                    Level completedLevel = levels[i];
                    completedLevel.isComplete = true;
                    levels[i] = completedLevel;
                    for (int j = 0; j < levels[i].honeys.Count; j++)
                    {
                        HoneyPickup honeyReference = levels[i].honeys[j].data.GetComponent<HoneyPickup>();
                        if (honeyReference.pickedUpPrior)
                        {
                            HoneyPickup.Honey honey = levels[i].honeys[j];
                            honey.pickedUpPrior = true;
                            honey.completed = true;
                            levels[i].honeys[j] = honey;
                        }
                    }
                }

            }
            if (pickedCount == levelData.honeys.Count)
            {
                transform.Find("HUD").gameObject.transform.Find("VictoryPopup").gameObject.transform.Find("AllHoneyCollected").gameObject.SetActive(true);
            }
        }
        victoryAnim.SetBool("Victory", true);
        //TimeManager.Instance.VictorySlowDown();
        transform.Find("HUD").gameObject.transform.Find("VictoryPopup").gameObject.SetActive(true);
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        EventManager.TriggerEvent<VictoryEvent, Vector3>(transform.position);
    }

    ////////////////////////////////////////////////////////////////////////////
    //Other Methods
    ////////////////////////////////////////////////////////////////////////////

    private void LevelLoaderInit()
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
    

    public void EraseCurrentLevelHoneyProgress()
    {
        if (currentLevelData != null)
        {
            Debug.Log("Erasing Data");
            LevelData levelData = currentLevelData.GetComponent<LevelData>();
            List<HoneyPickup.Honey> currentHoneys = levelData.honeys;
            for (int i = 0; i < currentHoneys.Count; i++)
            {
                HoneyPickup honeyReference = currentHoneys[i].data.GetComponent<HoneyPickup>();
                if (honeyReference.pickedUpPrior && !currentHoneys[i].completed)
                {
                    honeyReference.pickedUpPrior = false;
                    HoneyPickup.Honey honey = currentHoneys[i];
                    honey.pickedUpPrior = false;
                    currentHoneys[i] = honey;
                    ResourceManager.Instance.totalHoney--;
                }
            }

            foreach (Level level in levels)
            {
                for (int i = 0; i < level.honeys.Count; i++)
                {
                    HoneyPickup honeyReference = level.honeys[i].data.GetComponent<HoneyPickup>();
                    if (honeyReference.pickedUpPrior && !currentHoneys[i].completed)
                    {
                        HoneyPickup.Honey honey = level.honeys[i];
                        honey.pickedUpPrior = false;
                        level.honeys[i] = honey;
                    }

                }
            }
        }
    }

    bool isPlaying(Animator anim, string stateName)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
                anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            return true;
        else
            return false;
    }

}