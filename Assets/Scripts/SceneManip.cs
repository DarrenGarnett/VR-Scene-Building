using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using PathCreation;
using PathCreation.Examples;
using Dreamteck.Splines;

public class Command
{
    public string line;
    public string[] args;
    public float time;
    public float timeScale;
}

public class CommandList
{
    public Dictionary<string, string> functions = new Dictionary<string, string>();
    public List<string> referenceList = new List<string>();
    public List<Command> commands = new List<Command>();

    public CommandList(string file)
    {
        float scale = 1;
        initList(file, scale);
    }

    public CommandList(string file, float scale)
    {
        initList(file, scale);
    }
    
    private void initList(string file, float scale)
    {
        //if(File.Exists(Application.streamingAssetsPath + "/Text Files/" + file)) 
        if(File.Exists(ArchiveManager.textPath + file))
        {
            //string[] input = System.IO.File.ReadAllLines(Application.streamingAssetsPath + "/Text Files/" + file);
            string[] input = System.IO.File.ReadAllLines(ArchiveManager.textPath + file);

            List<string> lines = new List<string>();

            foreach(string command in input)
            {
                if(!command.Contains("//") && command != "") lines.Add(command);
            }

            referenceList.Add(file);

            //float curTime = 0, curScale = 1, prevScale = 1, scaleChangeTime = -1;
            float curTime = 0, maxTime = 0;
            int curLine = 0, startLine = 0;
            bool endToFunction = false;
            foreach(string line in lines)
            {
                Command temp = new Command();
                temp.line = line;
                temp.args = line.Split(' ');
                

                if(line.Split(' ')[0] == "TIME") 
                {
                    curTime = Convert.ToSingle(temp.args[1]);
                    if(curTime >= maxTime) maxTime = curTime;
                }
                /*else if(line.Contains("TIMESCALE")) 
                {
                    scaleChangeTime = curTime;
                    //Debug.LogError("Time scale changed at " + scaleChangeTime);
                    prevScale = curScale;
                    curScale = Convert.ToSingle(temp.args[1]);
                }*/
                else if(line.Contains("INCLUDE")) referenceList.Add(temp.args[1]);
                else if(line.Contains("FUNCTION")) 
                {
                    startLine = curLine;
                }
                else if(line.Contains("END")) 
                {
                    endToFunction = true;
                    functions.Add(temp.args[1], startLine.ToString() + "-" + curLine.ToString() + "-" + maxTime);
                    curTime = 0;
                    maxTime = 0;
                }
                else 
                {
                    //if(scaleChangeTime >= 0) temp.time = ((curTime - scaleChangeTime) / curScale) + scaleChangeTime;
                    //else temp.time = curTime;
                    //Debug.LogError("((" + curTime + "-" + scaleChangeTime + ") / " + curScale + ") + (" + scaleChangeTime + " / " + prevScale + ")");
                    //temp.time = ((curTime - scaleChangeTime) / curScale) + (scaleChangeTime / prevScale);
                    temp.time = curTime / scale;
                    temp.timeScale = scale;
                    //Debug.LogError("S:" + temp.timeScale + " " + temp.time + ": " + temp.line);

                    commands.Add(temp);
                    curLine++;
                }
            }

            if(!endToFunction) 
            {
                if(file == "main.txt")
                {
                    //Debug.Log("assigning default bounds to main");
                    functions.Add("main", startLine.ToString() + "-" + curLine.ToString() + "-" + maxTime);
                }
                else 
                {
                    //Debug.Log("assigning default bounds to " + file.Remove(file.Length - 4));
                    functions.Add(file.Remove(file.Length - 4), startLine.ToString() + "-" + curLine.ToString() + "-" + maxTime);
                }
            }

            //GlobalTimeScript.ResetSlider(maxTime);

            //commandList.commands = commandList.commands.OrderBy(n => n.time).ToList();
            //foreach(Command command in commands) Debug.LogError(command.time + " " + command.line);
            //foreach(KeyValuePair<string, string> kvp in functions) Debug.LogError("Function name: " + kvp.Key + "\nFuntion bounds: " + kvp.Value);
        }
        else Debug.LogError("Could not open " + file);
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
    public SwitchCameraScript switchCamera;
    private GameObject curObject;
    //private string[] commands;
    public int index = 0;
    private Command curCommand;
    public CommandList commandList;
    private bool waited = false;
    private float sceneDuration = 0;
    public PathManager pathManager;

    void setCurGameObject(string name)
    {
        name = name.Replace('.', '/');
        curObject = GameObject.Find(name);
    }

    Vector3 changeVec(Vector3 inVec, string[] terms)
    {
        string[] component = terms[2].Split('.');

        //store surrent values to only displace necessary fields
        Vector3 curVec = inVec;

        //given one value to change
        if(component.Count() == 3)
        {
            if(component[2] == "x") curVec.x = Convert.ToSingle(terms[3]);
            if(component[2] == "y") curVec.y = Convert.ToSingle(terms[3]);
            if(component[2] == "z") curVec.z = Convert.ToSingle(terms[3]);
        }
        //given three values to change
        else if(terms.Count() == 6) curVec = new Vector3(Convert.ToSingle(terms[3]), Convert.ToSingle(terms[4]), Convert.ToSingle(terms[5]));
        //built-in alternate term options
        else if(terms[2] == "length") curVec.x = Convert.ToSingle(terms[3]);
        else if(terms[2] == "height") curVec.y = Convert.ToSingle(terms[3]);
        else if(terms[2] == "width") curVec.z = Convert.ToSingle(terms[3]);

        return curVec;
    }

    void setTransform(string[] terms)
    {
        string[] component = terms[2].Split('.');

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
                    Debug.LogError("Invalid transform variable.");
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
                    Debug.LogError("Invalid transform variable.");
                    break;
            }
        }
    }

    float blend(float percentComplete, float init, float final)
    {
        return (percentComplete * (final - init)) + init;
    }

    Vector3 dynChangeVec(GameObject dynObject, Vector3 inVec, string[] terms, float startTime, float duration)
    {
        string[] component = terms[2].Split('.');
        //float duration = Convert.ToSingle(terms[3]);

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
        else Debug.LogError("Unrecognized transform component(s).");
        
        return curVec;
    }

    IEnumerator dynSetTransform(string[] terms, float timeScale)
    {
        //Debug.LogError("Dynamically updating transform...");
        string[] component = terms[2].Split('.');
        //foreach(string part in component) Debug.LogError(part);

        float duration = Convert.ToSingle(terms[3]) / timeScale;
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
                        if(dynObject != null) dynObject.transform.position = dynChangeVec(dynObject, dynObject.transform.position, terms, startTime, duration);
                        yield return null;
                    }
                    break;
                case "rotation":
                    while(globalTime.currTime - startTime < duration)
                    {
                        if(dynObject != null) dynObject.transform.eulerAngles = dynChangeVec(dynObject, dynObject.transform.eulerAngles, terms, startTime, duration);
                        yield return null;
                    }
                    break;
                case "scale":
                    while(globalTime.currTime - startTime < duration)
                    {
                        if(dynObject != null) dynObject.transform.localScale = dynChangeVec(dynObject, dynObject.transform.localScale, terms, startTime, duration);
                        yield return null;
                    }
                    break;
                default:
                    Debug.LogError("Invalid transform variable.");
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
                        if(dynObject != null) dynObject.transform.localScale = dynChangeVec(dynObject, dynObject.transform.localScale, terms, startTime, duration);
                        yield return null;
                    }
                    break;
                default:
                    Debug.LogError("Invalid transform variable.");
                    break;
            }
        }
    }

    void setAnimator(string[] terms, float timeScale)
    {
        //AnimationHandler is used to pause animations alongside the scene
        //If there is no AnimationHandler component on the object, add it
        if(!curObject.TryGetComponent<AnimationHandler>(out AnimationHandler animHandler)) curObject.AddComponent<AnimationHandler>();
        curObject.GetComponent<AnimationHandler>().speed = timeScale;

        //Check for too few terms
        if(terms.Count() < 4)
        {
            Debug.LogError("Too few terms to set Animator.");
            return;
        }
        //Setting animator with no avatar
        else
        {
            //Ensure the given animator controller is valid
            RuntimeAnimatorController RAC = Resources.Load("Animators/" + terms[3]) as RuntimeAnimatorController;
            if(RAC == null)
            {
                Debug.LogError("Cannot find '" + terms[3] + "' in Resources/Animators folder.");
                return;
            }

            //If there is an Animator component on the object, use it, else add an Animator component
            if(curObject.TryGetComponent<Animator>(out Animator curAnimator)) 
            {
                curAnimator.applyRootMotion = true;
                curAnimator.runtimeAnimatorController = RAC;
            }
            else
            {
                Animator newAnimator = curObject.AddComponent<Animator>();
                newAnimator.applyRootMotion = true;
                newAnimator.runtimeAnimatorController = RAC;
            }
        }
    }

    void setRenderer(string[] terms)
    {
        Renderer curRenderer = curObject.GetComponent<Renderer>();
        if(curRenderer == null)
        {
            Debug.LogError(terms[1] + " does not have a renderer. Can not set renderer value.");
            return;
        }

        //initialize as current color
        Color newColor = curRenderer.material.color;
        
        //switch material to mode that allows transparency
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

        //built-in term options
        if(value == "red") newColor.r = Convert.ToSingle(terms[3]);
        else if(value == "green") newColor.g = Convert.ToSingle(terms[3]);
        else if(value == "blue") newColor.b = Convert.ToSingle(terms[3]);
        else if(value == "alpha" || value == "transparency") newColor.a = Convert.ToSingle(terms[3]);
        else Debug.LogError("Invalid renderer variable.");

        curRenderer.material.color = newColor;
    }

    IEnumerator dynSetRenderer(string[] terms, float timeScale)
    {
        float duration = Convert.ToSingle(terms[3]) / timeScale;
        float startTime = globalTime.currTime;

        Renderer curRenderer = curObject.GetComponent<Renderer>();
        if(curRenderer == null)
        {
            Debug.LogError(terms[1] + " does not have a renderer. Can not set renderer value.");
            yield break;
        }

        //initialize as current color
        Color newColor = curRenderer.material.color;
        
        //switch material to mode that allows transparency
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

        while(globalTime.currTime - startTime < duration)
        {
            float timeSinceStart = globalTime.currTime - startTime;
            float percentComplete = timeSinceStart / duration;

            if(curRenderer != null) 
            {
                //start from current color
                newColor = curRenderer.material.color;

                //built-in term options
                if(value == "red") newColor.r = blend(percentComplete, Convert.ToSingle(terms[5]), Convert.ToSingle(terms[7]));
                else if(value == "green") newColor.g = blend(percentComplete, Convert.ToSingle(terms[5]), Convert.ToSingle(terms[7]));
                else if(value == "blue") newColor.b = blend(percentComplete, Convert.ToSingle(terms[5]), Convert.ToSingle(terms[7]));
                else if(value == "alpha" || value == "transparency") newColor.a = blend(percentComplete, Convert.ToSingle(terms[5]), Convert.ToSingle(terms[7]));
                else Debug.LogError("Invalid renderer variable.");

                curRenderer.material.color = newColor;
            }
            else Debug.LogError("Renderer is null.");

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
            Debug.LogError("Too few terms to create object.");
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
        
        
        //Get given prefab
        curObject = Resources.Load<GameObject>("Prefabs/" + prefabName);
        if(curObject == null)
        {
            Debug.LogError("Invalid Prefab.");
            return 1;
        }

        //Instantiate prefab as new object under the parent
        GameObject parent = GameObject.FindGameObjectWithTag("CreateParent");
        GameObject newGameObj = Instantiate(curObject, parent.transform) as GameObject;

        //Check if new object has a renderer, and add on if not
        newGameObj.TryGetComponent<MeshRenderer>(out MeshRenderer rend);
        if(rend == null) rend = newGameObj.AddComponent<MeshRenderer>() as MeshRenderer;

        //Mark object as created during runtime
        newGameObj.tag = "Runtime";

        //Apply all values to the new GameObject
        newGameObj.name = objectName;
        newGameObj.transform.position = curObject.transform.position + new Vector3(xpos, ypos, zpos);
        newGameObj.transform.rotation = curObject.transform.rotation;

        switchCamera.addTarget(newGameObj);

        return 0;
    }

    //Replaces the current scene with an empty one
    int unload(string[] terms)
    {
        SceneManager.LoadScene(sceneName: "Empty");
        
        return 0;
    }

    int setCell(string[] terms, float timeScale)
    {
        if(terms.Count() < 4)
        {
            Debug.LogError("Too few terms to set cell value.");
            return 1;
        }
        else
        {
            //curObject = GameObject.Find(terms[1]);
            setCurGameObject(terms[1]);

            if(curObject != null)
            {
                string[] component = terms[2].Split('.');
                string cellName = component[0].ToLower();

                if(cellName == "width" || cellName == "length" || cellName == "height") setTransform(terms);
                else if(cellName == "red" || cellName == "green" || cellName == "blue" || cellName == "alpha" || cellName == "transparency") setRenderer(terms);
                
                if(cellName.Contains("transform")) setTransform(terms);
                else if(cellName.Contains("animator")) setAnimator(terms, timeScale);
                else if(cellName.Contains("renderer")) setRenderer(terms);
            }
            else Debug.LogError("Unable to find '" + terms[1] + "' in '" + curCommand.line + "'");
        }
        return 0;
    }

    int dynSetCell(string[] terms, float timeScale)
    {
        if(terms.Count() < 8)
        {
            Debug.LogError("Too few terms to set cell value over time.");
            return 1;
        }
        else
        {
            //curObject = GameObject.Find(terms[1]);
            setCurGameObject(terms[1]);

            if(curObject != null)
            {
                string[] component = terms[2].Split('.');
                string cellName = component[0].ToLower();
                
                if(cellName == "width" || cellName == "length" || cellName == "height") StartCoroutine(dynSetTransform(terms, timeScale));
                else if(cellName == "red" || cellName == "green" || cellName == "blue" || cellName == "alpha" || cellName == "transparency") StartCoroutine(dynSetRenderer(terms, timeScale));
                else if(cellName.Contains("transform")) StartCoroutine(dynSetTransform(terms, timeScale));
                else if(cellName.Contains("renderer")) StartCoroutine(dynSetRenderer(terms, timeScale));
                else Debug.LogError("Invalid component '" + cellName+ "' in '" + curCommand.line + "'");
            }
            else Debug.LogError("Unable to find '" + terms[1] + "' in '" + curCommand.line + "'");
        }
        return 0;
    }

    //Destroys the specified object
    int destroy(string[] terms)
    {
        //Test for minimum terms
        if(terms.Count() < 2)
        {
            Debug.LogError("Too few terms to destroy object.");
            return 1;
        }
        else
        {
            //curObject = GameObject.Find(terms[1]);
            setCurGameObject(terms[1]);
            switchCamera.removeTarget(curObject);
            Destroy(curObject);
        }

        return 0;
    }

    int path(string[] terms)
    {
        bool segmentsValid = true;
        for(int i = 2; i < terms.Count(); i++)
        {
            string pathPath = terms[i].Remove(0, 1);
            if(GameObject.Find(pathPath) == null)
            {
                Debug.LogError("Invalid path segment '" + terms[i].Remove(0, 1) + "' in '" + curCommand.line + "'");
                segmentsValid = false;
                break;
            }
        }

        if(segmentsValid)
        {
            //initialize path to construct
            GameObject pathObject = new GameObject(terms[1]);
            pathObject.tag = "Runtime";

            GameObject parent = GameObject.FindGameObjectWithTag("PathParent");
            pathObject.transform.parent = parent.transform;

            //pathManager.UpdatePaths();

            pathObject.AddComponent<PathCreator>();
            PathCreator pathCreator = pathObject.GetComponent<PathCreator>() as PathCreator;
            BezierPath path = pathCreator.bezierPath;

            //start building on the new path from the path origin
            Vector3 curPoint = new Vector3(0, 0, 0);
            path.AddSegmentToEnd(curPoint);

            //for each path component in terms
            for(int i = 2; i < terms.Count(); i++)
            {
                //get data for current path component
                int sign = Convert.ToInt32(terms[i][0] + "1");
                string pathName = terms[i].Remove(0, 1);
                BezierPath pathToAdd = GameObject.Find(pathName).GetComponent<PathCreator>().bezierPath;
                
                path.GlobalNormalsAngle = pathToAdd.GlobalNormalsAngle;
                
                for(int j = 0; j < pathToAdd.NumAnchorPoints - 1; j++)
                {
                    //get points in current segment(0 and 3 are anchors, 1 and 2 are controls)
                    Vector3[] points = pathToAdd.GetPointsInSegment(j);
                    //foreach(Vector3 point in points) Debug.Log(point);

                    //get distance between anchor points in current segment
                    Vector3 dist = (points[3] - points[0]) * sign;
                    //Debug.Log("Distance in segment " + j + ": " + dist);
                        
                    //add segment to constructed path by distance in current segment
                    path.AddSegmentToEnd(curPoint + dist);

                    //set control points in added segment
                    path.SetPoint(path.NumPoints - 2, curPoint + ((points[2] - points[0]) * sign));
                    path.SetPoint(path.NumPoints - 3, curPoint + ((points[1] - points[0]) * sign));

                    //set current point to new point in constructed path
                    curPoint = curPoint + dist;
                }
            }

            //remove default segments(created when pathCreator initialized)
            path.DeleteSegment(0);
            path.DeleteSegment(1);

            if(SettingsManager.pathsVisible) pathCreator.DrawPath();
        }

        return 0;
    }

    //Puts the specified object on the specified path
    int move(string[] terms, float timeScale)
    {
        //Test for minimum terms
        if(terms.Count() < 4)
        {
            Debug.LogError("Too few terms to move object.");
            return 1;
        }
        else if(!GameObject.Find(terms[2]))
        {
            Debug.LogError("Path '" + terms[2] + "' is not in the scene.");
        }
        else
        {
            //Initialize path script
            //curObject = GameObject.Find(terms[1]);
            setCurGameObject(terms[1]);
            if(curObject)
            {
                //Set SplinFollower Component to curObject
                SplineFollower splineFollower = curObject.AddComponent<SplineFollower>();

                SplineComputer splineComputer = GameObject.Find(terms[2]).GetComponent<SplineComputer>();
                if (splineComputer == null)
                {
                    Debug.LogError("SplineComputer component not found on path object.");
                    return 1;
                }

                splineFollower.spline = splineComputer;
                splineFollower.followSpeed = 1.0f / timeScale; // Adjust based on your timeScale logic
                splineFollower.wrapMode = SplineFollower.Wrap.Default;


                /*                //Get PathFollower component if there is one, and add one if there isn't
                                curObject.TryGetComponent<PathFollower>(out PathFollower curFollower);
                                if(curFollower == null) curFollower = curObject.AddComponent<PathFollower>() as PathFollower;
                                else curFollower.pathChanged = true;
                                curFollower.enabled = true;

                                //Assign path by name
                                GameObject pathObject = GameObject.Find(terms[2]);
                                PathCreator curCreator = pathObject.GetComponent<PathCreator>() as PathCreator;

                                curFollower.pathCreator = curCreator;

                                //Set cycle duration
                                curFollower.cycleDuration = Convert.ToSingle(terms[3]) / timeScale;

                                curFollower.endOfPathInstruction = EndOfPathInstruction.Stop;

                                float progress = (globalTime.currTime - curCommand.time) / Convert.ToSingle(terms[3]);
                                float distance = progress * curCreator.path.length;

                                curFollower.offsetPosition = distance;

                                if(terms.Count() >= 8)
                                {
                                    Vector3 initAngles = new Vector3(Convert.ToSingle(terms[5]), Convert.ToSingle(terms[6]), Convert.ToSingle(terms[7]));
                                    curFollower.offsetRotation = Quaternion.Euler(initAngles);
                                }

                                if(SettingsManager.pathsVisible) curCreator.DrawPath();
                                pathObject.tag = "ActivePath";*/
            }
            else Debug.LogError("MOVE: " + terms[1] + " was not found in the scene.");
        }
        return 0;
    }

    //Removes the specified object from its current path
    int removeFromPath(string[] terms)
    {
        //Test for minimum terms
        if(terms.Count() < 2)
        {
            Debug.LogError("Too few terms to remove path.");
            return 1;
        }
        else
        {
            //curObject = GameObject.Find(terms[1]);
            setCurGameObject(terms[1]);
            
            PathFollower curFollower = curObject.GetComponent<PathFollower>();
            curFollower.distanceTravelled = curFollower.offsetPosition;

            //Ensure object is following a path
            if(curFollower != null) curFollower.pathCreator = null;
            else Debug.LogError(terms[1] + " is not following a path.");
        }
        
        return 0;
    }

    int pause(string[] terms)
    {
        //Debug.LogError("Pausing...");
        //pauseTime.PauseFunction();
        PauseScript.PauseFunction();
        return 0;
    }

    int setTimescale(string[] terms)
    {
        PlaybackSpeedScript.currPlaySpeed = Convert.ToSingle(terms[1]);

        //The 100 is arbitrary, just needs to be more than the options in the dropdown, i.e. > 8
        PlaybackSpeedScript.UpdatePlaybackSpeed(100);
        return 0;
    }

    int setCamera(string[] terms)
    {
        //curObject = GameObject.Find(terms[1]);
        setCurGameObject(terms[1]);
        Renderer curRend = curObject.GetComponent<Renderer>();

        if(curObject != null)
        {
            if(curRend != null) switchCamera.ChangeTarget(curObject);
            else
            {
                Debug.LogError(terms[1] + " requires a renderer for bounds calculations.");
                return 1;
            }
        }
        else
        {
            Debug.LogError(terms[1] + " does not exist or is misnamed.");
            return 1;
        }
        return 0;
    }
    
    //Determines which command is to be executed
    int execute(string command, string[] terms, float timeScale)
    {
        //Initialize return value
        int result = 0;

        //Debug.LogError(command);
        
        //Determine command by name
        switch(terms[0])
        {
            case "CREATE":
                result = create(terms);
                break;
            case "END":
                //result = unload(terms);
                break;
            case "SETOBJCELL":
                result = setCell(terms, timeScale);
                break;
            case "DESTROY":
                result = destroy(terms);
                break;
            case "DYNUPDATECELL":
                result = dynSetCell(terms, timeScale);
                break;
            case "PATH":
                result = path(terms);
                break;
            case "MOVE":
                result = move(terms, timeScale);
                break;
            case "REMOVEFROMPATH":
                result = removeFromPath(terms);
                break;
            case "PAUSE":
                result = pause(terms);
                break;
            case "TIMESCALE":
                //result = setTimescale(terms);
                break;
            case "LOOKAT":
                result = setCamera(terms);
                break;
            default:
                Debug.LogError("Unrecognized command: '" + terms[0] + "'");
                result = -1;
                break;
        }
        return result;
    }

    public void initCommandList()
    {
        //get list for entire input file
        commandList = new CommandList("main.txt");

        //initial list copy for reference
        CommandList temp = new CommandList("main.txt");

        //add called function lines to the main list
        getReferenceFunctions(temp, "main", 0, 1);

        int linesRemoved = 0;
        string bounds = commandList.functions["main"];
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
                    //Debug.LogError("Removing " + commandList.commands[i - linesRemoved].line);
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
        //foreach(Command c in commandList.commands) Debug.LogError(c.time + ": " + c.line);
        
        //globalTime.ResetSlider(commandList.commands[commandList.commands.Count - 1].time);
        if(sceneDuration == 0) globalTime.ResetSlider(Convert.ToSingle(bounds.Split('-')[2]));//GlobalTimeScript.ResetSlider(Convert.ToSingle(bounds.Split('-')[2]));
        else globalTime.ResetSlider(sceneDuration);//GlobalTimeScript.ResetSlider(sceneDuration);

        index = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        initCommandList();
        //Debug.Log(GlobalTimeScript.runtime);

        if(!PauseScript.paused)PauseScript.PauseFunction();
    }

    void getReferenceFunctions(CommandList curList, string functionName, float curTime, float curMult)
    {
        string bounds = curList.functions[functionName];
        //Debug.LogError(bounds);

        int lower = Convert.ToInt32(bounds.Split('-')[0]);
        int upper = Convert.ToInt32(bounds.Split('-')[1]);
        float duration = Convert.ToSingle(bounds.Split('-')[2]);

        int i;
        for(i = lower; i < upper; i++)
        {
            Command curCommand = curList.commands[i];
            //Debug.LogError(i + ": " + curCommand.line);

            if(curCommand.line.Contains("CALL"))
            {
                //Debug.LogError(curCommand.line + " in curList...");
                bool functionFound = false;
                foreach(string fileInfo in curList.referenceList)
                {
                    //Debug.Log("In " + fileInfo + " looking for " + curCommand.args[1]);
                    float timeMult = curMult;
                    if(curCommand.args.Count() >= 3) timeMult *= Convert.ToSingle(curCommand.args[2]);
                    //Debug.LogError("Reference mult = " + timeMult);
                    CommandList reference = new CommandList(fileInfo, timeMult);
                    //Debug.Log("Opening " + fileInfo + " for referencing...");

                    if(reference.functions.ContainsKey(curCommand.args[1]))
                    {
                        //Debug.LogError("Getting reference lines...");

                        string refBounds = reference.functions[curCommand.args[1]];
                        //Debug.LogError(refBounds);
                        int refLower = Convert.ToInt32(refBounds.Split('-')[0]);
                        int refUpper = Convert.ToInt32(refBounds.Split('-')[1]);
                        float refDuration = Convert.ToSingle(refBounds.Split('-')[2]);

                        //Debug.Log("ct = " + curTime + " | di = " + duration + " | rd = " + refDuration);
                        //GlobalTimeScript.ResetSlider(duration + curTime + refDuration);
                        if(sceneDuration < (duration + curTime + refDuration)) sceneDuration = duration + curTime + refDuration;
                        //Debug.Log("Total duration = " + sceneDuration);

                        for(int j = refLower; j < refUpper; j++)
                        {
                            Command refCommand = reference.commands[j];
                            //Debug.LogError(j + ": " + refCommand.line);

                            if(refCommand.line.Contains("CALL")) 
                            {
                                //Debug.LogError("Recurse");
                                getReferenceFunctions(reference, curCommand.args[1], curCommand.time + curTime, timeMult);
                            }
                            else 
                            {
                                //refCommand.time /= timeMult;
                                refCommand.time += curTime + curCommand.time;
                                //Debug.LogError("Adding " + refCommand.time + " " + refCommand.line);
                                commandList.commands.Add(refCommand);
                            }
                        }
                        
                        //Debug.LogError(curCommand.args[1] + " found in " + fileInfo);
                        functionFound = true;
                        break;
                    }
                }
                
                if(!functionFound) Debug.LogError("Couldn't find called function '" + curCommand.args[1] + "' in included files.");
            }
        }
    }

    IEnumerator waitOneFrame()
    {
        while(true)
        {
            if(!waited) 
            {
                //Debug.LogError("Waited one frame...");
                waited = true;
                yield return null;
            }
            else
            {
                //Debug.LogError("Catch up started.");
                waited = false;

                //Reset follow camera target list
                switchCamera.clearTargets();

                //Execute commands up to current index
                index = commandList.getIndexAtTime(globalTime.currTime);
                for(int i = 0; i < index; i++)
                {
                    curCommand = commandList.commands[i];
                    if(curCommand.args[0] != "PAUSE") 
                    {  
                        //Start cell updates at current time
                        if(curCommand.args[0] == "DYNUPDATECELL") 
                        {
                            float offset = globalTime.currTime - curCommand.time;
                            float percentComplete = offset / Convert.ToSingle(curCommand.args[3]);
                            
                            string[] newArgs = new string[12];

                            //If duration already passed
                            if(percentComplete >= 1)
                            {
                                //Call as SETOBJCELL(make change instant)
                                //Debug.LogError("Past duration: " + offset + " > " + Convert.ToSingle(curCommand.args[3]));
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
                                //Debug.LogError("SETOBJCELL" + line);
                                setCell(newArgs, curCommand.timeScale);
                            }
                            else
                            {
                                //Call with adjusted partial values
                                //Debug.LogError("Within duration: " + offset + " < " + Convert.ToSingle(curCommand.args[3]));
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
                                //Debug.LogError("DYNUPDATECELL" + line);
                                dynSetCell(newArgs, curCommand.timeScale);
                            }
                        }
                        else execute(curCommand.line, curCommand.args, curCommand.timeScale);

                        //Set animator to current time
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
    }

    public static void clearScene()
    {
        //Reset follow camera target list
        //switchCamera.clearTargets();

        GameObject[] createdObjects = GameObject.FindGameObjectsWithTag("Runtime");
        foreach(GameObject obj in createdObjects) 
        {
            //Debug.LogError("Deleting " + obj.name);
            if(obj != null) Destroy(obj);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //If the time slider is not currently being controlled
        if(!globalTime.isBeingControlledByUser)
        {
            //If the time slider was just changed, start the time change process
            if(globalTime.timeChanged)
            {
                //Always upause time upon time slider change
                //Debug.LogError("Time change detected.");
                //if(PauseScript.paused) pauseTime.PauseFunction();
                if(PauseScript.paused)PauseScript.PauseFunction();
/*
                //Mark created objects for destruction(happens at the end of current frame)
                GameObject[] createdObjects = GameObject.FindGameObjectsWithTag("Runtime");
                foreach(GameObject obj in createdObjects) 
                {
                    //Debug.LogError("Deleting " + obj.name);
                    if(obj != null) Destroy(obj);
                }
*/
                clearScene();

                //Have to wait one frame for old objects to destroy, and then execute commands
                StartCoroutine(waitOneFrame());

                globalTime.timeChanged = false;
            }

            //if in the range of commands and not in the command catch up process
            if(index < commandList.commands.Count && !waited)
            {
                //Get the current command
                curCommand = commandList.commands[index];

                //Execute it and move to the next one if it is time
                if(globalTime.currTime >= curCommand.time && !PauseScript.paused)
                {
                    execute(curCommand.line, curCommand.args, curCommand.timeScale);
                    index++;
                }
            }
        }
    }
}