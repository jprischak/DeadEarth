using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedController : MonoBehaviour {
    /**
    *  VARIABLES
    * */
    // Constant



    // Public
    public float    speed   = 0;


    // Serialized



    // Private
    private Animator    _controller     = null;





    /**
    *  CLASS FUNCTIONS
    * */
    private void Start()
    {
        _controller = GetComponent<Animator>();
    }


    private void Update()
    {
        _controller.SetFloat("Speed", speed);
    }








    /**
    *  FUNCTIONS
    * */
}
