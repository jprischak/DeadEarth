using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


// Public Enums of the AI System
public enum AIStateType         { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead }
public enum AITargetType        { None, Waypoint, Visual_Player, Visual_Light, Visual_Food, Audio}
public enum AITriggerEventType  { Enter, Stay, Exit}







// ----------------------------------------------------------------------
// Class	:	AITarget
// Desc		:	Describes a potential target to the AI System
// ----------------------------------------------------------------------
public struct AITarget
{
    // Variables
    private AITargetType    _targetType;             // The type of target
    private Collider        _collider;               // The collider
    private Vector3         _position;               // Current position in the world
    private float           _distance;               // Distance from player
    private float           _time;                   // Time the target was last ping'd


    // Getter functions
    public AITargetType     targetType   { get { return _targetType; } }
    public Collider         collider     { get { return _collider; } }
    public Vector3          position     { get { return _position; } }
    public float            distance     { get { return _distance; }          set { _distance = value; } }        
    public float            time         { get { return _time; } }



    // Functions
    public void Set(AITargetType t, Collider c, Vector3 p, float d)
    {
        _targetType = t;
        _collider = c;
        _position = p;
        _distance = d;
        _time = Time.time;
    }


    public void Clear()
    {
        _targetType = AITargetType.None;
        _collider = null;
        _position = Vector3.zero;
        _time = 0.0f;
        _distance = Mathf.Infinity;
    }
}






// ----------------------------------------------------------------------
// Class	:	AIStateMachine
// Desc		:	Base class for all AI State Machines
// ----------------------------------------------------------------------
public abstract class AIStateMachine : MonoBehaviour {

    /**
    *  VARIABLES
    * */
    // Public
    public AITarget     visualThreat    = new AITarget();
    public AITarget     audioThreat     = new AITarget();



    // Serialized Protected allows the inspector to assign values but keeps other classes from changing
    [SerializeField] protected SphereCollider   _targetTrigger       = null;
    [SerializeField] protected SphereCollider   _sensorTrigger       = null;
    [SerializeField] protected AIStateType      _currentStateType    = AIStateType.Idle;

    [SerializeField] [Range(0, 15)] protected float     _stoppingDistance    = 1.0f;




    // Protected
    protected AIState                               _currentState           = null;
    protected Dictionary<AIStateType, AIState>      _stateDictionary        = new Dictionary<AIStateType, AIState>();
    protected AITarget                              _target                 = new AITarget();
    protected Animator                              _animator               = null;
    protected NavMeshAgent                          _navAgent               = null;
    protected Collider                              _collider               = null;
    protected Transform                             _transform              = null;
    protected int                                   _rootPositionRefCount   = 0;
    protected int                                   _rootRotationRefCount   = 0;





    // Public Getters
    public Animator         aninimator
    {
        get
        {
            return _animator;
        }
    }
    public NavMeshAgent     navAgent
    {
        get
        {
            return _navAgent;
        }
    }
    public Vector3          sensorPosition
    {
        get
        {
            // Make sure that we have a sensortrigger
            if (_sensorTrigger == null)
                return Vector3.zero;


            // Start with getting our position
            Vector3 point = _sensorTrigger.transform.position;

            // Adjust our position for the center offset and any scalling that has happend in our parents
            point.x += _sensorTrigger.center.x * _sensorTrigger.transform.lossyScale.x;
            point.y += _sensorTrigger.center.y * _sensorTrigger.transform.lossyScale.y;
            point.z += _sensorTrigger.center.z * _sensorTrigger.transform.lossyScale.z;

            // return the accurate position.
            return point;
        }
    }
    public float            sensorRadius
    {
        get
        {
            // If we don't have a trigger than return 0
            if (_sensorTrigger == null)
                return 0.0f;


            // Find which axes has the greatest scale applied to it and muliply it by our radius to get the accurate radius.
            float radius = Mathf.Max(_sensorTrigger.radius * _sensorTrigger.transform.lossyScale.x,
                                        _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.y);

            return Mathf.Max(   radius,
                                _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.z);
        }
    }
    public bool             useRootPosition
    {
        get
        {
            return _rootPositionRefCount > 0;
        }
    }
    public bool             useRootRotation
    {
        get
        {
            return _rootRotationRefCount > 0;
        }
    }
    public AITargetType     targetType { get { return _target.targetType; } }
    public Vector3          targetPosition { get { return _target.position; } }








    /**
    *  CLASS FUNCTIONS
    * */
    // -----------------------------------------------------------------
    // Name	:	Awake
    // Desc	:	Cache Components
    // -----------------------------------------------------------------
    protected virtual void Awake()
    {
        // Cach all of the components that we need
        _animator       = GetComponent<Animator>();
        _navAgent       = GetComponent<NavMeshAgent>();
        _collider       = GetComponent<Collider>();
        _transform      = GetComponent<Transform>();


        // Do we have a valid Game Scene Manager
        if(GameSceneManager.instance != null)
        {
            // Register State Machine with the scene database
            if (_collider)
                GameSceneManager.instance.RegisterAIStateMachine(_collider.GetInstanceID(), this);

            if (_sensorTrigger)
                GameSceneManager.instance.RegisterAIStateMachine(_sensorTrigger.GetInstanceID(), this);
        }

    }

    // -----------------------------------------------------------------
    // Name	:	Start
    // Desc	:	Called by Unity prior to first update to setup the object
    // -----------------------------------------------------------------
    protected virtual void Start()
    {
        // Set the sensor trigger's parent to this state machine
        if (_sensorTrigger != null)
        {
            AISensor script = _sensorTrigger.GetComponent<AISensor>();

            if (script != null)
                script.parentStateMachine = this;
        }
            



        // Fetch all states on this game object
        AIState[] states = GetComponents<AIState>();

        // Loop through all the states and add them to the state dictionary
        foreach (AIState state in states)
        {
            // Add the state if it is not null and doesn't already exsist in the dictionary
            if (state != null && !_stateDictionary.ContainsKey(state.GetStateType()))
            {
                _stateDictionary[state.GetStateType()] = state;
                state.SetStateMachine(this);
            }
                
        }



        // Set the current state
        if (_stateDictionary.ContainsKey(_currentStateType))
        {
            _currentState = _stateDictionary[_currentStateType];
            _currentState.OnEnterState();
        }
        else
        {
            _currentState = null;
        }


        // Fetch all AIStateMachineLink derived behaviours from the animator
        // and set their State Machine references to this state machine
        if (_animator != null)
        {
            // Get all the behavious attached to our animator
            AIStateMachineLink[] scripts = _animator.GetBehaviours<AIStateMachineLink>();

            // Give a reference to our statemachine to all of the animator behaviours
            foreach(AIStateMachineLink script in scripts)
            {
                script.stateMachine = this;
            }
        }
    }

    // -------------------------------------------------------------------
    // Name :	Update
    // Desc	:	Called by Unity each frame. Gives the current state a
    //			chance to update itself and perform transitions.
    // -------------------------------------------------------------------
    protected virtual void Update()
    {
        // Make sure that we have a valid state
        if (_currentState == null)
            return;


        AIStateType newStateType = _currentState.OnUpdate();
        if(newStateType != _currentStateType)
        {
            AIState newState = null;


            // Check to see if we have a definition in our dictionary for our new state
            if(_stateDictionary.TryGetValue(newStateType, out newState))
            {
                // Let our old state clean up, let new state set up and change the states
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }
            // If we havent found the state in our dictionary set our state to idle
            else if (_stateDictionary.TryGetValue(AIStateType.Idle, out newState))
            {
                // Let our old state clean up, let new state set up and change the states
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }

            // Update our current state
            _currentStateType = newStateType;
        }

    }

    // -------------------------------------------------------------------
    // Name :	FixedUpdate
    // Desc	:	Called by Unity with each tick of the Physics system. It
    //			clears the audio and visual threats each update and
    //			re-calculates the distance to the current target
    // -------------------------------------------------------------------
    protected virtual void FixedUpdate()
    {
        visualThreat.Clear();
        audioThreat.Clear();


        if(_target.targetType != AITargetType.None)
        {
            _target.distance = Vector3.Distance(_transform.position, _target.position);
        }
    }


    // --------------------------------------------------------------------------
    //	Name	:	OnTriggerEnter
    //	Desc	:	Called by Physics system when the AI's Main collider enters
    //				its trigger. This allows the child state to know when it has 
    //				entered the sphere of influence	of a waypoint or last player 
    //				sighted position.
    // --------------------------------------------------------------------------
    protected virtual void OnTriggerEnter(Collider other)
    {
        // Return if we don't have a target or the collider is not ours
        if (_targetTrigger == null || other != _targetTrigger)
            return;


        // Notify child state.
        if (_currentState)
            _currentState.OnDestinationReached(true);
    }


    // --------------------------------------------------------------------------
    //	Name	:	OnTriggerExit
    //	Desc	:	Informs the child state that the AI entity is no longer at
    //				its destination (typically true when a new target has been
    //				set by the child.
    // --------------------------------------------------------------------------
    protected virtual void OnTriggerExit(Collider other)
    {
        if (_targetTrigger == null || _targetTrigger != other)
            return;

        if (_currentState != null)
            _currentState.OnDestinationReached(false);
    }

    // -----------------------------------------------------------
    // Name	:	OnAnimatorMove
    // Desc	:	Called by Unity after root motion has been
    //			evaluated but not applied to the object.
    //			This allows us to determine via code what to do
    //			with the root motion information
    // -----------------------------------------------------------
    protected virtual void OnAnimatorMove()
    {
        if (_currentState != null)
            _currentState.OnAnimatorUpdated();
    }

    // ----------------------------------------------------------
    // Name	: OnAnimatorIK
    // Desc	: Called by Unity just prior to the IK system being
    //		  updated giving us a chance to setup up IK Targets
    //		  and weights.
    // ----------------------------------------------------------
    protected virtual void OnAnimatorIK(int layerIndex)
    {
        if (_currentState != null)
            _currentState.OnAnimatorIkUpdated();
    }







    /**
    *  FUNCTIONS
    * */
    // ----------------------------------------------------------
    // Name	:	NavAgentControl
    // Desc	:	Configure the NavMeshAgent to enable/disable auto
    //			updates of position/rotation to our transform
    // ----------------------------------------------------------
    public void NavAgentControl(bool positionUpdate, bool rotationUpdate)
    {
        if(_navAgent != null)
        {
            _navAgent.updatePosition = positionUpdate;
            _navAgent.updateRotation = rotationUpdate;
        }
    }

    // ------------------------------------------------------------
    // Name	:	OnTriggerEvent
    // Desc	:	Called by our AISensor component when an AI Aggravator
    //			has entered/exited the sensor trigger.
    // -------------------------------------------------------------
    public virtual void OnTriggerEvent(AITriggerEventType type, Collider other)
    {
        if (_currentState != null)
            _currentState.OnTriggerEvent(type, other);
          
    }

    // --------------------------------------------------------------------
    // Name :	SetTarget (Overload)
    // Desc	:	Sets the current target and configures the target trigger.
    //			This method allows for specifying a custom stopping
    //			distance.
    // --------------------------------------------------------------------
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d, float s)
    {
        // Set the target Data
        _target.Set(t, c, p, d);

        // Configure and enable the target trigger at the correct
        // position and with the correct radius
        if (_targetTrigger != null)
        {
            _targetTrigger.radius               = s;
            _targetTrigger.transform.position   = _target.position;
            _targetTrigger.enabled              = true;
        }
    }

    // -------------------------------------------------------------------
    // Name :	SetTarget (Overload)
    // Desc	:	Sets the current target and configures the target trigger
    // -------------------------------------------------------------------
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d)
    {
        // Set the target info
        _target.Set(t, c, p, d);

        // Configure and enable the target trigger at the correct
        // position and with the correct radius
        if (_targetTrigger != null)
        {
            _targetTrigger.radius               = _stoppingDistance;
            _targetTrigger.transform.position   = _target.position;
            _targetTrigger.enabled              = true;
        }
    }

    // -------------------------------------------------------------------
    // Name :	SetTarget (Overload)
    // Desc	:	Sets the current target and configures the target trigger
    // -------------------------------------------------------------------
    public void SetTarget(AITarget t)
    {
        // Assign the new target
        _target = t;

        // Configure and enable the target trigger at the correct
        // position and with the correct radius
        if (_targetTrigger != null)
        {
            _targetTrigger.radius               = _stoppingDistance;
            _targetTrigger.transform.position   = t.position;
            _targetTrigger.enabled              = true;
        }
    }

    // -------------------------------------------------------------------
    // Name :	ClearTarget 
    // Desc	:	Clears the current target
    // -------------------------------------------------------------------
    public void ClearTarget()
    {
        _target.Clear();

        if (_targetTrigger != null)
            _targetTrigger.enabled = false;
    }

    // ----------------------------------------------------------
    // Name	:	AddRootMotionRequest
    // Desc	:	Called by the State Machine Behaviours to
    //			Enable/Disable root motion
    // ----------------------------------------------------------
    public void AddRootMotionRequest(int rootPosition, int rootRotation)
    {
        _rootPositionRefCount += rootPosition;
        _rootRotationRefCount += rootRotation;
    }

}
