using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GlobalTimeScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static float deltaTime;
    private float currTime;
    private float prevTime;
    public static float runtime = 50f;

    private bool isBeingControlledByUser;

    public Slider positionSlider;

    // Start is called before the first frame update
    void Start()
    {
        prevTime = 0f;
        positionSlider.maxValue = runtime;
        positionSlider.value = Time.deltaTime;
        isBeingControlledByUser = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(currTime < runtime && !PauseScript.paused && !isBeingControlledByUser)
        {
            currTime = positionSlider.value;
            deltaTime = currTime - prevTime;
            prevTime = currTime;
            currTime += Time.deltaTime;
            positionSlider.value = currTime;
        }
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        isBeingControlledByUser = true;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        isBeingControlledByUser = false;
    }

    public void ResetTime()
    {
        currTime = 0f;
    }
}
