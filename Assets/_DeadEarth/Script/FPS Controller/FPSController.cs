using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Enumerations
public enum PlayerMoveStatus { NotMoving, Crouching, Walking, Running, NotGrounded, Landing}
public enum CurveControlledBobCallbackType { Horizontal, Vertical }




// Delegates
public delegate void CurveControlledBobCallback();




[System.Serializable]
public class CurveControlledBobEvent
{
    public float                            Time        = 0.0f;
    public CurveControlledBobCallback       Function    = null;
    public CurveControlledBobCallbackType   Type        = CurveControlledBobCallbackType.Vertical;
}






[System.Serializable]
public class CurveControlledBob
{
    // Animation curve
    [SerializeField] AnimationCurve _bobCurve = new AnimationCurve( new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                                    new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                                    new Keyframe(2f, 0f));

    // Inspector Assigned Bob Control Variables
    [SerializeField] float      _horizontalMultiplier               = 0.01f;
    [SerializeField] float      _verticalMultiplier                 = 0.02f;
    [SerializeField] float      _verticalToHorizontalSpeedRatio     = 2.0f;
    [SerializeField] float      _baseInterval                       = 1.0f;



    // Private internal variables
    private float                           _prevXPlayHead;
    private float                           _prevYPlayHead;
    private float                           _xPlayHead;
    private float                           _yPlayHead;
    private float                           _curveEndTime;
    private List<CurveControlledBobEvent>   _events             = new List<CurveControlledBobEvent>();



    public void Initialize()
    {
        // Record time length of bob curve
        _curveEndTime = _bobCurve[_bobCurve.length - 1].time;

        // Reset values
        _xPlayHead          = 0.0f;
        _yPlayHead          = 0.0f;
        _prevXPlayHead      = 0.0f;
        _prevYPlayHead      = 0.0f;
    }


    public Vector3 GetVectorOffset(float speed)
    {
        _xPlayHead += (speed * Time.deltaTime) / _baseInterval;
        _yPlayHead += ((speed * Time.deltaTime) / _baseInterval) * _verticalToHorizontalSpeedRatio;


        // Make sure that we have not moved out of the range of our curve
        if (_xPlayHead > _curveEndTime)
            _xPlayHead -= _curveEndTime;

        if (_yPlayHead > _curveEndTime)
            _yPlayHead -= _curveEndTime;


        // Process events
        for(int i=0; i<_events.Count; i++)
        {
            CurveControlledBobEvent ev = _events[i];
            if(ev != null)
            {
                if(ev.Type == CurveControlledBobCallbackType.Vertical)
                {
                    if( (_prevYPlayHead < ev.Time && _yPlayHead >= ev.Time) ||
                        (_prevYPlayHead > _yPlayHead && (ev.Time > _prevYPlayHead || ev.Time <= _yPlayHead)))
                    {
                        ev.Function();
                    }
                }
                if (ev.Type == CurveControlledBobCallbackType.Horizontal)
                {
                    if ((_prevXPlayHead < ev.Time && _xPlayHead >= ev.Time) ||
                        (_prevXPlayHead > _xPlayHead && (ev.Time > _prevXPlayHead || ev.Time <= _xPlayHead)))
                    {
                        ev.Function();
                    }
                }
            }
        }




        float xPos = _bobCurve.Evaluate(_xPlayHead) * _horizontalMultiplier;
        float yPos = _bobCurve.Evaluate(_yPlayHead) * _verticalMultiplier;


        _prevXPlayHead = _xPlayHead;
        _prevYPlayHead = _yPlayHead;


        return new Vector3(xPos, yPos, 0f);

    }


    public void RegisterEventCallback(float time, CurveControlledBobCallback function, CurveControlledBobCallbackType type)
    {
        CurveControlledBobEvent ccbeEvent = new CurveControlledBobEvent();
        ccbeEvent.Time      = time;
        ccbeEvent.Function  = function;
        ccbeEvent.Type      = type;
        _events.Add(ccbeEvent);
        _events.Sort(
            delegate (CurveControlledBobEvent t1, CurveControlledBobEvent t2)
            {
                return (t1.Time.CompareTo(t2.Time));
            }
        );
    }


}







[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public List<AudioSource> AudioSources = new List<AudioSource>();
    private int _audioToUse = 0;







    // Inspector Assigned Locomotion Settings
    [SerializeField] private float                  _walkSpeed                  = 1.0f;
    [SerializeField] private float                  _runSpeed                   = 4.5f;
    [SerializeField] private float                  _jumpSpeed                  = 7.5f;
    [SerializeField] private float                  _crouchSpeed                = 1.0f;
    [SerializeField] private float                  _stickToGroundForce         = 5.0f;
    [SerializeField] private float                  _gravityMultiplier          = 2.5f;
    [SerializeField] private float                  _runStepLengthen            = 0.75f;
    [SerializeField] private GameObject             _flashLight                 = null;
    [SerializeField] private CurveControlledBob     _headBob                    = new CurveControlledBob();
    


    // Use Standard Assets Mouse Look cass for mouse input -> Camera Look Control
    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.MouseLook   _mouseLook      = new UnityStandardAssets.Characters.FirstPerson.MouseLook();



    // Private variables
    private Camera                          _camera                     = null;
    private CharacterController             _characterController        = null;
    private bool                            _jumpButtonPressed          = false;
    private bool                            _previouslyGrounded         = false;
    private bool                            _isWalking                  = true;
    private bool                            _isJumping                  = false;
    private bool                            _isCrouching                = false;
    private float                           _minimalFallingDistance     = 0.5f;
    private float                           _controllerHeight           = 0.0f;
    private Vector2                         _inputVector                = Vector2.zero;
    private Vector3                         _moveDirection              = Vector3.zero;
    private Vector3                         _localSpaceCameraPos        = Vector3.zero;
    private PlayerMoveStatus                _movementStatus             = PlayerMoveStatus.NotMoving;
   



    // Timers
    private float                   _fallingTimer               = 0.0f;

    


    // Public Properties
    public PlayerMoveStatus     movementStatus { get { return _movementStatus; } }
    public float                walkSpeed { get { return _walkSpeed; } }
    public float                runSpeed { get { return _runSpeed; } }







    // CLASS FUNCTIONS
    protected void Start()
    {
        // Cache component references
        _characterController    = GetComponent<CharacterController>();
        _controllerHeight       = _characterController.height;

        // Get the main camera and cache local position within the FPS rig
        _camera = Camera.main;
        _localSpaceCameraPos = _camera.transform.localPosition;

        // Set initial status to not moving
        _movementStatus = PlayerMoveStatus.NotMoving;

        // Reset timers
        _fallingTimer = 0.0f;

        // Setup Mouse Look Script
        _mouseLook.Init(transform, _camera.transform);

        // Initiate Head Bob object
        _headBob.Initialize();
        _headBob.RegisterEventCallback(1.5f, PlayFootStepSound, CurveControlledBobCallbackType.Vertical);


        // Turn the flashlight off to start
        if (_flashLight)
            _flashLight.SetActive(false);

    }


    protected void FixedUpdate()
    {
        // Read input from axis
        float horizontal    = Input.GetAxis("Horizontal");
        float vertical      = Input.GetAxis("Vertical");
        bool wasWalking     = _isWalking;
        _isWalking          = !Input.GetKey(KeyCode.LeftShift);


        // Set the desired speed to be either our walking speed or our running speed
        float speed = _isCrouching ? _crouchSpeed : _isWalking ? _walkSpeed : _runSpeed;
        _inputVector    = new Vector2(horizontal, vertical);


        // Normalize input if it exceeds 1 in combined length
        if (_inputVector.sqrMagnitude > 1) _inputVector.Normalize();


        // Always move along the camera formward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * _inputVector.y + transform.right * _inputVector.x;


        // Get a normal for the surface thatis being touched to move along it
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, _characterController.radius, Vector3.down, out hitInfo, _characterController.height / 2f, 1))
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;


        // Scale the movement by our current speed(walking value or running value)
        _moveDirection.x = desiredMove.x * speed;
        _moveDirection.z = desiredMove.z * speed;


        

        // If grounded
        if (_characterController.isGrounded)
        {
            // Apply severe down force to keep control sticking to the floor
            _moveDirection.y = -_stickToGroundForce;

            // If the jump button was pressed then apply speed in up direction and set isJumping to true. Also, reset jump button status
            if (_jumpButtonPressed)
            {
                _moveDirection.y = _jumpSpeed;
                _jumpButtonPressed = false;
                _isJumping = true;

                // TODO: Play Jumping Sound
            }
        }
        else
        {
            // Otherwise we are not on the ground so apply standard system gravaty multiplied by our gravity modifier
            _moveDirection += Physics.gravity * _gravityMultiplier * Time.fixedDeltaTime;
        }


        // Move the Character Controller
        _characterController.Move(_moveDirection * Time.fixedDeltaTime);


        // Are we moving
        Vector3 speedXZ = new Vector3(_characterController.velocity.x, 0.0f, _characterController.velocity.z);
        if (speedXZ.magnitude > 0.01f)
            _camera.transform.localPosition = _localSpaceCameraPos + _headBob.GetVectorOffset(speedXZ.magnitude * (_isCrouching || _isWalking ? 1.0f:_runStepLengthen));
        else
            _camera.transform.localPosition = _localSpaceCameraPos;

    }


    protected void Update()
    {
        // If we are falling increment timer
        if (_characterController.isGrounded)
            _fallingTimer = 0.0f;
        else
            _fallingTimer += Time.deltaTime;


        // Allow Mouse Look a chance to process mouse and rotate camera
        // The if statement makes sure the game has not been paused
        if (Time.timeScale > Mathf.Epsilon)
            _mouseLook.LookRotation(transform, _camera.transform);


        // Process the flash light button
        if(Input.GetButtonDown("Flashlight"))
        {
            if (_flashLight)
                _flashLight.SetActive(!_flashLight.activeSelf);
        }



        // Process the Jump Button
        // the jump state needs to read here to make sure it is not missed
        if (!_jumpButtonPressed && !_isCrouching)
            _jumpButtonPressed = Input.GetButtonDown("Jump");


        // Process the crouch button
        if(Input.GetButtonDown("Crouch"))
        {
            _isCrouching = !_isCrouching;
            _characterController.height = _isCrouching == true ? _controllerHeight / 2.0f : _controllerHeight;
        }



        // Calculate Character Status
        if (!_previouslyGrounded && _characterController.isGrounded)
        {
            if (_fallingTimer > _minimalFallingDistance)
            {
                // TODO: Play Landing Sound
            }

            _moveDirection.y = 0f;
            _isJumping = false;
            _movementStatus = PlayerMoveStatus.Landing;
        }
        else
        if (!_characterController.isGrounded)
            _movementStatus = PlayerMoveStatus.NotGrounded;
        else
        if (_characterController.velocity.sqrMagnitude < 0.01f)
            _movementStatus = PlayerMoveStatus.NotMoving;
        else
        if
            (_isCrouching)
            _movementStatus = PlayerMoveStatus.Crouching;
        else
        if (_isWalking)
            _movementStatus = PlayerMoveStatus.Walking;
        else
            _movementStatus = PlayerMoveStatus.Running;



        _previouslyGrounded = _characterController.isGrounded;
    }


    void PlayFootStepSound()
    {
        if (_isCrouching)
            return;

        AudioSources[_audioToUse].Play();
        _audioToUse = (_audioToUse == 0) ? 1 : 0;
    }
}
