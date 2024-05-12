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
    public GlobalTimeScript GTS;
    public SceneManip sceneManip;
    
    void Start()
    {
        fileDropdown.options.Clear();
        foreach(string file in ArchiveManager.textFiles)
        {
            fileDropdown.options.Add(new TMP_Dropdown.OptionData() {text = file});
        }

        editsMade = false;

        curFile = "main.txt";
        getText(curFile);
    }

    void getText(string filename)
    {
        if(File.Exists(ArchiveManager.textPath + filename))
        {
            curText = File.ReadAllText(ArchiveManager.textPath + filename);
            fileInput.text = curText;
        }
        else Debug.LogError(filename + " could not be opened.");
    }

    public void setText()
    {
        File.WriteAllText(ArchiveManager.textPath + curFile, fileInput.text);
        editsMade = true;
    }

    public void resetText()
    {
        fileInput.text = curText;
        setText();
    }

    public void switchFile()
    {
        curFile = ArchiveManager.textFiles[fileDropdown.value];
        getText(curFile);
    }

    public void openFileSelector()
    {
        if(!PauseScript.paused) PauseScript.PauseFunction();
    }

    // resetting the game or scene's current time to zero if edits were made to ensure synchronization with new settings or script contents.
    public void closeFileSelector()
    {
        if(editsMade)
        {
            editsMade = false;
            //Scene curScene = SceneManager.GetActiveScene();
            //SceneManager.LoadScene(curScene.name);

            GTS.currTime = 0f;
            //GlobalTimeScript.positionSlider.value = 0;
            //sceneManip.index = sceneManip.commandList.getIndexAtTime(GTS.currTime);
            SceneManip.clearScene();
            sceneManip.initCommandList();
        }
        else if(PauseScript.paused) PauseScript.PauseFunction();
    }
}
