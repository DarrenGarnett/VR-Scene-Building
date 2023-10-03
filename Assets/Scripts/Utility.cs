using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Utility
{
    //Author: KelsoMRK, Jul 6, 2012
    //https://forum.unity.com/threads/hiow-to-get-children-gameobjects-array.142617/
    public static List<GameObject> GetChildren(this GameObject go)
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform tran in go.transform)
        {
            children.Add(tran.gameObject);
        }
        return children;
    }

    //source: https://forum.unity.com/threads/getting-the-bounds-of-the-group-of-objects.70979/
    public static Bounds GetBounds(GameObject obj)
    {
        Bounds bounds = new Bounds();

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if(renderers.Length > 0)
        {
            //Find first enabled renderer to start encapsulate from it
            foreach (Renderer renderer in renderers)
            {
                if (renderer.enabled)
                {
                    bounds = renderer.bounds;
                     break;
                }
            }

            //Encapsulate for all renderers
            foreach (Renderer renderer in renderers)
            {
                if (renderer.enabled)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
        }
        return bounds;
    }

    // source: tgraupmann, Mar 14, 2012
    // https://forum.unity.com/threads/change-gameobject-layer-at-run-time-wont-apply-to-child.10091/
    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }
       
        obj.layer = newLayer;
       
        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
