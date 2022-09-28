using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelStats : MonoBehaviour
{
    public string levelName;
    public LevelData thisLevel;
    GameObject[] levels;
    [SerializeField] GameObject stats;
    [SerializeField] GameObject lockedScreen;
    [SerializeField] GameObject lockedText;
    [SerializeField] GameObject lockedTextHeader;
    public int numToUnlock;

    private float _percentToUnlock = 0.75f;
    private int _numRemaining;
    public int currentLevelHoneyTotal;
    // Start is called before the first frame update
    void Start()
    {
        levelName = transform.Find("Portal").gameObject.GetComponent<Portal>().sceneName;
        stats = transform.Find("Level Stats").transform.Find("Stats").gameObject;
        lockedScreen = transform.Find("Level Stats").transform.Find("Locked").gameObject;
        lockedTextHeader = lockedScreen.transform.Find("Level Locked Text").gameObject;
        lockedText = lockedScreen.transform.Find("Number to unlock").gameObject ;

        levels = GameObject.FindGameObjectsWithTag("LevelData");
        foreach (GameObject level in levels)
        {
            LevelData leveldata = level.GetComponent<LevelData>();
            if (leveldata.level == levelName)
            {
                thisLevel = leveldata;
                break;
            }
            if (leveldata.level == SceneManager.GetActiveScene().name)
            {
                thisLevel = leveldata;
                break;
        }
        }
        
    }
    
    // Update is called once per frame
    void Update()
    {
        

        if (GameManager.Instance.fsm.GetStateCurrent() == GameManager.GameState.LevelSelect)
        {
            if (thisLevel != null || ResourceManager.Instance.totalHoney >= numToUnlock)
            {
                lockedScreen.SetActive(false);
                stats.SetActive(true);
                transform.Find("OpenDoor").gameObject.SetActive(true);
                transform.Find("Portal").gameObject.SetActive(true);
                stats.transform.Find("Level Name").gameObject.GetComponent<Text>().text = levelName;
                if (thisLevel != null)
                {
                    if (thisLevel.isComplete)
                    {
                        stats.transform.Find("Level Complete").gameObject.transform.Find("Gold Star").gameObject.SetActive(true);
                        stats.transform.Find("Level Complete").gameObject.transform.Find("Red X").gameObject.SetActive(false);
                    }
                    else
                    {
                        stats.transform.Find("Level Complete").gameObject.transform.Find("Gold Star").gameObject.SetActive(false);
                        stats.transform.Find("Level Complete").gameObject.transform.Find("Red X").gameObject.SetActive(true);
                    }
                    for (int i = 0; i < thisLevel.honeys.Count; i++)
                    {
                        HoneyPickup honeyReference = thisLevel.honeys[i].data.GetComponent<HoneyPickup>();
                        if (honeyReference.pickedUpPrior)
                        {
                            currentLevelHoneyTotal++;
                        }

                    }
                    if (currentLevelHoneyTotal == thisLevel.honeys.Count)
                    {
                        stats.transform.Find("Honey Collected").gameObject.transform.Find("Gold Star").gameObject.SetActive(true);
                        stats.transform.Find("Honey Collected").gameObject.transform.Find("Red X").gameObject.SetActive(false);
                    }
                    else
                    {
                        stats.transform.Find("Honey Collected").gameObject.transform.Find("Gold Star").gameObject.SetActive(false);
                        stats.transform.Find("Honey Collected").gameObject.transform.Find("Red X").gameObject.SetActive(true);
                    }
                    currentLevelHoneyTotal = 0;
                    if (thisLevel.completionTime <= thisLevel.allotedTime / 2 && thisLevel.completionTime != 0)
                    {
                        stats.transform.Find("Speedy Time").gameObject.transform.Find("Gold Star").gameObject.SetActive(true);
                        stats.transform.Find("Speedy Time").gameObject.transform.Find("Red X").gameObject.SetActive(false);
                    }
                    else
                    {
                        stats.transform.Find("Speedy Time").gameObject.transform.Find("Gold Star").gameObject.SetActive(false);
                        stats.transform.Find("Speedy Time").gameObject.transform.Find("Red X").gameObject.SetActive(true);
                    }
                }
            }
            else
            {

                lockedScreen.SetActive(true);
                _numRemaining = numToUnlock - ResourceManager.Instance.totalHoney;
                lockedText.transform.GetComponent<Text>().text = "Find " + _numRemaining.ToString("0") + " more honeys in previous levels to unlock this level";
                transform.Find("OpenDoor").gameObject.SetActive(false);
                transform.Find("Portal").gameObject.SetActive(false);
            }
        }
        else if (GameManager.Instance.fsm.GetStateCurrent() == GameManager.GameState.Level)
        {// While in level state, check to see if threshold of honeys were collected
            if (thisLevel != null)
            {
                for (int i = 0; i < thisLevel.honeys.Count; i++)
                {
                    HoneyPickup honeyReference = thisLevel.honeys[i].data.GetComponent<HoneyPickup>();
                    if (honeyReference.pickedUpPrior)
                    {
                        currentLevelHoneyTotal++;
                    }

                }
                //Unlock door if specified count (numToUnlock) is found
                //Otherwise unlock if current count is more than 75% of total
                if ((numToUnlock != 0 && currentLevelHoneyTotal >= numToUnlock) || (numToUnlock == 0 && currentLevelHoneyTotal >= thisLevel.honeys.Count * _percentToUnlock))
                {
                    transform.Find("OpenDoor").gameObject.SetActive(true);
                    lockedScreen.SetActive(false);

                    transform.Find("Portal").gameObject.SetActive(true);
                }
                else
                {
                    transform.Find("OpenDoor").gameObject.SetActive(false);
                    transform.Find("Portal").gameObject.SetActive(false);
                    lockedTextHeader.GetComponent<Text>().text = "Exit Locked";
                    lockedScreen.SetActive(true);
                    if(numToUnlock != 0)
                    {
                        _numRemaining = numToUnlock - currentLevelHoneyTotal;
                    }
                    else
                    {
                        _numRemaining = (int)(thisLevel.honeys.Count * _percentToUnlock) - currentLevelHoneyTotal;
                    }
                    lockedText.transform.GetComponent<Text>().text = "Find " + _numRemaining.ToString("0") + " more honeys to win!";

                }
                currentLevelHoneyTotal = 0;



            }
        }
    }
}
