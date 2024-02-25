using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Straighten : MonoBehaviour
{
    BezierPath bp;

    void Start()
    {
        bp = GetComponent<PathCreator>().bezierPath;

        StraightenSeg();
    }

    void StraightenSeg()
    {
        Vector3 curAnchor = bp.GetPoint(0);
        Vector3 nextAnchor = bp.GetPoint(3);

        // Make the control points point to the opposing anchor point 
        Vector3 curControlStraightened = (nextAnchor - curAnchor).normalized + curAnchor;
        Vector3 nextControlStraightened = (curAnchor - nextAnchor).normalized + nextAnchor;

        //bp.controlMode = BezierPath.ControlMode.Free;
        //Debug.Log(bp.controlMode);
        
        bp.SetPoint(1, curControlStraightened);
        bp.SetPoint(2, nextControlStraightened);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
