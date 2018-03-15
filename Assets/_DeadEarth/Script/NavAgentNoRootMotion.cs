using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentNoRootMotion : MonoBehaviour
{

    /**
    *  VARIABLES
    * */
    // Public
    public AIWaypointNetwork    waypointNetwork         = null;
    public int                  waypointIndex           = 0;
    public NavMeshPathStatus    pathStatus              = NavMeshPathStatus.PathInvalid;
    public bool                 hasPath                 = false;
    public bool                 pathPending             = false;
    public AnimationCurve       jumpCurve               = new AnimationCurve();


    // Private
    private NavMeshAgent        _navAgent               = null;
    private Animator            _animator               = null;
    private float               origninalMaxSpeed       = 0.0f;  





    /**
    *  CLASS FUNCTIONS
    * */
    private void Start()
    {
        // Get our component
        _navAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();


        if (_navAgent)
            origninalMaxSpeed = _navAgent.speed;


        // Make sure that we have a network set
        if (waypointNetwork == null)
            return;


        SetNextDestination(false);

    }


    private void Update()
    {
        int turnOnSpot = 0;


        // Update the inspector to show us what is going on with our agent
        pathStatus      = _navAgent.pathStatus;
        hasPath         = _navAgent.hasPath;
        pathPending     = _navAgent.pathPending;



        // This is used to figure out the desirred velocity from the nav agent
        Vector3     cross       = Vector3.Cross(transform.forward, _navAgent.desiredVelocity.normalized);
        float       horizontal  = (cross.y < 0) ? -cross.magnitude : cross.magnitude;
        horizontal = Mathf.Clamp(horizontal * 2.32f, -2.32f, 2.32f);



        // We will handle turning if we have slowed down or our direction is greater than 10 degrees of our desired direction
        if(_navAgent.desiredVelocity.magnitude < 1.0f && Vector3.Angle(transform.forward, _navAgent.desiredVelocity) > 10.0f)
        {
            _navAgent.speed = 0.1f;
            turnOnSpot = (int)Mathf.Sign(horizontal);
        }
        else
        {
            _navAgent.speed = origninalMaxSpeed;
            turnOnSpot = 0;
        }




        // Send the variables to the the animator
        _animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
        _animator.SetFloat("Vertical", _navAgent.desiredVelocity.magnitude, 0.1f, Time.deltaTime);
        _animator.SetInteger("TurnOnSpot", turnOnSpot);




        // We are going to run a coruotine if we are in a off mesh link
        //if (_navAgent.isOnOffMeshLink)
        //{
        //    StartCoroutine(Jump(1.0f));
        //    return;
        //}



        // If we have made it to our waypoint or have an invalid path move to the next waypoint
        if ((!_navAgent.hasPath && !_navAgent.pathPending) || pathStatus == NavMeshPathStatus.PathInvalid)
            SetNextDestination(true);


        // If something has got in the way of our path then build a new path
        if (_navAgent.isPathStale)
            SetNextDestination(false);
    }








    /**
    *  FUNCTIONS
    * */
    private void SetNextDestination(bool increment)
    {
        // Make sure that our network has been set
        if (!waypointNetwork)
            return;


        int incStep = increment ? 1 : 0;
        Transform nextWaypointTransform = null;



        // This will find out if our next waypoint is out of range, if it is it resets to zero and sets the transfrom of
        // the next waypoint to our variable.
        int nextWaypoint = (waypointIndex + incStep >= waypointNetwork.waypoints.Count) ? 0 : (waypointIndex + incStep);
        nextWaypointTransform = waypointNetwork.waypoints[waypointIndex];


        // If we have a valid waypoint transform set it to our nav agent destination and increment our index
        if (nextWaypointTransform != null)
        {
            waypointIndex = nextWaypoint;
            _navAgent.destination = nextWaypointTransform.position;
            return;
        }


        // Increment our waypoint index
        waypointIndex++;
    }



    // We are going to handle the movement of our agent if we are in a off mesh link
    IEnumerator Jump(float duration)
    {
        OffMeshLinkData     data            = _navAgent.currentOffMeshLinkData;
        Vector3             startPos        = _navAgent.transform.position;
        Vector3             endPos          = data.endPos + (_navAgent.baseOffset * Vector3.up);
        float               time            = 0.0f;

        while (time <= duration)
        {
            float t = time / duration;

            _navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + jumpCurve.Evaluate(t) * Vector3.up;
           
            time += Time.deltaTime;

            yield return null;
        }


        // Let the agent know we are done handling the off mesh link behaviour
        _navAgent.CompleteOffMeshLink();

    }
}
