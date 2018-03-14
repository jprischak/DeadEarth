using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum DoorState { Open, Animating, Closed };


public class SlidingDoorDemo : MonoBehaviour
{

    /**
    *  VARIABLES
    * */
    // Public
    public float            slidingdistance     = 4.0f;
    public float            duration            = 1.0f;                         // Don't set to zero
    public AnimationCurve   jumpCurve           = new AnimationCurve();



    // Private
    private Vector3         closedPosition      = Vector3.zero;
    private Vector3         openPosition        = Vector3.zero;
    private DoorState       doorState           = DoorState.Closed;






    /**
    *  CLASS FUNCTIONS
    * */
    private void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + (slidingdistance * -transform.right);
    }


    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Space) && doorState != DoorState.Animating)
            StartCoroutine( AnimateDoor( doorState == DoorState.Open ? DoorState.Closed : DoorState.Open ) );
    }








    /**
    *  FUNCTIONS
    * */

    IEnumerator AnimateDoor(DoorState state)
    {
        // Set our state to animating to prevent us trying to animate again
        doorState = DoorState.Animating;


        // Variables
        float       time        = 0.0f;
        Vector3     startPos    = (state == DoorState.Open ? closedPosition : openPosition);
        Vector3     endPos      = (state == DoorState.Open ? openPosition : closedPosition);


        while(time <= duration)
        {
            float t = time / duration;

            transform.position = Vector3.Lerp(startPos, endPos, jumpCurve.Evaluate(t) );
            time += Time.deltaTime;
            yield return null;
        }


        // Just make sure that we end at the proper location
        transform.position = endPos;

        // Set the state of our door
        doorState = state;
    }
}
