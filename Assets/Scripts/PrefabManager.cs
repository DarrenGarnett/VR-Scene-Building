using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Dummiesman;

public class PrefabManager : MonoBehaviour
{
    string objPath = string.Empty;
    string error = string.Empty;
    GameObject loadedObject;

    public void LoadTest()
    {
        objPath = @"C:\Users\darre\Documents\VRSBUTBI-Production\Demo Test File\Cat_v1_L3.123cb1b1943a-2f48-4e44-8f71-6bbe19a3ab64\12221_Cat_v1_l3.obj";
        if(!File.Exists(objPath))
        {
            Debug.LogError("File doesn't exist.");
        }
        else
        {
            if(loadedObject != null) Destroy(loadedObject);
            loadedObject = new OBJLoader().Load(objPath);
            /*OBJLoader loader = new OBJLoader();
            loadedObject = new OBJObjectBuilder("test", loader).Build();
            Debug.Log("success");*/
        }
    }
}
