using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PathCreation;

public class PathManager : MonoBehaviour
{
    bool inPathEdit = false;
    public bool inPathCreation = false;

    public GameObject waypointPrefab;
    public static List<GameObject> paths = new List<GameObject>();
    public List<GameObject> waypoints = new List<GameObject>();
    private Vector3 waypointSpawn;
    private int numSpawned;

    private GameObject curPath;
    private GameObject pathInCreation;
    public float heightAboveTerrain = 0.25f;
    private LineRenderer normalRend;

    public TMP_Dropdown pathDropdown;
    public TMP_InputField pathNameInput;
    public GameObject removeButton;
    public GameObject saveButton;
    public ToggleButton straightenButton;

    private GameObject selectedWaypoint;
    private TerrainCollider terrainCollider;

    bool editingPathName = false;

    public bool snapToGround = false;
    List<int> straightenedSegments = new List<int>();
    
    void Start()
    {
        UpdateDropdown();

        normalRend = gameObject.GetComponent<LineRenderer>();
        normalRend.startWidth = 0.05f;
        normalRend.endWidth = 0.05f;

        waypointSpawn = new Vector3(-10, 0, 0);
        waypointSpawn.y = heightAboveTerrain + Terrain.activeTerrain.SampleHeight(waypointSpawn);
        
        terrainCollider = Terrain.activeTerrain.GetComponent<TerrainCollider>();
    }

    IEnumerator MonitorNameInput()
    {
        while(inPathCreation)
        {
            // Block camera movement while in the path name text box
            if(pathNameInput.isFocused)
            {
                editingPathName = true;
                CameraMovement.lockMovement = true;
            }

            // Allow camera movement once out of the path name text box
            if(editingPathName != pathNameInput.isFocused)
            {
                editingPathName = false;
                CameraMovement.lockMovement = false;
            }

            yield return null;
        }
    }

    IEnumerator DisplayCurPath()
    {
        // wait one frame for setup
        yield return null;

        while(inPathEdit)
        {
            // Manage straighten button display
            if(waypoints.Count > 0)
            {
                int curIndex = GetWaypointIndex(selectedWaypoint);
                if(curIndex == waypoints.Count - 1 && curIndex != 0) curIndex--;

                if(straightenedSegments.Contains(curIndex)) straightenButton.SetOn();
                else straightenButton.SetOff();
            }

            // Display or hide the current path
            if(curPath)
            {
                PathCreator curCreator = curPath.GetComponent<PathCreator>();
                if(waypoints.Count >= 2)
                {
                    //Debug.Log("Displaying path...");
                    List<Vector3> anchorPoints = new List<Vector3>();
                    foreach(GameObject waypoint in waypoints) anchorPoints.Add(waypoint.transform.position);
                    curCreator.bezierPath = new BezierPath(anchorPoints);

                    // Set normals to up by default(changes with snapping)
                    //for(int i = 0; i < buildingCreator.path.localNormals.Length; i++) buildingCreator.path.localNormals[i] = Vector3.up;

                    // Apply snapping and straigtening while toggled
                    if(snapToGround) SnapPath(curCreator);
                    foreach(int segmentIndex in straightenedSegments) StraightenSegment(curCreator, segmentIndex);

                    //DrawNormals();

                    curCreator.DrawPathEdit();
                }
                else
                {
                    //Debug.Log("Hiding path...");
                    curCreator.ClearPath();
                }
            }

            yield return null;
        }
    }

    public void StartEdit()
    {
        inPathEdit = true;

        UpdateDropdown();
        UpdateCurPath();

        StartCoroutine(DisplayCurPath());
    }

    public void StopEdit()
    {
        inPathEdit = false;
        inPathCreation = false;

        if(pathInCreation) Destroy(pathInCreation);

        HideCurPath();
    }

    public void UpdateDropdown()
    {
        pathDropdown.options.Clear();

        paths = Utility.GetChildren(GameObject.FindGameObjectWithTag("PathParent"));
        foreach(GameObject path in paths)
        {
            pathDropdown.options.Add(new TMP_Dropdown.OptionData() {text = path.name});
        }
    }

    public void UpdateCurPath()
    {
        HideCurPath();

        curPath = paths[pathDropdown.value];

        BezierPath path = curPath.GetComponent<PathCreator>().bezierPath;

        for(int i = 0; i < path.NumPoints; i += 3)
        {
            GameObject newWaypoint = Instantiate(waypointPrefab);
            newWaypoint.transform.position = path.GetPoint(i);
            waypoints.Add(newWaypoint);
        }
    }

    void SetCurPath(GameObject newCurPath)
    {
        HideCurPath();

        curPath = newCurPath;

        BezierPath path = curPath.GetComponent<PathCreator>().bezierPath;

        for(int i = 0; i < path.NumPoints; i += 3)
        {
            GameObject newWaypoint = Instantiate(waypointPrefab);
            newWaypoint.transform.position = path.GetPoint(i);
            waypoints.Add(newWaypoint);
        }
    }

    void HideCurPath()
    {
        if(curPath) 
        {
            curPath.GetComponent<PathCreator>().ClearPath();
            ClearWaypoints();
        }
    }

    public void CreateNewPath()
    {   
        inPathCreation = true;

        pathNameInput.text = "Path" + paths.Count;

        GameObject buildingPath = new GameObject("BuildingPath");
        PathCreator buildingCreator = buildingPath.AddComponent<PathCreator>();
        buildingCreator.bezierPath.DeleteSegment(0);
        buildingCreator.bezierPath.DeleteSegment(1);
        
        SetCurPath(buildingPath);
        pathInCreation = buildingPath;

        StartCoroutine(MonitorNameInput());
    }

    public void CancelNewPath()
    {
        Destroy(curPath);

        UpdateCurPath();

        inPathCreation = false;
    }

    public void SavePath()
    {
        GameObject newPath = Instantiate(curPath);
        newPath.name = pathNameInput.text;

        GameObject parent = GameObject.FindGameObjectWithTag("PathParent");
        newPath.transform.parent = parent.transform;

        PathCreator newCreator = newPath.GetComponent<PathCreator>();
        //newCreator.ClearPath();

        //for(int i = 0; i < newCreator.path.localNormals.Length; i++) newCreator.path.localNormals[i] = Vector3.up;

        // Apply snapping and straigtening while toggled
        if(snapToGround) 
        {
            SnapPath(newCreator);
            newCreator.objectsFollowTerrain = true;
        }

        foreach(int segmentIndex in straightenedSegments) StraightenSegment(newCreator, segmentIndex);

        newPath.tag = "Path";
        UpdateDropdown();

        pathDropdown.value = paths.Count - 1;

        inPathCreation = false;
    }


    public void TogglePathSnapped()
    {
        snapToGround = !snapToGround;
    }

    void SnapPath(PathCreator creator)
    {
        foreach(GameObject waypoint in waypoints)
        {
            Vector3 groundPos = new Vector3(waypoint.transform.position.x, Terrain.activeTerrain.SampleHeight(waypoint.transform.position) + heightAboveTerrain, waypoint.transform.position.z);
            waypoint.transform.position = groundPos;
        }

        for(int i = 0; i < creator.path.localPoints.Length; i++)
        {
            creator.path.localPoints[i].y = Terrain.activeTerrain.SampleHeight(creator.path.localPoints[i]) + heightAboveTerrain;

            /*
            // may collide with objects above terrain?
            RaycastHit hit;
            if(Physics.Raycast(creator.path.localPoints[i], Vector3.down, out hit, Mathf.Infinity))
            {
                creator.path.localPoints[i].y = hit.point.y + heightAboveTerrain;
            }
            else Debug.Log("Path snap ray missed.");
            */
        
            Vector3 dir;
            if(i == 0) dir = creator.path.localPoints[i] - creator.path.localPoints[i + 1];
            else dir = creator.path.localPoints[i] - creator.path.localPoints[i - 1];
            //creator.path.localNormals[i] = Vector3.Cross(dir, Vector3.right).normalized;
        }
    }

    public void ToggleSegmentStraight()
    {
        int selectedIndex = GetWaypointIndex(selectedWaypoint);
        
        // Ensure valid index
        if(selectedIndex >= 0)
        {
            if(selectedIndex == waypoints.Count - 1 && selectedIndex != 0) selectedIndex--;

            if(straightenedSegments.Contains(selectedIndex)) straightenedSegments.Remove(selectedIndex);
            else straightenedSegments.Add(selectedIndex);
        }

        /*string paste = "";
        foreach(int index in straightenedSegments) paste += " " + index.ToString();
        Debug.Log(paste);//*/
    }

    public void StraightenSegment(PathCreator creator, int index)
    {
        if(index + 1 < waypoints.Count)
        {
            Vector3 curAnchor = waypoints[index].transform.position;
            Vector3 nextAnchor = waypoints[index + 1].transform.position;
            
            // Pattern in BezierPath goes: Anchor, Control, Control. 
            // Adjust index from waypoints(anchors) to get control
            int curControlIndex = (index * 3) + 1;
            int nextControlIndex = curControlIndex + 1; 

            // Make the control points point to the opposing anchor point 
            Vector3 curControlStraightened = (nextAnchor - curAnchor).normalized + curAnchor;
            Vector3 nextControlStraightened = (curAnchor - nextAnchor).normalized + nextAnchor;
            
            creator.bezierPath.SetPoint(curControlIndex, curControlStraightened);
            creator.bezierPath.SetPoint(nextControlIndex, nextControlStraightened);
        }
    }
    /*
    void DrawNormals()
    {
        int rendPoints = 0;
        
        normalRend.positionCount = buildingCreator.path.localNormals.Length * 3;

        for(int i = 0; i < buildingCreator.path.localNormals.Length; i++)
        {
            Vector3 curPoint = buildingCreator.path.localPoints[i];
            Vector3 curNorm = buildingCreator.path.localNormals[i];
            normalRend.SetPosition(rendPoints, curPoint);
            rendPoints++;
            normalRend.SetPosition(rendPoints, curPoint + curNorm);
            rendPoints++;
            normalRend.SetPosition(rendPoints, curPoint);
            rendPoints++;
        }
    }
    */

    /*
    // Waypoint Functions
    */
    public void ClearWaypoints()
    {
        foreach(GameObject waypoint in waypoints)
        {
            Destroy(waypoint);
        }

        waypoints.Clear();
    }

    public void AddWaypoint()
    {
        // Instantiate a new waypoint at the adjusted position
        GameObject newWaypoint = Instantiate(waypointPrefab);

        // Determine spawn point
        if(numSpawned % 5 == 0) 
        {
            waypointSpawn.x = -10;
            waypointSpawn.z = (numSpawned / 5) * -5;
        }
        else waypointSpawn.x += 5;
        waypointSpawn.y = heightAboveTerrain + Terrain.activeTerrain.SampleHeight(waypointSpawn);
        numSpawned++;
        
        newWaypoint.transform.position = waypointSpawn;

        waypoints.Add(newWaypoint);

        SelectWaypoint(newWaypoint);

        if(CameraMovement.topdownMode) StartCoroutine(TrackMousePosition(newWaypoint));

        //if(waypoints.Count == 2) StartCoroutine(DrawBuiltPath(buildingCreator));
    }

    IEnumerator TrackMousePosition(GameObject obj)
    {
        while(!Input.GetMouseButtonDown(0))
        {
            if(CameraMovement.topdownMode) 
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // Reference: John French, Jan 22 2020
                // https://gamedevbeginner.com/how-to-convert-the-mouse-position-to-world-space-in-unity-2d-3d/#screen_to_world_3d
                RaycastHit hit;
                if(terrainCollider.Raycast(ray, out hit, 1000))
                {
                    Vector3 terrainMousePos = hit.point + new Vector3(0, heightAboveTerrain, 0);
                    obj.transform.position = hit.point;
                }
            }

            yield return null;
        }
    }

    public void SelectWaypoint(GameObject newSelectedWaypoint)
    {
        if(selectedWaypoint) selectedWaypoint.GetComponent<WaypointUtil>().selected = false;
        newSelectedWaypoint.GetComponent<WaypointUtil>().selected = true;
        selectedWaypoint = newSelectedWaypoint;

        // Show remove button for selected waypoint
        removeButton.SetActive(true);
    }

    public void DeleteWaypoint()
    {
        // Hide remove button since unselecting
        removeButton.SetActive(false);

        // Update waypoint labels
        for(int i = GetWaypointIndex(selectedWaypoint); i < waypoints.Count; i++)
        {
            waypoints[i].GetComponent<WaypointUtil>().SetLabel();
        }

        // Remove from straightened segments list if applicable
        int indexSelected = GetWaypointIndex(selectedWaypoint);
        if(straightenedSegments.Contains(indexSelected - 1)) straightenedSegments.Remove(indexSelected - 1);
        if(straightenedSegments.Contains(indexSelected)) straightenedSegments.Remove(indexSelected);

        waypoints.Remove(selectedWaypoint);

        Destroy(selectedWaypoint);

        /*if(waypoints.Count < 2) 
        {
            //Debug.Log("Stopping path draw");
            StopCoroutine(DrawBuiltPath(buildingCreator));
            HideBuiltPath();
        }*/
    }

    public int GetWaypointIndex(GameObject waypoint)
    {
        return waypoints.IndexOf(waypoint);
    }
}
