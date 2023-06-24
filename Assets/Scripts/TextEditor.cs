using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;

public class TextEditor : MonoBehaviour
{
    public Button editButton;
    public bool openFileWindow = false;
    public TMP_Dropdown fileDropdown;
    public TMP_InputField fileInput;
    private string curText;
    private string curFile;
    private List<string> textFiles = new List<string>();
    private string dirPath = Application.streamingAssetsPath + "/Text Files/";
    private bool editsMade;

    // Start is called before the first frame update
    void Start()
    {
        string[] files = Directory.GetFiles(dirPath);
        string mainFile = "main.txt";

        if(!File.Exists(dirPath + mainFile)) Debug.LogError("main.txt is missing!");

        textFiles.Add(mainFile);
        for(int i = 0; i < files.Length; i++)
        {
            if(!files[i].Contains("meta") && !files[i].Contains(mainFile))
            {
                textFiles.Add(files[i].Remove(0, dirPath.Length));
            }
        }

        fileDropdown.options.Clear();
        foreach(string file in textFiles) 
        {
            fileDropdown.options.Add(new TMP_Dropdown.OptionData() {text = file});
        }

        curFile = mainFile;
        getText(curFile);

        editsMade = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void getText(string filename)
    {
        if(File.Exists(dirPath + filename))
        {
            curText = File.ReadAllText(dirPath + filename);
            fileInput.text = curText;
        }
        else Debug.LogError(filename + " could not be opened.");
    }

    public void setText()
    {
        File.WriteAllText(dirPath + curFile, fileInput.text);
        editsMade = true;
    }

    public void resetText()
    {
        fileInput.text = curText;
        setText();
    }

    public void switchFile()
    {
        curFile = textFiles[fileDropdown.value];
        getText(curFile);
    }

    public void openFileSelector()
    {
        if(!PauseScript.paused) PauseScript.PauseFunction();
    }

    public void closeFileSelector()
    {
        if(editsMade)
        {
            editsMade = false;
            Scene curScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(curScene.name);
        }
        else if(PauseScript.paused) PauseScript.PauseFunction();
    }
}
