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

    public int getIndexAtTime(float time)
    {
        int index = 0;

        foreach(Command command in commands)
        {
            if(command.time < time) index++;
        }
        
        return index;
    }
}

public class SceneManip : MonoBehaviour
{
    public GlobalTimeScript globalTime;
    public PauseScript pauseTime;
    private GameObject curObject;
    //private string[] commands;
    public string textPath;
    private int index = 0;
    private Command curCommand;
    //private List<Command> commands = new List<Command>();
    private CommandList commandList;
    public string mainFunction;
    private bool waited = false;

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

        //float timeSinceStart = Time.time - startTime;
        float timeSinceStart = globalTime.currTime - startTime;
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
        //float startTime = Time.time;
        float startTime = globalTime.currTime;
        GameObject dynObject = curObject;

        if(component.Count() > 1)
        {
            switch(component[1].ToLower())
            {
                case "position":
                    while(globalTime.currTime - startTime < duration)
                    {
                        if(dynObject != null) dynObject.transform.position = dynChangeVec(dynObject, dynObject.transform.position, terms, startTime);
                        yield return null;
                    }
                    break;
                case "rotation":
                    while(globalTime.currTime - startTime < duration)
                    {
                        if(dynObject != null) dynObject.transform.eulerAngles = dynChangeVec(dynObject, dynObject.transform.eulerAngles, terms, startTime);
                        yield return null;
                    }
                    break;
                case "scale":
                    while(globalTime.currTime - startTime < duration)
                    {
                        if(dynObject != null) dynObject.transform.localScale = dynChangeVec(dynObject, dynObject.transform.localScale, terms, startTime);
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
                    while(globalTime.currTime - startTime < duration)
                    {
                        if(dynObject != null) dynObject.transform.localScale = dynChangeVec(dynObject, dynObject.transform.localScale, terms, startTime);
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
        curObject.AddComponent<AnimationHandler>();
        /*if(curObjectcurObject.GetComponent(terms[3]) == null) 
        {
                //curObject.AddComponent(component[0]);
                curComponent = curObject.GetComponent(component[0]);
        }*/

        Animator curAnimator = curObject.GetComponent<Animator>();
        curAnimator.runtimeAnimatorController = Resources.Load("Animators/" + terms[3]) as RuntimeAnimatorController;
    }

    void setRenderer(string[] terms)
    {
        Renderer curRenderer = curObject.GetComponent<Renderer>();
        if(curRenderer == null)
        {
            Debug.Log("curRenderer is null.");
            return;
        }
        //Debug.Log(curObject + " " + curRenderer);
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
            //Debug.Log("Changing alpha");
            //curRenderer.material.SetFloat("_Mode", 2);
            newColor.a = Convert.ToSingle(terms[3]);
        }
        else Debug.Log("Invalid renderer variable.");

        //Debug.Log("New color: " + newColor);
        Debug.Log("Old color: " + curRenderer.material.color);
        curRenderer.material.color = newColor;
        Debug.Log("New color: " + curRenderer.material.color);
    }

    IEnumerator dynSetRenderer(string[] terms)
    {
        float duration = Convert.ToSingle(terms[3]);
        //float startTime = Time.time;
        float startTime = globalTime.currTime;

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

        while(globalTime.currTime - startTime < duration)
        {
            float timeSinceStart = globalTime.currTime - startTime;
            float percentComplete = timeSinceStart / duration;

            if(curRenderer != null) 
            {
                newColor = curRenderer.material.color;

                if(value == "red") newColor.r = blend(percentComplete, Convert.ToSingle(terms[5]), Convert.ToSingle(terms[7]));
                else if(value == "green") newColor.g = blend(percentComplete, Convert.ToSingle(terms[5]), Convert.ToSingle(terms[7]));
                else if(value == "blue") newColor.b = blend(percentComplete, Convert.ToSingle(terms[5]), Convert.ToSingle(terms[7]));
                else if(value == "alpha" || value == "transparency") newColor.a = blend(percentComplete, Convert.ToSingle(terms[5]), Convert.ToSingle(terms[7]));
                else Debug.Log("Invalid renderer variable.");

                //Debug.Log("New color: " + newColor);
                curRenderer.material.color = newColor;
            }
            else Debug.Log("Renderer is null.");

            //Debug.Log("Yield return null");
            yield return null;
        }
    }

    //Creates a new instance of the specified prefab with optional specified transformation values
    int create(string[] terms)
    {
        //Assign default command terms
        string prefabName = "default", objectName = "default";
        float xpos = 0, ypos = 0, zpos = 0;

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
        if(termCount >= 6)
        {
            xpos = Convert.ToSingle(terms[3]);
            ypos = Convert.ToSingle(terms[4]);
            zpos = Convert.ToSingle(terms[5]);
        }
        
        
        //Create new instance of the given prefab
        curObject = Resources.Load<GameObject>("Prefabs/" + prefabName);
        if(curObject == null)
        {
            Debug.Log("Invalid Prefab.");
            return 1;
        }
        curObject.tag = "Player";
        //curObject.SetParent(GameObject.Find("Dynamic").transform);
        GameObject NewGameObj = Instantiate(curObject) as GameObject;

        //Apply all values to the new GameObject
        NewGameObj.name = objectName;
        NewGameObj.transform.position = new Vector3(xpos, ypos, zpos);

        //Debug.Log(NewGameObj.name + " initital color = " + NewGameObj.GetComponent<Renderer>().material.color);

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
            //Debug.Log(curObject.name + " pre-change color = " + curObject.GetComponent<Renderer>().material.color);

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
            else Debug.Log("Can not set value, component not attached or wrong name: " + component[0]);
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
            else if(cellName == "red" || cellName == "green" || cellName == "blue" || cellName == "alpha" || cellName == "transparency") StartCoroutine(dynSetRenderer(terms));
            else if(curComponent != null)
            {
                if(cellName.Contains("transform")) StartCoroutine(dynSetTransform(terms));
                if(cellName.Contains("renderer")) StartCoroutine(dynSetRenderer(terms));
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

    int path(string[] terms)
    {
        //initialize path to construct
        GameObject pathObject = new GameObject(terms[1]);
        pathObject.tag = "Player";
        pathObject.AddComponent<PathCreator>();
        PathCreator pathCreator = pathObject.GetComponent<PathCreator>() as PathCreator;
        BezierPath path = pathCreator.bezierPath;
        
        //start building on the new path with the first point of the first path component
        Vector3 curPoint = GameObject.Find(terms[3].Remove(0, 1)).GetComponent<PathCreator>().bezierPath.GetPointsInSegment(0)[0];
        path.AddSegmentToEnd(curPoint);

        //for each path component in terms
        for(int i = 3; i < terms.Count(); i++)
        {
            //get data for current path component
            int sign = Convert.ToInt32(terms[i][0] + "1");
            string pathName = terms[i].Remove(0, 1);
            BezierPath pathToAdd = GameObject.Find(pathName).GetComponent<PathCreator>().bezierPath;
            
            //Vector3 pos = GameObject.Find(pathName).transform.position;
            //Debug.Log(pathName + " has " + pathToAdd.NumAnchorPoints + " segments at " + pos);
            
            for(int j = 1; j < pathToAdd.NumAnchorPoints; j++)
            {
                //get points in current segment(0 and 3 are anchors, 1 and 2 are controls)
                Vector3[] points = pathToAdd.GetPointsInSegment(j);

                //get distance between anchor points in current segment
                Vector3 dist = (points[3] - points[0]) * sign;
                //Debug.Log("Dist = " + dist);
                    
                //add segment to constructed path by distance in current segment
                path.AddSegmentToEnd(curPoint + dist);

                //set control points in added segment
                path.SetPoint(path.NumPoints - 2, curPoint + ((points[2] - points[0]) * sign));
                path.SetPoint(path.NumPoints - 3, curPoint + ((points[1] - points[0]) * sign));

                //set current point to new point in constructed path
                curPoint = curPoint + dist;
                //Debug.Log("New curPoint = " + curPoint);
            }
        }

        //remove default segments(created when pathCreator initialized)
        path.DeleteSegment(0);
        path.DeleteSegment(1);

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

            //curFollower.globalTime = globalTime;
            float progress = (globalTime.currTime - curCommand.time) / Convert.ToSingle(terms[3]);
            //Debug.Log(progress);
            //curFollower.
            float distance = progress * curCreator.path.length;
            //Debug.Log(distance);
            curFollower.offsetPosition = distance;
            //curFollower.offsetPosition = curCreator.path.GetPointAtDistance(distance, curFollower.endOfPathInstruction);
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

    int pause(string[] terms)
    {
        //Debug.Log("Pausing...");
        pauseTime.PauseFunction();
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

        //Debug.Log(command);
        
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
                result = path(terms);
                break;
            case "MOVE":
                result = move(terms);
                break;
            case "REMOVEFROMPATH":
                result = removeFromPath(terms);
                break;
            case "PAUSE":
                result = pause(terms);
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
        //curObject = GameObject.Find("Car");
        //Debug.Log(curObject.GetComponent("Animator"));

        //get list for entire input file
        commandList = new CommandList(textPath);

        //initial list copy for reference
        CommandList temp = new CommandList(textPath);

        //add called function lines to the main list
        getReferenceFunctions(temp, mainFunction, 0);

        int linesRemoved = 0;
        string bounds = commandList.functions[mainFunction];
        //Debug.Log("Building " + mainFunction + ", bounds: " + bounds);
        int lower = Convert.ToInt32(bounds.Split('-')[0]);
        int upper = Convert.ToInt32(bounds.Split('-')[1]);
        
        //remove lines outside of main function and call lines
        for(int i = 0; i < temp.commands.Count(); i++)
        {
            if(i >= lower && i < upper)
            {
                Command curCommand = temp.commands[i];
                if(temp.commands[i].line.Contains("CALL")) 
                {
                    //Debug.Log("Removing " + commandList.commands[i - linesRemoved].line);
                    commandList.commands.RemoveAt(i - linesRemoved);
                    linesRemoved++;
                }
            }
            else 
            {
                commandList.commands.RemoveAt(i - linesRemoved);
                linesRemoved++;
            }
        }        

        commandList.commands = commandList.commands.OrderBy(n => n.time).ToList();
        //foreach(Command command in commandList.commands) Debug.Log(command.time + " " + command.line);
        //Debug.Log("________________________________________");

        //Debug.Log("0: " + commandList.getIndexAtTime(0) + "| 1500: " + commandList.getIndexAtTime(1500f));

        //GlobalTimeScript.runtime = commandList.commands[commandList.commands.Count - 1].time;
        globalTime.ResetSlider(commandList.commands[commandList.commands.Count - 1].time);
    }

    void getReferenceFunctions(CommandList curList, string functionName, float curTime)
    {
        string bounds = curList.functions[functionName];
        //Debug.Log(bounds);
        int lower = Convert.ToInt32(bounds.Split('-')[0]);
        int upper = Convert.ToInt32(bounds.Split('-')[1]);
        int i;
        for(i = lower; i < upper; i++)
        {
            Command curCommand = curList.commands[i];
            //Debug.Log(i + ": " + curCommand.line);

            if(curCommand.line.Contains("CALL"))
            {
                //Debug.Log(curCommand.line + " in curList...");
                foreach(string fileInfo in curList.referenceList)
                {
                    CommandList reference = new CommandList("Assets/Text Files/" + fileInfo);
                    //Debug.Log("Opening " + fileInfo + " for referencing...");

                    if(reference.functions.ContainsKey(curCommand.args[1]))
                    {
                        //Debug.Log("Getting reference lines...");
                        string refBounds = reference.functions[curCommand.args[1]];
                        //Debug.Log(refBounds);
                        int refLower = Convert.ToInt32(refBounds.Split('-')[0]);
                        int refUpper = Convert.ToInt32(refBounds.Split('-')[1]);
                        for(int j = refLower; j < refUpper; j++)
                        {
                            Command refCommand = reference.commands[j];
                            //Debug.Log(j + ": " + refCommand.line);

                            if(refCommand.line.Contains("CALL")) 
                            {
                                //Debug.Log("Recurse");
                                getReferenceFunctions(reference, curCommand.args[1], curCommand.time + curTime);
                            }
                            else 
                            {
                                refCommand.time += curTime + curCommand.time;
                                //Debug.Log("Adding " + refCommand.time + " " + refCommand.line);
                                commandList.commands.Add(refCommand);
                            }
                        }
                    }
                    else Debug.Log("Invalid function name.");
                }
            }
        }
    }

    IEnumerator waitOneFrame()
    {
        //Debug.Log("Before while...");
        while(true)
        {
            //Debug.Log("In while...");
            if(!waited) 
            {
                //Debug.Log("Waited one frame...");
                waited = true;
                yield return null;
            }
            else
            {
                //Debug.Log("Done waiting.");
                waited = false;
                //execute commands up to current index
                index = commandList.getIndexAtTime(globalTime.currTime);
                for(int i = 0; i < index; i++)
                {
                    curCommand = commandList.commands[i];
                    if(curCommand.args[0] != "PAUSE") 
                    {  
                        if(curCommand.args[0] == "MOVE")
                        {

                        }

                        //start cell updates at current time
                        if(curCommand.args[0] == "DYNUPDATECELL") 
                        {
                            float offset = globalTime.currTime - curCommand.time;
                            float percentComplete = offset / Convert.ToSingle(curCommand.args[3]);
                            
                            string[] newArgs = new string[12];

                            //if duration already passed
                            if(percentComplete >= 1)
                            {
                                //call as SETOBJCELL
                                //Debug.Log("Past duration: " + offset + " > " + Convert.ToSingle(curCommand.args[3]));
                                
                                newArgs[1] = curCommand.args[1];
                                newArgs[2] = curCommand.args[2];

                                if(curCommand.args.Count() == 12) 
                                {   
                                    newArgs[3] = curCommand.args[9];
                                    newArgs[4] = curCommand.args[10];
                                    newArgs[5] = curCommand.args[11];
                                }
                                else newArgs[3] = curCommand.args[7];

                                string line = "";
                                foreach(string arg in newArgs) line += arg + " ";
                                Debug.Log("SETOBJCELL" + line);
                                setCell(newArgs);
                            }
                            else
                            {
                                //call with adjusted partial values
                                //Debug.Log("Within duration: " + offset + " < " + Convert.ToSingle(curCommand.args[3]));
                                
                                newArgs[1] = curCommand.args[1];
                                newArgs[2] = curCommand.args[2];
                                newArgs[4] = curCommand.args[4];

                                float offsetDuration = Convert.ToSingle(curCommand.args[3]) * (1 - percentComplete);
                                if(offsetDuration > 0) newArgs[3] = offsetDuration.ToString();

                                if(curCommand.args.Count() == 12) 
                                {
                                    float offsetVal1 = Convert.ToSingle(curCommand.args[5]) + ((Convert.ToSingle(curCommand.args[9]) - Convert.ToSingle(curCommand.args[5])) * percentComplete);
                                    float offsetVal2 = Convert.ToSingle(curCommand.args[6]) + ((Convert.ToSingle(curCommand.args[10]) - Convert.ToSingle(curCommand.args[6])) * percentComplete);
                                    float offsetVal3 = Convert.ToSingle(curCommand.args[7]) + ((Convert.ToSingle(curCommand.args[11]) - Convert.ToSingle(curCommand.args[7])) * percentComplete);
                                
                                    newArgs[5] = offsetVal1.ToString();
                                    newArgs[6] = offsetVal2.ToString();
                                    newArgs[7] = offsetVal3.ToString();
                                    newArgs[8] = curCommand.args[8];
                                    newArgs[9] = curCommand.args[9];
                                    newArgs[10] = curCommand.args[10];
                                    newArgs[11] = curCommand.args[11];
                                }
                                else
                                {
                                    float offsetVal = Convert.ToSingle(curCommand.args[5]) + ((Convert.ToSingle(curCommand.args[7]) - Convert.ToSingle(curCommand.args[5])) * percentComplete);

                                    newArgs[5] = offsetVal.ToString();
                                    newArgs[6] = curCommand.args[6];
                                    newArgs[7] = curCommand.args[7];
                                }

                                string line = "";
                                foreach(string arg in newArgs) line += arg + " ";
                                Debug.Log("DYNUPDATECELL" + line);
                                dynSetCell(newArgs);
                            }
                        }
                        else execute(curCommand.line, curCommand.args);

                        //set animator to current time
                        if(curCommand.line.Contains("Animator")) 
                        {
                            Animator anim = GameObject.Find(curCommand.args[1]).GetComponent<Animator>();
                            anim.Update(globalTime.currTime - curCommand.time);
                        }
                    }
                }
                break;
            }
        }
        //Debug.Log("After while...");
    }

    // Update is called once per frame
    void Update()
    {
        if(!globalTime.isBeingControlledByUser)
        {
            if(globalTime.timeChanged)
            {
                Debug.Log("Time change detected.");
                if(PauseScript.paused) pauseTime.PauseFunction();

                //clear created objects
                GameObject[] createdObjects = GameObject.FindGameObjectsWithTag("Player");
                foreach(GameObject obj in createdObjects) 
                {
                    //Debug.Log("Deleting " + obj.name);
                    if(obj != null) Destroy(obj);
                }

                //Debug.Log("Calling waitoneframe...");
                StartCoroutine(waitOneFrame());
/*
                //execute commands up to current index
                index = commandList.getIndexAtTime(globalTime.currTime);
                for(int i = 0; i < index; i++)
                {
                    curCommand = commandList.commands[i];
                    if(curCommand.args[0] != "PAUSE") execute(curCommand.line, curCommand.args);
                }
*/
                globalTime.timeChanged = false;

            }

            if(index < commandList.commands.Count)
            {
                //Get the current command
                curCommand = commandList.commands[index];

                //if(Time.time > curCommand.time)
                if(globalTime.currTime > curCommand.time)
                {
                    execute(curCommand.line, curCommand.args);
                    index++;
                }
            }
        }
        else 
        {
            //Debug.Log("Slider in use...");
            //difference = index - commandList.getIndexAtTime(globalTime.currTime);
            //index = commandList.getIndexAtTime(globalTime.currTime);
            //timeChanged = true;
        }
    }
}