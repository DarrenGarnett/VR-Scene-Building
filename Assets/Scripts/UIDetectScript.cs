using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDetectScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        CameraMovement.drag = false;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        CameraMovement.drag = true;
    }
}
