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
    //public static GameObject[] paths;
    public static List<GameObject> paths = new List<GameObject>();
    private GameObject curPath;

    public TMP_Dropdown pathDropdown;
    public TMP_InputField pathNameInput;
    
    void Start()
    {
        updatePaths();
        updateDropdown();
        setCurPath();
    }

    public void updateDropdown()
    {
        pathDropdown.options.Clear();
        //Debug.Log("Dropdown updated.");
        foreach(GameObject path in paths)
        {
            pathDropdown.options.Add(new TMP_Dropdown.OptionData() {text = path.name});
        }
    }

    public void updatePaths()
    {
        //paths = GameObject.FindGameObjectsWithTag("Path");
        paths = Utility.getChildren(GameObject.FindGameObjectWithTag("PathParent"));
        foreach(GameObject path in paths) Debug.Log(path.name);
    }

    public void setCurPath()
    {
        if(curPath) curPath.GetComponent<PathCreator>().ClearPath();

        curPath = curPath = paths[pathDropdown.value];
        //Debug.Log("Cur path: " + curPath);

        curPath.GetComponent<PathCreator>().DrawPath();
    }

    public void editMode()
    {
        updateDropdown();
        setCurPath();

        inPathEdit = true;
        inPathCreation = false;
        //Debug.Log("Editing Path...");
    }

    public void createMode()
    {
        pathNameInput.text = "Path" + paths.Count;
        //InputSystem.DisableDevice(Keyboard.current);

        inPathEdit = false;
        inPathCreation = true;
        //Debug.Log("Creating Path...");
    }

    public void exitMode()
    {
        curPath.GetComponent<PathCreator>().ClearPath();

        inPathEdit = false;
        inPathCreation = false;
        //Debug.Log("Exiting Path Edit...");
    }

    public void buildNewPath()
    {
        GameObject[] waypoints = GameObject.FindGameObjectsWithTag("Waypoint");

        GameObject newPath = new GameObject(pathNameInput.text);

        GameObject parent = GameObject.FindGameObjectWithTag("PathParent");
        newPath.transform.parent = parent.transform;

        newPath.tag = "Path";
        updatePaths();

        PathCreator creator = newPath.AddComponent<PathCreator>();

        List<Vector3> anchorPoints = new List<Vector3>();
        foreach(GameObject waypoint in waypoints) anchorPoints.Add(waypoint.transform.position);
        creator.bezierPath = new BezierPath(anchorPoints);
        creator.bezierPath.GlobalNormalsAngle = 90;
        creator.enabled = true;

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
                float yOffset = 0.25f; // Change this value to whatever offset you want

                // Add the offset to the hit point's y-coordinate
                Vector3 waypointPosition = hit.point + new Vector3(0, yOffset, 0);
                //Debug.Log("Waypoint at: " + waypointPosition);

                // Instantiate a waypoint at the adjusted position
                Instantiate(waypoint, waypointPosition, Quaternion.identity);

                //Tag so we can find the waypoints
                waypoint.tag = "Waypoint";
            }
        }
    }
}
