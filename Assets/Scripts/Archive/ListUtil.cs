using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListUtil : MonoBehaviour
{
    public GameObject listParent;
    List<GameObject> listObjects = new List<GameObject>();
    public GameObject selectedObject = null;
    public Color selected, unselected;

    void Awake()
    {
        listObjects = Utility.GetChildren(listParent);
        //foreach(GameObject obj in listObjects) Debug.Log(obj.name);
    }

    public void AddToList(GameObject obj, string name)
    {
        //Debug.Log("Making a button for " + name);
        obj = Instantiate(obj, listParent.transform);
        obj.name = name;
        ChangeText(obj, name);

        listObjects.Add(obj);
    }

    public void RemoveFromList(string name)
    {
        GameObject listObject = listObjects.Find(obj => obj.name == name);

        Destroy(listObject);
        listObjects.Remove(listObject);
    }

    public void Select(GameObject curObject)
    {
        //Debug.Log("Selected " + curObject.name);
        if(selectedObject)
        {
            Image selectedImage = selectedObject.GetComponent<Image>();
            selectedImage.color = unselected;
        }

        selectedObject = curObject;

        Image curImage = curObject.GetComponent<Image>();
        curImage.color = selected;

        //ArchiveManager.SelectItem(curObject.name);
    }

    public void SelectByName(string objectName)
    {
        selectedObject = listObjects.Find(obj => obj.name == objectName);

        if(selectedObject)
        {
            Image curImage = selectedObject.GetComponent<Image>();
            curImage.color = selected;
        }
        else Debug.LogError("Unable to find listobject for the given name");
    }

    public static void ChangeText(GameObject obj, string newText)
    {
        Text objText = obj.GetComponentInChildren<Text>();
        if(objText) objText.text = newText;

        TMP_Text tmpText = obj.GetComponentInChildren<TMP_Text>();
        if(tmpText) tmpText.text = newText;
    }

    public void SetName(string objectName, string newName)
    {
        //Debug.Log("Changing " + objectName + " to " + newName);
        GameObject obj = listObjects.Find(obj => obj.name == objectName);

        if(obj) 
        {
            ChangeText(obj, newName);
            obj.name = newName;
        }
        else Debug.LogError("Unable to find listobject for the given name");
    }

    public void ClearList()
    {
        foreach(GameObject obj in listObjects) Destroy(obj);
        listObjects.Clear();
    }
}
