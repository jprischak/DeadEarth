﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// ----------------------------------------------------------------------
// CLASS : AIZombieState_Alerted1
// DESC	 : An AIState that implements a zombies Alerted Behaviour
// ----------------------------------------------------------------------
public class AIZombieState_Alerted1 : AIZombieState
{
    // Inspector Assigned
    [SerializeField] [Range(1, 60)] float   _maxDuration                = 10.0f;
    [SerializeField] float                  _waypointAngleThreshold     = 90.0f;
    [SerializeField] float                  _threatAngleThreshold       = 10.0f;
    [SerializeField] float                  _directionChangeTime        = 1.5f;


    // Private Variables
    private float _timer = 0.0f;
    private float _directionChangeTimer = 0.0f;


    // ------------------------------------------------------------------
    // Name	:	GetStateType
    // Desc	:	Returns the type of the state
    // ------------------------------------------------------------------
    public override AIStateType GetStateType()
    {
        return AIStateType.Alerted;
    }



    // ------------------------------------------------------------------
    // Name	:	OnEnterState
    // Desc	:	Called by the State Machine when first transitioned into
    //			this state. It initializes a timer and configures the
    //			the state machine
    // ------------------------------------------------------------------
    public override void OnEnterState()
    {
        Debug.Log("Entering Alerted State");
        base.OnEnterState();

        if (_zombieStateMachine == null) return;


        // Configure State Machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0.0f;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;


        _timer = _maxDuration;
        _directionChangeTimer = 0.0f;
    }



    // ---------------------------------------------------------------------
    // Name	:	OnUpdate
    // Desc	:	The engine of this state
    // ---------------------------------------------------------------------
    public override AIStateType OnUpdate()
    {
        // No state machine then bail
        if (_zombieStateMachine == null) return AIStateType.Idle;

        // Reduce Timer
        _timer -= Time.deltaTime;
        _directionChangeTimer += Time.deltaTime;


        // If we have been in the alerted state for too long, set our waypoint back as our target and look for it
        if (_timer <= 0.0f)
        {
            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));
            _zombieStateMachine.navAgent.isStopped = false;
            _timer = _maxDuration;
        }


        // Is the player visible
        if (_zombieStateMachine.visualThreat.targetType == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
            return AIStateType.Pursuit;
        }

        // Is the threat an audio emitter
        if (_zombieStateMachine.audioThreat.targetType == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.audioThreat);
            _timer = _maxDuration;
        }

        // Is the threat a flashlight
        if (_zombieStateMachine.visualThreat.targetType == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
            _timer = _maxDuration;
        }

        // Is the threat food
        if (_zombieStateMachine.audioThreat.targetType == AITargetType.None &&
            _zombieStateMachine.visualThreat.targetType == AITargetType.Visual_Food &&
            _zombieStateMachine.targetType == AITargetType.None)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
            return AIStateType.Pursuit;
        }




        float angle;

        if( ( _zombieStateMachine.targetType == AITargetType.Audio ||
            _zombieStateMachine.targetType == AITargetType.Visual_Light ) || _zombieStateMachine.isTargetReached)
        {
            angle = AIState.FindSignAngle(  _zombieStateMachine.transform.forward,
                                            _zombieStateMachine.targetPosition - _zombieStateMachine.transform.position);

            if(_zombieStateMachine.targetType == AITargetType.Audio && Mathf.Abs(angle) < _threatAngleThreshold)
            {
                return AIStateType.Pursuit;
            }


            // This keeps an uniteligent zombie from constantly going between left and right with out actually moving
            if(_directionChangeTimer > _directionChangeTime)
            {
                if (Random.value < _zombieStateMachine.intellignece)               
                    _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
                
                else
                   _zombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                

                _directionChangeTimer = 0.0f;
            }
            

        }
        else 
        if(_zombieStateMachine.targetType == AITargetType.Waypoint && !_zombieStateMachine.navAgent.pathPending)
        {
            angle = AIState.FindSignAngle(  _zombieStateMachine.transform.forward,
                                            _zombieStateMachine.navAgent.steeringTarget - _zombieStateMachine.transform.position);

            if (Mathf.Abs(angle) < _waypointAngleThreshold)
                return AIStateType.Patrol;

            _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
        }
        else
        {
            if(_directionChangeTimer > _directionChangeTime)
            {
                _zombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                _directionChangeTimer = 0.0f;
            }
        }



        return AIStateType.Alerted;
    }

}
