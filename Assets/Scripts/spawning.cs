using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

public class spawning : MonoBehaviour
{
    public GameObject curObject;
    public string[] commands;

    int Spawn(string command, string[] terms)
    {
        string objectName = "default";
        float xpos = 0, ypos = 0, zpos = 0;
        int xrot = 0, yrot = 0, zrot= 0;
        float xscl = 1, yscl = 1, zscl = 1;

        int termCount = terms.Count();
        if(termCount <= 2) 
        {
            Debug.Log("Invalid terms for spawning obect with command '" + command + "'");
            return 1;
        }
        if(termCount >= 2) objectName = terms[1];
        if(termCount >= 5)
        {
            xpos = Convert.ToSingle(terms[2]);
            ypos = Convert.ToSingle(terms[3]);
            zpos = Convert.ToSingle(terms[4]);
        }
        if(termCount >= 8)
        {
            xrot = Convert.ToInt32(terms[5]);
            yrot = Convert.ToInt32(terms[6]);
            zrot = Convert.ToInt32(terms[7]);
        }
        if(termCount >= 11)
        {
            xscl = Convert.ToSingle(terms[8]);
            yscl = Convert.ToSingle(terms[9]);
            zscl = Convert.ToSingle(terms[10]);
        }
        
        //string matPath = "SimpleTownLite/_Materials/SimpleTownLite_Dumpster";

        //curObject = Resources.Load<GameObject>("SimpleTownLite/_Prefabs/Vehicles_PizzaCar");
        curObject = Resources.Load<GameObject>("SimpleTownLite/_Prefabs/" + objectName);

        GameObject NewGameObj = Instantiate(curObject) as GameObject;

        Quaternion init_rot = Quaternion.Euler(xrot, yrot, zrot);
        //Renderer init_rend = NewGameObj.GetComponent<MeshRenderer>();

        //init_rend.material = Resources.Load<Material>(matPath);
        NewGameObj.transform.localScale = new Vector3(xscl, yscl, zscl);
        NewGameObj.transform.rotation = init_rot;
        NewGameObj.transform.position = new Vector3(xpos, ypos, zpos);

        return 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        string textPath = @"Assets/Text Files/input.txt";
        if(File.Exists(textPath)) 
        {
            commands = System.IO.File.ReadAllLines(textPath);
            Debug.Log("File Contents:");
            foreach(string command in commands)
            {
                int result;

                Debug.Log(command);
                string[] terms = command.Split(' ');
                foreach(string term in terms)
                {
                    //Debug.Log("term = " + term);
                }
                if(terms[0] == "spawn") result = Spawn(command, terms);
                else Debug.Log("Unrecognized command: '" + terms[0] + "'");
            }
        }
        else Debug.Log("Could not open file");
/*
        //curObject = (GameObject)Resources.Load("SimpleTownLite/_Prefabs/Vehicles_PizzaCar", typeof(GameObject));
        curObject = Resources.Load<GameObject>("SimpleTownLite/_Prefabs/Vehicles_PizzaCar");

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
*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
