using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapsingPlatform : MonoBehaviour
{
    [SerializeField] private ParticleSystem droppingParticles;
    [SerializeField] private float baseEmissionRate = 0.1f;
    [SerializeField] private float fastEmissionRate = 10f;
    [SerializeField] private float collapseDelay = 2f;
    [SerializeField] private float timeToReset = 3f;

    bool _performingReset;

    bool _isCollapsed;
    float _timeCollapsed;

    bool _isCollapseScheduled;
    float _timeToCollapse;
    float _nextRandomSound;

    private GameObject _platform;

    private GameObject _collapsedParent;
    private List<Vector3> _collapsedInitPos;
    private List<Quaternion> _collapsedInitRot;


    // Start is called before the first frame update
    void Start()
    {
        _platform = transform.Find("Platform").gameObject;

        _collapsedParent = transform.Find("Collapsed").gameObject;
        _collapsedInitPos = new List<Vector3>(_collapsedParent.transform.childCount);
        _collapsedInitRot = new List<Quaternion>(_collapsedParent.transform.childCount);
        for (int childIdx = 0; childIdx < _collapsedParent.transform.childCount; ++childIdx)
        {
            Rigidbody child = _collapsedParent.transform.GetChild(childIdx).GetComponent<Rigidbody>();
            _collapsedInitPos.Add(child.position);
            _collapsedInitRot.Add(child.rotation);
        }

        _nextRandomSound = Time.timeSinceLevelLoad + Random.Range(2f, 7f);

        ResetCollapsed();
    }

    private void FixedUpdate()
    {
        // kludge to delay latching the game object isActive state
        // until one physics update frame after Reset is called to
        // ensure that child object updates still occur from a Reset
        if (_performingReset)
        {
            _collapsedParent.SetActive(false);
            _performingReset = false;
        }

        if (_isCollapseScheduled && Time.timeSinceLevelLoad >= _timeToCollapse)
        {
            Collapse();
        }

        if (_isCollapsed && Time.timeSinceLevelLoad > (_timeCollapsed + timeToReset))
        {
            ResetCollapsed();
        }

        // Randomly emit falling dirt sounds while not collapsed -- no easy way to hook when burst particle emissions
        // occur without making a dedicated MonoBehavior for the particle systems.
        if (!_isCollapsed && Time.timeSinceLevelLoad > _nextRandomSound)
        {
            _nextRandomSound = Time.timeSinceLevelLoad + Random.Range(2f, 15f);
            EventManager.TriggerEvent<DirtFallEvent, Vector3>(transform.position);
        }
    }

    void ResetCollapsed()
    {
        _performingReset = true; 

        // Only call this during FixedUpdate! (or Atart())
        for (int childIdx = 0; childIdx < _collapsedParent.transform.childCount; ++childIdx)
        {
            Rigidbody child = _collapsedParent.transform.GetChild(childIdx).GetComponent<Rigidbody>();
            child.isKinematic = true;
            child.position  = _collapsedInitPos[childIdx];
            child.rotation  = _collapsedInitRot[childIdx];
            child.velocity          = Vector3.zero;
            child.angularVelocity   = Vector3.zero;
        }
        droppingParticles.Play();
        var emission = droppingParticles.emission;
        emission.rateOverTime = baseEmissionRate;

        _platform.SetActive(true);
        _isCollapsed = false;
        _timeCollapsed = 0f;
        _isCollapseScheduled = false;
        _timeToCollapse = 0f;
    }

    void Collapse()
    {
        // Only call this during FixedUpdate!
        if (_isCollapsed)
        {
            return;
        }

        droppingParticles.Stop();
        _platform.SetActive(false);
        _collapsedParent.SetActive(true);

        for (int childIdx = 0; childIdx < _collapsedParent.transform.childCount; ++childIdx)
        {
            Rigidbody child = _collapsedParent.transform.GetChild(childIdx).GetComponent<Rigidbody>();
            child.isKinematic = false;
            child.AddRelativeForce(Random.insideUnitSphere.normalized,  ForceMode.Impulse);
            child.AddRelativeTorque(Random.insideUnitSphere.normalized, ForceMode.Impulse);
        }

        _timeCollapsed = Time.timeSinceLevelLoad;
        _isCollapsed = true;

        EventManager.TriggerEvent<PlatformCollapseEvent, Vector3>(transform.position);
    }

    public void ScheduleCollapse()
    {
        if (!_isCollapseScheduled)
        {
            var emission = droppingParticles.emission;
            emission.rateOverTime = fastEmissionRate;

            _isCollapseScheduled = true;
            _timeToCollapse = Time.timeSinceLevelLoad + collapseDelay;

            EventManager.TriggerEvent<DirtFallEvent, Vector3>(transform.position);
        }
    }
}
