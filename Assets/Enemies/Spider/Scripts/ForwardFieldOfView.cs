using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardFieldOfView : MonoBehaviour
{
    public float fov = 90f;
    public float currentFOVAngle = 0f;
    public float FOVDistance = 10f;
    public float lookHeight = 0.1f;


    public  bool playerInView;
    private Vector3 playerDirection;
    private Vector3 playerPosition;
    private float playerTime;
    private bool playerFound;
    private Vector3 _playerHeightOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private int _raycast = 5;
    private float _degreesToNextRaycast;
    private Vector3[] vertices;
    private int vertexIndex;
    private Vector3 origin;
    private int[] triangles;
    private int triangleIndex;
    private Vector2[] uv;
    private float initialAngle;

    private void Start()
    {
        playerTime = 0f;
        WideViewUpdate();
    }
    void Update()
    {
        if (transform.GetComponentInParent<SpiderController>().isWideWebAttacking())
        {
            WideViewUpdate();
        }
        if(transform.GetComponentInParent<SpiderController>().isApproaching() || transform.GetComponentInParent<SpiderController>().isDetecting() || transform.GetComponentInParent<SpiderController>().isPunching() || transform.GetComponentInParent<SpiderController>().isLeaping())
        {
            ForwardUpdate();
        }

    }
    private void ForwardUpdate()
    {
        currentFOVAngle = initialAngle;
        RaycastHit hit;
        LayerMask layerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("Wall") | LayerMask.GetMask("Ground") | LayerMask.GetMask("Player") | LayerMask.GetMask("Destructables");
        bool wallHit = Physics.Raycast(origin, (transform.forward + new Vector3(0f, playerDirection.normalized.y, 0f)), out hit, FOVDistance, layerMask);
        if (!wallHit)
        {
            Debug.DrawRay(origin, (transform.forward + new Vector3(0f, playerDirection.normalized.y, 0f)) * FOVDistance, Color.green);

        }
        else
        {
            if (hit.collider.CompareTag("Player"))
            {
                playerInView = true;
                playerTime = Time.timeSinceLevelLoad;
                Debug.DrawRay(origin, (transform.forward + new Vector3(0f, playerDirection.normalized.y, 0f)) * hit.distance, Color.red);

            }
            else
            {
                playerInView = false;
                Debug.DrawRay(origin, (transform.forward + new Vector3(0f, playerDirection.normalized.y, 0f)) * hit.distance, Color.yellow);
            }
        }
    }

    public bool WideViewUpdate()
    {
        _degreesToNextRaycast = fov / _raycast;
        if(initialAngle > 180)
        {
            currentFOVAngle = initialAngle - 180;
        }
        else
        {
            currentFOVAngle = initialAngle + 180;
        }
        vertices = new Vector3[_raycast + 2];
        vertices[0] = origin;
        triangles = new int[_raycast * 3];
        uv = new Vector2[vertices.Length];
        vertexIndex = 1;
        triangleIndex = 0;
        LayerMask layerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("Wall") | LayerMask.GetMask("Ground") | LayerMask.GetMask("Player") | LayerMask.GetMask("Destructables");
        for (int i = 0; i <= _raycast; i++)
        {

            Vector3 vertex;
            RaycastHit hit; 
            bool wallHit = Physics.Raycast(origin, (GetVector(currentFOVAngle)), out hit, FOVDistance, layerMask);
            if (!wallHit)
            {
                vertex = origin + ( GetVector(currentFOVAngle) ) * FOVDistance;
                //Debug.DrawRay(origin, (GetVector(currentFOVAngle) ) * FOVDistance, Color.green);
            }
            else
            {
                vertex = hit.point;
                
                if(hit.collider.CompareTag("Player"))
                {
                    playerFound = true;
                    playerInView = true;
                    playerTime = Time.timeSinceLevelLoad;
                    //Debug.DrawRay(origin, (GetVector(currentFOVAngle) ) * hit.distance, Color.red);
                }
                else
                {
                    //Debug.DrawRay(origin, (GetVector(currentFOVAngle) ) * hit.distance, Color.yellow);
                }
            }
            vertices[vertexIndex] = vertex;
            if (i > 0)
            {
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }
            vertexIndex++;
            currentFOVAngle -= _degreesToNextRaycast;
        }
        if (!playerFound)
        {
            playerInView = false;
        }
        playerFound = false;

        return true;
    }

    public void AimFOV(Vector3 direction)
    {   //Set to middle of range
        initialAngle = GetAngle(direction) - (fov / 2f);
    }
    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public Vector3 GetVector(float angle)
    {
        float angleInRadians = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleInRadians), playerDirection.normalized.y, Mathf.Sin(angleInRadians));
    }
    public float GetAngle(Vector3 direction)
    {
        direction = direction.normalized;
        float degrees = -Mathf.Atan2(-direction.x, -direction.z) * Mathf.Rad2Deg;
        if (degrees < 0)
        {
            degrees += 360;
        }
        return degrees;
    }
    public void SetLookHeight(Vector3 position, bool seePlayer)
    {
        if (seePlayer)
        {
            playerPosition = position;
            playerDirection = (position + _playerHeightOffset) - origin ;
        }
/*        if (seePlayer)
        {
            //lookHeight = height;

        }
       else if ((playerTime != 0f && Time.timeSinceLevelLoad > playerTime + 5f )|| height < -3f)
        {
            lookHeight = 0.1f;
            //playerTime = 0;
        }*/
    }

    public Vector3[] GetRange()
    {

        return vertices;
    }
}