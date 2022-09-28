using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SuperHoneyPickup : MonoBehaviour
{
    public bool pickedUpPrior = false;
    private Animator _anim;
    public Material mat;

    public float powerDuration = 10f;

    [System.Serializable]
    public struct Honey
    {
        public bool pickedUpPrior;
        public bool completed;
        public Transform data;
        public Vector3 position;
 

        public Honey(bool wasPickedUp, Transform newData, bool levelComplete)
        {
            pickedUpPrior = wasPickedUp;
            data = newData;
            position = data.position;
            completed = levelComplete;
        }
    }
    private void Update()
    {
        _anim = GetComponent<Animator>();
        if (_anim == null)
        {
            Debug.LogError("Animator not found!");
        }
        else
        {
            if (pickedUpPrior)
            {
                _anim.SetBool("Picked", true);
                mat.color = Color.HSVToRGB(Mathf.Repeat(Time.timeSinceLevelLoad, 1) % 2, 1, 1);
                GetComponent<MeshRenderer>().material = mat;
            }
            else
            {
                _anim.SetBool("Picked", false);
                GetComponent<MeshRenderer>().material.color = Color.HSVToRGB(Mathf.Repeat(Time.timeSinceLevelLoad, 1) % 2, 1, 1);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Player":
                HandleTriggerEnterPlayer(other);
                break;
            default:
                break;
        }
    }

    private void HandleTriggerEnterPlayer(Collider other)
    {
        if (!this.pickedUpPrior)
        {
            ResourceManager.Instance.IncreaseTotalHoney(1);
        }
        EventManager.TriggerEvent<CollectSuperHoneyEvent, Vector3>(transform.position);
        this.pickedUpPrior = true;
        this.gameObject.SetActive(false);
        ResourceManager.Instance.CollectSuperHoney(1, powerDuration);
        if (!GameManager.Instance.tutorialManager.firstSuperHoneyFound)
        {
            GameManager.Instance.tutorialManager.firstSuperHoneyFound = true;
            GameManager.Instance.tutorialManager.SuperHoneyTutorial();
        }
    }
}
