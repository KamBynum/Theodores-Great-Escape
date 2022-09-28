using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebShotCounter : MonoBehaviour
{

    public Text webCount;
    public GameObject webIcon;
    public GameObject webIconBackground;

    void Update()
    {
        if (webCount && GameManager.Instance.fsm.GetStateCurrent() == GameManager.GameState.Level)
        {
            webCount.text = GameManager.Instance.Player().GetWebCount().ToString();
            if (GameManager.Instance.Player().GetWebCount() == 0)
            {
                webCount.gameObject.SetActive(false);
                webIcon.SetActive(false);
                webIconBackground.SetActive(false);
            }
            else
            {
                webCount.gameObject.SetActive(true);
                webIcon.SetActive(true);
                webIconBackground.SetActive(true);
            }
        }
        
    }
}
