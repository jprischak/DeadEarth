using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Feeding1 : AIZombieState
{

    // Inspector Assigned variables
    [SerializeField] Transform                          _bloodParticlesMount        = null;
    [SerializeField] float                              _slerpSpeed                 = 5.0f;
    [SerializeField] [Range(0.01f, 1.0f)]   float       _bloodParticlesBurstTime    = 0.1f;
    [SerializeField] [Range(1, 100)]        int         _bloodParticlsBurstAmount   = 10;

    


    // Private variables
    private int     _eatingStateHash        = Animator.StringToHash("FeedingState");
    private int     _eatingLayerIndex       = -1;
    private float   _timer                  = 0.0f;




    // ------------------------------------------------------------------
    // Name	:	GetStateType
    // Desc	:	Returns the type of the state
    // ------------------------------------------------------------------
    public override AIStateType GetStateType()
    {
        return AIStateType.Feeding;
    }



    // ------------------------------------------------------------------
    // Name	:	OnEnterState
    // Desc	:	Called by the State Machine when first transitioned into
    //			this state. It configures the
    //			the state machine
    // ------------------------------------------------------------------
    public override void OnEnterState()
    {
        Debug.Log("Entering Feeding State");

        base.OnEnterState();

        if (_zombieStateMachine == null) return;


        // Get Layer index
        if (_eatingLayerIndex == -1)
            _eatingLayerIndex = _zombieStateMachine.aninimator.GetLayerIndex("Cinematic");


        // Reset our timer
        _timer = 0.0f;


        // Configure State Machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0.0f;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = true;
        _zombieStateMachine.attackType = 0;

    }



    // ------------------------------------------------------------------
    // Name	:	OnExitState
    // Desc	:	Called by the State Machine when transitioned out of
    //			this state. 
    // ------------------------------------------------------------------
    public override void OnExitState()
    {
        if(_zombieStateMachine != null)
        {
            _zombieStateMachine.feeding = false;
        }
    }




    // ---------------------------------------------------------------------
    // Name	:	OnUpdateAI
    // Desc	:	The engine of this state
    // ---------------------------------------------------------------------
    public override AIStateType OnUpdate()
    {
        // Update our timer
        _timer += Time.deltaTime;



        if(_zombieStateMachine.satisfaction > 0.9f)
        {
            _zombieStateMachine.GetWaypointPosition(false);
            return AIStateType.Alerted;
        }


        // If Visual Threat then drop into alert mode
        if (_zombieStateMachine.visualThreat.targetType != AITargetType.None && _zombieStateMachine.visualThreat.targetType != AITargetType.Visual_Food)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.visualThreat);
            return AIStateType.Alerted;
        }



        // If Audio Threat then drop into alert mode
        if (_zombieStateMachine.audioThreat.targetType == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.audioThreat);
            return AIStateType.Alerted;
        }



        // Is the feeding animation playing now
        if (_zombieStateMachine.aninimator.GetCurrentAnimatorStateInfo(_eatingLayerIndex).shortNameHash == _eatingStateHash)
        {
            _zombieStateMachine.satisfaction = Mathf.Min(_zombieStateMachine.satisfaction + ( (Time.deltaTime * _zombieStateMachine.replenishRate) / 100.0f ), 1.0f);

            // Display the blood particles as long as we have valid componets
            if(GameSceneManager.instance && GameSceneManager.instance.bloodParticles && _bloodParticlesMount)
            {
                if(_timer > _bloodParticlesBurstTime)
                {
                   
                    ParticleSystem system = GameSceneManager.instance.bloodParticles;

                    // Set the position of our particle system
                    system.transform.position = _bloodParticlesMount.transform.position;
                    system.transform.rotation = _bloodParticlesMount.transform.rotation;
                    system.simulationSpace = ParticleSystemSimulationSpace.World;

                    // Set the particle system to begin emitting and reset our timer
                    system.Emit(_bloodParticlsBurstAmount);
                    _timer = 0.0f;
                }               
            }
        }


        if(!_zombieStateMachine.useRootRotation)
        {
            // Keep the zombie facing the player at all times
            Vector3 targetPos = _zombieStateMachine.targetPosition;
            targetPos.y = _zombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }


        // Stay in Feeding state
        return AIStateType.Feeding;
    }
}
