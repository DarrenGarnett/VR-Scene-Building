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
    public static float runtime = 0f;

    public bool isBeingControlledByUser;
    public bool timeChanged;

    private static Slider positionSlider;

    // Start is called before the first frame update
    void Start()
    {
        positionSlider = this.GetComponent<Slider>();
        
        prevTime = 0f;
        positionSlider.maxValue = runtime;
        positionSlider.value = Time.deltaTime;
        isBeingControlledByUser = false;
        timeChanged = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(currTime >= runtime) ResetTime();
        else if(!PauseScript.paused && !isBeingControlledByUser)
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
        timeChanged = true;
    }

    public void ResetTime()
    {
        currTime = 0f;
    }

    public static void ResetSlider(float max)
    {
        runtime = max;
        positionSlider.maxValue = max;
        positionSlider.value = 0;
    }
}
