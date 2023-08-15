using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PathCreation;

public class PathManager : MonoBehaviour
{
    public GameObject waypoint;
    public bool inPathCreation = false;
    public bool inPathEdit = false;
    public static GameObject[] paths;
    private GameObject curPath;

    public TMP_Dropdown pathDropdown;
    
    void Start()
    {
        paths = GameObject.FindGameObjectsWithTag("Path");
        UpdateDropdown();
        SetPath();
    }

    void UpdateDropdown()
    {
        pathDropdown.options.Clear();
        foreach(GameObject path in paths)
        {
            pathDropdown.options.Add(new TMP_Dropdown.OptionData() {text = path.name});
        }
    }

    public void SetPath()
    {
        if(curPath) curPath.GetComponent<PathCreator>().ClearPath();

        curPath = curPath = paths[pathDropdown.value];
        Debug.Log("Cur path: " + curPath);

        curPath.GetComponent<PathCreator>().DrawPath();
    }

    public void editMode()
    {
        inPathEdit = true;
        inPathCreation = false;
        Debug.Log("Editing Path...");
    }

    public void createMode()
    {
        inPathEdit = false;
        inPathCreation = true;
        Debug.Log("Creating Path...");
    }

    public void exitMode()
    {
        inPathEdit = false;
        inPathCreation = false;
        Debug.Log("Exiting Path Edit...");
    }

    public void buildNewPath()
    {
        clearWaypoints();
    }

    public void clearWaypoints()
    {
        GameObject[] waypoints = GameObject.FindGameObjectsWithTag("Waypoint");

        foreach(GameObject waypoint in waypoints)
        {
            Destroy(waypoint);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if((Input.GetMouseButtonDown(0) && inPathCreation) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if(Physics.Raycast(ray, out hit))
            {
                // Define an offset to raise the waypoint above the terrain
                float yOffset = 5f; // Change this value to whatever offset you want

                // Add the offset to the hit point's y-coordinate
                Vector3 waypointPosition = hit.point + new Vector3(0, yOffset, 0);
                Debug.Log("Waypoint at: " + waypointPosition);

                // Instantiate a waypoint at the adjusted position
                Instantiate(waypoint, waypointPosition, Quaternion.identity);

                //Tag so we can find the waypoints
                waypoint.tag = "Waypoint";
            }
        }
    }
}
