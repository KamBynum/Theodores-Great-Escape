using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GameManager))]
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    public float levelRemainingTime;
    public float lossSlowdowFactor = 0.05f;
    public float lossSlowdownLength = 3f;
    public float victorySlowdowFactor = 0.05f;
    public float victorySlowdownLength = 3f;
    private float _levelTimer = 0.0f;
    public float levelTimerStartDelay = 2f;
    public Text currentTime;
    private bool _entryComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Update()
    {
        if(Time.timeScale != 0 && GameManager.Instance.State != GameManager.GameState.Victory && !GameManager.Instance.tutorialManager.tutorialActive)
        {
            Time.timeScale += (1f / lossSlowdownLength) * Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
            UpdateLevelTimer();
        }
        else if (GameManager.Instance.State == GameManager.GameState.Victory)
        {

            Time.timeScale += (1f / victorySlowdownLength) * Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);

        }
        if (GameManager.Instance.victoryAnim.GetBool("Victory") && GameManager.Instance.victoryAnim.GetCurrentAnimatorStateInfo(0).length <= GameManager.Instance.victoryAnim.GetCurrentAnimatorStateInfo(0).normalizedTime)
        {
            TimeManager.Instance.Pause();

        }

        if (levelRemainingTime <= 0 && GameManager.Instance.State == GameManager.GameState.Lose)
        {
            currentTime.text = "Time's Up!";
        }
    }
    public void Pause()
    {
        Time.timeScale = 0f;
    }
    public void Resume()
    {
        Time.timeScale = 1f;
    }

    public void LossSlowDown()
    {
        Time.timeScale = lossSlowdowFactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
    public void VictorySlowDown()
    {
        Time.timeScale = victorySlowdowFactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        GameManager.Instance.victoryAnim.SetBool("Victory", true);
    }

    public void UpdateLevelTimer()
    {
        if (GameManager.Instance.State == GameManager.GameState.Level && Time.timeScale != 0 && !_entryComplete)
        {
            _levelTimer += Time.deltaTime;
            levelRemainingTime = _levelTimer;
            if (levelRemainingTime > levelTimerStartDelay)
            {
                
                ResetTimer();
                ResourceManager.Instance.CollectHoney(ResourceManager.Instance.maxHoney);
            }
        }
        else if (GameManager.Instance.State == GameManager.GameState.Level && Time.timeScale != 0 && _entryComplete)
        {
            _levelTimer += Time.deltaTime;
            levelRemainingTime = (GameManager.Instance.currentLevelData.GetComponent<LevelData>().allotedTime - _levelTimer);
            currentTime.text = GetTime(levelRemainingTime);
            if(levelRemainingTime <= 0)
            {
                GameManager.Instance.fsm.ImmediateTransitionToState(GameManager.GameState.Lose);
            }
        }
    }
    public void ClearTimer()
    {
        levelRemainingTime = 0;
        _levelTimer = 0;
        currentTime.text = GetTime(levelRemainingTime);
        _entryComplete = false;
    }

    public void ResetTimer()
    {
        levelRemainingTime = 0.000000001f;
        _levelTimer = 0;
        currentTime.text = GetTime(levelRemainingTime);
        _entryComplete = true;
    }
    public string GetTime(float rawTime)
    {
        float minutes = Mathf.Floor(rawTime / 60f);
        float seconds = Mathf.Floor(rawTime % 60f);
        
        if(seconds >= 10)
        {
            return  minutes.ToString("0") + " : " + seconds.ToString("0");  
        }
        else
        {
            return minutes.ToString("0") + " : 0" + seconds.ToString("0");
        }
        
    }

}
