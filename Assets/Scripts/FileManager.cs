using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using System;

public class FileManager : MonoBehaviour
{
    public static string textPath;
    public static List<string> textFiles = new List<string>();
    public static string rootPath;
    private bool inEditor;

    void OnEnable()
    {
        textPath = Application.streamingAssetsPath + "/Text Files/";
        MoveTextFiles();
        PrefabFilesPlaceholder();
        GeneratePathFile();
    }

    void MoveTextFiles()
    {
        int dirCount = Directory.GetDirectories(Application.streamingAssetsPath).Length;
        //Debug.LogError("Dirs in streaming assets?: " + dirCount);
        inEditor = textPath.Contains("/Assets/");

        string[] path = textPath.Split('/');
        rootPath = "";
        for(int i = 0; i < path.Length - 4; i++) rootPath += "/" + path[i];
        rootPath = rootPath.Remove(0, 1);

        if(!inEditor) textPath = rootPath + "/Text Files/";

        if(!inEditor && dirCount > 0)
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

    //https://stackoverflow.com/questions/53968958/how-can-i-get-all-prefabs-from-a-assets-folder-getting-not-valid-cast-exception
    void PrefabFilesPlaceholder()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs");

        List<string> prefabNames = new List<string>();
        foreach(GameObject prefab in prefabs) prefabNames.Add(prefab.name + "\n\tPosition: " + prefab.transform.position + "\n\tRotation: " + prefab.transform.rotation.eulerAngles);

        if(!inEditor) File.WriteAllLines(rootPath + "/models.txt", prefabNames);
    }

    void GeneratePathFile()
    {
        GameObject[] paths = GameObject.FindGameObjectsWithTag("Path");

        List<string> pathData = new List<string>();
        foreach(GameObject path in paths) pathData.Add(path.name + "\n\tPosition: " + path.transform.position + "\n\tRotation: " + path.transform.rotation.eulerAngles);

        if(!inEditor) File.WriteAllLines(rootPath + "/paths.txt", pathData);
    }
}