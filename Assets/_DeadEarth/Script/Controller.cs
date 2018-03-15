using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {
    /**
    *  VARIABLES
    * */
    // Constant



    // Public



    // Serialized



    // Private
    private Animator    _animator           = null;
    private int         horizontalID        = 0;
    private int         verticalID          = 0;
    private int         attackID            = 0;




    /**
    *  CLASS FUNCTIONS
    * */
    private void Start()
    {
        _animator       = GetComponent<Animator>();
        horizontalID    = Animator.StringToHash("Horizontal");
        verticalID      = Animator.StringToHash("Vertical");
        attackID        = Animator.StringToHash("Attack");
    }


    private void Update()
    {
        float xAxis = Input.GetAxis("Horizontal") * 2.32f;
        float yAxis = Input.GetAxis("Vertical") * 5.66f;


        _animator.SetFloat(horizontalID, xAxis, 0.1f, Time.deltaTime);
        _animator.SetFloat(verticalID, yAxis, 1.0f, Time.deltaTime);

        if (Input.GetMouseButtonDown(0))
            _animator.SetTrigger(attackID);
    }








    /**
    *  FUNCTIONS
    * */
}
