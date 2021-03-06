﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


// -----------------------------------------------------------------
// Class	: AIZombieState_Pursuit1
// Desc		: A Zombie state used for pursuing a target
// -----------------------------------------------------------------

public class AIZombieState_Pursuit1 : AIZombieState
{
    // Inspector assigned
    [SerializeField] [Range(0, 10)] float           _speed                          = 3.0f;
    [SerializeField] float                          _slerpSpeed                     = 5.0f;
    [SerializeField] float                          _repathDistanceMultiplier       = 0.035f;
    [SerializeField] float                          _repathVisualMinDuration        = 0.05f;
    [SerializeField] float                          _repathVisualMaxDuration        = 5.0f;
    [SerializeField] float                          _repathAudioMinDuration         = 0.25f;
    [SerializeField] float                          _repathAudioMaxDuration         = 5.0f;
    [SerializeField] float                          _maxDuration                    = 40.0f;
    [SerializeField] [Range(0.0f, 1.0f)]    float   _lookAtWeight                   = 0.7f;
    [SerializeField] [Range(0.0f, 90.0f)]   float   _lookAtAngleThreshold           = 15.0f;



    // Private Variables
    private float           _timer                  = 0.0f;
    private float           _repathTimer            = 0.0f;
    private float           _currentLookAtWeight    = 0.0f;




    // ------------------------------------------------------------------
    // Name	:	GetStateType
    // Desc	:	Returns the type of the state
    // ------------------------------------------------------------------
    public override AIStateType GetStateType()
    {
        return AIStateType.Pursuit;
    }


    // ------------------------------------------------------------------
    // Name	:	OnEnterState
    // Desc	:	Called by the State Machine when first transitioned into
    //			this state. It initializes a timer and configures the
    //			the state machine
    // ------------------------------------------------------------------
    public override void OnEnterState()
    {
        Debug.Log("Entering Pursuit State");

        base.OnEnterState();

        if (_zombieStateMachine == null) return;


        // Configure State Machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;
        _currentLookAtWeight = 0.0f;

        // Zombies will only purse for so long before breaking off
        _timer          = 0.0f;
        _repathTimer    = 0.0f;
       


        // Set path
        _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.targetPosition);
        _zombieStateMachine.navAgent.isStopped = false;
    }

    // ---------------------------------------------------------------------
    // Name	:	OnUpdateAI
    // Desc	:	The engine of this state
    // ---------------------------------------------------------------------
    public override AIStateType OnUpdate()
    {
        _timer          += Time.deltaTime;
        _repathTimer    += Time.deltaTime;


        if (_timer > _maxDuration)
            return AIStateType.Patrol;


        // If we are chasing the player and have entered the melee trigger then attack
        if (_zombieStateMachine.targetType == AITargetType.Visual_Player && _zombieStateMachine.inMeleeRange)
        {
            return AIStateType.Attack;
        }


        // Otherwise this is navigation to areas of interest so use the standard target threshold
        if (_zombieStateMachine.isTargetReached)
        {
            switch(_zombieStateMachine.targetType)
            {
                // If we have reached the source
                case AITargetType.Audio:
                case AITargetType.Visual_Light:
                    _zombieStateMachine.ClearTarget();      // Clear the threat
                    return AIStateType.Alerted;             // Become alert and scan for targets

                case AITargetType.Visual_Food:
                    return AIStateType.Feeding;
            }
        }



        // If for any reason the nav agent has lost its path then call then drop into alerted state
        // so it will try to re-aquire the target or eventually giveup and resume patrolling
        if ( _zombieStateMachine.navAgent.isPathStale ||
            (!_zombieStateMachine.navAgent.hasPath && _zombieStateMachine.navAgent.pathPending)||
            _zombieStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            return AIStateType.Alerted;
        }



        // If we don't have a path, then we will set our speed to zero to keep our zombie from running randomly
        if (_zombieStateMachine.navAgent.pathPending)
            _zombieStateMachine.speed = 0;
        else
        {
            _zombieStateMachine.speed = _speed;



            // If we are close to the target that was a player and we still have the player in our vision then keep facing right at the player
            if (!_zombieStateMachine.useRootPosition &&
                _zombieStateMachine.targetType == AITargetType.Visual_Player &&
                _zombieStateMachine.visualThreat.targetType == AITargetType.Visual_Player &&
                _zombieStateMachine.isTargetReached)
            {
                Vector3 targetPos = _zombieStateMachine.targetPosition;
                targetPos.y = _zombieStateMachine.transform.position.y;
                Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);

                _zombieStateMachine.transform.rotation = newRot;
            }
            else
            // SLowly update our rotation to match the nav agents desired rotation BUT only if we are not persuing the player and are really close to him
            if (!_zombieStateMachine.useRootRotation && !_zombieStateMachine.isTargetReached)
            {
                // Generate a new Quaternion representing the rotation we should have
                Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navAgent.desiredVelocity);

                // Smoothly rotate to that new rotation over time
                _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);

            }
            else
            if (_zombieStateMachine.isTargetReached)
            {
                return AIStateType.Alerted;
            }

        }




        // Is the player visible
        if (_zombieStateMachine.visualThreat.targetType == AITargetType.Visual_Player)
        {
            // The position is different - maybe same threat but it has moved so repath periodically
            if (_zombieStateMachine.targetPosition != _zombieStateMachine.visualThreat.position)
            {
                // Repath more frequently as we get closer to the target (try and save some CPU cycles)
                if (Mathf.Clamp(_zombieStateMachine.visualThreat.distance * _repathDistanceMultiplier, _repathAudioMinDuration, _repathAudioMaxDuration) < _repathTimer)
                {
                    // Repath the agent
                    _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.visualThreat.position);
                    _repathTimer = 0.0f;
                }
            }

            // Make sure this is the current target
            _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);


            // Remain in pursuit state
            return AIStateType.Pursuit;
        }


        // If our target is the last sighting of a player then remain
        // in pursuit as nothing else can override
        if (_zombieStateMachine.targetType == AITargetType.Visual_Player)
            return AIStateType.Pursuit;


        // If we have a visual threat that is the player's light
        if (_zombieStateMachine.visualThreat.targetType == AITargetType.Visual_Light)
        {
            // and we currently have a lower priority target then drop into alerted
            // mode and try to find source of light
            if (_zombieStateMachine.targetType == AITargetType.Audio || _zombieStateMachine.targetType == AITargetType.Visual_Food)
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
                return AIStateType.Alerted;
            }
            else
            if(_zombieStateMachine.targetType == AITargetType.Visual_Light)
            {
                // Get unique ID of the collider of our target
                int currentID = _zombieStateMachine.targetColliderID;

                // If this is the same light
                if (currentID == _zombieStateMachine.visualThreat.collider.GetInstanceID())
                {
                    // The position is different - maybe same threat but it has moved so repath periodically
                    if (_zombieStateMachine.targetPosition != _zombieStateMachine.visualThreat.position)
                    {
                        // Repath more frequently as we get closer to the target (try and save some CPU cycles)
                        if (Mathf.Clamp(_zombieStateMachine.visualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration , _repathVisualMaxDuration) < _repathTimer)
                        {
                            // Repath the agent
                            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.visualThreat.position);
                            _repathTimer = 0.0f;
                        }
                    }

                    _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
                    return AIStateType.Pursuit;
                }
                else
                {
                    _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
                    return AIStateType.Alerted;
                }
            }
        }
        else
        if(_zombieStateMachine.audioThreat.targetType == AITargetType.Audio)
        {
            if(_zombieStateMachine.targetType == AITargetType.Visual_Food)
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.audioThreat);
                return AIStateType.Alerted;
            }
            else
            if(_zombieStateMachine.targetType == AITargetType.Audio)
            {
                // Get unique ID of the collider of our target
                int currentID = _zombieStateMachine.targetColliderID;

                if(currentID == _zombieStateMachine.audioThreat.collider.GetInstanceID())
                {
                    // The position is different - maybe same threat but it has moved so repath periodically
                    if (_zombieStateMachine.targetPosition != _zombieStateMachine.audioThreat.position)
                    {
                        // Repath more frequently as we get closer to the target (try and save some CPU cycles)
                        if (Mathf.Clamp(_zombieStateMachine.audioThreat.distance * _repathDistanceMultiplier, _repathAudioMinDuration, _repathAudioMaxDuration) < _repathTimer)
                        {
                            // Repath the agent
                            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.audioThreat.position);
                            _repathTimer = 0.0f;
                        }
                    }

                    _zombieStateMachine.SetTarget(_zombieStateMachine.audioThreat);
                    return AIStateType.Pursuit;
                }
                else
                {
                    _zombieStateMachine.SetTarget(_zombieStateMachine.audioThreat);
                    return AIStateType.Alerted;
                }
            }

        }



        // Default
        return AIStateType.Pursuit;
    }


    public override void OnAnimatorIkUpdated()
    {
        if (_zombieStateMachine == null)
            return;

        if (Vector3.Angle(_zombieStateMachine.transform.forward, _zombieStateMachine.targetPosition - _zombieStateMachine.transform.position) < _lookAtAngleThreshold)
        {
            _zombieStateMachine.aninimator.SetLookAtPosition(_zombieStateMachine.targetPosition + Vector3.up);
            _currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, _lookAtWeight, Time.deltaTime);
            _zombieStateMachine.aninimator.SetLookAtWeight(_currentLookAtWeight);
        }
        else
        {
            _currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, 0.0f, Time.deltaTime);
            _zombieStateMachine.aninimator.SetLookAtWeight(_currentLookAtWeight);
        }
    }

}
