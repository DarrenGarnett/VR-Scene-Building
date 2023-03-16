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

    private Slider positionSlider;

    // Start is called before the first frame update
    void Start()
    {
        positionSlider = this.GetComponent<Slider>();
        prevTime = 0f;
        positionSlider.maxValue = runtime;
        positionSlider.value = Time.deltaTime;
        isBeingControlledByUser = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(currTime >= runtime)
        {
            ResetTime();
        }
        else if(!PauseScript.paused)
        {
            currTime = positionSlider.value;
            deltaTime = currTime - prevTime;
            prevTime = currTime;
            currTime += Time.deltaTime;
            if (!isBeingControlledByUser)
            {
                positionSlider.SetValueWithoutNotify(currTime);
            }
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
        positionSlider.value = 0;
        currTime = 0;
    }
}
