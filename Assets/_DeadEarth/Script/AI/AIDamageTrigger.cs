using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// -----------------------------------------------------------------
// Class	: AIDamageTrigger
// Desc		: Used to handle inflicting damage on the player
// -----------------------------------------------------------------
public class AIDamageTrigger : MonoBehaviour {

    // Inspector assigned
    [SerializeField] string     _paramater                  = "";
    [SerializeField] int        _bloodParticleBurstAmount   = 10;
    [SerializeField] float      _damageAmount               = 0.1f;



    // Private variables
    private AIStateMachine      _stateMachine       = null;
    private Animator            _animator           = null;
    private int                 _paramaterHash      = -1;
    private GameSceneManager    _gameSceneManager   = null;



    // ------------------------------------------------------------
    // Name	:	Start
    // Desc	:	Called on object start-up to initialize the script.
    // ------------------------------------------------------------
    private void Start()
    {
        // Cache state machine and animator references
        _stateMachine = transform.root.GetComponentInChildren<AIStateMachine>();

        _gameSceneManager = GameSceneManager.instance;


        if (_animator == null)
            _animator = _stateMachine.aninimator;

        // Generate parameter hash for more efficient parameter lookups from the animator
        _paramaterHash = Animator.StringToHash(_paramater);

    }


    // -------------------------------------------------------------
    // Name	:	OnTriggerStay
    // Desc	:	Called by Unity each fixed update that THIS trigger
    //			is in contact with another.
    // -------------------------------------------------------------
    private void OnTriggerStay(Collider other)
    {
        // If we don't have an animator return
        if (!_animator)
            return;

        // If this is the player object and our parameter is set for damafe
        if (other.gameObject.CompareTag("Player") && _animator.GetFloat(_paramaterHash) > 0.9)
        {
            if(GameSceneManager.instance && GameSceneManager.instance.bloodParticles)
            {
                ParticleSystem system = GameSceneManager.instance.bloodParticles;

                // Temporary Code
                system.transform.position = transform.position;
                system.transform.rotation = Camera.main.transform.rotation;

                system.simulationSpace = ParticleSystemSimulationSpace.World;
                system.Emit(_bloodParticleBurstAmount);
            }


            if(_gameSceneManager != null)
            {
                PlayerInfo info = _gameSceneManager.GetPlayerInfo(other.GetInstanceID());

                if(info != null && info.characterManager != null)
                {
                    info.characterManager.TakeDamage(_damageAmount);
                }
            }
        }
    }
}
