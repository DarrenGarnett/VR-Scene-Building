using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using System;
using System.Linq;
using Dummiesman;

public class ArchiveManager : MonoBehaviour
{
    public static string textPath;
    public static List<string> textFiles = new List<string>();
    public static string rootPath;
    private bool inUnityEditor;

    public static List<GameObject> prefabs = new List<GameObject>();
    public ListUtil prefabList;
    public GameObject listButton;
    GameObject curPrefab;
    Bounds curBounds;
    public GameObject previewCameraObj;
    Camera previewCamera;
    Vector3 previewAreaOrigin = new Vector3(0, 1000, 0);
    LineRenderer lineRend;

    public TMP_Dropdown listDropdown;

    void OnEnable()
    {
        textPath = Application.streamingAssetsPath + "/Text Files/";
        MoveTextFiles();
    }

    void Start()
    {
        InitPrefabList();
        UpdateCurPrefab();
        UpdatePreviewArea();
    }

    /* 
    // Object file management
    */
    public void EnterPrefabView()
    {
        Debug.Log("Entering prefab view...");
        StartPreviewCam();
        StartLineGrid();
        //UpdateCurPrefab();
        //DisplayCurPrefab();
    }

    public void ExitPRefabView()
    {
        previewCameraObj.SetActive(true);
        lineRend.positionCount = 0;
    }

    void InitPrefabList()
    {
        // get all prefabs in prefabs folder
        GameObject[] resourcesPrefabs = Resources.LoadAll<GameObject>("Prefabs");

        // generate others from stored objects

        // instantiate them in the scene for manipulation
        foreach(GameObject prefab in resourcesPrefabs)
        {
            GameObject parent = GameObject.FindGameObjectWithTag("PrefabParent");
            GameObject prefabObj = Instantiate(prefab, parent.transform);

            // remove (clone) from end of name
            prefabObj.name = prefabObj.name.Remove(prefabObj.name.Length - 7);

            prefabObj.transform.position += previewAreaOrigin;
            
            prefabs.Add(prefabObj);
            prefabObj.SetActive(false);

            // ensure new object has a renderer for bounds calculation
            prefabObj.TryGetComponent<MeshRenderer>(out MeshRenderer rend);
            if(rend == null) rend = prefabObj.AddComponent<MeshRenderer>() as MeshRenderer;

            Utility.SetLayerRecursively(prefabObj, 6);
        }

        // apply saved transformations

        // update list of objects
        //UpdatePrefabDropdown();
        foreach(GameObject prefab in prefabs)
        {
            //Debug.Log(prefab.name);
            prefabList.AddToList(listButton, prefab.name);
        }
    }

    void UpdatePrefabDropdown()
    {
        //prefabs = Resources.LoadAll<GameObject>("Prefabs").ToList();

        //*** REPLACE WITH DYNAMIC BUTTONS LATER ***
        listDropdown.options.Clear();
        foreach(GameObject prefab in prefabs)
        {
            listDropdown.options.Add(new TMP_Dropdown.OptionData() {text = prefab.name});
        }
    }

    public static void SetName(string itemName, string value)
    {
        //Drew Noakes, https://stackoverflow.com/questions/1485766/finding-an-item-in-a-list-using-c-sharp
        GameObject prefabItem = prefabs.Find(prefab => prefab.name == itemName);
        if(prefabItem) 
        {
            prefabItem.name = value;
        }
        //string textItem = textFiles.Find(itemName);
        //Debug.Log(temp + temp.name);
    }

    public void UpdateCurPrefab()
    {
        // unselect previous prefab
        if(curPrefab)
        {
            curPrefab.SetActive(false);
        }

        // select current prefab
        curPrefab = prefabs[listDropdown.value];
        curPrefab.SetActive(true);
        curBounds = Utility.GetBounds(curPrefab);
    }

    void SetCurPrefab(GameObject prefab)
    {
        if(curPrefab)
        {
            curPrefab.SetActive(false);
        }

        // select current prefab
        curPrefab = prefab;
        curPrefab.SetActive(true);
        curBounds = Utility.GetBounds(curPrefab);
    }
/*
    void DisplayCurPrefab()
    {
        curPrefab = Instantiate(prefabs[0]);
        curPrefab.transform.position += previewAreaOrigin;
    }
*/
    void UpdatePreviewArea()
    {
        if(Terrain.activeTerrain) previewAreaOrigin = Terrain.activeTerrain.terrainData.size * 2;
    }

    void StartPreviewCam()
    {
        previewCameraObj.transform.position = previewAreaOrigin;
        previewCameraObj.transform.position += new Vector3(8, 8, 8);//replace with largest dimension * 2

        previewCameraObj.SetActive(true);
    }

    void StartLineGrid()
    {
        int xSize = 4, zSize = 2;

        lineRend = gameObject.GetComponent<LineRenderer>();
        //https://stackoverflow.com/questions/72240485/how-to-add-the-default-line-material-back-to-the-linerenderer-material
        lineRend.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lineRend.startColor = Color.white;
        lineRend.startWidth = 0.05f;
        lineRend.positionCount = 0;
        lineRend.positionCount = (4 * xSize + 2) + (4 * zSize + 2);// - 1;

        int pointCount = 0;
        for(int i = -xSize; i <= xSize; i++)
        {
            if(i % 2 == 0)
            {
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(i, 0, zSize));
                pointCount++;
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(i, 0, -zSize));
                pointCount++;
            }
            else
            {
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(i, 0, -zSize));
                pointCount++;
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(i, 0, zSize));
                pointCount++;
            }
        }

        //pointcount--; could sync up to save 1 more point

        for(int i = -zSize; i <= zSize; i++)
        {
            if(i % 2 == 0)
            {
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(xSize, 0, i));
                pointCount++;
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(-xSize, 0, i));
                pointCount++;
            }
            else
            {
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(-xSize, 0, i));
                pointCount++;
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(xSize, 0, i));
                pointCount++;
            }
        }

        //Debug.Log(pointCount);
        //lineRend.positionCount = pointCount;
    }

    public void AddPrefab()
    {
        BuildPrefab("test");
    }

    void BuildPrefab(string objPath)
    {
        objPath = @"C:\Users\darre\Documents\VRSBUTBI-Production\Demo Test File\Cat_v1_L3.123cb1b1943a-2f48-4e44-8f71-6bbe19a3ab64\12221_Cat_v1_l3.obj";
        if(!File.Exists(objPath))
        {
            Debug.LogError("File doesn't exist.");
        }
        else
        {
            GameObject newPrefab = new OBJLoader().Load(objPath);
            GameObject parent = GameObject.FindGameObjectWithTag("PrefabParent");
            newPrefab.transform.parent = parent.transform;
            newPrefab.SetActive(false);

            /*OBJLoader loader = new OBJLoader();
            loadedObject = new OBJObjectBuilder("test", loader).Build();
            Debug.Log("success");*/

            prefabs.Add(newPrefab);
            UpdatePrefabDropdown();
            listDropdown.value = prefabs.Count - 1;

            SetCurPrefab(newPrefab);
        }
    }

    /*
    // Text file management
    */
    void MoveTextFiles()
    {
        int dirCount = Directory.GetDirectories(Application.streamingAssetsPath).Length;
        //Debug.LogError("Dirs in streaming assets?: " + dirCount);
        inUnityEditor = textPath.Contains("/Assets/");

        string[] path = textPath.Split('/');
        rootPath = "";
        for(int i = 0; i < path.Length - 4; i++) rootPath += "/" + path[i];
        rootPath = rootPath.Remove(0, 1);

        if(!inUnityEditor) textPath = rootPath + "/Text Files/";

        if(!inUnityEditor && dirCount > 0)
        {          
            //https://stackoverflow.com/questions/70694442/directory-move-throws-ioexception-regardless-of-the-fileshare-options-for-inte
            string source = Application.streamingAssetsPath;
            string target = rootPath;
            foreach (var file in Directory.EnumerateFiles(source))
            {
                var dest = Path.Combine(target, Path.GetFileName(file));
                File.Move(file, dest);
            }

            foreach (var dir in Directory.EnumerateDirectories(source))
            {
                var dest = Path.Combine(target, Path.GetFileName(dir));
                Directory.Move(dir, dest);
            }
        }
        //else Debug.LogError("Not moving text files, in editor or folder already in root.");

        string mainFile = "main.txt";
        string[] files = Directory.GetFiles(textPath);

        if(!File.Exists(textPath + mainFile)) Debug.LogError("main.txt is missing!");

        textFiles.Add(mainFile);
        for(int i = 0; i < files.Length; i++)
        {
            if(!files[i].Contains("meta") && !files[i].Contains(mainFile))
            {
                textFiles.Add(files[i].Remove(0, textPath.Length));
            }
        }
    }

    void UpdateTextTab()
    {
        //foreach 
    }

    /*
    //https://stackoverflow.com/questions/53968958/how-can-i-get-all-prefabs-from-a-assets-folder-getting-not-valid-cast-exception
    void PrefabFilesPlaceholder()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs");

        List<string> prefabNames = new List<string>();
        foreach(GameObject prefab in prefabs) prefabNames.Add(prefab.name + "\n\tPosition: " + prefab.transform.position + "\n\tRotation: " + prefab.transform.rotation.eulerAngles);

        if(!inUnityEditor) File.WriteAllLines(rootPath + "/models.txt", prefabNames);
    }

    void GeneratePathFile()
    {
        GameObject[] paths = GameObject.FindGameObjectsWithTag("Path");

        List<string> pathData = new List<string>();
        foreach(GameObject path in paths) pathData.Add(path.name + "\n\tPosition: " + path.transform.position + "\n\tRotation: " + path.transform.rotation.eulerAngles);

        if(!inUnityEditor) File.WriteAllLines(rootPath + "/paths.txt", pathData);
    }*/
}