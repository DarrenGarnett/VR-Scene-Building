using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using PathCreation;
using PathCreation.Examples;

public class Command
{
    public string line;
    public string[] args;
    public float time;
}

public class SceneManip : MonoBehaviour
{
    private GameObject curObject;
    private string[] commands;
    public string textPath;
    private int index;
    private Command curCommand;

    int termCheck()
    {


        return -1;
    }

    //Creates a new instance of the specified prefab with optional specified transformation values
    int spawn(string command, string[] terms)
    {
        //Assign default command terms
        string prefabName = "default", objectName = "default";
        float xpos = 0, ypos = 0, zpos = 0;
        int xrot = 0, yrot = 0, zrot= 0;
        float xscl = 1, yscl = 1, zscl = 1;

        //Test for minimum terms
        int termCount = terms.Count();
        if(termCount < 4) 
        {
            Debug.Log("Invalid terms for spawning obect with command '" + command + "'");
            return 1;
        }

        //Assign the given terms
        if(termCount >= 4) 
        {
            prefabName = terms[2];
            objectName = terms[3];
        }
        if(termCount >= 7)
        {
            xpos = Convert.ToSingle(terms[4]);
            ypos = Convert.ToSingle(terms[5]);
            zpos = Convert.ToSingle(terms[6]);
        }
        if(termCount >= 10)
        {
            xrot = Convert.ToInt32(terms[7]);
            yrot = Convert.ToInt32(terms[8]);
            zrot = Convert.ToInt32(terms[9]);
        }
        if(termCount >= 13)
        {
            xscl = Convert.ToSingle(terms[10]);
            yscl = Convert.ToSingle(terms[11]);
            zscl = Convert.ToSingle(terms[12]);
        }
        
        //Create new instance of the given prefab
        curObject = Resources.Load<GameObject>("SimpleTownLite/_Prefabs/" + prefabName);
        GameObject NewGameObj = Instantiate(curObject) as GameObject;

        //Apply all values to the new GameObject
        NewGameObj.name = objectName;
        NewGameObj.transform.localScale = new Vector3(xscl, yscl, zscl);
        Quaternion init_rot = Quaternion.Euler(xrot, yrot, zrot);
        NewGameObj.transform.rotation = init_rot;
        NewGameObj.transform.position = new Vector3(xpos, ypos, zpos);

        return 0;
    }

    //Translates the specified object by a specified offset
    int translate(string command, string[] terms)
    {
        //Test for minimum terms
        if(terms.Count() != 6)
        {
            Debug.Log("Invalid terms for moving obect with command '" + command + "'");
            return 1;
        }
        else
        {
            //Apply given offset values
            curObject = GameObject.Find(terms[2]);
            float xdis = Convert.ToSingle(terms[3]);
            float ydis = Convert.ToSingle(terms[4]);
            float zdis = Convert.ToSingle(terms[5]);

            curObject.transform.position += new Vector3(xdis, ydis, zdis);
        }

        return 0;
    }

    //Applies the specified animator to the specified object
    int animate(string command, string[] terms)
    {
        //Test for minimum terms
        if(terms.Count() < 4)
        {
            Debug.Log("Invalid terms for moving obect with command '" + command + "'");
            return 1;
        }
        else
        {
            //Apply given animator
            curObject = GameObject.Find(terms[2]);
            if(curObject == null)
            {
                Debug.Log("Can't apply animator, could not find gameObject.");
                return 1;
            }
            
            Animator curAnimator = curObject.GetComponent<Animator>();
            if(curAnimator == null)
            {
                Debug.Log("Can't apply animator, could not find animator.");
                return 1;
            }
            
            curAnimator.runtimeAnimatorController = Resources.Load("SimpleTownLite/_Demo/" + terms[3]) as RuntimeAnimatorController;
            
            curAnimator.SetFloat("speed_multi", Convert.ToSingle(terms[4]));
        }

        return 0;
    }

    //Applies an already existing path to the specified object
    int path(string command, string[] terms)
    {
        //Test for minimum terms
        if(terms.Count() < 4)
        {
            Debug.Log("Invalid terms for moving obect with command '" + command + "'");
            return 1;
        }
        else
        {
            //Initialize path script
            curObject = GameObject.Find(terms[2]);
            PathFollower curFollower = curObject.GetComponent<PathFollower>() as PathFollower;
            curFollower.enabled = true;

            //Assign path by name
            GameObject pathObject = GameObject.Find(terms[3]);
            PathCreator curCreator = pathObject.GetComponent<PathCreator>() as PathCreator;

            curFollower.pathCreator = curCreator;
        }
        return 0;
    }

    int unload(string command, string[] terms)
    {
        //Test for minimum terms
        if(terms.Count() < 2)
        {
            Debug.Log("Invalid terms for moving obect with command '" + command + "'");
            return 1;
        }
        else SceneManager.LoadScene(sceneName: "Empty");
        
        return 0;
    }

    int destroy(string command, string[] terms)
    {
        //Test for minimum terms
        if(terms.Count() < 3)
        {
            Debug.Log("Invalid terms for moving obect with command '" + command + "'");
            return 1;
        }
        else
        {
            curObject = GameObject.Find(terms[2]);
            Destroy(curObject);
        }

        return 0;
    }
    
    int removeFromPath(string command, string[] terms)
    {
        //Test for minimum terms
        if(terms.Count() < 3)
        {
            Debug.Log("Invalid terms for moving obect with command '" + command + "'");
            return 1;
        }
        else
        {
            curObject = GameObject.Find(terms[2]);
            curObject.GetComponent<PathFollower>().pathCreator = null;
        }
        
        return 0;
    }
    
    //Determines which command is to be executed
    int execute(string command)
    {
        //Initialize return value
        int result;

        Debug.Log(command);
        string[] terms = command.Split(' ');
        
        //Determine command by name
        switch(terms[1])
        {
            case "spawn":
                result = spawn(command, terms);
                break;
            case "translate":
                result = translate(command, terms);
                break;
            case "animate":
                result = animate(command, terms);
                break;
            case "path":
                result = path(command, terms);
                break;
            case "end":
                result = unload(command, terms);
                break;
            case "destroy":
                result = destroy(command, terms);
                break;
            case "removeFromPath":
                result = removeFromPath(command, terms);
                break;
            default:
                Debug.Log("Unrecognized command: '" + terms[1] + "'");
                result = -1;
                break;
        }
        return result;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Ensure that the input file path is correct
        if(File.Exists(textPath)) 
        {
            //Store the contents of the input file
            commands = System.IO.File.ReadAllLines(textPath);
            string[] lines = commands.Clone() as string[];

            int i = 0;
            foreach(string line in lines)
            {
                if(!line.Contains("//"))
                {
                    lines[i] = line;
                    i++;
                }
            }

            commands = lines.Clone() as string[];

            //foreach(string command in commands) Debug.Log("Command " + command);
            //foreach(string line in lines) Debug.Log("Line: " + line);
        }
        else Debug.Log("Could not open file");
    }

    // Update is called once per frame
    void Update()
    {
        if(index < commands.Count())
        {
            //Get the current command
            curCommand = getCommand(index);

            //Ensure the current command's time has been reached, and that the last command has not been reached
            //if(Time.time > curCommand.time && index < commands.Count() - 1)
            if(Time.time > curCommand.time)
            {
                //Execute the current command
                execute(curCommand.line);

                //Move to the next command
                index++;
            }
        }  
    }

    //Gets the command data for the given index
    Command getCommand(int index)
    {
        //Create new command object
        Command ret = new Command();

        //Get command data from stored command lines
        ret.line = commands[index];
        ret.args = ret.line.Split(' ');
        ret.time = Convert.ToSingle(ret.args[0]);

        return ret;
    }
}