using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Make the camera's target to follow available to other scripts
    //public static Transform target;
    public static GameObject target;

    public float smoothSpeed = 0.125f;
    public static Vector3 locationOffset;
    public static Vector3 rotationOffset;

    public float zoom = 1f;
    public float zoomStep = 0.25f;

    void Update()
    {
        if(target != null)
        {
            //Debug.Log(target.name);

            Renderer rend = target.GetComponent<Renderer>();
            if(rend != null)
            {
                //get the facing direction for each axis
                Vector3 directions = target.transform.forward + target.transform.right + target.transform.up;
                //Debug.Log("Target bounds: " + rend.bounds.size);

                //zoom by scroll input, with lower bound 1
                zoom -= Input.mouseScrollDelta.y * zoomStep;
                if(zoom < 1f) zoom = 1f;

                //get camera offset for the top, back, left corner, regardless of orientation
                locationOffset = rend.bounds.extents * 2f;
                locationOffset.x *= zoom * directions.x;
                locationOffset.y *= zoom * directions.y;
                locationOffset.z *= zoom * directions.z;

                //rotate to face targeted object better
                rotationOffset.x = 20;
                rotationOffset.y = 210;
                rotationOffset.z = 0;
            }
            else 
            {
                locationOffset = new Vector3(0, 0, 0);
                rotationOffset = new Vector3(0, 0, 0);
            }

            Vector3 desiredPosition = target.transform.position + locationOffset;// + target.transform.rotation * locationOffset;
            //Debug.Log("Camera view offset: " + locationOffset);
            //Debug.Log("Target base position: " + target.transform.position);
            //Debug.Log("Camera desired position: " + desiredPosition);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            Quaternion desiredrotation = target.transform.rotation * Quaternion.Euler(rotationOffset);
            Quaternion smoothedrotation = Quaternion.Lerp(transform.rotation, desiredrotation, smoothSpeed);
            transform.rotation = smoothedrotation;
        }
    }
}
