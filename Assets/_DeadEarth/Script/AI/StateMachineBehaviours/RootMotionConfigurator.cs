using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// ------------------------------------------------------------
// CLASS	:	RootMotionConfigurator
// DESC		:	A State Machine Behaviour that communicates
//				with an AIStateMachine derived class to
//				allow for enabling/disabling root motion on
//				a per animation state basis.
// ------------------------------------------------------------
public class RootMotionConfigurator : AIStateMachineLink
{
    /**
   *  VARIABLES
   * */

    // Public



    // Serialized
    [SerializeField] private int        _rootPosition       = 0;
    [SerializeField] private int        _rootRotation       = 0;





    /**
    *  CLASS FUNCTIONS
    * */
    // --------------------------------------------------------
    // Name	:	OnStateEnter
    // Desc	:	Called prior to the first frame the
    //			animation assigned to this state.
    // --------------------------------------------------------
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_stateMachine)
        {
            // Request the enabling/disabling of root motion for this animation state 
            _stateMachine.AddRootMotionRequest(_rootPosition, _rootRotation);
        }

    }

    // --------------------------------------------------------
    // Name	:	OnStateExit
    // Desc	:	Called on the last frame of the animation prior
    //			to leaving the state.
    // --------------------------------------------------------
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_stateMachine)
        {
            // Inform the AI State Machine that we wish to relinquish our root motion request.
            _stateMachine.AddRootMotionRequest(-_rootPosition, -_rootRotation);
        }

    }

}
