using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListUtil : MonoBehaviour
{
    public GameObject listParent;
    List<GameObject> listObjects = new List<GameObject>();
    public GameObject curObject;

    void Awake()
    {
        listObjects = Utility.GetChildren(listParent);
        curObject = listObjects[0];
        //foreach(GameObject obj in listObjects) Debug.Log(obj.name);
    }

    public void AddToList(GameObject obj, string name)
    {
        obj = Instantiate(obj, listParent.transform);
        obj.name = name;
        ChangeText(obj, name);

        listObjects.Add(obj);
    }

    public void RemoveFromList(GameObject obj)
    {
        Destroy(obj);
        listObjects.Remove(obj);
    }

    public static void ChangeText(GameObject obj, string newText)
    {
        Text objText = obj.GetComponentInChildren<Text>();
        if(objText) objText.text = newText;

        TMP_Text tmpText = obj.GetComponentInChildren<TMP_Text>();
        if(tmpText) tmpText.text = newText;
    }
}
