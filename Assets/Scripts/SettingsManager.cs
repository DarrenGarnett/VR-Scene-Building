using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SettingsManager : MonoBehaviour
{
    public Toggle pathVisibilityToggle;
    public static bool pathsVisible;

    // Start is called before the first frame update
    void Start()
    {
        string[] settings = File.ReadAllLines(ArchiveManager.rootPath + "/settings.txt");
        pathsVisible = bool.Parse(settings[0].Split(':')[1]);
        pathVisibilityToggle.isOn = pathsVisible;
        //Debug.Log(pathsVisible);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void pathVisibilityChanged()
    {
        pathsVisible = pathVisibilityToggle.isOn;
        //Debug.Log(pathsVisible);
        saveSettings();
    }

    void saveSettings()
    {
        File.WriteAllText(ArchiveManager.rootPath + "/settings.txt", "Paths Visible:" + pathsVisible);
        //Debug.Log(JsonUtility.ToJson(this));
    }
}