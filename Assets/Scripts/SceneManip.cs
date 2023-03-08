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
/*
public class CommandFile
{
    public Dictionary<string, CommandList> functions = new Dictionary<string, CommandList>();
    public List<string> referenceList = new List<string>();
    public CommandFile(string file)
    {
        if(File.Exists(file)) 
        {
            string[] input = System.IO.File.ReadAllLines(file);

            List<string> lines = new List<string>();

            foreach(string line in input)
            {
                if(!line.Contains("//") && line != "") lines.Add(command);
            }

            int curline = 0;
            while(curline < lines.Count())
            {
                CommandList curFunction = new CommandList();
                bool toNext = 0;
                
                while(!toNext)
                {
                    if(lines[curline].contains("INCLUDE")) referenceList.Add(temp.args[1]);
                    else if(lines[curline].Contains("FUNCTION")) referenceList.Add(temp.args[1]);
                    else if(lines[curline].Contains("END")) toNext = 1;
                    else 
                    {
                        curline++;
                    }
                }
            }

            //commandList.commands = commandList.commands.OrderBy(n => n.time).ToList();
            //foreach(Command command in commands) Debug.Log(command.time + " " + command.line);
        }
        else Debug.Log("Could not open " + file);
    }
}
*/
public class CommandList
{
    public Dictionary<string, string> functions = new Dictionary<string, string>();
    public List<string> referenceList = new List<string>();
    public List<Command> commands = new List<Command>();
    
    public CommandList(string file)
    {
        if(File.Exists(file)) 
        {
            string[] input = System.IO.File.ReadAllLines(file);

            List<string> lines = new List<string>();

            foreach(string command in input)
            {
                if(!command.Contains("//") && command != "") lines.Add(command);
            }

            float curTime = 0;
            int curLine = 0, startLine = 0;
            foreach(string line in lines)
            {
                Command temp = new Command();
                temp.line = line;
                temp.args = line.Split(' ');
                

                if(line.Contains("TIME")) curTime = Convert.ToSingle(temp.args[1]);
                else if(line.Contains("INCLUDE")) referenceList.Add(temp.args[1]);
                else if(line.Contains("FUNCTION")) 
                {
                    startLine = curLine;
                }
                else if(line.Contains("END")) 
                {
                    functions.Add(temp.args[1], startLine.ToString() + "-" + curLine.ToString());
                    curTime = 0;
                }
                else 
                {
                    temp.time = curTime;
                    commands.Add(temp);
                    curLine++;
                }
            }

            //commandList.commands = commandList.commands.OrderBy(n => n.time).ToList();
            //foreach(Command command in commands) Debug.Log(command.time + " " + command.line);
            //foreach(KeyValuePair<string, string> kvp in functions) Debug.Log("Function name: " + kvp.Key + "\nFuntion bounds: " + kvp.Value);
        }
        else Debug.Log("Could not open " + file);
    }
}

public class SceneManip : MonoBehaviour
{
    private GameObject curObject;
    //private string[] commands;
    public string textPath;
    private int index = 0;
    private Command curCommand;
    //private List<Command> commands = new List<Command>();
    private CommandList commandList;

    Vector3 changeVec(Vector3 inVec, string[] terms)
    {
        string[] component = terms[2].Split('.');

        Vector3 curVec = inVec;
        if(component.Count() == 3)
        {
            if(component[2] == "x") curVec.x = Convert.ToSingle(terms[3]);
            if(component[2] == "y") curVec.y = Convert.ToSingle(terms[3]);
            if(component[2] == "z") curVec.z = Convert.ToSingle(terms[3]);
        }
        else if(terms.Count() == 6) curVec = new Vector3(Convert.ToSingle(terms[3]), Convert.ToSingle(terms[4]), Convert.ToSingle(terms[5]));
        else if(terms[2] == "length") curVec.x = Convert.ToSingle(terms[3]);
        else if(terms[2] == "height") curVec.y = Convert.ToSingle(terms[3]);
        else if(terms[2] == "width") curVec.z = Convert.ToSingle(terms[3]);

        return curVec;
    }

    void setTransform(string[] terms)
    {
        string[] component = terms[2].Split('.');
        //foreach(string part in component) Debug.Log(part);

        if(component.Count() > 1)
        {
            switch(component[1].ToLower())
            {
                case "position":
                    curObject.transform.position = changeVec(curObject.transform.position, terms);
                    break;
                case "rotation":
                    curObject.transform.eulerAngles = changeVec(curObject.transform.eulerAngles, terms);
                    break;
                case "scale":
                    curObject.transform.localScale = changeVec(curObject.transform.localScale, terms);
                    break;
                default:
                    Debug.Log("Invalid transform variable.");
                    break;
            }
        }
        else
        {
            switch(component[0].ToLower())
            {    
                case "length":
                case "width":
                case "height":
                    curObject.transform.localScale = changeVec(curObject.transform.localScale, terms);
                    break;
                default:
                    Debug.Log("Invalid transform variable.");
                    break;
            }
        }
    }

    float blend(float percentComplete, float init, float final)
    {
        return (percentComplete * (final - init)) + init;
    }

    Vector3 dynChangeVec(GameObject dynObject, Vector3 inVec, string[] terms, float startTime)
    {
        string[] component = terms[2].Split('.');
        float duration = Convert.ToSingle(terms[3]);

        float timeSinceStart = Time.time - startTime;
        float percentComplete = timeSinceStart / duration;

        Vector3 curVec = inVec;
        if(component.Count() == 3)
        {
            if(component[2] == "x") curVec.x = blend(percentComplete, Convert.ToSingle(terms[5]), Convert.ToSingle(terms[7]));
            if(component[2] == "y") curVec.y = blend(percentComplete, Convert.ToSingle(terms[5]), Convert.ToSingle(terms[7]));
            if(component[2] == "z") curVec.z = blend(percentComplete, Convert.ToSingle(terms[5]), Convert.ToSingle(terms[7]));
        }
        else if(terms.Count() == 12) 
        {
            float curX = blend(percentComplete, Convert.ToSingle(terms[5]), Convert.ToSingle(terms[9]));
            float curY = blend(percentComplete, Convert.ToSingle(terms[6]), Convert.ToSingle(terms[10]));
            float curZ = blend(percentComplete, Convert.ToSingle(terms[7]), Convert.ToSingle(terms[11]));
            curVec = new Vector3(curX, curY, curZ);
        }
        else if(terms[2] == "length") curVec.x = blend(percentComplete, Convert.ToSingle(terms[5]), Convert.ToSingle(terms[7]));
        else if(terms[2] == "height") curVec.y = blend(percentComplete, Convert.ToSingle(terms[5]), Convert.ToSingle(terms[7]));
        else if(terms[2] == "width") curVec.z = blend(percentComplete, Convert.ToSingle(terms[5]), Convert.ToSingle(terms[7]));
        else Debug.Log("Unrecognized transform component(s).");
        
        return curVec;
    }

    IEnumerator dynSetTransform(string[] terms)
    {
        //Debug.Log("Dynamically updating transform...");
        string[] component = terms[2].Split('.');
        //foreach(string part in component) Debug.Log(part);

        float duration = Convert.ToSingle(terms[3]);
        float startTime = Time.time;
        GameObject dynObject = curObject;

        if(component.Count() > 1)
        {
            switch(component[1].ToLower())
            {
                case "position":
                    while(Time.time - startTime < duration)
                    {
                        dynObject.transform.position = dynChangeVec(dynObject, dynObject.transform.position, terms, startTime);
                        yield return null;
                    }
                    break;
                case "rotation":
                    while(Time.time - startTime < duration)
                    {
                        dynObject.transform.eulerAngles = dynChangeVec(dynObject, dynObject.transform.eulerAngles, terms, startTime);
                        yield return null;
                    }
                    break;
                case "scale":
                    while(Time.time - startTime < duration)
                    {
                        dynObject.transform.localScale = dynChangeVec(dynObject, dynObject.transform.localScale, terms, startTime);
                        yield return null;
                    }
                    break;
                default:
                    Debug.Log("Invalid transform variable.");
                    break;
            }
        }
        else
        {
            switch(component[0].ToLower())
            {    
                case "length":
                case "width":
                case "height":
                    while(Time.time - startTime < duration)
                    {
                        dynObject.transform.localScale = dynChangeVec(dynObject, dynObject.transform.localScale, terms, startTime);
                        yield return null;
                    }
                    break;
                default:
                    Debug.Log("Invalid transform variable.");
                    break;
            }
        }
    }

    void setAnimator(string[] terms)
    {            
        Animator curAnimator = curObject.GetComponent<Animator>();
        curAnimator.runtimeAnimatorController = Resources.Load("SimpleTownLite/_Demo/" + terms[3]) as RuntimeAnimatorController;
    }

    void setRenderer(string[] terms)
    {
        Renderer curRenderer = curObject.GetComponent<Renderer>();
        Color newColor = curRenderer.material.color;
        //Debug.Log("Original color: " + newColor);
        //source: https://answers.unity.com/questions/1016155/standard-material-shader-ignoring-setfloat-propert.html
        curRenderer.material.SetFloat("_Mode", 2);
        curRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        curRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        curRenderer.material.SetInt("_ZWrite", 0);
        curRenderer.material.DisableKeyword("_ALPHATEST_ON");
        curRenderer.material.EnableKeyword("_ALPHABLEND_ON");
        curRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        curRenderer.material.renderQueue = 3000;

        string[] component = terms[2].Split('.');
        string value = component[0].ToLower();
        //foreach(string part in component) Debug.Log(part);

        if(value == "red") newColor.r = Convert.ToSingle(terms[3]);
        else if(value == "green") newColor.g = Convert.ToSingle(terms[3]);
        else if(value == "blue") newColor.b = Convert.ToSingle(terms[3]);
        else if(value == "alpha" || value == "transparency") 
        {
            //curRenderer.material.SetFloat("_Mode", 2);
            newColor.a = Convert.ToSingle(terms[3]);
        }
        else Debug.Log("Invalid renderer variable.");

        //Debug.Log("New color: " + newColor);
        curRenderer.material.color = newColor;
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
            string cellName = component[0].ToLower();
            
            Component curComponent = curObject.GetComponent(component[0]);
            if(cellName == "width" || cellName == "length" || cellName == "height") setTransform(terms);
            else if(cellName == "red" || cellName == "green" || cellName == "blue" || cellName == "alpha" || cellName == "transparency") setRenderer(terms);
            else if(curComponent != null)
            {
                if(cellName.Contains("transform")) setTransform(terms);
                if(cellName.Contains("animator")) setAnimator(terms);
                if(cellName.Contains("renderer")) setRenderer(terms);
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
            string cellName = component[0].ToLower();
            
            Component curComponent = curObject.GetComponent(component[0]);
            if(cellName == "width" || cellName == "length" || cellName == "height") StartCoroutine(dynSetTransform(terms));
            else if(curComponent != null)
            {
                if(cellName.Contains("transform")) StartCoroutine(dynSetTransform(terms));
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
        //get master list
        commandList = new CommandList(textPath);
        //commandList = new CommandList();
/*
        List<Command> tempList = commandList.commands.ToList();

        foreach(Command command in commandList.commands)
        {
            if(command.args[0] == "CALL")
            {
                tempList.Remove(command);
            }
        }
*/
        //commandList.commands = tempList.ToList();
        CommandList temp = new CommandList(textPath);
        foreach(string fileInfo in commandList.referenceList) 
        {
            //Debug.Log("Adding " + fileInfo);
            CommandList reference = new CommandList("Assets/Text Files/" + fileInfo);
            //float relativeTime = Convert.ToSingle(fileInfo.Split(' ')[1]);
            int curCmd = 0;
            foreach(Command cmd in commandList.commands)
            {
                if(cmd.line.Contains("CALL"))
                {
                    if(reference.functions.ContainsKey(cmd.args[1])) 
                    {
                        //Debug.Log(fileInfo + ": " + cmd.args[1] + " found.");
                        //temp.commands.RemoveAt(curCmd);
                        //temp.commands.RemoveAt(0);
                        
                        string bounds = reference.functions[cmd.args[1]];
                        int lower = Convert.ToInt32(bounds.Split('-')[0]);
                        int upper = Convert.ToInt32(bounds.Split('-')[1]);
                        int i;
                        for(i = lower; i < upper; i++)
                        {
                            reference.commands[i].time += cmd.time;
                            temp.commands.Add(reference.commands[i]);
                        }
                        
                        temp.commands.RemoveAt(curCmd);
                        Debug.Log("Removing at index " + curCmd);
                        //curCmd += i;
                    }
                    else Debug.Log(cmd.args[1] + " not found in " + fileInfo);
                }
                else curCmd++;
                //cmd.time += relativeTime;
                //commandList.commands.Add(cmd);
            }
        }


        //foreach(Command command in temp.commands) Debug.Log("Temp: " + command.time + " " + command.line);
        commandList = temp;

        commandList.commands = commandList.commands.OrderBy(n => n.time).ToList();
        foreach(Command command in commandList.commands) Debug.Log(command.time + " " + command.line);
/*
        int i = 0;
        while(commandList.commands[i].args[0] == "INCLUDE")
        {
            CommandList referenceList = new CommandList(commandList.commands[0].args[1]);
            //foreach(Command command in referenceList.commands) masterList.commands.Add(command);
            i++;
        }

        //Ensure that the input file path is correct
        if(File.Exists(textPath)) 
        {
            //Store the contents of the input file
            string[] input = System.IO.File.ReadAllLines(textPath);
            //string[] lines = commands.Clone() as string[];
            List<string> lines = new List<string>();

            foreach(string command in input)
            {
                if(!command.Contains("//") && command != "") lines.Add(command);
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
                    commandList.commands.Add(temp);
                }
            }

            commandList.commands = commandList.commands.OrderBy(n => n.time).ToList();
            foreach(Command command in commandList.commands) Debug.Log(command.time + " " + command.line);
        }
        else Debug.Log("Could not open file");
*/
    }

    // Update is called once per frame
    void Update()
    {
        if(index < commandList.commands.Count)
        {
            //Get the current command
            curCommand = commandList.commands[index];

            if(Time.time > curCommand.time)
            {
                execute(curCommand.line, curCommand.args);
                index++;
            }
        }
        //foreach()
    }
}