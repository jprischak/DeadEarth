using System.Collections;
using System.Collections.Generic;
using UnityEngine;




// ---------------------------------------------------
// CLASS    : AIZombieStateMachine
// DESC     : State Machine used by zombie characters
// ---------------------------------------------------
public class AIZombieStateMachine : AIStateMachine
{
    /*
     * VARIABLES
     * */

    // Serialized allows the inspector to assign values but keeps other classes from changing
    [SerializeField] [Range(0, 100)]        int         _health         = 100;
    [SerializeField] [Range(10f, 360.0f)]   float       _fov            = 50.0f;
    [SerializeField] [Range(0.0f, 1.0f)]    float       _sight          = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)]    float       _hearing        = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)]    float       _aggression     = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)]    float       _intelligence   = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)]    float       _satisfaction   = 1.0f;
    [SerializeField]                        float       _replenishRate  = 0.5f;
    [SerializeField]                        float       _depleshenRate  = 0.1f;




    // Private
    private bool    _feeding        = false;
    private bool    _crawling       = false;
    private float   _speed          = 0.0f;
    private int     _seeking        = 0;
    private int     _attackType     = 0;
    private int     _speedHash      = Animator.StringToHash("Speed");
    private int     _seekingHash    = Animator.StringToHash("Seeking");
    private int     _feedingHash    = Animator.StringToHash("Feeding");
    private int     _attackHash     = Animator.StringToHash("Attack");




    // Public Properties
    public float    fov             { get { return _fov; } }
    public float    hearing         { get { return _hearing; } }
    public float    sight           { get { return _sight; } }
    public float    satisfaction    { get { return _satisfaction; }     set { _satisfaction = value; } }
    public float    aggression      { get { return _aggression; }       set { _aggression = value; } }
    public float    intellignece    { get { return _intelligence; } }
    public float    speed           { get { return _speed; }            set { _speed = value; } }
    public float    replenishRate   { get { return _replenishRate; } }
    public int      health          { get { return _health; }           set { _health = value; } }
    public int      attackType      { get { return _attackType; }       set { _attackType = value; } }
    public int      seeking         { get { return _seeking; }          set { _seeking = value; } }
    public bool     crawling        { get { return _crawling; } }
    public bool     feeding         { get { return _feeding; }          set { _feeding = value; } }




	/**
	 *  CLASS FUNCTIONS
	 * */
     //---------------------------------------------------------------------------------
     // Name :  Update
     // Desc :  Refresh the animator with the up-to-date values for it's parameters.
     //---------------------------------------------------------------------------------
	protected override void Update()
	{
		base.Update();


        // Send our variables and triggers over to our animator
		if(_animator != null)
		{
			_animator.SetFloat      (_speedHash,       _speed);
			_animator.SetBool       (_feedingHash,     _feeding);
			_animator.SetInteger    (_seekingHash,     _seeking);
			_animator.SetInteger    (_attackHash ,     _attackType);

		}


        // Removes satisfaction from the zombie as they move around the scene
        _satisfaction = Mathf.Max(0, _satisfaction - ( (_depleshenRate * Time.deltaTime) / 100) * Mathf.Pow(_speed, 3.0f));
	}
}
