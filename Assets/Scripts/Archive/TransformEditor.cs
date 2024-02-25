using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TransformEditor : MonoBehaviour
{
    public TMP_InputField xPosIn, yPosIn, zPosIn;
    public TMP_InputField xRotIn, yRotIn, zRotIn;
    public TMP_InputField xSclIn, ySclIn, zSclIn;

    Transform curTransform = null;

    public bool awaitPreviewRefresh = false;

    public void SetCurTransform(Transform tf)
    {
        //if(curTransform) StoreTransform(curTransform);
        curTransform = tf;
        UpdateInputFields();
    }

    public void UpdateInputFields()
    {
        Vector3 offset = ArchiveManager.previewAreaOrigin;
        int precision = 3;
        xPosIn.text = Math.Round(curTransform.position.x - offset.x, precision).ToString();
        yPosIn.text = Math.Round(curTransform.position.y - offset.y, precision).ToString();
        zPosIn.text = Math.Round(curTransform.position.z - offset.z, precision).ToString();

        xRotIn.text = Math.Round(curTransform.eulerAngles.x, precision).ToString();
        yRotIn.text = Math.Round(curTransform.eulerAngles.y, precision).ToString();
        zRotIn.text = Math.Round(curTransform.eulerAngles.z, precision).ToString();

        xSclIn.text = Math.Round(curTransform.localScale.x, precision).ToString();
        ySclIn.text = Math.Round(curTransform.localScale.y, precision).ToString();
        zSclIn.text = Math.Round(curTransform.localScale.z, precision).ToString();
    }

    void FixedUpdate()
    {
        if(curTransform != null)
        {
            if(awaitPreviewRefresh) awaitPreviewRefresh = false;

            Transform updatedTransform = curTransform;
            // https://stackoverflow.com/questions/8356982/c-sharp-how-to-return-a-correct-checking-float-parsestring
            float xPos, yPos, zPos;
            Vector3 updatedPos = ArchiveManager.previewAreaOrigin;
            if(float.TryParse(xPosIn.text, out xPos)) updatedPos.x += xPos;
            if(float.TryParse(yPosIn.text, out yPos)) updatedPos.y += yPos;
            if(float.TryParse(zPosIn.text, out zPos)) updatedPos.z += zPos;
            updatedTransform.position = updatedPos;

            float xRot, yRot, zRot;
            Vector3 updatedRot = new Vector3(0,0,0);
            if(float.TryParse(xRotIn.text, out xRot)) updatedRot.x += xRot;
            if(float.TryParse(yRotIn.text, out yRot)) updatedRot.y += yRot;
            if(float.TryParse(zRotIn.text, out zRot)) updatedRot.z += zRot;
            updatedTransform.eulerAngles = updatedRot;

            float xScl, yScl, zScl;
            Vector3 updatedScl = new Vector3(0,0,0);
            if(float.TryParse(xSclIn.text, out xScl)) updatedScl.x += xScl;
            if(float.TryParse(ySclIn.text, out yScl)) updatedScl.y += yScl;
            if(float.TryParse(zSclIn.text, out zScl)) updatedScl.z += zScl;

            if(updatedTransform.localScale != updatedScl) awaitPreviewRefresh = true;

            updatedTransform.localScale = updatedScl;
            
            //Debug.Log(updatedTransform.position);
        }
    }
}
