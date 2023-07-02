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
    public TMP_Dropdown fileDropdown;
    public TMP_InputField fileInput;
    private string curText;
    private string curFile;
    private bool editsMade;
    
    void Start()
    {
        fileDropdown.options.Clear();
        foreach(string file in FileManager.textFiles)
        {
            fileDropdown.options.Add(new TMP_Dropdown.OptionData() {text = file});
        }

        editsMade = false;

        curFile = "main.txt";
        getText(curFile);
    }

    void getText(string filename)
    {
        if(File.Exists(FileManager.textPath + filename))
        {
            curText = File.ReadAllText(FileManager.textPath + filename);
            fileInput.text = curText;
        }
        else Debug.LogError(filename + " could not be opened.");
    }

    public void setText()
    {
        File.WriteAllText(FileManager.textPath + curFile, fileInput.text);
        editsMade = true;
    }

    public void resetText()
    {
        fileInput.text = curText;
        setText();
    }

    public void switchFile()
    {
        curFile = FileManager.textFiles[fileDropdown.value];
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
