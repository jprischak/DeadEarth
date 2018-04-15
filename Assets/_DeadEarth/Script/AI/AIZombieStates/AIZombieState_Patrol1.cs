using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


// ----------------------------------------------------------------
// CLASS	:	AIZombieState_Patrol1
// DESC		:	Generic Patrolling Behaviour for a Zombie
// ----------------------------------------------------------------
public class AIZombieState_Patrol1 : AIZombieState
{
    // Inspector Assigned
    [SerializeField] [Range(0.0f, 3.0f)]float       _speed                  = 2.0f;
    [SerializeField] float                          _turnOnSpotThreashold   = 90.0f;
    [SerializeField] float                          _slerpSpeed             = 5.0f;


    // ------------------------------------------------------------
    // Name	:	GetStateType
    // Desc	:	Called by parent State Machine to get this state's
    //			type.
    // ------------------------------------------------------------
    public override AIStateType GetStateType()
    {
        return AIStateType.Patrol;
    }


    // ------------------------------------------------------------
    // Name	:	OnUpdate
    // Desc	:	Called by the state machine each frame to give this
    //			state a time-slice to update itself. It processes 
    //			threats and handles transitions as well as keeping
    //			the zombie aligned with its proper direction in the
    //			case where root rotation isn't being used.
    // ------------------------------------------------------------
    public override AIStateType OnUpdate()
    {
        // No state machine then bail
        if (_zombieStateMachine == null) return AIStateType.Idle;

        // Is the player visible
        if (_zombieStateMachine.visualThreat.targetType == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
            return AIStateType.Pursuit;
        }

        // Is the threat a flashlight
        if (_zombieStateMachine.visualThreat.targetType == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
            return AIStateType.Alerted;
        }

        // Is the threat an audio emitter
        if (_zombieStateMachine.audioThreat.targetType == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.audioThreat);
            return AIStateType.Alerted;
        }

        // We have seen a dead body so lets pursue that if we are hungry enough
        if (_zombieStateMachine.visualThreat.targetType == AITargetType.Visual_Food)
        {
            // If the distance to hunger ratio means we are hungry enough to stray off the path that far
            if ((1.0f - _zombieStateMachine.satisfaction) > (_zombieStateMachine.visualThreat.distance / _zombieStateMachine.sensorRadius))
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
                return AIStateType.Pursuit;
            }
           
        }



        // Keep us from moving if we don't have a path
        if (_zombieStateMachine.navAgent.pathPending)
        {
            _zombieStateMachine.speed = 0;
            return AIStateType.Patrol;
        }
        else
            _zombieStateMachine.speed = _speed;




        // Calculate angle we need to turn through to be facing our target
        float angle = Vector3.Angle(_zombieStateMachine.transform.forward, (_zombieStateMachine.navAgent.steeringTarget - _zombieStateMachine.transform.position));

        // If its too big then drop out of Patrol and into Altered
        if (angle > _turnOnSpotThreashold)
            return AIStateType.Alerted;


        // If root rotation is not being used then we are responsible for keeping zombie rotated
        // and facing in the right direction. 
        if (!_zombieStateMachine.useRootRotation)
        {
            // Generate a new Quaternion representing the rotation we should have
            Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navAgent.desiredVelocity);

            // Smoothly rotate to that new rotation over time
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }


        // If for any reason the nav agent has lost its path then call the NextWaypoint function
        // so a new waypoint is selected and a new path assigned to the nav agent.
        if ( _zombieStateMachine.navAgent.isPathStale || 
            !_zombieStateMachine.navAgent.hasPath || 
            _zombieStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            _zombieStateMachine.GetWaypointPosition(true);
        }


        // Stay in Patrol State
        return AIStateType.Patrol;
    }


    // ------------------------------------------------------------------
    // Name	:	OnEnterState
    // Desc	:	Called by the State Machine when first transitioned into
    //			this state. 
    // ------------------------------------------------------------------
    public override void OnEnterState()
    {
        Debug.Log("Entering Patrol State");
        base.OnEnterState();

        if (_zombieStateMachine == null) return;


        // Configure State Machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;


        // Set Destination
        _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));
        

        // Make sure NavAgent is switched on
        _zombieStateMachine.navAgent.isStopped = false;

    }


    // ----------------------------------------------------------------------
    // Name	:	OnDestinationReached
    // Desc	:	Called by the parent StateMachine when the zombie has reached
    //			its target (entered its target trigger
    // ----------------------------------------------------------------------
    public override void OnDestinationReached(bool isReached)
    {
        // Only interesting in processing arrivals not departures
        if (_zombieStateMachine == null || !isReached)
            return;

        // Select the next waypoint in the waypoint network
        if (_zombieStateMachine.targetType == AITargetType.Waypoint)
            _zombieStateMachine.GetWaypointPosition(true);
    }


    // -----------------------------------------------------------------------
    // Name	:	OnAnimatorIKUpdated
    // Desc	:	Override IK Goals
    // -----------------------------------------------------------------------
    public override void OnAnimatorIkUpdated()
    {
        
    }


   

}
