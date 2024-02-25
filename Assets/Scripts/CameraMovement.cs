using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CameraMovement : MonoBehaviour
{
    public float panSpeed = 5f;
    public float moveSpeed = 1f;

    private Vector3 initPos;
    private Vector3 curPos;
    private Quaternion initRot;

    private Rigidbody cameraBody;

    public static bool lockMovement = false;
    public bool cameraStaysLevel = true;

    public static bool topdownMode = false;
    public TextMeshProUGUI topdownButtonText;

    private float zoomOffset = 0;

    public static bool disableDrag = false;
    private Vector3 curRotation;

    void Start()
    {
        // Save the initial position and rotation of the camera
        initPos = this.transform.position;
        curPos = initPos;
        initRot = this.transform.rotation;

        cameraBody = this.GetComponent<Rigidbody>();

        topdownButtonText.text = "to 2D";

        curRotation = Vector3.zero;
    }

    void FixedUpdate()
    {
        if(!lockMovement)
        {
            if(topdownMode)
            {
                // Handle axis input
                Vector3 forwardMovement = (moveSpeed * Input.GetAxis("Vertical")) * transform.up;
                Vector3 lateralMovement = (moveSpeed * Input.GetAxis("Horizontal")) * transform.right;

                if(cameraStaysLevel)
                {
                    forwardMovement.y = 0;
                    lateralMovement.y = 0;
                }
                
                Vector3 verticalMovement = new Vector3(0, moveSpeed * (Convert.ToInt32(Input.GetKey("space")) - Convert.ToInt32(Input.GetKey(KeyCode.LeftShift))), 0);
                zoomOffset += verticalMovement.y;
                
                curPos += forwardMovement + lateralMovement + verticalMovement;
            }
            else
            {
                // Drag screen to rotate if not disabled or over a UI element
                if((Input.GetMouseButton(0) || Input.GetMouseButtonDown(0)) && (!disableDrag && !EventSystem.current.IsPointerOverGameObject()))
                {
                    this.transform.eulerAngles += panSpeed * new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
                    curRotation = this.transform.eulerAngles;
                }

                // Handle axis input
                Vector3 forwardMovement = (moveSpeed * Input.GetAxis("Vertical")) * transform.forward;
                Vector3 lateralMovement = (moveSpeed * Input.GetAxis("Horizontal")) * transform.right;

                if(cameraStaysLevel)
                {
                    forwardMovement.y = 0;
                    lateralMovement.y = 0;
                }
                
                Vector3 verticalMovement = new Vector3(0, moveSpeed * (Convert.ToInt32(Input.GetKey("space")) - Convert.ToInt32(Input.GetKey(KeyCode.LeftShift))), 0);
                curPos += forwardMovement + lateralMovement + verticalMovement;
            }
        }

        cameraBody.MovePosition(curPos);
    }

    public void ResetCamera()
    {
        curPos = initPos;
        this.transform.position = initPos;
        this.transform.rotation = initRot;

        topdownMode = false;
        topdownButtonText.text = "to 2D";
    }

    public static void SetLock(bool val)
    {
        lockMovement = val;
    }

    public void ToggleTopdown()
    {
        if(topdownMode)
        {
            // Revert topdown transformations
            transform.eulerAngles = curRotation;
            curPos += new Vector3(0, -40 - zoomOffset, -40 - zoomOffset);
            topdownButtonText.text = "to 2D";
        }
        else 
        {
            // Apply topdown transformations
            transform.eulerAngles = new Vector3(90, 0, 0);
            curPos += new Vector3(0, 40 + zoomOffset, 40 + zoomOffset);
            topdownButtonText.text = "to 3D";
        }

        topdownMode = !topdownMode;
    }
}
