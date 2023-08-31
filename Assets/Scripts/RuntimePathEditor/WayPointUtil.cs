using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointUtil : MonoBehaviour
{
    public GameObject pathManagerObj;
    private PathManager pathManager;

    GameObject xaxis, yaxis, zaxis;
    AxisUtil xutil, yutil, zutil;
    public bool selected = false;
    
    public float waypointDistanceScaleFactor = 0.1f;
    private Vector3 initScale;

    void Awake()
    {
        List<GameObject> axes = Utility.GetChildren(gameObject);
        xaxis = axes[0];
        yaxis = axes[1];
        zaxis = axes[2];

        xutil = xaxis.GetComponent<AxisUtil>();
        yutil = yaxis.GetComponent<AxisUtil>();
        zutil = zaxis.GetComponent<AxisUtil>();

        pathManager = pathManagerObj.GetComponent<PathManager>();

        initScale = transform.localScale;
    }

    void FixedUpdate()
    {
        if(selected)
        {
            if(CameraMovement.topdownMode) Show2DAxes();
            else Show3DAxes();
        }
        else HideAxes();

        // Scale waypoint relative to distance from the camera
        transform.localScale = initScale * (Vector3.Distance(Camera.main.transform.position, transform.position) * waypointDistanceScaleFactor);
    }

    void OnMouseDown()
    {
        pathManager.SelectWaypoint(gameObject);
    }

    public void Show2DAxes()
    {
        //Debug.Log("2 axis display.");
        xaxis.SetActive(true);
        yaxis.SetActive(false);
        zaxis.SetActive(true);
    }

    public void Show3DAxes()
    {
        //Debug.Log("3 axis display.");
        xaxis.SetActive(true);
        yaxis.SetActive(true);
        zaxis.SetActive(true);
    }

    public void HideAxes()
    {
        //Debug.Log("no axis display.");
        xaxis.SetActive(false);
        yaxis.SetActive(false);
        zaxis.SetActive(false);
    }
}
