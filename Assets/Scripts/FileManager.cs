using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;

public class FileManager : MonoBehaviour
{
    public static string textPath = Application.streamingAssetsPath + "/Text Files/";
    public static List<string> textFiles = new List<string>();

    void OnEnable()
    {
        MoveTextFiles();
    }

    void MoveTextFiles()
    {
        int dirCount = Directory.GetDirectories(Application.streamingAssetsPath).Length;
        //Debug.LogError("Dirs in streaming assets?: " + dirCount);

        string[] path = textPath.Split('/');
        string rootPath = "";
        for(int i = 0; i < path.Length - 4; i++) rootPath += "/" + path[i];
        rootPath = rootPath.Remove(0, 1);

        textPath = rootPath + "/Text Files/";

        if(!textPath.Contains("/Assets/") && dirCount > 0)
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
}
