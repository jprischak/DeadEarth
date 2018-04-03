﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;




// -----------------------------------------------------------------------------------
//  CLASS   :   AIZombieState
//  DESC    :   The immediate base class of all zombie states. It provides the event
//              procossing and storage of the current threats.
// ------------------------------------------------------------------------------------
public abstract class AIZombieState : AIState
{

    // Private
    protected int _playerLayerMask  = -1;
    protected int _bodyPartLayer    = -1;



    // -------------------------------------------------------------------------------------
    // Name	:	Awake
    // Desc	:	Calculate the masks and layers used for raycasting and layer testing
    // 
    // -------------------------------------------------------------------------------------
    private void Awake()
    {
        // Get a mask for line of sight testing with the player. (+1) is a hack to include the
        // default layer in the current version of unity.
        _playerLayerMask = LayerMask.GetMask("Player", "AI Body Part") + 1;

        // Get the layer index of the AI Body Part layer
        _bodyPartLayer = LayerMask.NameToLayer("AI Body Part");
    }



    // -------------------------------------------------------------------------------------
    // Name	:	OnTriggerEvent
    // Desc	:	Called by the parent state machine when threats enter/stay/exit the zombie's
    //			sensor trigger, This will be any colliders assigned to the Visual or Audio
    //			Aggravator layers or the player.
    //			It examines the threat and stored it in the parent machine Visual or Audio
    //			threat members if found to be a higher priority threat.
    // --------------------------------------------------------------------------------------
    public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)
    {
        // If we don't have a parent state machine then bail
        if (_stateMachine == null)
            return;


        // We are not interested in exit events so only step in and process if its an
        // enter or stay.
        if (eventType != AITriggerEventType.Exit)
        {
            // What is the type of the current visual threat we have stored
            AITargetType curType = _stateMachine.visualThreat.targetType;


            // Is the collider that has entered our sensor a player
            if (other.CompareTag("Player"))
            {
                // Get distance from the sensor origin to the collider
                float distance = Vector3.Distance(_stateMachine.sensorPosition, other.transform.position);


                // If the currently stored threat is not a player or if this player is closer than a player
                // previously stored as the visual threat...this could be more important
                if (curType != AITargetType.Visual_Player ||
                    (curType == AITargetType.Visual_Player && distance < _stateMachine.visualThreat.distance))
                {
                    RaycastHit hitInfo;

                    // Is the collider within our view cone and do we have line or sight
                    if (ColliderIsVisible(other, out hitInfo, _playerLayerMask))
                    {
                        // Yep...it's close and in our FOV and we have line of sight so store as the current most dangerous threat
                        _stateMachine.visualThreat.Set(AITargetType.Visual_Player, other, other.transform.position, distance);
                    }
                }
            }

        }
    }



    // -------------------------------------------------------------------------------------
    // Name	:	ColliderIsVisible
    // Desc	:	Test the passed collider against the zombie's FOV and using the passed
    //			layer mask for line of sight testing.
    // -------------------------------------------------------------------------------------
    protected virtual bool ColliderIsVisible(Collider other, out RaycastHit hitInfo, int layerMask = -1)
    {
        // Let's make sure we have something to return
        hitInfo = new RaycastHit();


        // We need the state machine to be an AIZombieStateMachine
        if (_stateMachine == null || _stateMachine.GetType() != typeof(AIZombieStateMachine))   return false;
        AIZombieStateMachine zombieMachine = (AIZombieStateMachine)_stateMachine;

        // Calculate the angle between the sensor origin and the direction of the collider
        Vector3     head        = _stateMachine.sensorPosition;
        Vector3     direction   = other.transform.position - head;
        float       angle       = -Vector3.Angle(direction, transform.forward);


        // If the angle is greater than half our FOV then it is outside the view cone so
        // return false - no visibility
        if (angle > (zombieMachine.fov * 0.5f))
            return false;


        // Now we need to test line of sight. Perform a ray cast from our sensor origin in the direction of the collider for distance
        // of our sensor radius scaled by the zombie's sight ability. This will return ALL hits.
        RaycastHit[] hits = Physics.RaycastAll(head, direction.normalized, _stateMachine.sensorRadius * zombieMachine.sight, layerMask);


        // Find the closest collider that is NOT the AIs own body part. If its not the target then the target is obstructed
        float       closestColliderDistance     = float.MaxValue;
        Collider    closestCollider             = null;


        // Examine each hit
        for (int i=0; i<hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            // Is this hit closer than any we previously have found and stored
            if (hit.distance < closestColliderDistance)
            {
                // If the hit is on the body part layer
                if (hit.transform.gameObject.layer == _bodyPartLayer)
                {
                    // And assuming it is not our own body part
                    if (_stateMachine != GameSceneManager.instance.GetAIStateMachine(hit.rigidbody.GetInstanceID()))
                    {
                        // Store the collider, distance and hit info.
                        closestColliderDistance     = hit.distance;
                        closestCollider             = hit.collider;
                        hitInfo                     = hit;
                    }
                }
                else
                {
                    // Its not a body part so simply store this as the new closest hit we have found
                    closestColliderDistance         = hit.distance;
                    closestCollider                 = hit.collider;
                    hitInfo                         = hit;
                }
            }
        }


        // If the closest hit is the collider we are testing against, it means we have line-of-sight
        // so return true.
        if (closestCollider && closestCollider.gameObject == other.gameObject)
            return true;


        // otherwise, something else is closer to us than the collider so line-of-sight is blocked
        return false;
    }




}
