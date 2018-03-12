using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;


[CustomEditor(typeof(AIWaypointNetwork))]
public class AIWaypointNetworkEditor : Editor
{



    /**
    *  CLASS FUNCTIONS
    * */

    // This is used to make a custom inspector
    public override void OnInspectorGUI()
    {
        AIWaypointNetwork network = (AIWaypointNetwork)target;


        network.displayMode = (PathDisplayMode)EditorGUILayout.EnumPopup("Display Mode: ", network.displayMode);

        if (network.displayMode == PathDisplayMode.Paths)
        {
            network.uIStart = EditorGUILayout.IntSlider("Waypoint Start: ", network.uIStart, 0, network.waypoints.Count);
            network.uIEnd = EditorGUILayout.IntSlider("Waypoint End: ", network.uIEnd, 0, network.waypoints.Count);
        }


        // This will invoke the original drawing of the inspector
        DrawDefaultInspector();
    }



    // This is used to make a custom scene view 
    private void OnSceneGUI()
    {
        AIWaypointNetwork network = (AIWaypointNetwork)target;


        // This will draw the labels above the waypoints
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;

        for (int i=0; i<network.waypoints.Count; i++)
        {
            if(network.waypoints[i] != null)
                Handles.Label(network.waypoints[i].position, "Waypoint " + i.ToString(), style);
        }




        // 
        // This is used for determining what kind of lines should be drawn between the wayponts
        //
        if (network.displayMode == PathDisplayMode.Connections)
        {
            // This will build a list then draw a line between the waypoints
            Vector3[] linePoints = new Vector3[network.waypoints.Count + 1];

            for (int i = 0; i <= network.waypoints.Count; i++)
            {
                // This line will check to see if our i is equal to the count of the array. If it is not equal the index will be set
                // to i, if it is equal it will be reset to 0
                int index = i != network.waypoints.Count ? i : 0;



                // Check to see if we have not set our waypoint to a position. If we have not set the position of the waypoint to infinity so
                // that we can easily see that we forgot to give it a position.
                if (network.waypoints[index] != null)
                    linePoints[i] = network.waypoints[index].position;
                else
                    linePoints[i] = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            }
            Handles.color = Color.cyan;
            Handles.DrawPolyLine(linePoints);
        }



        if (network.displayMode == PathDisplayMode.Paths)
        {
            NavMeshPath path = new NavMeshPath();

            if (network.waypoints[network.uIStart] != null && network.waypoints[network.uIEnd] != null)
            {
                Vector3 from = network.waypoints[network.uIStart].position;
                Vector3 to = network.waypoints[network.uIEnd].position;


                NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path);


                Handles.color = Color.yellow;
                Handles.DrawPolyLine(path.corners);
            }
        }

    }

    
    






    /**
    *  FUNCTIONS
    * */





}
