using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    public float minTimePerSlide = 0.25f;
    public bool tutorialStarted;
    public bool tutorialActive;
    public bool firstHoneyFound;
    public bool firstSuperHoneyFound;
    public bool firstCottonFound;
    public bool firstSuperPunchTriggered;
    public bool firstPillbugFound;
    public bool firstBeeFound;
    public bool firstSpiderFound;
    public bool firstBirdFound;
    public bool firstBreakableWallFound;
    public bool firstSmallTeddyTriggered;

    public GameObject tutorialBackground;
    public GameObject tutorialBackgroundImage;
    public GameObject initialPopup;
    public GameObject[] initialPopupSlides;
    public GameObject pillBugPopup;
    public GameObject[] pillbugPopupSlides;
    public GameObject beePopup;
    public GameObject[] beePopupSlides;
    public GameObject spiderPopup;
    public GameObject[] spiderPopupSlides;
    public GameObject birdPopup;
    public GameObject[] birdPopupSlides;
    public GameObject honeyPopup;
    public GameObject[] honeyPopupSlides;
    public GameObject cottonPopup;
    public GameObject[] cottonPopupSlides;
    public GameObject superPunchPopup;
    public GameObject[] superPunchPopupSlides;
    public GameObject superHoneyPopup;
    public GameObject[] superHoneyPopupSlides;
    public GameObject breakableWallPopup;
    public GameObject[] breakableWallPopupSlides;
    public GameObject smallTeddyPopup;
    public GameObject[] smallTeddyPopupSlides;

    private GameObject tutorialTopic;
    private string currentTopic;
    private float _timeSlideAppeared;

    // Big kludge to only allow the tutorial manager to pause time on level startup after this point
    // this is a workaround to the decentralized state management going on where we actually cannot
    // tell when certain level load activities have completed or not. As a work around, we will wait
    // ~5 seconds post startup... This only needs to work for another day so this dirty fix is good enough.
    private float _startupPauseHoldoff;

    void Awake()
    {
        if (initialPopup != null && initialPopup.transform.childCount > 0)
        {
            initialPopupSlides = new GameObject[initialPopup.transform.childCount];
            for (int i = 0; i < initialPopup.transform.childCount; ++i)
                initialPopupSlides[i] = initialPopup.transform.GetChild(i).gameObject;
        }
        if (pillBugPopup != null && pillBugPopup.transform.childCount > 0)
        {
            pillbugPopupSlides = new GameObject[pillBugPopup.transform.childCount];
            for (int i = 0; i < pillBugPopup.transform.childCount; ++i)
                pillbugPopupSlides[i] = pillBugPopup.transform.GetChild(i).gameObject;
        }
        if (beePopup != null && beePopup.transform.childCount > 0)
        {
            beePopupSlides = new GameObject[beePopup.transform.childCount];
            for (int i = 0; i < pillBugPopup.transform.childCount; ++i)
                beePopupSlides[i] = beePopup.transform.GetChild(i).gameObject;
        }
        if (spiderPopup != null && spiderPopup.transform.childCount > 0)
        {
            spiderPopupSlides = new GameObject[spiderPopup.transform.childCount];
            for (int i = 0; i < spiderPopup.transform.childCount; ++i)
                spiderPopupSlides[i] = spiderPopup.transform.GetChild(i).gameObject;
        }
        if (birdPopup != null && birdPopup.transform.childCount > 0)
        {
            birdPopupSlides = new GameObject[birdPopup.transform.childCount];
            for (int i = 0; i < birdPopup.transform.childCount; ++i)
                birdPopupSlides[i] = birdPopup.transform.GetChild(i).gameObject;
        }
        if (honeyPopup != null && honeyPopup.transform.childCount > 0)
        {
            honeyPopupSlides = new GameObject[honeyPopup.transform.childCount];
            for (int i = 0; i < honeyPopup.transform.childCount; ++i)
                honeyPopupSlides[i] = honeyPopup.transform.GetChild(i).gameObject;
        }
        if (cottonPopup != null && cottonPopup.transform.childCount > 0)
        {
            cottonPopupSlides = new GameObject[cottonPopup.transform.childCount];
            for (int i = 0; i < cottonPopup.transform.childCount; ++i)
                cottonPopupSlides[i] = cottonPopup.transform.GetChild(i).gameObject;
        }
        if (superPunchPopup != null && superPunchPopup.transform.childCount > 0)
        {
            superPunchPopupSlides = new GameObject[superPunchPopup.transform.childCount];
            for (int i = 0; i < superPunchPopup.transform.childCount; ++i)
                superPunchPopupSlides[i] = superPunchPopup.transform.GetChild(i).gameObject;
        }
        if (superHoneyPopup != null && superHoneyPopup.transform.childCount > 0)
        {
            superHoneyPopupSlides = new GameObject[superHoneyPopup.transform.childCount];
            for (int i = 0; i < superHoneyPopup.transform.childCount; ++i)
                superHoneyPopupSlides[i] = superHoneyPopup.transform.GetChild(i).gameObject;
        }
        if (breakableWallPopup != null && breakableWallPopup.transform.childCount > 0)
        {
            breakableWallPopupSlides = new GameObject[breakableWallPopup.transform.childCount];
            for (int i = 0; i < breakableWallPopup.transform.childCount; ++i)
                breakableWallPopupSlides[i] = breakableWallPopup.transform.GetChild(i).gameObject;
        }
        if (smallTeddyPopup != null && smallTeddyPopup.transform.childCount > 0)
        {
            smallTeddyPopupSlides = new GameObject[smallTeddyPopup.transform.childCount];
            for (int i = 0; i < smallTeddyPopup.transform.childCount; ++i)
                smallTeddyPopupSlides[i] = smallTeddyPopup.transform.GetChild(i).gameObject;
        }

    }
    void Start()
    {
    }
    private void Update()
    {
        if (tutorialBackgroundImage != null && tutorialBackground != null)
        {
            if (tutorialActive && Time.unscaledTime > _startupPauseHoldoff)
            {
                TimeManager.Instance.Pause();
            }

            if (Input.anyKeyDown && tutorialBackgroundImage.activeSelf && canProceed())
            {
                EventManager.TriggerEvent<ClickEvent, Vector3>(transform.position);
                NextMessage();
            }
            else if (GameManager.Instance.State == GameManager.GameState.InitialTutorial && !tutorialStarted)
            {
                StartInitialTutorial();
            }
        }

    }
    private bool canProceed()
    {
        return Time.unscaledTime > _timeSlideAppeared + minTimePerSlide;
    }

    private void TutorialInit()
    {
        GameManager.Instance.DisablePlayer();
        tutorialActive = true;
        // See notes at declaration of _startupPauseHoldoff on purpose
        _startupPauseHoldoff = Time.unscaledTime + 1f;

        tutorialBackground.SetActive(true);
        tutorialBackgroundImage.SetActive(true);
        for (int i = 1; i < tutorialBackgroundImage.transform.childCount; ++i)
        {
            if (tutorialBackgroundImage.transform.GetChild(i).gameObject.activeSelf)
            {
                tutorialTopic = tutorialBackgroundImage.transform.GetChild(i).gameObject;
                break;
            }
        }
        _timeSlideAppeared = Time.unscaledTime;
    }
    private void NextMessage()
    {
        bool found = false;

        for (int i = 0; i < tutorialTopic.transform.childCount; ++i)
        {
            GameObject slide = tutorialTopic.transform.GetChild(i).gameObject;
            if (slide.activeSelf && found == false)
            {
                _timeSlideAppeared = Time.unscaledTime;
                found = true;
                slide.SetActive(false);
                if(i != tutorialTopic.transform.childCount - 1)
                {
                    GameObject nextSlide = tutorialTopic.transform.GetChild(i + 1).gameObject;
                    nextSlide.SetActive(true);
                }
                else
                {
                    GameObject firstSlide = tutorialTopic.transform.GetChild(0).gameObject;
                    firstSlide.SetActive(true);
                    ResetTutorial();                               
                } 

            }
        }
    }
    public void StartInitialTutorial()
    {
        currentTopic = "Initial";
        initialPopup.SetActive(true);
        tutorialStarted = true;
        TutorialInit();
    }
    public void HoneyTutorial()
    {
        TimeManager.Instance.Pause();
        currentTopic = "Honey";
        honeyPopup.SetActive(true);
        TutorialInit();
    }
    public void CottonTutorial()
    {
        TimeManager.Instance.Pause();
        currentTopic = "Cotton";
        cottonPopup.SetActive(true);
        TutorialInit();
    }
    public void SuperPunchTutorial()
    {
        TimeManager.Instance.Pause();
        currentTopic = "SuperPunch";
        superPunchPopup.SetActive(true);
        TutorialInit();
    }
    public void SuperHoneyTutorial()
    {
        TimeManager.Instance.Pause();
        currentTopic = "SuperHoney";
        superHoneyPopup.SetActive(true);
        TutorialInit();
    }
    public void BreakableWallTutorial()
    {
        TimeManager.Instance.Pause();
        currentTopic = "BeakableWall";
        breakableWallPopup.SetActive(true);
        TutorialInit();
    }
    public void SmallTeddyTutorial()
    {
        TimeManager.Instance.Pause();
        currentTopic = "SmallTeddy";
        smallTeddyPopup.SetActive(true);
        TutorialInit();
    }
    public void PillbugTutorial()
    {
        TimeManager.Instance.Pause();
        currentTopic = "Pillbug";
        pillBugPopup.SetActive(true);
        TutorialInit();
    }
    public void BeeTutorial()
    {
        TimeManager.Instance.Pause();
        currentTopic = "Bee";
        beePopup.SetActive(true);
        TutorialInit();
    }
    public void SpiderTutorial()
    {
        TimeManager.Instance.Pause();
        currentTopic = "Spider";
        spiderPopup.SetActive(true);
        TutorialInit();
    }
    public void BirdTutorial()
    {
        TimeManager.Instance.Pause();
        currentTopic = "Bird";
        birdPopup.SetActive(true);
        TutorialInit();
    }
    private void ResetTutorial()
    {
        tutorialBackgroundImage.SetActive(false);
        tutorialBackground.SetActive(false);
        tutorialActive = false;
        initialPopup.SetActive(false);
        pillBugPopup.SetActive(false);
        beePopup.SetActive(false);
        spiderPopup.SetActive(false);
        birdPopup.SetActive(false);
        honeyPopup.SetActive(false);
        cottonPopup.SetActive(false);
        superPunchPopup.SetActive(false);
        superHoneyPopup.SetActive(false);
        breakableWallPopup.SetActive(false);
        smallTeddyPopup.SetActive(false);
        GameManager.Instance.EnablePlayer();
        TimeManager.Instance.Resume();
        if (currentTopic == "Initial") { 
        
            currentTopic = "";
            GameManager.Instance.fsm.ImmediateTransitionToState(GameManager.GameState.LevelSelect);
        }
        else
        {
            currentTopic = "";
        }
    }
}
