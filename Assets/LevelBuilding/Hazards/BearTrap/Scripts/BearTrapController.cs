using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BearTrapController : MonoBehaviour
{
    public float trapPower = 1f;

    private Animator        _anim;
    private BearTrappable   _trappedObject;

    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTrappedObject(BearTrappable obj)
    {
        _trappedObject = obj;
    }
    public BearTrappable GetTrappedObject()
    {
        return _trappedObject;
    }

    public void TrapOpened()
    {
        _anim.SetBool("isOpen", true);
    }
    public void TrapClosed()
    {
        EventManager.TriggerEvent<BearTrapCloseEvent, Vector3>(transform.position);
        _anim.SetBool("isOpen", false);

        // If the player escaped before the jaws snapped all of the way shut
        // then they won't be damaged!
        if (_trappedObject)
        {
            _trappedObject.BearTrapped(trapPower, transform.up);
        }
    }

}
