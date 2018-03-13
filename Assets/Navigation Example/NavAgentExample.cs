using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



[RequireComponent (typeof(NavMeshAgent))]
public class NavAgentExample : MonoBehaviour
{

    /**
    *  VARIABLES
    * */
    public AIWaypointNetwork waypointNetwork = null;
    public int waypointIndex = 0;
    public NavMeshPathStatus pathStatus = NavMeshPathStatus.PathInvalid;
    public bool hasPath = false;
    public bool pathPending = false;


    private NavMeshAgent _navAgent = null;





    /**
    *  CLASS FUNCTIONS
    * */
    private void Start()
    {
        // Get our component
        _navAgent = GetComponent<NavMeshAgent>();


        // Make sure that we have a network set
        if (waypointNetwork == null)
            return;


        SetNextDestination(false);

    }


    private void Update()
    {
        // Update the inspector to show us what is going on with our agent
        pathStatus = _navAgent.pathStatus;
        hasPath = _navAgent.hasPath;
        pathPending = _navAgent.pathPending;



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
        if(nextWaypointTransform != null)
        {
            waypointIndex = nextWaypoint;
            _navAgent.destination = nextWaypointTransform.position;
            return;
        }


        // Increment our waypoint index
        waypointIndex++;
    }
}
