using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// ----------------------------------------------------------------------
// Class	:	AIState
// Desc		:	The base class of all AI States used by our AI System.
// ----------------------------------------------------------------------
public abstract class AIState : MonoBehaviour {

    /**
    *  VARIABLES
    * */
    // Protected
    protected AIStateMachine stateMachine;




    /*
     *  DEFUALT HANDLERS
     * */
    public virtual void OnEnterState() {}
    public virtual void OnExitState() {}
    public virtual void OnAnimatorUpdated() {}
    public virtual void OnAnimatorIkUpdated() {}
    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other) {}
    public virtual void OnDestinationReached(bool isReached) {}



    /**
    *  FUNCTIONS
    * */
    // Abstract Functions
    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();


    // Public Method
    // Called by the parent state machine to assign its reference
    public void SetStateMachine(AIStateMachine machine)
    {
        stateMachine = machine;
    }


    





}
