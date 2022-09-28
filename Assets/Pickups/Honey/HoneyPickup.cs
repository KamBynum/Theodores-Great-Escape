using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HoneyPickup : MonoBehaviour
{
    public bool pickedUpPrior = false;
    private Animator _anim;
    public Material mat;

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
                GetComponent<MeshRenderer>().material = mat;
            }
            else
            {
                _anim.SetBool("Picked", false);

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
        EventManager.TriggerEvent<CollectHoneyEvent, Vector3>(transform.position);
        this.pickedUpPrior = true;
        this.gameObject.SetActive(false);
        ResourceManager.Instance.CollectHoney(1);
        if (!GameManager.Instance.tutorialManager.firstHoneyFound)
        {
            GameManager.Instance.tutorialManager.firstHoneyFound = true;
            GameManager.Instance.tutorialManager.HoneyTutorial();
        }
    }
}
