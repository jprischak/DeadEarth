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
    protected AIStateMachine _stateMachine;




    /*
     *  DEFUALT HANDLERS
     * */
    public virtual void OnEnterState() {}
    public virtual void OnExitState() {}
    public virtual void OnAnimatorUpdated()
    {
        if (_stateMachine.useRootPosition)
            _stateMachine.navAgent.velocity = _stateMachine.aninimator.deltaPosition / Time.deltaTime;

        if (_stateMachine.useRootRotation)
            _stateMachine.transform.rotation = _stateMachine.aninimator.rootRotation;
    }
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
    public virtual void SetStateMachine(AIStateMachine machine)
    {
        _stateMachine = machine;
    }


    // -----------------------------------------------------------------------------
    // Name	:	ConvertSphereColliderToWorldSpace
    // Desc	:	Converts the passed sphere collider's position and radius into
    //			world space taking into acount hierarchical scaling.
    // -----------------------------------------------------------------------------
    public static void ConvertSphereColliderToWorldSpace(SphereCollider col, out Vector3 pos, out float radius)
    {
        // Default Values
        pos = Vector3.zero;
        radius  = 0.0f;


        // If no valid sphere collider return
        if (col == null) return;


        // Calculate world space position of sphere center
        pos = col.transform.position;
        pos.x += col.center.x * col.transform.lossyScale.x;
        pos.y += col.center.y * col.transform.lossyScale.y;
        pos.z += col.center.z * col.transform.lossyScale.z;



        // Calculate world space radius of sphere
        radius = Mathf.Max( col.radius * col.transform.lossyScale.x,
                            col.radius * col.transform.lossyScale.y);
        radius = Mathf.Max( radius, 
                            col.radius * col.transform.lossyScale.z);




    }




    // -----------------------------------------------------------------------
    // Name	:	FindSignedAngle
    // Desc	:	Returns the signed angle between to vectors (in degrees)
    // -----------------------------------------------------------------------
    public static float FindSignAngle(Vector3 fromVector, Vector3 toVector)
    {
        if (fromVector == toVector)
            return 0.0f;


        float angle = Vector3.Angle(fromVector, toVector);
        Vector3 cross = Vector3.Cross(fromVector, toVector);

        angle *= Mathf.Sign(cross.y);

        return angle;
    }


}
