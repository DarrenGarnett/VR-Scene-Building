using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PathCreation;

public class PathManager : MonoBehaviour
{
    
    public bool inPathCreation = false;
    public bool inPathEdit = false;
    
    public GameObject waypointPrefab;
    public static List<GameObject> paths = new List<GameObject>();
    public List<GameObject> waypoints = new List<GameObject>();
    private Vector3 waypointSpawn;
    private int numSpawned;

    private GameObject curPath;
    private GameObject buildingPath;
    private PathCreator buildingCreator;
    public float heightAboveTerrain = 0.25f;
    private LineRenderer normalRend;

    public TMP_Dropdown pathDropdown;
    public TMP_InputField pathNameInput;
    public GameObject removeButton;
    public ToggleButton straightenButton;

    private GameObject selectedWaypoint;
    private TerrainCollider terrainCollider;

    bool editingPathName = false;

    public bool snapToGround = false;
    List<int> straightenedSegments = new List<int>();
    
    void Start()
    {
        updatePaths();
        updateDropdown();
        //setCurPath();

        buildingPath = new GameObject("BuildingPath");

        GameObject parent = GameObject.FindGameObjectWithTag("PathParent");
        buildingPath.transform.parent = parent.transform;
        buildingCreator = buildingPath.AddComponent<PathCreator>();
        buildingCreator.bezierPath.DeleteSegment(0);
        buildingCreator.bezierPath.DeleteSegment(1);
        buildingCreator.bezierPath.GlobalNormalsAngle = 90;

        normalRend = gameObject.GetComponent<LineRenderer>();
        normalRend.startWidth = 0.05f;
        normalRend.endWidth = 0.05f;

        waypointSpawn = new Vector3(-10, 0, 0);
        waypointSpawn.y = heightAboveTerrain + Terrain.activeTerrain.SampleHeight(waypointSpawn);
        
        terrainCollider = Terrain.activeTerrain.GetComponent<TerrainCollider>();
    }

    void Update()
    {
        /*if(CameraMovement.topdownMode)
        {
            if((Input.GetMouseButtonDown(0) && inPathCreation) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;

                if(Physics.Raycast(ray, out hit))
                {
                    // Add the offset to the hit point's y-coordinate
                    Vector3 waypointPosition = hit.point + new Vector3(0, heightAboveTerrain, 0);
                    //Debug.Log("Waypoint at: " + waypointPosition);

                    // Instantiate a waypoint at the adjusted position
                    GameObject newWaypoint = Instantiate(waypoint, waypointPosition, Quaternion.identity);

                    waypoint.tag = "Waypoint";
                    p.Add(newWaypoint);

                    //buildingCreator.bezierPath.AddSegmentToEnd(waypointPosition);

                    if(p.Count == 2)
                    {
                        buildingCreator.bezierPath.DeleteSegment(0);
                        buildingCreator.bezierPath.DeleteSegment(1);
                    }

                    if(p.Count >= 2) 
                    {
                        List<Vector3> anchorPoints = new List<Vector3>();
                        foreach(GameObject waypoint in p) anchorPoints.Add(waypoint.transform.position);
                        buildingCreator.bezierPath = new BezierPath(anchorPoints);

                        int rendPoints = 0;
                        normalRend.positionCount = buildingCreator.path.localNormals.Length * 3;

                        for(int i = 0; i < buildingCreator.path.localNormals.Length; i++)
                        {
                            buildingCreator.path.localPoints[i].y = Terrain.activeTerrain.SampleHeight(buildingCreator.path.localPoints[i]) + heightAboveTerrain;
                    
                            Vector3 dir;
                            if(i == 0) dir = buildingCreator.path.localPoints[i] - buildingCreator.path.localPoints[i + 1];
                            else dir = buildingCreator.path.localPoints[i] - buildingCreator.path.localPoints[i - 1];
                            //buildingCreator.path.localNormals[i] = Vector3.Cross(dir, Vector3.right).normalized;

                            Vector3 curPoint = buildingCreator.path.localPoints[i];
                            Vector3 curNorm = buildingCreator.path.localNormals[i];
                            normalRend.SetPosition(rendPoints, curPoint);
                            rendPoints++;
                            normalRend.SetPosition(rendPoints, curPoint + curNorm);
                            rendPoints++;
                            normalRend.SetPosition(rendPoints, curPoint);
                            rendPoints++;
                        }

                        buildingCreator.DrawPathEdit();
                    }

                    //p.Add(waypoint);

                    //GameObject[] waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
                }
            }
        }*/

        // Block camera movement while in the path name text box
        if(pathNameInput.isFocused)
        {
            //Debug.Log("In path name input.");
            editingPathName = true;
            CameraMovement.lockMovement = true;
        }

        // Allow camera movement once out of the path name text box
        if(editingPathName != pathNameInput.isFocused)
        {
            //Debug.Log("No longer editing path name.");
            editingPathName = false;
            CameraMovement.lockMovement = false;
        }

        // Manage straighten button display
        if(inPathCreation && waypoints.Count > 0)
        {
            int curIndex = GetWaypointIndex(selectedWaypoint);
            if(curIndex == waypoints.Count - 1 && curIndex != 0) curIndex--;

            if(straightenedSegments.Contains(curIndex)) straightenButton.SetOn();
            else straightenButton.SetOff();
        }
    }

    void SnapPath()
    {
        foreach(GameObject waypoint in waypoints)
        {
            Vector3 groundPos = new Vector3(waypoint.transform.position.x, Terrain.activeTerrain.SampleHeight(waypoint.transform.position) + heightAboveTerrain, waypoint.transform.position.z);
            waypoint.transform.position = groundPos;
        }

        for(int i = 0; i < buildingCreator.path.localNormals.Length; i++)
        {
            buildingCreator.path.localPoints[i].y = Terrain.activeTerrain.SampleHeight(buildingCreator.path.localPoints[i]) + heightAboveTerrain;
        
            Vector3 dir;
            if(i == 0) dir = buildingCreator.path.localPoints[i] - buildingCreator.path.localPoints[i + 1];
            else dir = buildingCreator.path.localPoints[i] - buildingCreator.path.localPoints[i - 1];
            //buildingCreator.path.localNormals[i] = Vector3.Cross(dir, Vector3.right).normalized;
        }
    }

    public void TogglePathSnapped()
    {
        snapToGround = !snapToGround;
    }

    void StraightenPath()
    {
        // Reference: bronxbomber92, Nov 11 2006
        // https://forum.unity.com/threads/math-problem.8114/#post-59715
        Vector3 vA = waypoints[0].transform.position;
        Vector3 vB = waypoints[waypoints.Count - 1].transform.position;

        Vector3 lineVec = vB - vA;
        
        Vector3 v2 = (vB - vA).normalized;

        Vector3 linePos;

        //could also use GetClosestPointOnPath() from VetexPath?
        foreach(GameObject waypoint in waypoints)
        {
            Vector3 v1 = waypoint.transform.position - vA;
            float dist = Vector3.Distance(vA, vB);
            float dot = Vector3.Dot(v2, v1);

            if(dot <= 0) linePos = vA;
            else if(dot >= dist) linePos = vB;
            else
            {
                Vector3 v3 = v2 * dot;
                linePos = vA + v3;
            }

            waypoint.transform.position = linePos;
        }

        for(int i = 0; i < buildingCreator.path.localPoints.Length; i++)
        {
            buildingCreator.path.localPoints[i] = lineVec * (i / buildingCreator.path.localPoints.Length);
        }
    }

    public void ToggleSegmentStraight()
    {
        int selectedIndex = GetWaypointIndex(selectedWaypoint);
        
        // Ensure valid index
        if(selectedIndex >= 0)
        {
            if(selectedIndex == waypoints.Count - 1 && selectedIndex != 0) selectedIndex--;

            if(straightenedSegments.Contains(selectedIndex))
            {
                //Debug.Log("removing segment " + selectedIndex);
                straightenedSegments.Remove(selectedIndex);
            }
            else
            {
                //Debug.Log("adding segment " + selectedIndex);
                straightenedSegments.Add(selectedIndex);
            }
        }

        /*string paste = "";
        foreach(int index in straightenedSegments) paste += " " + index.ToString();
        Debug.Log(paste);//*/
    }

    public void StraightenSegment(int index)
    {
        //int curIndex = waypoints.IndexOf(selectedWaypoint);
        //if(index == waypoints.Count - 1) index--;

        //Vector3[] segmentPoints = buildingCreator.path.GetPointsInSegment(controlIndex);
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

            //buildingCreator.bezierPath.controlMode = BezierPath.ControlMode.Free;
            //Debug.Log(buildingCreator.bezierPath.controlMode);
            
            buildingCreator.bezierPath.SetPoint(curControlIndex, curControlStraightened);
            buildingCreator.bezierPath.SetPoint(nextControlIndex, nextControlStraightened);

            //Instantiate(waypointPrefab, curControlStraightened, Quaternion.identity);
            //Instantiate(waypointPrefab, nextControlStraightened, Quaternion.identity);
        }
    }

    IEnumerator DrawBuiltPath()
    {
        //Debug.Log("Constructed path display on.");

        while(inPathCreation)
        {
            //Debug.Log(waypoints.Count);
            if(waypoints.Count >= 2)
            {
                List<Vector3> anchorPoints = new List<Vector3>();
                foreach(GameObject waypoint in waypoints) anchorPoints.Add(waypoint.transform.position);
                buildingCreator.bezierPath = new BezierPath(anchorPoints);

                // Apply snapping and straigtening while toggled
                if(snapToGround) SnapPath();
                foreach(int segmentIndex in straightenedSegments) StraightenSegment(segmentIndex);

                int rendPoints = 0;
                normalRend.positionCount = buildingCreator.path.localNormals.Length * 3;

                for(int i = 0; i < buildingCreator.path.localNormals.Length; i++)
                {
                    /*buildingCreator.path.localPoints[i].y = Terrain.activeTerrain.SampleHeight(buildingCreator.path.localPoints[i]) + heightAboveTerrain;
            
                    Vector3 dir;
                    if(i == 0) dir = buildingCreator.path.localPoints[i] - buildingCreator.path.localPoints[i + 1];
                    else dir = buildingCreator.path.localPoints[i] - buildingCreator.path.localPoints[i - 1];
                    //buildingCreator.path.localNormals[i] = Vector3.Cross(dir, Vector3.right).normalized;*/

                    Vector3 curPoint = buildingCreator.path.localPoints[i];
                    Vector3 curNorm = buildingCreator.path.localNormals[i];
                    normalRend.SetPosition(rendPoints, curPoint);
                    rendPoints++;
                    normalRend.SetPosition(rendPoints, curPoint + curNorm);
                    rendPoints++;
                    normalRend.SetPosition(rendPoints, curPoint);
                    rendPoints++;
                }

                buildingCreator.DrawPathEdit();
            }

            

            yield return null; 
        }
    }

    void HideBuiltPath()
    {
        normalRend.positionCount = 0;
        buildingCreator.ClearPath();
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
        paths = Utility.GetChildren(GameObject.FindGameObjectWithTag("PathParent"));
        //foreach(GameObject path in paths) Debug.Log(path.name);
    }

    public void setCurPath()
    {
        if(curPath) curPath.GetComponent<PathCreator>().ClearPath();
        //clearEditNodes();

        curPath = curPath = paths[pathDropdown.value];
        //Debug.Log("Cur path: " + curPath);

        //curPath.GetComponent<PathCreator>().DrawPath();
        curPath.GetComponent<PathCreator>().DrawPathEdit();
    }

    public void editMode()
    {
        updateDropdown();
        setCurPath();
        normalRend.positionCount = 0;
        buildingCreator.ClearPath();
        ClearWaypoints();

        inPathEdit = true;
        inPathCreation = false;
        //Debug.Log("Editing Path...");
    }

    public void createMode()
    {
        curPath.GetComponent<PathCreator>().ClearPath();
        //clearEditNodes();

        pathNameInput.text = "Path" + paths.Count;

        inPathEdit = false;
        inPathCreation = true;
        //Debug.Log("Creating Path...");
    }

    public void exitMode()
    {
        curPath.GetComponent<PathCreator>().ClearPath();
        //clearEditNodes();

        inPathEdit = false;
        inPathCreation = false;
        //Debug.Log("Exiting Path Edit...");
    }

    public void clearEditNodes()
    {
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("PathEdit");
        foreach(GameObject node in nodes) Destroy(node);
    }

    public void SavePath()
    {

    }

    public void buildNewPath()
    {
        //GameObject[] waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
        
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

        for(int i = 0; i < creator.path.localPoints.Length; i++) 
        {
            /*RaycastHit hit;
            if(Physics.Raycast(creator.path.localPoints[i], Vector3.down, out hit, Mathf.Infinity))
            {
                //Debug.Log("Ray hit below");
                creator.path.localPoints[i] = hit.point + new Vector3(0, heightAboveTerrain, 0);
            }
            else if(Physics.Raycast(creator.path.localPoints[i], Vector3.up, out hit, Mathf.Infinity))
            {
                Debug.Log("Ray hit above");
                creator.path.localPoints[i] = hit.point + new Vector3(0, heightAboveTerrain, 0);
            }
            else 
            {
                Debug.LogError("Path Leveling Ray Missed. ");
            }*/

            creator.path.localPoints[i].y = Terrain.activeTerrain.SampleHeight(creator.path.localPoints[i]) + heightAboveTerrain;
            //Debug.Log(Terrain.activeTerrain.terrainData.GetInterpolatedNormal(Vector3.Normalize(creator.path.localPoints[i]).x, Vector3.Normalize(creator.path.localPoints[i]).z));
            //creator.path.localNormals[i] = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(creator.path.localPoints[i].x, creator.path.localPoints[i].z);
            //Vector3 v1, v2;
            //Vector3 right = new Vector3(creator.path.localPoints[i].x, Terrain.activeTerrain.SampleHeight(creator.path.localPoints[i] + new Vector3(0.1, 0, 0), creator.path.localPoints[i].z));
            //v1 = 
            Vector3 dir;
            if(i == 0) dir = creator.path.localPoints[i] - creator.path.localPoints[i + 1];
            else dir = creator.path.localPoints[i] - creator.path.localPoints[i - 1];
            Vector3 right = Vector3.Cross(dir, Vector3.up);
            Vector3 dir2 = right + creator.path.localPoints[i];
            float temp = Terrain.activeTerrain.SampleHeight(dir2);
            right.y = temp;
            //Vector3 dir2 = creator.path.localPoints[i] - new Vector3(right.x, Terrain.activeTerrain.SampleHeight(creator.path.localPoints[i] + right), right.z);
            //creator.path.localNormals[i] = Vector3.Cross(dir, Vector3.right).normalized;
            //creator.path.localNormals[i] = Vector3.Cross(dir, right).normalized;

            //Debug.Log(creator.path.localPoints[i]);
        }

        /*for(int i = 0; i < creator.bezierPath.NumPoints; i++)
        {
            Debug.Log(Terrain.activeTerrain.SampleHeight(creator.bezierPath.GetPoint(i)));
            Vector3 curPoint = creator.bezierPath.GetPoint(i);
            creator.bezierPath.SetPoint(i, new Vector3(curPoint.x, Terrain.activeTerrain.SampleHeight(curPoint) + heightAboveTerrain, curPoint.z));
        }*/

        ClearWaypoints();
    }

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

        if(waypoints.Count == 2) StartCoroutine(DrawBuiltPath());
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

        if(waypoints.Count < 2) 
        {
            //Debug.Log("Stopping path draw");
            StopCoroutine(DrawBuiltPath());
            HideBuiltPath();
        }
    }

    public int GetWaypointIndex(GameObject waypoint)
    {
        return waypoints.IndexOf(waypoint);
    }
}
