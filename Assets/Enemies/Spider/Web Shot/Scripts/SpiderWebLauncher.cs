using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderWebLauncher : MonoBehaviour
{
    public GameObject webBall;
    private Vector3 newPos;
    private bool _launched = true;
    private bool _wideLaunch = false;
    SpiderController spider;
    NavMeshAgent spiderNavMesh;


    private void Awake()
    {
        newPos = Vector3.zero;
        spider = transform.parent.parent.parent.parent.parent.GetComponent<SpiderController>();
        spiderNavMesh = transform.parent.parent.parent.parent.parent.GetComponent<NavMeshAgent>();
    }
    private void Update()
    {
        if((newPos != Vector3.zero && !_launched) || _wideLaunch)
        {
            FacePlayer(newPos);
        }
    }
    public bool Launch(GameObject player)
    {
        _launched = false;
        GameObject newBall = Instantiate(webBall);
        newBall.layer = LayerMask.NameToLayer("EnemyAttack");
        SpiderWebController webShot = newBall.GetComponent<SpiderWebController>();
        float timeToCollide = (player.transform.position - transform.position).magnitude / webShot.speed;
        newPos = player.transform.position + player.GetComponent<Rigidbody>().velocity * timeToCollide;
        RaycastHit hit;
        bool wallHit = Physics.Raycast(player.transform.position, newPos, out hit, 2f);
        if (wallHit)
        {
            newPos = hit.point;
        }
        newBall.transform.position = transform.position;
        newBall.transform.forward = (newPos - transform.position + new Vector3(0f, 0.5f, 0f)).normalized;
        _launched = true;
        EventManager.TriggerEvent<SpiderWebAttackEvent, Vector3>(transform.position);
        return _launched;
    }

    public bool WideLaunch(GameObject player, Vector3[] points)
    {
        _wideLaunch = true;
        for (int i = 0; i < points.Length; ++i){
            GameObject newBall = Instantiate(webBall);
            newBall.layer = LayerMask.NameToLayer("EnemyAttack");
            SpiderWebController webShot = newBall.GetComponent<SpiderWebController>();
            newBall.transform.position = transform.position;
            newBall.transform.forward = (points[i] - transform.position).normalized;
        }
        EventManager.TriggerEvent<SpiderWebAttackEvent, Vector3>(transform.position);
        _wideLaunch = false;
        return _launched;
    }
    private void FacePlayer(Vector3 newPos)
    {
        
        Vector3 lookPos = (newPos - transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(-lookPos);
       
        if (spider && spiderNavMesh)
        {
            gameObject.transform.rotation = Quaternion.Slerp(transform.rotation, rotation, spiderNavMesh.angularSpeed);
        }
        
    }
}