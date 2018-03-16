using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


// Enums
public enum AIStateType         { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead }
public enum AITargetType        { None, Waypoint, Visual_Player, Visual_Light, Visual_Food, Audio}
public enum AITriggerEventType  { Enter, Stay, Exit}







/**
 * Describes a potential target to the AI system
 * */
public struct AITarget
{
    // Variables
    private AITargetType    targetType;             // The type of target
    private Collider        collider;               // The collider
    private Vector3         position;               // Current position in the world
    private float           distance;               // Distance from player
    private float           time;                   // Time the target was last ping'd


    // Getter functions
    public AITargetType     GetTargetType   { get { return targetType; } }
    public Collider         GetCollider     { get { return collider; } }
    public Vector3          GetPosition     { get { return position; } }
    public float            Distance     { get { return distance; }          set { distance = value; } }        
    public float            GetTime         { get { return time; } }



    // Functions
    public void Set(AITargetType t, Collider c, Vector3 p, float d)
    {
        targetType = t;
        collider = c;
        position = p;
        distance = d;
        time = Time.time;
    }


    public void Clear()
    {
        targetType = AITargetType.None;
        collider = null;
        position = Vector3.zero;
        time = 0.0f;
        distance = Mathf.Infinity;
    }
}







public abstract class AIStateMachine : MonoBehaviour {

    /**
    *  VARIABLES
    * */
    // Constant



    // Public
    public AITarget     visualThreat    = new AITarget();
    public AITarget     audioThreat     = new AITarget();



    // Serialized
    [SerializeField] protected SphereCollider   targetTrigger       = null;
    [SerializeField] protected SphereCollider   sensorTrigger       = null;
    [SerializeField] protected AIStateType      currentStateType    = AIStateType.Idle;

    [SerializeField] [Range(0, 15)] protected float     stoppingDistance    = 1.0f;




    // Protected
    protected AIState                               currentState        = null;
    protected Dictionary<AIStateType, AIState>      stateDictionary     = new Dictionary<AIStateType, AIState>();
    protected AITarget                              myTarget            = new AITarget();
    protected Animator                              _animator           = null;
    protected NavMeshAgent                          _navAgent           = null;
    protected Collider                              _collider           = null;
    protected Transform                             _transform          = null;





    // Public Properties
    public Animator         GetAninimator   { get { return _animator; } }
    public NavMeshAgent     GetNavAgent     { get { return _navAgent; } }








    /**
    *  CLASS FUNCTIONS
    * */

    protected virtual void Awake()
    {
        // Cach all of the components that we need
        _animator       = GetComponent<Animator>();
        _navAgent       = GetComponent<NavMeshAgent>();
        _collider       = GetComponent<Collider>();
        _transform      = GetComponent<Transform>();
    }


    protected virtual void Start()
    {
        // Fetch all states on this game object
        AIState[] states = GetComponents<AIState>();

        // Loop through all the states and add them to the state dictionary
        foreach (AIState state in states)
        {
            // Add the state if it is not null and doesn't already exsist in the dictionary
            if (state != null && !stateDictionary.ContainsKey(state.GetStateType()))
            {
                stateDictionary[state.GetStateType()] = state;
                state.SetStateMachine(this);
            }
                
        }


        if (stateDictionary.ContainsKey(currentStateType))
        {
            currentState = stateDictionary[currentStateType];
            currentState.OnEnterState();
        }
        else
            currentState = null;
    }


    protected virtual void Update()
    {
        // Make sure that we have a valid state
        if (currentState == null)
            return;


        AIStateType newStateType = currentState.OnUpdate();
        if(newStateType != currentStateType)
        {
            AIState newState = null;


            // Check to see if we have a definition in our dictionary for our new state
            if(stateDictionary.TryGetValue(newStateType, out newState))
            {
                // Let our old state clean up, let new state set up and change the states
                currentState.OnExitState();
                newState.OnEnterState();
                currentState = newState;
            }
            // If we havent found the state in our dictionary set our state to idle
            else if (stateDictionary.TryGetValue(AIStateType.Idle, out newState))
            {
                // Let our old state clean up, let new state set up and change the states
                currentState.OnExitState();
                newState.OnEnterState();
                currentState = newState;
            }

            // Update our current state
            currentStateType = newStateType;
        }

    }


    protected virtual void FixedUpdate()
    {
        visualThreat.Clear();
        audioThreat.Clear();


        if(myTarget.GetTargetType != AITargetType.None)
        {
            myTarget.Distance = Vector3.Distance(_transform.position, myTarget.GetPosition);
        }
    }









    /**
    *  FUNCTIONS
    * */
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d, float s)
    {
        myTarget.Set(t, c, p, d);

        if (targetTrigger != null)
        {
            targetTrigger.radius = s;
            targetTrigger.transform.position = myTarget.GetPosition;
            targetTrigger.enabled = true;
        }
    }


    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d)
    {
        myTarget.Set(t, c, p, d);

        if(targetTrigger != null)
        {
            targetTrigger.radius = stoppingDistance;
            targetTrigger.transform.position = myTarget.GetPosition;
            targetTrigger.enabled = true;
        }
    }


    public void SetTarget(AITarget t)
    {
        myTarget = t;

        if (targetTrigger != null)
        {
            targetTrigger.radius = stoppingDistance;
            targetTrigger.transform.position = t.GetPosition;
            targetTrigger.enabled = true;
        }
    }

    // This will clear our current target
    public void ClearTarget()
    {
        myTarget.Clear();

        if (targetTrigger != null)
            targetTrigger.enabled = false;
    }
}
