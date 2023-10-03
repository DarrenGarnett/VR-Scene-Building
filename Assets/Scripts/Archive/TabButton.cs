using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TabManager tabManager;
    public GameObject tabWindow;

    public Image background;
    public Color selected, hover, unselected;

    void Start()
    {
        tabManager.AddTabButton(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabManager.SelectTab(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tabManager.HoverOn(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabManager.HoverOff(this);
    }
}
