using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    //Author: KelsoMRK, Jul 6, 2012
    //https://forum.unity.com/threads/hiow-to-get-children-gameobjects-array.142617/
    public static List<GameObject> getChildren(this GameObject go)
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform tran in go.transform)
        {
            children.Add(tran.gameObject);
        }
        return children;
    }
}
