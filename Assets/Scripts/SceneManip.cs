using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
    //private string[] commands;
    public string textPath;
    private int index = 0;
    private Command curCommand;
    private List<Command> commands = new List<Command>();

    void setTransform(string[] terms)
    {
        string[] component = terms[2].Split('.');
        //foreach(string part in component) Debug.Log(part);

        switch(component[1].ToLower())
        {
            case "position":
                Vector3 curPos = curObject.transform.position;
                if(component.Count() == 3)
                {
                    if(component[2] == "x") curPos.x = Convert.ToSingle(terms[3]);
                    if(component[2] == "y") curPos.y = Convert.ToSingle(terms[3]);
                    if(component[2] == "z") curPos.z = Convert.ToSingle(terms[3]);
                }
                else if(terms.Count() == 6) curPos = new Vector3(Convert.ToSingle(terms[3]), Convert.ToSingle(terms[4]), Convert.ToSingle(terms[5]));
                curObject.transform.position = curPos;
                break;
            case "rotation":
                Vector3 curRot = curObject.transform.eulerAngles;
                if(component.Count() == 3)
                {
                    if(component[2] == "x") curRot.x = Convert.ToSingle(terms[3]);
                    if(component[2] == "y") curRot.y = Convert.ToSingle(terms[3]);
                    if(component[2] == "z") curRot.z = Convert.ToSingle(terms[3]);
                }
                else if(terms.Count() == 6) curRot = new Vector3(Convert.ToSingle(terms[3]), Convert.ToSingle(terms[4]), Convert.ToSingle(terms[5]));
                curObject.transform.eulerAngles = curRot;
                break;
            case "scale":
                Vector3 curScl = curObject.transform.localScale;
                if(component.Count() == 3)
                {
                    if(component[2] == "x") curScl.x = Convert.ToSingle(terms[3]);
                    if(component[2] == "y") curScl.y = Convert.ToSingle(terms[3]);
                    if(component[2] == "z") curScl.z = Convert.ToSingle(terms[3]);
                }
                else if(terms.Count() == 6) curScl = new Vector3(Convert.ToSingle(terms[3]), Convert.ToSingle(terms[4]), Convert.ToSingle(terms[5]));
                curObject.transform.localScale = curScl;
                break;
            default:
                Debug.Log("Invalid transform variable.");
                break;
        }
    }

    IEnumerator dynSetTransform(string[] terms)
    {
        string[] component = terms[2].Split('.');
        //foreach(string part in component) Debug.Log(part);

        switch(component[1].ToLower())
        {
            case "position":
                Vector3 curPos = curObject.transform.position;
                if(component.Count() == 3)
                {
                    if(component[2] == "x") curPos.x = Convert.ToSingle(terms[3]);
                    if(component[2] == "y") curPos.y = Convert.ToSingle(terms[3]);
                    if(component[2] == "z") curPos.z = Convert.ToSingle(terms[3]);
                }
                else if(terms.Count() == 6) curPos = new Vector3(Convert.ToSingle(terms[3]), Convert.ToSingle(terms[4]), Convert.ToSingle(terms[5]));
                curObject.transform.position = curPos;
                break;
            default:
                Debug.Log("Invalid transform variable.");
                break;
        }
        yield return null;
    }

    void setAnimator(string[] terms)
    {            
        Animator curAnimator = curObject.GetComponent<Animator>();
        curAnimator.runtimeAnimatorController = Resources.Load("SimpleTownLite/_Demo/" + terms[3]) as RuntimeAnimatorController;
    }
    
    //Creates a new instance of the specified prefab with optional specified transformation values
    int create(string[] terms)
    {
        //Assign default command terms
        string prefabName = "default", objectName = "default";
        float xpos = 0, ypos = 0;

        //Test for minimum terms
        int termCount = terms.Count();
        if(termCount < 3) 
        {
            Debug.Log("Too few terms to create object.");
            return 1;
        }

        //Assign the given terms
        if(termCount >= 3) 
        {
            objectName = terms[1];
            prefabName = terms[2];
        }
        if(termCount >= 5)
        {
            xpos = Convert.ToSingle(terms[3]);
            ypos = Convert.ToSingle(terms[4]);
        }
        
        
        //Create new instance of the given prefab
        curObject = Resources.Load<GameObject>("SimpleTownLite/_Prefabs/" + prefabName);
        GameObject NewGameObj = Instantiate(curObject) as GameObject;

        //Apply all values to the new GameObject
        NewGameObj.name = objectName;
        NewGameObj.transform.position = new Vector3(xpos, ypos, 0);

        return 0;
    }

    //Replaces the current scene with an empty one
    int unload(string[] terms)
    {
        SceneManager.LoadScene(sceneName: "Empty");
        
        return 0;
    }

    int setCell(string[] terms)
    {
        if(terms.Count() < 4)
        {
            Debug.Log("Too few terms to set cell value.");
            return 1;
        }
        else
        {
            curObject = GameObject.Find(terms[1]);

            string[] component = terms[2].Split('.');
            
            Component curComponent = curObject.GetComponent(component[0]);
            if(curComponent != null)
            {
                //Debug.Log("Valid component: " + component[0]);
                string cellName = terms[2];
                if(cellName.Contains("Transform")) setTransform(terms);
                if(cellName.Contains("Animator")) setAnimator(terms);
                return 0;
            }
            else Debug.Log("Can not set value, invalid component name.");
        }
        return 1;
    }

    int dynSetCell(string[] terms)
    {
        if(terms.Count() < 8)
        {
            Debug.Log("Too few terms to set cell value over time.");
            return 1;
        }
        else
        {
            curObject = GameObject.Find(terms[1]);

            string[] component = terms[2].Split('.');

            float startTime = Time.time;
            
            Component curComponent = curObject.GetComponent(component[0]);
            if(curComponent != null)
            {
                //Debug.Log("Valid component: " + component[0]);
                string cellName = terms[2];
                if(cellName.Contains("Transform")) dynSetTransform(terms);
                return 0;
            }
            else Debug.Log("Can not set value, invalid component name.");
        }
        return 1;
    }

    //Destroys the specified object
    int destroy(string[] terms)
    {
        //Test for minimum terms
        if(terms.Count() < 2)
        {
            Debug.Log("Too few terms to destroy object.");
            return 1;
        }
        else
        {
            curObject = GameObject.Find(terms[1]);
            Destroy(curObject);
        }

        return 0;
    }

    //Applies an already existing path to the specified object
    int move(string[] terms)
    {
        //Test for minimum terms
        if(terms.Count() < 4)
        {
            Debug.Log("Too few terms to move object.");
            return 1;
        }
        else
        {
            //Initialize path script
            curObject = GameObject.Find(terms[1]);
            PathFollower curFollower = curObject.GetComponent<PathFollower>() as PathFollower;
            curFollower.enabled = true;

            //Assign path by name
            GameObject pathObject = GameObject.Find(terms[2]);
            PathCreator curCreator = pathObject.GetComponent<PathCreator>() as PathCreator;

            curFollower.pathCreator = curCreator;

            //Set cycle duration
            curFollower.cycleDuration = Convert.ToSingle(terms[3]);

            curFollower.endOfPathInstruction = EndOfPathInstruction.Stop;
        }
        return 0;
    }

    //Removes the specified object from its current path
    int removeFromPath(string[] terms)
    {
        //Test for minimum terms
        if(terms.Count() < 2)
        {
            Debug.Log("Too few terms to remove path.");
            return 1;
        }
        else
        {
            curObject = GameObject.Find(terms[1]);
            curObject.GetComponent<PathFollower>().pathCreator = null;
        }
        
        return 0;
    }

    //Translates the specified object by a specified offset
    int translate(string[] terms)
    {
        //Test for minimum terms
        if(terms.Count() != 6)
        {
            //Debug.Log("Invalid terms for moving obect with command '" + command + "'");
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
    
    //Determines which command is to be executed
    int execute(string command, string[] terms)
    {
        //Initialize return value
        int result = 0;

        Debug.Log(command);
        
        //Determine command by name
        switch(terms[0])
        {
            case "CREATE":
                result = create(terms);
                break;
            case "END":
                result = unload(terms);
                break;
            case "SETOBJCELL":
                result = setCell(terms);
                break;
            case "DESTROY":
                result = destroy(terms);
                break;
            case "DYNUPDATECELL":
                result = dynSetCell(terms);
                break;
            case "PATH":
                Debug.Log(terms[0] + " not currently supported.");
                break;
            case "MOVE":
                result = move(terms);
                break;
            case "REMOVEFROMPATH":
                result = removeFromPath(terms);
                break;
            case "PAUSE":
                Debug.Log(terms[0] + " not currently supported.");
                break;
            default:
                Debug.Log("Unrecognized command: '" + terms[0] + "'");
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
            string[] input = System.IO.File.ReadAllLines(textPath);
            //string[] lines = commands.Clone() as string[];
            List<string> lines = new List<string>();

            foreach(string command in input)
            {
                if(!command.Contains("//")) lines.Add(command);
            }

            float curTime = 0;
            foreach(string line in lines)
            {
                Command temp = new Command();
                temp.line = line;
                temp.args = line.Split(' ');

                
                if(line.Contains("TIME")) curTime = Convert.ToSingle(temp.args[1]);
                else 
                {
                    temp.time = curTime;
                    commands.Add(temp);
                }
            }

            commands = commands.OrderBy(n => n.time).ToList();
            //foreach(Command command in commands) Debug.Log(command.time + " " + command.line);
        }
        else Debug.Log("Could not open file");
    }

    // Update is called once per frame
    void Update()
    {
        if(index < commands.Count)
        {
            //Get the current command
            curCommand = commands[index];

            if(Time.time > curCommand.time)
            {
                execute(curCommand.line, curCommand.args);
                index++;
            }
        }
    }
}