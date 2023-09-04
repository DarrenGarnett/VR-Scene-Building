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
    private bool editingPathName = false;

    private GameObject selectedWaypoint;
    
    void Start()
    {
        updatePaths();
        updateDropdown();
        //setCurPath();

        buildingPath = new GameObject("BuildingPath");

        GameObject parent = GameObject.FindGameObjectWithTag("PathParent");
        buildingPath.transform.parent = parent.transform;
        buildingCreator = buildingPath.AddComponent<PathCreator>();
        buildingCreator.bezierPath.GlobalNormalsAngle = 90;

        normalRend = gameObject.GetComponent<LineRenderer>();
        normalRend.startWidth = 0.05f;
        normalRend.endWidth = 0.05f;

        waypointSpawn = new Vector3(-10, 0, 0);
        waypointSpawn.y = heightAboveTerrain + Terrain.activeTerrain.SampleHeight(waypointSpawn);
        
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
        clearEditNodes();

        curPath = curPath = paths[pathDropdown.value];
        //Debug.Log("Cur path: " + curPath);

        curPath.GetComponent<PathCreator>().DrawPathEdit();
    }

    public void editMode()
    {
        updateDropdown();
        setCurPath();
        normalRend.positionCount = 0;
        buildingCreator.ClearPath();

        inPathEdit = true;
        inPathCreation = false;
        //Debug.Log("Editing Path...");
    }

    public void createMode()
    {
        curPath.GetComponent<PathCreator>().ClearPath();
        clearEditNodes();

        pathNameInput.text = "Path" + paths.Count;

        inPathEdit = false;
        inPathCreation = true;
        //Debug.Log("Creating Path...");
    }

    public void exitMode()
    {
        curPath.GetComponent<PathCreator>().ClearPath();
        clearEditNodes();

        inPathEdit = false;
        inPathCreation = false;
        //Debug.Log("Exiting Path Edit...");
    }

    public void clearEditNodes()
    {
        GameObject[] nodes = GameObject.FindGameObjectsWithTag("PathEdit");
        foreach(GameObject node in nodes) Destroy(node);
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

        clearWaypoints();
    }

    public void clearWaypoints()
    {
        //GameObject[] waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
        waypoints.Clear();

        foreach(GameObject waypoint in waypoints)
        {
            Destroy(waypoint);
        }
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

        /*StartCoroutine(TrackMousePosition(newWaypoint));

        WayPointUtil wayPointUtil = newWaypoint.GetComponent<WayPointUtil>();
        
        if(CameraMovement.topdownMode) wayPointUtil.Show2DAxes();
        else wayPointUtil.Show3DAxes();*/

    }

    IEnumerator TrackMousePosition(GameObject obj)
    {
        while(!Input.GetMouseButtonDown(0))
        {
            if(CameraMovement.topdownMode) 
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;
                if(Physics.Raycast(ray, out hit))
                {
                    obj.transform.position = hit.point;
                    //Debug.Log("Distance from hit: " + hit.distance);
                    float waypointSize = hit.distance / 10;
                    obj.transform.localScale = new Vector3(waypointSize, waypointSize, waypointSize);
                }
            }

            yield return null;
        }
    }

    public void SelectWaypoint(GameObject newSelectedWaypoint)
    {
        /*// Clear previous axes if needed
        if(selectedWaypoint) selectedWaypoint.GetComponent<WayPointUtil>().hideAxes();
        
        // Set current
        if(CameraMovement.topdownMode) wayPointUtil.Show2DAxes();
        else wayPointUtil.Show3DAxes();
        
        selectedWaypoint = selected;*/
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

        waypoints.Remove(selectedWaypoint);

        Destroy(selectedWaypoint);
    }

    public int GetWaypointIndex(GameObject waypoint)
    {
        return waypoints.IndexOf(waypoint);
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
    }
}
