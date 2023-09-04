using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisUtil : MonoBehaviour
{
    // Reference: Jayanam, Nov 25 2018
    // https://www.youtube.com/watch?v=0yHBDZHLRbQ
    private Vector3 initMouseOffset;
    private float initMouseZ;

    private Vector3 prevPos;

    void OnMouseDown()
    {
        // Get initial z screen coordinate
        initMouseZ = Camera.main.WorldToScreenPoint(transform.position).z;

        // Get initial mouse offset from axis
        initMouseOffset = transform.parent.transform.position - GetMouseWorldPos();

        prevPos = transform.position;

        // Disable camera drag while dragging axis
        CameraMovement.disableDrag = true;
    }

    void OnMouseUp()
    {
        CameraMovement.disableDrag = false;
    }

    void OnMouseDrag()
    {
        // Change position from difference in mouse position        
        Vector3 worldPos = GetMouseWorldPos() + initMouseOffset;
        Vector3 curPos = transform.parent.transform.position;
        if(gameObject.name == "Xaxis") curPos.x = worldPos.x;
        if(gameObject.name == "Yaxis") curPos.y = worldPos.y;
        if(gameObject.name == "Zaxis") curPos.z = worldPos.z;

        transform.parent.transform.position = curPos;
    }

    Vector3 GetMouseWorldPos()
    {
        // Get mouse screen coordinates
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = initMouseZ;

        // Convert to world coordinates
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }
}
