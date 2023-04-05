using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCameraScript : MonoBehaviour
{
    // Boolean to tell if main camera is active
    bool mainCameraActive;

    // List of all models on paths
    public List<GameObject> models = new List<GameObject>();

    private int size = 0, initSize = 0;

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

        //Get any target objects within the scene objects before runtime with Target tag
        GameObject[] modelsArray = GameObject.FindGameObjectsWithTag("Target");
        foreach(GameObject obj in modelsArray) models.Add(obj);
        size = models.Count;
        initSize = size;

        mainCameraControls = GameObject.FindGameObjectWithTag("MainCameraMovement");
        modelsIdx = 0;
        //CameraFollow.target = models[modelsIdx];
        CameraFollow.target = null;
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
            CameraFollow.target = models[modelsIdx];
            mainCameraControls.SetActive(false);
            CameraMovement.drag = false;
        }
        /*
         * Case where followCamera is using the last model in models;
         * set modelIdx to 0, deactivate followCamera, and activate mainCamera
         * also show the main camera controls
         */
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
            CameraFollow.target = models[modelsIdx];
        }
    }

    public void ChangeTarget(GameObject targetObj)
    {
        mainCamera.enabled = false;
        mainCameraActive = false;
        followCamera.enabled = true;
        CameraFollow.target = targetObj;
        mainCameraControls.SetActive(false);
        CameraMovement.drag = false;
    }

    public void addTarget(GameObject targetObj)
    {
        models.Add(targetObj);
        size = models.Count;
    }

    public void removeTarget(GameObject targetObj)
    {
        models.Remove(targetObj);
        size = models.Count;
    }

    public void clearTargets()
    {
        //models.Clear();
        models.RemoveRange(initSize, size - initSize);
        size = initSize;
    }
}
