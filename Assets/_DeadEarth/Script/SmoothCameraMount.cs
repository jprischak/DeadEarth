using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCameraMount : MonoBehaviour {

    /**
    *  VARIABLES
    * */
    // Public
    public Transform    mount   = null;
    public float        speed   = 5.0f;





    /**
    *  CLASS FUNCTIONS
    * */
    private void Start()
    {

    }


    private void Update()
    {

    }


    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, mount.position, Time.deltaTime * speed);
        transform.rotation = Quaternion.Slerp(transform.rotation, mount.rotation, Time.deltaTime * speed);
    }








    /**
    *  FUNCTIONS
    * */
}
