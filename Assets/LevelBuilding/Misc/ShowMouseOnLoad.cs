using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowMouseOnLoad : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
    }
}
