using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// -------------------------------------------------------------------------
// CLASS	:	GameSceneManager
// Desc		:	Singleton class that acts as the scene database
// -------------------------------------------------------------------------
public class GameSceneManager : MonoBehaviour {

    /**
    *  VARIABLES
    * */
    // Constant



    // Public



    // Serialized
    [SerializeField] private ParticleSystem         _bloodParticles     = null;


    // Private
    private static GameSceneManager                 _Instance           = null;
    private Dictionary<int, AIStateMachine>         _StateMachine       = new Dictionary<int, AIStateMachine>();
    





    /**
    *  CLASS FUNCTIONS
    * */
    private void Start()
    {

    }


    private void Update()
    {

    }


    





    /**
    *  FUNCTIONS
    * */
    // GETTER FUNCTIONS
    public static GameSceneManager instance
    {
        // If we don't currently of a reference to the game scene manager we will search the scene for a object of type
        get
        {
            if (_Instance == null)
                _Instance = (GameSceneManager)FindObjectOfType(typeof(GameSceneManager));

            return _Instance;
        }
    }


    public ParticleSystem bloodParticles { get { return _bloodParticles; } }




    // --------------------------------------------------------------------
    // Name	:	RegisterAIStateMachine
    // Desc	:	Stores the passed state machine in the dictionary with
    //			the supplied key
    // --------------------------------------------------------------------
    public void RegisterAIStateMachine(int key, AIStateMachine stateMachine)
    {
        if(!_StateMachine.ContainsKey(key))
        {
            _StateMachine[key] = stateMachine;
        }
    }


    // --------------------------------------------------------------------
    // Name	:	GetAIStateMachine
    // Desc	:	Returns an AI State Machine reference searched on by the
    //			instance ID of an object
    // --------------------------------------------------------------------
    public AIStateMachine GetAIStateMachine(int key)
    {
        AIStateMachine machine = null;

        if(_StateMachine.TryGetValue(key, out machine))
        {
            return machine;
        }

        return null;
           
    }
    
}
