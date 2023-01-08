using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class spawning : MonoBehaviour
{
    public GameObject curObject;

    // Start is called before the first frame update
    void Start()
    {
        //curObject = (GameObject)Resources.Load("SimpleTownLite/_Prefabs/Vehicles_PizzaCar", typeof(GameObject));
        curObject = Resources.Load<GameObject>("SimpleTownLite/_Prefabs/Vehicles_PizzaCar");

        float xpos = 0, ypos = 0, zpos = 0;
        int xrot = 90, yrot = 0, zrot = 0;
        float xscl = 3, yscl = 3, zscl = 6;
        string matPath = "SimpleTownLite/_Materials/SimpleTownLite_Dumpster";

        for(int i = 0; i < 5; i++)
        {
            GameObject NewGameObj = Instantiate(curObject) as GameObject;

            Quaternion init_rot = Quaternion.Euler(xrot, yrot, zrot);
            Renderer init_rend = NewGameObj.GetComponent<MeshRenderer>();

            //init_rend.material = Resources.Load<Material>("SimpleTownLite/_Materials/SimpleTownLite_Dumpster");
            init_rend.material = Resources.Load<Material>(matPath);
            NewGameObj.transform.localScale = new Vector3(xscl, yscl, zscl);
            NewGameObj.transform.rotation = init_rot;
            NewGameObj.transform.position = new Vector3(xpos + i, ypos + i, zpos + i);

            //curObject = (GameObject)PrefabUtility.InstantiatePrefab(curObject as GameObject);
            //Instantiate(curObject, transform.position, transform.rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
