using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour {

    /**
    *  VARIABLES
    * */
    // Constant



    // Public



    // Serialized



    // Private
    protected AIStateMachine stateMachine;





    /**
    *  CLASS FUNCTIONS
    * */










    /**
    *  FUNCTIONS
    * */
    public abstract AIStateType GetStateType();

    public abstract AIStateType OnUpdate();


    public void SetStateMachine(AIStateMachine machine)
    {
        stateMachine = machine;
    }


    public virtual void OnEnterState()
    {

    }


    public virtual void OnExitState()
    {
        
    }


    public virtual void OnAnimatorUpdated()
    {

    }


    public virtual void OnAnimatorIkUpdated()
    {

    }


    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other)
    {

    }

    
    public virtual void OnDestinationReached(bool isReached)
    {

    }





}
