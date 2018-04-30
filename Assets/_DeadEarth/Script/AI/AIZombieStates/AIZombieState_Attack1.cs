using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// -----------------------------------------------------------------
// Class	: AIZombieState_Attack1
// Desc		: A Zombie state used for attacking a target
// -----------------------------------------------------------------
public class AIZombieState_Attack1 : AIZombieState
{

    // Inspector Assigned
    [SerializeField] [Range(0, 10)]         float       _speed                  = 0.0f;
    [SerializeField]                        float       _stoppingDistance       = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)]    float       _lookAtWeight           = 0.7f;
    [SerializeField] [Range(0.0f, 90.0f)]   float       _lookAtAngleThreshold   = 15.0f;
    [SerializeField]                        float       _slerpSpeed             = 5.0f;


    // Private variables
    private float _currentLookAtWeight = 0.0f;




    // Mandatory Overrides
    // ------------------------------------------------------------------
    // Name	:	GetStateType
    // Desc	:	Returns the type of the state
    // ------------------------------------------------------------------
    public override AIStateType GetStateType()
    {
        return AIStateType.Attack;
    }

    // ------------------------------------------------------------------
    // Name	:	OnEnterState
    // Desc	:	Called by the State Machine when first transitioned into
    //			this state. It configures the
    //			the state machine
    // ------------------------------------------------------------------
    public override void OnEnterState()
    {
        base.OnEnterState();


        Debug.Log("Entering Attack State");


        if (_zombieStateMachine == null)
            return;

        // Configure the State Machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.seeking         = 0;
        _zombieStateMachine.feeding         = false;
        _zombieStateMachine.attackType      = Random.Range(0, 100);
        _zombieStateMachine.speed           = _speed;
        _currentLookAtWeight                = 0.0f;
    }

    // ------------------------------------------------------------------
    // Name	:	OnExitState
    // Desc	:	Called by the State Machine when transitioned out of
    //			this state. 
    // ------------------------------------------------------------------
    public override void OnExitState()
    {
        _zombieStateMachine.attackType = 0;
    }

    // ---------------------------------------------------------------------
    // Name	:	OnUpdateAI
    // Desc	:	The engine of this state
    // ---------------------------------------------------------------------
    public override AIStateType OnUpdate()
    {

        Vector3 targetPos;
        Quaternion newRot;


        if (Vector3.Distance(_zombieStateMachine.transform.position, _zombieStateMachine.targetPosition) < _stoppingDistance)
            _zombieStateMachine.speed = 0;
        else
            _zombieStateMachine.speed = _speed;


        // Do we have a visual threat that is the player
        if(_zombieStateMachine.visualThreat.targetType == AITargetType.Visual_Player)
        {
            // Set new target
            _zombieStateMachine.SetTarget(_stateMachine.visualThreat);

            // If we are not in melee range any more than go back to pursuit mode
            if (!_zombieStateMachine.inMeleeRange)
                return AIStateType.Pursuit;


            if(!_zombieStateMachine.useRootRotation)
            {
                // Keep the zombie facing the player at all times
                targetPos       = _zombieStateMachine.targetPosition;
                targetPos.y     = _zombieStateMachine.transform.position.y;
                newRot          = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
                _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
            }


            // Create a new random attack
            _zombieStateMachine.attackType = Random.Range(0, 100);

            return AIStateType.Attack;
        }


        // Player has stepped outside our FOV or hidden, so face in his/her direction and then drop back to alerted mode to 
        // give AI a chance to re-aquire target
        if(!_zombieStateMachine.useRootRotation)
        {
            targetPos       = _zombieStateMachine.targetPosition;
            targetPos.y     = _zombieStateMachine.transform.position.y;
            newRot          = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
            _zombieStateMachine.transform.rotation = newRot;
        }




        // Stay in Alerted State
        return AIStateType.Alerted;
    }

    // ---------------------------------------------------------------------
    // Name	:	OnUpdateAI
    // Desc	:	The engine of this state
    // ---------------------------------------------------------------------
    public override void OnAnimatorIkUpdated()
    {
        if (_zombieStateMachine == null)
            return;

        if(Vector3.Angle(_zombieStateMachine.transform.forward, _zombieStateMachine.targetPosition - _zombieStateMachine.transform.position) < _lookAtAngleThreshold)
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


