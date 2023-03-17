using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GlobalTimeScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static float deltaTime;
    public float currTime;
    public float prevTime;
    public static float runtime = 50f;

    public bool isBeingControlledByUser;

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
        if(currTime >= runtime) ResetTime();
        else if(!PauseScript.paused)
        {
            currTime = positionSlider.value;
            deltaTime = currTime - prevTime;
            prevTime = currTime;
            currTime += Time.deltaTime;

            if(!isBeingControlledByUser)positionSlider.SetValueWithoutNotify(currTime);
        }
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("User change in slider...");
        isBeingControlledByUser = true;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log("Slider change stopped...");
        isBeingControlledByUser = false;
    }

    public void ResetTime()
    {
        currTime = 0f;
    }
}
