using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToggleButton : MonoBehaviour, IPointerClickHandler
{
    Button button;
    Image background;
    bool on = false;
    public Color onColor;
    public Color offColor;
    public bool overrideButtonColor = false;

    public GameObject toggleObject;

    // Start is called before the first frame update
    void Start()
    {
        button = gameObject.GetComponent<Button>();
        background = gameObject.GetComponent<Image>();

        background.color = offColor;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        on = !on;

        if(!overrideButtonColor)
        {
            if(on) background.color = onColor;
            else background.color = offColor;
        }

        if(toggleObject)
        {
            if(toggleObject.activeSelf) toggleObject.SetActive(false);
            else toggleObject.SetActive(true);
        }
    }

    public void SetOn()
    {
        on = true;
        background.color = onColor;
    }

    public void SetOff()
    {
        on = false;
        background.color = offColor;
    }
}
