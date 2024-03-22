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
using SimpleFileBrowser;

public class ArchiveManager : MonoBehaviour
{
    public class Prefab
    {
        public GameObject obj;
        public string name;
        public string displayName;
        public Transform transform;

        public Prefab(GameObject go)
        {
            obj = go;
            name = obj.name;
            displayName = obj.name;
            transform = obj.transform;
        }
    }

    public static string rootPath;

    public static string textPath;
    public static List<string> textFiles = new List<string>();
    private bool inUnityEditor;

    public static string objectPath;
    public static List<Prefab> prefabs = new List<Prefab>();
    public ListUtil prefabListUtil;
    public GameObject listButton;
    public static Prefab curPrefab = null;
    Bounds curBounds;
    public GameObject previewCameraObj;
    Camera previewCamera;
    public static Vector3 previewAreaOrigin = new Vector3(0, 1000, 0);
    LineRenderer lineRend;
    public TransformEditor transformEditor;
    bool editorActive = false;
    public RectTransform previewWindow;
    public RenderTexture previewImage;

    public static string pathPath;

/*
// File Management
*/
    void MoveFilesToRoot()
    {
        int dirCount = Directory.GetDirectories(Application.streamingAssetsPath).Length;

        inUnityEditor = textPath.Contains("/Assets/");

        string[] path = textPath.Split('/');
        rootPath = "";
        for(int i = 0; i < path.Length - 4; i++) rootPath += "/" + path[i];
        rootPath = rootPath.Remove(0, 1);

        if(!inUnityEditor) 
        {
            textPath = rootPath + "/Text Files/";
            objectPath = rootPath + "/Objects/";
            pathPath = rootPath + "/Paths/";
        }

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

/*
// Timed calls
*/
    void OnEnable()
    {
        textPath = Application.streamingAssetsPath + "/Text Files/";
        objectPath = Application.streamingAssetsPath + "/Objects/";
        pathPath = Application.streamingAssetsPath + "/Paths/";
        MoveFilesToRoot();

        lineRend = gameObject.GetComponent<LineRenderer>();

        previewCamera = previewCameraObj.GetComponent<Camera>();
    }

    void Start()
    {
        InitPrefabList();
        RecallPrefabData();
        InitPrefabListUI();
    }

    void FixedUpdate()
    {
        // update current selected prefab
        if(prefabListUtil.selectedObject)
        {
            string objName = prefabListUtil.selectedObject.name;

            if(curPrefab == null || objName != curPrefab.name)
            {
                SetCurPrefab(prefabs.Find(prefab => prefab.name == objName));
                UpdatePreviewCam();
                UpdateLineGrid();
            }
        }

        // update current prefab transform if needed
        if(transformEditor.awaitPreviewRefresh)
        {
            // make sure curBounds is up to date
            curBounds = Utility.GetBounds(curPrefab.obj);

            UpdatePreviewCam();
            UpdateLineGrid();
        }
    }

    void OnApplicationQuit()
    {
        StorePrefabData();
    }

/* 
// Object management
*/
    void InitPrefabList()
    {
        // Remove existing prefabs(if any)
        foreach(Prefab prefab in prefabs) Destroy(prefab.obj);
        prefabs.Clear();
        prefabListUtil.ClearList();

        // get all prefabs in prefabs folder
        GameObject[] resourcesPrefabs = Resources.LoadAll<GameObject>("Prefabs");

        // instantiate them in the scene for manipulation
        foreach(GameObject prefab in resourcesPrefabs)
        {
            GameObject parent = GameObject.FindGameObjectWithTag("PrefabParent");
            GameObject prefabObj = Instantiate(prefab, parent.transform);

            // remove (clone) from end of name
            prefabObj.name = prefabObj.name.Remove(prefabObj.name.Length - 7);

            prefabObj.transform.position += previewAreaOrigin;
            
            prefabs.Add(new Prefab(prefabObj));
            prefabObj.SetActive(false);

            // ensure new object has a renderer for bounds calculation
            prefabObj.TryGetComponent<MeshRenderer>(out MeshRenderer rend);
            if(rend == null) rend = prefabObj.AddComponent<MeshRenderer>() as MeshRenderer;

            Utility.SetLayerRecursively(prefabObj, 6);
        }

        // get prefabs from file directory
        string[] objFiles = Directory.GetFiles(objectPath, "*.obj");
        foreach(string objFile in objFiles) BuildPrefab(objFile, false);

        foreach(var dir in Directory.EnumerateDirectories(objectPath))
        {
            objFiles = Directory.GetFiles(dir, "*.obj");
            foreach(string objFile in objFiles) BuildPrefab(objFile, false);
        }
    }

    void InitPrefabListUI()
    {
        foreach(Prefab prefab in prefabs)
        {
            prefabListUtil.AddToList(listButton, prefab.name, prefab.displayName);
        }
    }

    public void RefreshPrefabList()
    {
        StorePrefabData();
        InitPrefabList();
        RecallPrefabData();
        InitPrefabListUI();

        //select new instance of selected prefab 
        if(curPrefab != null) 
        {
            Prefab curInstance = prefabs.Find(prefab => prefab.name == curPrefab.name);
            SetCurPrefab(curInstance);
            prefabListUtil.SelectByName(curInstance.name);
        }
    }

    void SetCurPrefab(Prefab prefab)
    {
        if(curPrefab != null) curPrefab.obj.SetActive(false);

        curPrefab = prefab;
        curPrefab.obj.SetActive(true);
        curBounds = Utility.GetBounds(curPrefab.obj);

        transformEditor.SetCurTransform(curPrefab.transform);
    }

    public static void SetDisplayName(string prefabName, string newName)
    {
        Prefab prefab = prefabs.Find(prefab => prefab.name == prefabName);
        if(prefab != null) prefab.displayName = newName;
    }
    
    void UpdatePreviewArea()
    {
        if(Terrain.activeTerrain) previewAreaOrigin = Terrain.activeTerrain.terrainData.size * 2;
    }

    public void AddPrefab()
    {
        FileBrowser.SetFilters( true, new FileBrowser.Filter("Objects", ".obj"));
		FileBrowser.SetDefaultFilter( ".obj" );
		FileBrowser.AddQuickLink("Users", "C:\\Users", null);

		StartCoroutine(BrowseForObj());
    }

    IEnumerator BrowseForObj()
	{
		yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load Files and Folders", "Load");

		if(FileBrowser.Success)
		{
			for(int i = 0; i < FileBrowser.Result.Length; i++) 
            {
                string copyPath = Path.Combine(objectPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[i]));

                if(FileBrowserHelpers.IsDirectory(FileBrowser.Result[i]))
                {
                    FileBrowserHelpers.CopyDirectory(FileBrowser.Result[i], copyPath);
                    foreach(FileSystemEntry entry in FileBrowserHelpers.GetEntriesInDirectory(FileBrowser.Result[i], true))
                    {
                        if(entry.Extension == ".obj")
                        {
                            BuildPrefab(entry.Path, true);
                            break;
                        }
                    }
                }
                else 
                {
                    FileBrowserHelpers.CopyFile(FileBrowser.Result[i], copyPath);
                    BuildPrefab(copyPath, true);
                }
            }
		}
	}

    void BuildPrefab(string objPath, bool isNew)
    {
        if(!File.Exists(objPath)) Debug.LogError("File doesn't exist.");
        else
        {
            GameObject newObject = new OBJLoader().Load(objPath);
            GameObject parent = GameObject.FindGameObjectWithTag("PrefabParent");
            newObject.transform.parent = parent.transform;
            newObject.transform.position += previewAreaOrigin;
            newObject.transform.localScale = new Vector3(1, 1, 1);
            
            newObject.SetActive(false);

            newObject.TryGetComponent<MeshRenderer>(out MeshRenderer rend);
            if(rend == null) rend = newObject.AddComponent<MeshRenderer>() as MeshRenderer;


            Utility.SetLayerRecursively(newObject, 6);

            Prefab newPrefab = new Prefab(newObject);
            prefabs.Add(newPrefab);

            if(isNew)
            {
                StorePrefab(newPrefab);
                prefabListUtil.AddToList(listButton, newPrefab.name, newPrefab.displayName);
                
                SetCurPrefab(newPrefab);
                UpdatePreviewCam();
                UpdateLineGrid();
                prefabListUtil.SelectByName(newPrefab.name);
            }
        }
    }

    public void RemovePrefab()
    {
        if(curPrefab == null) return;

        prefabListUtil.RemoveFromList(curPrefab.name);
        prefabListUtil.selectedObject = null;

        int i = prefabs.FindIndex(prefab => prefab == curPrefab);

        prefabs.RemoveAt(i);

        Destroy(curPrefab.obj);
        curPrefab = null;

        lineRend.positionCount = 0;
    }
    
    public void EnterPrefabView()
    {
        UpdatePreviewCam();
        //UpdateLineGrid();
        
        //DisplayCurPrefab();
    }

    public void ExitPrefabView()
    {
        previewCameraObj.SetActive(false);
    }

    void UpdatePreviewCam()
    {
        previewCameraObj.transform.position = previewAreaOrigin;

        // https://forum.unity.com/threads/get-the-element-with-maximum-value-in-a-vector3.31470/
        float max = Mathf.Max(Mathf.Max(curBounds.size.x, curBounds.size.y), curBounds.size.z);
        max *= 2;
        previewCameraObj.transform.position += new Vector3(max, 1.5f * max, max);//replace with largest dimension * 2

        previewCameraObj.SetActive(true);
    }

    void UpdateLineGrid()
    {
        int xSize = (int)Math.Round(curBounds.size.x), zSize = (int)Math.Round(curBounds.size.z);

        //https://stackoverflow.com/questions/72240485/how-to-add-the-default-line-material-back-to-the-linerenderer-material
        lineRend.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lineRend.startColor = Color.white;
        lineRend.startWidth = 0.05f;
        lineRend.positionCount = 0;
        lineRend.positionCount = (4 * xSize + 2) + (4 * zSize + 2) - 1;

        int pointCount = 0;
        bool right = false;
        for(int i = -xSize; i <= xSize; i++)
        {
            if(right)
            {
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(i, 0, zSize));
                lineRend.SetPosition(pointCount + 1, previewAreaOrigin + new Vector3(i, 0, -zSize));
            }
            else
            {
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(i, 0, -zSize));
                lineRend.SetPosition(pointCount + 1, previewAreaOrigin + new Vector3(i, 0, zSize));
            }
            pointCount += 2;
            right = !right;
        }

        pointCount--; //sync up to save 1 more point

        for(int i = zSize; i >= -zSize; i--)
        {
            if(right)
            {
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(xSize, 0, i));
                lineRend.SetPosition(pointCount + 1, previewAreaOrigin + new Vector3(-xSize, 0, i));
            }
            else
            {
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(-xSize, 0, i));
                lineRend.SetPosition(pointCount + 1, previewAreaOrigin + new Vector3(xSize, 0, i));
            }
            pointCount += 2;
            right = !right;
        }
    }

    public void TogglePrefabEditor()
    {
        float expandedWidth = 535;
        float contractedWidth = 405;

        if(editorActive)
        {
            previewWindow.anchoredPosition = new Vector2(105, previewWindow.anchoredPosition.y);
            previewWindow.sizeDelta = new Vector2(expandedWidth, previewWindow.sizeDelta.y);
            ChangeWidth(previewImage, 2 * (int)expandedWidth);
        }
        else
        {
            previewWindow.anchoredPosition = new Vector2(170, previewWindow.anchoredPosition.y);
            previewWindow.sizeDelta = new Vector2(contractedWidth, previewWindow.sizeDelta.y);
            ChangeWidth(previewImage, 2 * (int)contractedWidth);
        }

        editorActive = !editorActive;
    }

    // Adpated from Neohun, https://discussions.unity.com/t/resize-rendertexture-at-runtime/113573/2
    void ChangeWidth(RenderTexture renderTexture, int width) 
    {
        if(renderTexture) 
        {
            renderTexture.Release();
            renderTexture.width = width;
            renderTexture.Create();
        }
    }

    void StorePrefabData()
    {
        List<string> data = new List<string>();

        for(int i = 0; i < prefabs.Count; i++)
        {
            Vector3 pos = prefabs[i].transform.position - previewAreaOrigin;
            Vector3 rot = prefabs[i].transform.eulerAngles;
            Vector3 scl = prefabs[i].transform.localScale;
            
            data.Add(prefabs[i].name + ',' + prefabs[i].displayName + ',' + pos.x + ',' + pos.y + ',' + pos.z + ',' + rot.x + ',' + rot.y + ',' + rot.z + ',' + scl.x + ',' + scl.y + ',' + scl.z);
        }

        File.WriteAllLines(objectPath + "/data.txt", data);
    }

    void StorePrefab(Prefab prefab)
    {
        Vector3 pos = prefab.transform.position - previewAreaOrigin;
        Vector3 rot = prefab.transform.eulerAngles;
        Vector3 scl = prefab.transform.localScale;

        string data = prefab.name + ',' + prefab.displayName + ',' + pos.x + ',' + pos.y + ',' + pos.z + ',' + rot.x + ',' + rot.y + ',' + rot.z + ',' + scl.x + ',' + scl.y + ',' + scl.z;
        File.AppendAllText(objectPath + "/data.txt", data);
    }

    void RecallPrefabData()
    {
        if(!File.Exists(objectPath + "/data.txt")) return;

        string[] transforms = File.ReadAllLines(objectPath + "/data.txt");
        
        foreach(string transform in transforms)
        {
            string[] vals = transform.Split(',');
            Prefab prefab = prefabs.Find(prefab => prefab.name == vals[0]);
            if(prefab != null)
            {
                prefab.transform.position = new Vector3(Convert.ToSingle(vals[2]), Convert.ToSingle(vals[3]), Convert.ToSingle(vals[4])) + previewAreaOrigin;
                prefab.transform.eulerAngles = new Vector3(Convert.ToSingle(vals[5]), Convert.ToSingle(vals[6]), Convert.ToSingle(vals[7]));
                prefab.transform.localScale = new Vector3(Convert.ToSingle(vals[8]), Convert.ToSingle(vals[9]), Convert.ToSingle(vals[10]));

                prefab.name = vals[0];
                prefab.displayName = vals[1];
            }
        }
    }

    public void RevertEdits()
    {
        if(curPrefab == null) return;

        string[] data = File.ReadAllLines(objectPath + "/data.txt");
        string[] vals = data[prefabs.FindIndex(prefab => prefab.name == curPrefab.name)].Split(',');
        
        curPrefab.transform.position = new Vector3(Convert.ToSingle(vals[2]), Convert.ToSingle(vals[3]), Convert.ToSingle(vals[4])) + previewAreaOrigin;
        curPrefab.transform.eulerAngles = new Vector3(Convert.ToSingle(vals[5]), Convert.ToSingle(vals[6]), Convert.ToSingle(vals[7]));
        curPrefab.transform.localScale = new Vector3(Convert.ToSingle(vals[8]), Convert.ToSingle(vals[9]), Convert.ToSingle(vals[10]));
        transformEditor.UpdateInputFields();

        prefabListUtil.SetName(curPrefab.name, vals[1]);
        curPrefab.name = vals[1];
    }

/*
// Text management
*/

}