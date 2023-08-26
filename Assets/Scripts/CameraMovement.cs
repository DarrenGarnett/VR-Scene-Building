using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float panSpeed = 5f;
    public float moveSpeed = 1f;

    private Vector3 initPos;
    private Vector3 curPos;
    private Quaternion initRot;

    public static bool lockMovement = false;

    void Start()
    {
        // Save the initial position and rotation of the camera
        initPos = this.transform.position;
        curPos = initPos;
        initRot = this.transform.rotation;
    }

    void FixedUpdate()
    {
        if(!lockMovement)
        {
            // Handle click dragging
            if(Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))
            {
                Debug.Log("Dragging...");
                this.transform.eulerAngles += panSpeed * new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
            }
            
            // Handle axis input
            if((Input.GetAxis("Horizontal") != 0) || (Input.GetAxis("Vertical") != 0)) Debug.Log("Moving...");

            float xinput = moveSpeed * Input.GetAxis("Horizontal");
            float yinput = moveSpeed * (Convert.ToInt32(Input.GetKey("space")) - Convert.ToInt32(Input.GetKey(KeyCode.LeftShift)));
            float zinput = moveSpeed * Input.GetAxis("Vertical");
            curPos += new Vector3(xinput, yinput, zinput);

            this.GetComponent<Rigidbody>().MovePosition(curPos);
            //this.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(speed * Input.GetAxis("Horizontal"), 0, speed * Input.GetAxis("Vertical")));
        }
    }

    public void ResetCamera()
    {
        curPos = initPos;
        this.transform.position = initPos;
        this.transform.rotation = initRot;
    }

    public static void SetLock(bool val)
    {
        lockMovement = val;
    }
}
