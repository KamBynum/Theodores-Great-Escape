using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance {get ; private set ;}
    [Tooltip("Maximum honey of the character")]
    public float maxHoney = 10f;

    [Tooltip("Current honey of the character")]
    public float currentHoney;

    [Tooltip("Reference to Honey Bar")]
    public HoneyBar honeyBar;

    [Tooltip("Total Honey collected in the game")]
    public int totalHoney;

    [Tooltip("Maximum stuffing of the character")]
    public float maxStuffing = 10f;

    [Tooltip("Current stuffing of the character")]
    public float currentStuffing;

    [Tooltip("Reference to Stuffing Bar")]
    public StuffingBar stuffingBar;

    [Tooltip("Honey Particles")]
    public ParticleSystem honeyParticles;

    [Tooltip("Super Honey Duration")]
    public float superHoneyDuration = 10f;

    [Tooltip("Super Honey Active")]
    public bool hasSuperHoney = false;
    //private static ResourceManager instance;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        InitPlayerUI();
        ClampStuffingLevel();
        ClampHoneyLevel();
        UpdatePlayerUI();
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    ////////////////////////////////////////////////////////////////////////////
    /// Honey and Stuffing management
    ////////////////////////////////////////////////////////////////////////////

    public void TakeDamage(float Damage)
    {
        if (hasSuperHoney) return;
        if(GameManager.Instance.State == GameManager.GameState.Level && TimeManager.Instance.levelRemainingTime > 2f)
        {
            currentHoney -= Damage;

            ClampHoneyLevel();
            ClampStuffingLevel();
            UpdatePlayerUI();

            if (honeyParticles)
            {
                honeyParticles.transform.position = GameManager.Instance.PlayerTransform().position + 0.5f*Vector3.up;
                honeyParticles.Play();
            }

            if (currentHoney <= 0)
            {
                GameManager.Instance.Player().Death();
                GameManager.Instance.fsm.ImmediateTransitionToState(GameManager.GameState.Lose);
                //GameManager.Instance.UpdateGameState(GameManager.GameState.Lose);
            }
        }
        
    }

    public void CollectSuperHoney(float qty, float duration)
    {
        currentHoney += qty;
        ClampHoneyLevel();
        UpdatePlayerUI();
        StartCoroutine(ConsumeSuperHoney(duration));
    }

    IEnumerator ConsumeSuperHoney(float duration)
    {
        
        hasSuperHoney = true;
        Debug.Log("Consuming Super Honey");
        yield return new WaitForSeconds(duration);
        Debug.Log("Super Honey Consumed");
        hasSuperHoney = false;
    }

    public void CollectHoney(float honey)
    {
        currentHoney += honey;
        ClampHoneyLevel();
        UpdatePlayerUI();
    }

    public void IncreaseTotalHoney(int honey)
    {
        totalHoney += honey;
        UpdatePlayerUI();
    }
    public float CollectCotton(float cotton)
    {
        // Used to add or subtract cotton. If more than max cotton
        // is attempted to be added, then the remained overflow amount
        // is returned. If more than 0 cotton is attempted to be
        // removed then the delta negative amount is returned.
        currentStuffing += cotton;
        float remainder = ClampStuffingLevel();
        UpdatePlayerUI();

        return remainder;
    }

    public float GetCurrentStuffing()
    {
        return currentStuffing;
    }

    public float GetMaxStuffing()
    {
        return maxStuffing;
    }

    public void SetSuperPunchState(bool isActive)
    {
        stuffingBar.SetBarHighlightState(isActive);
    }

    public bool GetSuperPunchState()
    {
        return stuffingBar.GetBarHighlightState();
    }

    private void ClampHoneyLevel()
    {
        currentHoney = Mathf.Clamp(currentHoney, 0f, maxHoney);
    }

    private float ClampStuffingLevel()
    {
        float remainder = 0f;
        if (currentStuffing < 0)
        {
            remainder = currentStuffing;
        }
        else if (currentStuffing > maxStuffing)
        {
            remainder = currentStuffing - maxStuffing;
        }
        currentStuffing = Mathf.Clamp(currentStuffing, 0f, maxStuffing);

        return remainder;
    }

    private void InitPlayerUI()
    {
        currentStuffing = maxStuffing / 2;
        currentHoney = maxHoney;
        if (null != honeyBar)
        {
            honeyBar.SetMaxHoney(maxHoney);
        }
        if (null != stuffingBar)
        {
            stuffingBar.SetMaxStuffing(maxStuffing);
        }
    }

    private void UpdatePlayerUI()
    {
        if (null != honeyBar)
        {
            honeyBar.SetHoney(currentHoney);
        }
        if (null != stuffingBar)
        {
            stuffingBar.SetStuffing(currentStuffing);
        }
    }

}
