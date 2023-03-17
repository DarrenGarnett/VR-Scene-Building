using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCameraScript : MonoBehaviour
{
    // Boolean to tell if main camera is active
    bool mainCameraActive;

    // List of all models on paths
    public List<GameObject> models = new List<GameObject>();

    private int size = 0;

    // Int to track position in models list
    int modelsIdx;

    // Main camera and follow camera
    public Camera mainCamera, followCamera;

    // UI element to control the movement of the main camera
    private GameObject mainCameraControls;

    private void Start()
    {
        mainCameraActive = true;
        mainCamera.enabled = true;
        followCamera.enabled = false;

        GameObject[] modelsArray = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject obj in modelsArray) models.Add(obj);
        size = models.Count;

        mainCameraControls = GameObject.FindGameObjectWithTag("MainCameraMovement");
        modelsIdx = 0;
        CameraFollow.target = models[modelsIdx].transform;
    }

    private void Update()
    {
        GameObject[] modelsArray = GameObject.FindGameObjectsWithTag("Player");
        if(size != modelsArray.Length)
        {
            //Debug.Log("Models size changed.");
            models.Clear();
            foreach(GameObject obj in modelsArray) models.Add(obj);
            size = models.Count;
        }
    }

    public void ChangeCamera()
    {
        /*
         * Case where main camera is active;
         * deactivate and switch to followCamera,
         * and hide the main camera controls
         */
        if(mainCameraActive)
        {
            mainCamera.enabled = false;
            mainCameraActive = false;
            followCamera.enabled = true;
            CameraFollow.target = models[modelsIdx].transform;
            mainCameraControls.SetActive(false);
            CameraMovement.drag = false;
        }
        /*
         * Case where followCamera is using the last model in models;
         * set modelIdx to 0, deactivate followCamera, and activate mainCamera
         * also show the main camera controls
         */
        //else if(modelsIdx == models.Length - 1)
        else if(modelsIdx == size - 1)
        {
            modelsIdx = 0;
            followCamera.enabled = false;
            mainCamera.enabled = true;
            mainCameraActive = true;
            mainCameraControls.SetActive(true);
            CameraMovement.drag = true;
        }
        /*
         * All other cases;
         * increment modelsIdx by 1 and set the target of followCamera to be the next model's transform
         */
        else
        {
            modelsIdx++;
            CameraFollow.target = models[modelsIdx].transform;
        }
    }
}
