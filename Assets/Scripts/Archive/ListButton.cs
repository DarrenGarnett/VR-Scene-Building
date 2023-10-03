using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ListButton : MonoBehaviour, IPointerClickHandler
{
    ListUtil listUtil;
    public GameObject nameInput;
    TMP_InputField nameInputField;
    string curName, newName;

    void Start()
    {
        listUtil = GameObject.FindGameObjectWithTag("PrefabList").GetComponent<ListUtil>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.clickCount == 2) 
        {
            //Debug.Log ("double click at " + Time.time);
            OpenNameInput();
            StartCoroutine(AwaitSubmit());
        }
        
        listUtil.curObject = gameObject;
    }

    public IEnumerator AwaitSubmit()
    {
        while(true)
        {
            if(Input.GetKeyUp(KeyCode.Return)) break;

            GameObject selectedObj = EventSystem.current.currentSelectedGameObject;
            if(gameObject != selectedObj && nameInput != selectedObj) break;
            
            yield return null;
        }

        CloseNameInput();
    } 

    public void OpenNameInput()
    {
        nameInput.SetActive(true);
        
        nameInputField = nameInput.GetComponent<TMP_InputField>();
        nameInputField.text = gameObject.name;

        nameInputField.Select();
        nameInputField.MoveToEndOfLine(false, false);
    }

    public void CloseNameInput()
    {
        nameInput.SetActive(false);

        ListUtil.ChangeText(gameObject, nameInputField.text);
        ArchiveManager.SetName(gameObject.name, nameInputField.text);
        gameObject.name = nameInputField.text;
    }
}
