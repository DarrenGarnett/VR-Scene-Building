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

/*public class ArchiveData
{
    public List<TransformData> transforms;

    public ArchiveData()
    {
        transforms = new List<TransformData>();
    }
}*/

public class TransformData
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;

    public TransformData(Transform transform)
    {
        position = RoundVec(transform.position, 3);
        rotation = RoundVec(transform.eulerAngles, 3);
        scale = RoundVec(transform.localScale, 3);
    }

    public void RemovePreviewOffset()
    {
        position -= ArchiveManager.previewAreaOrigin;
        position = RoundVec(position, 3);
    }

    Vector3 RoundVec(Vector3 cur, int precision)
    {
        Vector3 rounded = new Vector3(0, 0, 0);
        rounded.x = (float)Math.Round(cur.x, precision);
        rounded.y = (float)Math.Round(cur.y, precision);
        rounded.z = (float)Math.Round(cur.z, precision);
        return rounded;
    }

    public static TransformData CreateFromJSON(string json)
    {
        return JsonUtility.FromJson<TransformData>(json);
    }

    public string ConvertToJSON()
    {
        return JsonUtility.ToJson(this);
    }
}

public class ArchiveManager : MonoBehaviour
{
    class Item
    {
        public string name;
        public GameObject obj;
    }

    public static string rootPath;

    public static string textPath;
    public static List<string> textFiles = new List<string>();
    private bool inUnityEditor;

    public static string objectPath;
    public static List<GameObject> prefabs = new List<GameObject>();
    List<string> prefabOriginalNames = new List<string>();
    public ListUtil prefabList;
    public GameObject listButton;
    public static GameObject curPrefab = null;
    Bounds curBounds;
    public GameObject previewCameraObj;
    Camera previewCamera;
    public static Vector3 previewAreaOrigin = new Vector3(0, 1000, 0);
    LineRenderer lineRend;
    public TransformEditor transformEditor;
    bool editorActive = false;
    public RectTransform previewWindow;
    public RenderTexture previewImage;

    public TMP_Dropdown listDropdown;

    void OnEnable()
    {
        textPath = Application.streamingAssetsPath + "/Text Files/";
        objectPath = Application.streamingAssetsPath + "/Objects/";
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
        if(prefabList.selectedObject)
        {
            string objName = prefabList.selectedObject.name;
            if(!curPrefab)
            {
                SetCurPrefab(prefabs.Find(prefab => prefab.name == objName));
                UpdatePreviewCam();
                UpdateLineGrid();
            }
            else if(objName != curPrefab.name) 
            {
                //Debug.Log("Detected selected prefab change, changing to " + objName);
                SetCurPrefab(prefabs.Find(prefab => prefab.name == objName));
                UpdatePreviewCam();
                UpdateLineGrid();
            }
        }
        if(transformEditor.awaitPreviewRefresh)
        {
            // make sure curBounds is up to date
            curBounds = Utility.GetBounds(curPrefab);

            UpdatePreviewCam();
            UpdateLineGrid();
        }
    }

    void OnApplicationQuit()
    {
        StorePrefabData();
    }

    void StorePrefabData()
    {
        List<string> data = new List<string>();

        for(int i = 0; i < prefabs.Count; i++)
        {
            //TransformData curTransform = new TransformData(prefab.transform);
            //curTransform.RemovePreviewOffset();
            //final.transforms.Add(curTransform);
            //Debug.Log(curTransform.ConvertToJSON());
            Vector3 pos = prefabs[i].transform.position - previewAreaOrigin;
            Vector3 rot = prefabs[i].transform.eulerAngles;
            Vector3 scl = prefabs[i].transform.localScale;
            
            data.Add(prefabOriginalNames[i] + ',' + prefabs[i].name + ',' + pos.x + ',' + pos.y + ',' + pos.z + ',' + rot.x + ',' + rot.y + ',' + rot.z + ',' + scl.x + ',' + scl.y + ',' + scl.z);
        }

        //Debug.Log(JsonUtility.ToJson(final));
        File.WriteAllLines(objectPath + "/data.txt", data);
    }

    void StoreTransform(GameObject go)
    {
        Vector3 pos = go.transform.position - previewAreaOrigin;
        Vector3 rot = go.transform.eulerAngles;
        Vector3 scl = go.transform.localScale;
        string data = go.name + ',' + go.name + ',' + pos.x + ',' + pos.y + ',' + pos.z + ',' + rot.x + ',' + rot.y + ',' + rot.z + ',' + scl.x + ',' + scl.y + ',' + scl.z;
        File.AppendAllText(objectPath + "/data.txt", data);
    }

    void RecallPrefabData()
    {
        string[] transforms = File.ReadAllLines(objectPath + "/data.txt");
        
        foreach(string transform in transforms)
        {
            string[] vals = transform.Split(',');
            GameObject prefab = prefabs.Find(prefab => prefab.name == vals[0]);
            if(prefab)
            {
                prefab.transform.position = new Vector3(Convert.ToSingle(vals[2]), Convert.ToSingle(vals[3]), Convert.ToSingle(vals[4])) + previewAreaOrigin;
                prefab.transform.eulerAngles = new Vector3(Convert.ToSingle(vals[5]), Convert.ToSingle(vals[6]), Convert.ToSingle(vals[7]));
                prefab.transform.localScale = new Vector3(Convert.ToSingle(vals[8]), Convert.ToSingle(vals[9]), Convert.ToSingle(vals[10]));

                prefab.name = vals[1];
            }
        }
    }

    public void RevertEdits()
    {
        if(!curPrefab) return;

        //Debug.Log("Reverting edits to " + curPrefab.name);

        string[] data = File.ReadAllLines(objectPath + "/data.txt");
        
        /*foreach(string transform in transforms)
        {
            string[] vals = transform.Split(',');
            if(vals[0] == curPrefab.name || vals[1] == curPrefab.name)
            {
                curPrefab.transform.position = new Vector3(Convert.ToSingle(vals[2]), Convert.ToSingle(vals[3]), Convert.ToSingle(vals[4])) + previewAreaOrigin;
                curPrefab.transform.eulerAngles = new Vector3(Convert.ToSingle(vals[5]), Convert.ToSingle(vals[6]), Convert.ToSingle(vals[7]));
                curPrefab.transform.localScale = new Vector3(Convert.ToSingle(vals[8]), Convert.ToSingle(vals[9]), Convert.ToSingle(vals[10]));
                transformEditor.UpdateInputFields();

                prefabList.SetName(curPrefab.name, vals[1]);
                curPrefab.name = vals[1];

                break;
            }
        }*/
        string[] vals = data[prefabs.FindIndex(prefab => prefab.name == curPrefab.name)].Split(',');
        
        curPrefab.transform.position = new Vector3(Convert.ToSingle(vals[2]), Convert.ToSingle(vals[3]), Convert.ToSingle(vals[4])) + previewAreaOrigin;
        curPrefab.transform.eulerAngles = new Vector3(Convert.ToSingle(vals[5]), Convert.ToSingle(vals[6]), Convert.ToSingle(vals[7]));
        curPrefab.transform.localScale = new Vector3(Convert.ToSingle(vals[8]), Convert.ToSingle(vals[9]), Convert.ToSingle(vals[10]));
        transformEditor.UpdateInputFields();

        prefabList.SetName(curPrefab.name, vals[1]);
        curPrefab.name = vals[1];
    }
/*
// Generic methods
*/

    static Item GetItem(string itemName)
    {
        Item item = new Item();

        string textItem = textFiles.Find(filename => filename == itemName);
        if(textItem != null)
        {
            //Debug.Log(itemName + " is a text file");
            item.name = textItem;
            item.obj = null;
            return item;
        }

        //Drew Noakes, https://stackoverflow.com/questions/1485766/finding-an-item-in-a-list-using-c-sharp
        GameObject prefabItem = prefabs.Find(prefab => prefab.name == itemName);
        if(prefabItem) 
        {
            //Debug.Log(itemName + " is a prefab");
            item.name = prefabItem.name;
            item.obj = prefabItem;
            return item;
        }

        return null;
    }

    public static void SetName(string itemName, string value)
    {
        /*//Drew Noakes, https://stackoverflow.com/questions/1485766/finding-an-item-in-a-list-using-c-sharp
        GameObject prefabItem = prefabs.Find(prefab => prefab.name == itemName);
        if(prefabItem) 
        {
            prefabItem.name = value;
        }*/
        //string textItem = textFiles.Find(itemName);
        //Debug.Log(temp + temp.name);

        Item item = GetItem(itemName);
        if(item != null) item.obj.name = value;
    }

    public static void RemoveFromList(string itemName)
    {
        Item item = GetItem(itemName);
        if(item.obj)
        {
            Destroy(item.obj);
            prefabs.Remove(item.obj);
        }
        else
        {
            textFiles.Remove(item.name);
        }
    }

    /*public static void SelectItem(string itemName)
    {
        Item item = GetItem(itemName);
        if(item.obj) SetCurPrefab(item.obj);
        //Debug.Log("currrent prefab = " + curPrefab.name);
    }*/

/* 
// Object file management
*/
    public void InitPrefabList()
    {
        // Remove existing prefabs(if any)
        foreach(GameObject prefab in prefabs) Destroy(prefab);
        prefabs.Clear();
        prefabList.ClearList();

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
            
            prefabs.Add(prefabObj);
            prefabOriginalNames.Add(prefabObj.name);
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
        foreach(GameObject prefab in prefabs)
        {
            prefabList.AddToList(listButton, prefab.name);
        }
    }

    public void RefreshPrefabList()
    {
        InitPrefabList();
        InitPrefabListUI();

        //select new instance of selected prefab 
        if(curPrefab) 
        {
            GameObject curInstance = prefabs.Find(prefab => prefab.name == curPrefab.name);
            SetCurPrefab(curInstance);
            prefabList.SelectByName(curInstance.name);
        }
    }

    /*public void UpdateCurPrefab(string prefabName)
    {
        //Debug.Log("Updating current prefab to " + prefabName);

        // unselect previous prefab
        if(curPrefab)
        {
            curPrefab.SetActive(false);
        }

        // select current prefab
        curPrefab.SetActive(true);
        curBounds = Utility.GetBounds(curPrefab);
    }*/

    void SetCurPrefab(GameObject prefab)
    {
        if(curPrefab) curPrefab.SetActive(false);

        // select current prefab
        curPrefab = prefab;
        curPrefab.SetActive(true);
        curBounds = Utility.GetBounds(curPrefab);

        transformEditor.SetCurTransform(curPrefab.transform);

        //Debug.Log("Set current prefab to " + prefab.name);
    }

    /*void DisplayCurPrefab()
    {
        curPrefab = Instantiate(prefabs[0]);
        curPrefab.transform.position += previewAreaOrigin;
    }*/

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

        //BuildPrefab(@"C:\Users\darre\Documents\VRSBUTBI-Production\Demo Test File\Cat_v1_L3.123cb1b1943a-2f48-4e44-8f71-6bbe19a3ab64\12221_Cat_v1_l3.obj", true);
        //BuildPrefab(@"C:\Users\darre\Documents\CatModel\Cat.obj", true);
    }

    IEnumerator BrowseForObj()
	{
		// Show a load file dialog and wait for a response from user
		// Load file/folder: both, Allow multiple selection: true
		// Initial path: default (Documents), Initial filename: empty
		// Title: "Load File", Submit button text: "Load"
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
                            //Debug.Log("Building at " + entry.Path);
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
                //File.Copy(@FileBrowser.Result[i], Application.streamingAssetsPath + "/Objects/" + filepath[filepath.Length - 1], true);
            }
		}
	}

    void BuildPrefab(string objPath, bool isNew)
    {
        if(!File.Exists(objPath)) Debug.LogError("File doesn't exist.");
        else
        {
            GameObject newPrefab = new OBJLoader().Load(objPath);
            GameObject parent = GameObject.FindGameObjectWithTag("PrefabParent");
            newPrefab.transform.parent = parent.transform;
            newPrefab.transform.position += previewAreaOrigin;
            
            newPrefab.SetActive(false);

            newPrefab.TryGetComponent<MeshRenderer>(out MeshRenderer rend);
            if(rend == null) rend = newPrefab.AddComponent<MeshRenderer>() as MeshRenderer;

            /*OBJLoader loader = new OBJLoader();
            loadedObject = new OBJObjectBuilder("test", loader).Build();
            Debug.Log("success");*/

            //Debug.Log(newPrefab.transform.position);
            prefabs.Add(newPrefab);
            prefabOriginalNames.Add(newPrefab.name);
            //UpdatePrefabDropdown();
            //listDropdown.value = prefabs.Count - 1;
            //StoreTransform(newPrefab);

            Utility.SetLayerRecursively(newPrefab, 6);

            if(isNew)
            {
                newPrefab.transform.localScale = new Vector3(1, 1, 1);
                StoreTransform(newPrefab);
                prefabList.AddToList(listButton, newPrefab.name);
                
                SetCurPrefab(newPrefab);
                prefabList.SelectByName(newPrefab.name);
            }
        }
    }

    public void RemovePrefab()
    {
        if(!curPrefab) return;

        prefabList.RemoveFromList(curPrefab.name);
        prefabList.selectedObject = null;

        prefabs.Remove(curPrefab);
        Destroy(curPrefab);
        curPrefab = null;
    }
    
    public void EnterPrefabView()
    {
        //Debug.Log("Entering prefab view...");
        UpdatePreviewCam();
        UpdateLineGrid();
        
        //DisplayCurPrefab();
    }

    public void ExitPrefabView()
    {
        previewCameraObj.SetActive(false);
        //lineRend.positionCount = 0;
    }

    void UpdatePreviewCam()
    {
        //Debug.Log("Moving cam to " + previewAreaOrigin);
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
        //xSize = Mathf.Max(xSize, zSize);
        //zSize = xSize;

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
            //if(i % 2 == 0)
            if(right)
            {
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(i, 0, zSize));
                lineRend.SetPosition(pointCount + 1, previewAreaOrigin + new Vector3(i, 0, -zSize));
                //Debug.Log(new Vector3(i, 0, zSize));
                //Debug.Log(new Vector3(i, 0, -zSize));
            }
            else
            {
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(i, 0, -zSize));
                lineRend.SetPosition(pointCount + 1, previewAreaOrigin + new Vector3(i, 0, zSize));
                //Debug.Log(new Vector3(i, 0, -zSize));
                //Debug.Log(new Vector3(i, 0, zSize));
            }
            pointCount += 2;
            right = !right;
        }

        pointCount--; //sync up to save 1 more point

        for(int i = zSize; i >= -zSize; i--)
        {
            /*
            // note: before was i % 2 == 0, changed to try to remove diagonal line in certain(even?) cases  
            */
            //if(i % 2 == 0)
            if(right)
            {
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(xSize, 0, i));
                lineRend.SetPosition(pointCount + 1, previewAreaOrigin + new Vector3(-xSize, 0, i));
                //Debug.Log(new Vector3(xSize, 0, i));
                //Debug.Log(new Vector3(-xSize, 0, i));
            }
            else
            {
                lineRend.SetPosition(pointCount, previewAreaOrigin + new Vector3(-xSize, 0, i));
                lineRend.SetPosition(pointCount + 1, previewAreaOrigin + new Vector3(xSize, 0, i));
                //Debug.Log(new Vector3(-xSize, 0, i));
                //Debug.Log(new Vector3(xSize, 0, i));
            }
            pointCount += 2;
            right = !right;
        }

        //Debug.Log(pointCount);
        //lineRend.positionCount = pointCount;
        //Debug.Log("Grid updated");
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

    /*void StoreTransform(Transform data)
    {
        TransformData output = new TransformData(data);
        output.RemovePreviewOffset();
        Debug.Log(output.ConvertToJSON());
    }*/

/*
// Text file management
*/
    void MoveFilesToRoot()
    {
        int dirCount = Directory.GetDirectories(Application.streamingAssetsPath).Length;
        //Debug.LogError("Dirs in streaming assets?: " + dirCount);
        inUnityEditor = textPath.Contains("/Assets/");

        string[] path = textPath.Split('/');
        rootPath = "";
        for(int i = 0; i < path.Length - 4; i++) rootPath += "/" + path[i];
        rootPath = rootPath.Remove(0, 1);

        if(!inUnityEditor) 
        {
            textPath = rootPath + "/Text Files/";
            objectPath = rootPath + "/Objects/";
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

    //https://stackoverflow.com/questions/53968958/how-can-i-get-all-prefabs-from-a-assets-folder-getting-not-valid-cast-exception
    /*void PrefabFilesPlaceholder()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs");

        List<string> prefabNames = new List<string>();
        foreach(GameObject prefab in prefabs) prefabNames.Add(prefab.name + "\n\tPosition: " + prefab.transform.position + "\n\tRotation: " + prefab.transform.rotation.eulerAngles);

        if(!inUnityEditor) File.WriteAllLines(rootPath + "/models.txt", prefabNames);
    }*/

    /*void GeneratePathFile()
    {
        GameObject[] paths = GameObject.FindGameObjectsWithTag("Path");

        List<string> pathData = new List<string>();
        foreach(GameObject path in paths) pathData.Add(path.name + "\n\tPosition: " + path.transform.position + "\n\tRotation: " + path.transform.rotation.eulerAngles);

        if(!inUnityEditor) File.WriteAllLines(rootPath + "/paths.txt", pathData);
    }*/
}