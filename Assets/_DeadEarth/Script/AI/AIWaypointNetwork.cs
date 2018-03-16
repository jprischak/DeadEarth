using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum PathDisplayMode { None, Connections, Paths}






public class AIWaypointNetwork : MonoBehaviour {

    /**
    *  VARIABLES
    * */
    [HideInInspector]public PathDisplayMode displayMode = PathDisplayMode.Connections;
    [HideInInspector] public int uIStart = 0;
    [HideInInspector] public int uIEnd = 0;

    public List<Transform> waypoints = new List<Transform>();
    





    /**
    *  CLASS FUNCTIONS
    * */
    








    /**
    *  FUNCTIONS
    * */
}
