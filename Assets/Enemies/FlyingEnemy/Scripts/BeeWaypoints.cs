using UnityEngine;

public class BeeWaypoints : MonoBehaviour
{
    public GameObject[] waypoints;
    // Start is called before the first frame update
    void Start()
    {
        if(waypoints.Length <= 0)
        {
            Debug.Log("No waypoints found.");
        }
    }
}
