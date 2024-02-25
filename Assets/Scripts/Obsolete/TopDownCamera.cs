using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    public float scrollSpeed = 1f;
    public float zoomSpeed = 1f;

    void FixedUpdate()
    {
        float Xin = Input.GetAxis("Horizontal") * scrollSpeed;
        float Zin = Input.GetAxis("Vertical");
        float Yin = -Input.mouseScrollDelta.y * zoomSpeed;
        transform.position += new Vector3(Xin, Yin, Zin);

        if((Input.GetAxis("Horizontal") != 0) || (Input.GetAxis("Vertical") != 0)) Debug.Log("TDC scroll.");
    }
}
