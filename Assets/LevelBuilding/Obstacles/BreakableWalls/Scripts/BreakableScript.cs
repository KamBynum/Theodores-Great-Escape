using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(SphereCollider))]
public class BreakableScript : MonoBehaviour, IPunchable
{
    public List<GameObject> Children = new List<GameObject>();
    private Dictionary<GameObject, float> Dists = new Dictionary<GameObject, float>();
    public List<GameObject> Dislodged = new List<GameObject>();

    public float PowerThreshold = 1.8f;
    public bool BreakAll = false;
    public bool ClearRubble = true;
    public float BreakRadius = 2.5f;
    public float CellMass = 1f;
    public float CellDrag = 0f;
    public float CellAngularDrag = 0.05f;
    public float VelocityCoefficient = 0.5f;
    private int DestrucablesLayer = 0;
    private SphereCollider collider;
    void Start()
    {
        DestrucablesLayer = LayerMask.NameToLayer("Destructables");
        int count = this.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            GameObject child = this.transform.GetChild(i).gameObject;
            child.GetComponent<Rigidbody>().mass = CellMass;
            child.GetComponent<Rigidbody>().drag = CellDrag;
            child.GetComponent<Rigidbody>().angularDrag = CellAngularDrag;
            Children.Add(child);
        }
        collider = GetComponent<SphereCollider>();
        collider.radius = 5f;
        collider.isTrigger = true;
    }

    void Update()
    {

    }

    public void HandlePunch(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        // check that player is powerful enough
        if (power < PowerThreshold) return;
        // Sort children by distance from punch impact
        if (!BreakAll)
        {
            Children = Children.OrderBy(x =>
            {
                float dist = Vector3.Distance(x.transform.position, point);
                if (!Dists.ContainsKey(x))
                    Dists.Add(x, dist);
                return dist;
            }).ToList();
        }

        // Dislodge children
        List<GameObject> NewChildren = new List<GameObject>(Children);
        for (int i = 0; i < Children.Count; i++)
        {
            GameObject child = Children[i];
            if (!BreakAll && Dists[child] > BreakRadius)
                break;
            child.layer = DestrucablesLayer;
            Dislodged.Add(child);
            NewChildren.Remove(child);
            child.GetComponent<Rigidbody>().isKinematic = false;
            float DistanceCoefficient = ((Children.Count - i) / Children.Count) * 1000;
            // TODO: Change 1.8f to power once it is updating
            child.GetComponent<Rigidbody>().AddForce(power * direction.normalized * DistanceCoefficient * VelocityCoefficient);
        }
        EventManager.TriggerEvent<WallBreakEvent, Vector3, float>(point, power);
        Children = NewChildren;
        if (ClearRubble) StartCoroutine(CleanUp());
        Dists.Clear();
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
        if (!GameManager.Instance.tutorialManager.firstBreakableWallFound)
        {
            GameManager.Instance.tutorialManager.firstBreakableWallFound = true;
            GameManager.Instance.tutorialManager.BreakableWallTutorial();
        }
    }
    IEnumerator CleanUp()
    {
        // wait X seconds, then set collider to trigger so objects fall out of the way
        yield return new WaitForSeconds(1.5f);
        List<GameObject> Removed = new List<GameObject>();
        Dislodged.ForEach(x =>
        {
            Removed.Add(x);
            Children.Remove(x);
            x.GetComponent<Collider>().isTrigger = true;
        });
        // delete once off screen
        yield return new WaitForSeconds(2);
        Removed.ForEach(x =>
        {
            Dislodged.Remove(x);
            Destroy(x);
        });
    }
}
