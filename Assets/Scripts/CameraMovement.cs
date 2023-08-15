using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float panSpeed = 5f;
    public float moveSpeed = 1f;
    // Need this to be accessible in switch camera script
    public static bool drag = true;
    //private Vector3 origin;
    //private Vector3 diff;
    bool forw = false;
    bool back = false;
    bool left = false;
    bool right = false;

    private Vector3 initPos;
    private Vector3 curPos;
    private Quaternion initRot;

    private GameObject UI;

    // Start is called before the first frame update
    void Start()
    {
        // Save the initial position and rotation of the camera
        initPos = this.transform.position;
        curPos = initPos;
        initRot = this.transform.rotation;

        // Get the UI to reference its position
        UI = GameObject.FindGameObjectWithTag("UIElement");
    }

    void FixedUpdate()
    {
        if(Input.GetMouseButton(0) || Input.GetMouseButtonDown(0)){
            if(forw){
                // this.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 0, speed));
                this.transform.position += new Vector3(0, 0, (moveSpeed * Time.deltaTime));
            }
            else if(back){
                // this.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 0, -1 * speed));
                this.transform.position += new Vector3(0, 0, (-1 * moveSpeed * Time.deltaTime));
            }
            else if(left){
                // this.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(-1 * speed, 0, 0));
                this.transform.position += new Vector3((-1 * moveSpeed * Time.deltaTime), 0, 0);
            }
            else if(right){
                //Debug.Log("Moving Right");
                // this.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(speed, 0, 0));
                this.transform.position += new Vector3((moveSpeed * Time.deltaTime), 0, 0);
            }
            else if(drag){
                //Debug.Log("Dragging");
                this.transform.eulerAngles += panSpeed * new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
            }
        }
        
        //this.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(speed * Input.GetAxis("Horizontal"), 0, speed * Input.GetAxis("Vertical")));
        curPos += new Vector3(moveSpeed * Input.GetAxis("Horizontal"), moveSpeed * (Convert.ToInt32(Input.GetKey("space")) - Convert.ToInt32(Input.GetKey(KeyCode.LeftShift))), moveSpeed * Input.GetAxis("Vertical"));
        this.GetComponent<Rigidbody>().MovePosition(curPos);
        forw = false;
        back = false;
        left = false;
        right = false;
    }

    public void Forward(){
        forw = true;
    }

    public void Backward(){
        back = true;
    }

    public void Left(){
        left = true;
    }

    public void Right(){
        right = true;
    }

    public void ResetCamera()
    {
        this.transform.position = initPos;
        this.transform.rotation = initRot;
    }
}
