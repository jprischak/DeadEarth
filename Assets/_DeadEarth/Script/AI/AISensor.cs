using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// ----------------------------------------------------------------------
// Class	:	AISensor
// Desc		:	Notifies the parent AIStateMachine of any threats that
//				enter its trigger via the AIStateMachine's OnTriggerEvent
//				method.
// ----------------------------------------------------------------------
public class AISensor : MonoBehaviour {
    /**
    *  VARIABLES
    * */
    // Constant



    // Public



    // Serialized



    // Private
    private AIStateMachine _parentStateMachine = null;






    /**
    *  CLASS FUNCTIONS
    * */
    private void OnTriggerEnter(Collider other)
    {
        if (_parentStateMachine != null)
            _parentStateMachine.OnTriggerEvent(AITriggerEventType.Enter, other);
    }


    private void OnTriggerStay(Collider other)
    {
        if (_parentStateMachine != null)
            _parentStateMachine.OnTriggerEvent(AITriggerEventType.Stay, other);
    }


    private void OnTriggerExit(Collider other)
    {
        if (_parentStateMachine != null)
            _parentStateMachine.OnTriggerEvent(AITriggerEventType.Exit, other);
    }








    /**
    *  FUNCTIONS
    * */
    // SETTER FUNCTIONS
    public AIStateMachine parentStateMachine
    {
        set
        {
            _parentStateMachine = value;
        }
    }

}
