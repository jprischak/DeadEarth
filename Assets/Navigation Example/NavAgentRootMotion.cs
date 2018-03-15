using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentRootMotion : MonoBehaviour
{

    /**
    *  VARIABLES
    * */
    // Public
    public AIWaypointNetwork    waypointNetwork         = null;
    public AnimationCurve       jumpCurve               = new AnimationCurve();
    public NavMeshPathStatus    pathStatus              = NavMeshPathStatus.PathInvalid;
    public int                  waypointIndex           = 0;
    public float                modelSpeed              = 0;
    public bool                 hasPath                 = false;
    public bool                 pathPending             = false;
    public bool                 mixedMode               = true;
    


    // Private
    private NavMeshAgent        _navAgent               = null;
    private Animator            _animator               = null;
    private float               smoothAngle             = 0.0f;   






    /**
    *  CLASS FUNCTIONS
    * */
    private void Start()
    {
        // Get our component
        _navAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();



        // Set the nav agent to not update the rotation
        _navAgent.updateRotation = false;


        // Make sure that we have a network set
        if (waypointNetwork == null)
            return;


        SetNextDestination(false);

    }


    private void Update()
    {

        // Update the inspector to show us what is going on with our agent
        pathStatus      = _navAgent.pathStatus;
        hasPath         = _navAgent.hasPath;
        pathPending     = _navAgent.pathPending;



        // This will convert our desired velocity to our local space
        Vector3 localDesiredVelocity = transform.InverseTransformVector(_navAgent.desiredVelocity);

        // This will take the local velocity and convert it to rad and then turn it to degrees
        float angle = Mathf.Atan2(localDesiredVelocity.x, localDesiredVelocity.z) * Mathf.Rad2Deg;

        // This is used to smooth the angle as it moves, will limit to less than 80 degrees a frame
        smoothAngle = Mathf.MoveTowardsAngle(smoothAngle, angle, 80.0f * Time.deltaTime);


        modelSpeed = localDesiredVelocity.z;


        // Send values to the animator
        _animator.SetFloat("Angle", smoothAngle);
        _animator.SetFloat("Speed", modelSpeed, 0.1f, Time.deltaTime);


        // This will manually move our models transform
        if(_navAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon)
        {
            if(!mixedMode || 
                ( mixedMode && Mathf.Abs(angle) < 80.0f && _animator.GetCurrentAnimatorStateInfo(0).IsName("Base.Locomotion") ) )
            {
                Quaternion lookRotation = Quaternion.LookRotation(_navAgent.desiredVelocity, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5.0f * Time.deltaTime);
            }
            
        }
        


        // We are going to run a coruotine if we are in a off mesh link
        //if (_navAgent.isOnOffMeshLink)
        //{
        //    StartCoroutine(Jump(1.0f));
        //    return;
        //}



        // If we have made it to our waypoint or have an invalid path move to the next waypoint
        if ((_navAgent.remainingDistance <= _navAgent.stoppingDistance && !_navAgent.pathPending) || pathStatus == NavMeshPathStatus.PathInvalid)
            SetNextDestination(true);


        // If something has got in the way of our path then build a new path
        if (_navAgent.isPathStale)
            SetNextDestination(false);
    }


    // This is called right before the lateupdate function
    private void OnAnimatorMove()
    {
        if(mixedMode && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Base.Locomotion") )
            transform.rotation = _animator.rootRotation;


        _navAgent.velocity = _animator.deltaPosition / Time.deltaTime;
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
