using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Class handling time management with user interaction via UI slider.
public class GlobalTimeScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Static variable to store time difference between frames.
    // It is static, because ensures that any part of the game or any script that needs this value can access it directly without needing to instantiate or access a specific object's version of the 
    public static float deltaTime;
    // Current time updated each frame.
    public float currTime;
    // Previous frame's time.
    public float prevTime;
    // Total runtime of the time controller.This means that until runtime is set to a specific value elsewhere, the slider will not allow any adjustment by the user
    //It is static to to be accessed globally, maintaining a consistent reference to the total duration or maximum time for processes that need to be synchronized across various components or scripts in the game.
    public static float runtime = 0f;
    // Flag to check if user is controlling the slider.
    public bool isBeingControlledByUser;
    // Flag to check if time was changed by the slider.
    public bool timeChanged;
    // Reference to the slider UI component.
    public Slider positionSlider;

    // Initialize component references and variables.
    void Start()
    {
        positionSlider = this.GetComponent<Slider>();
        
        prevTime = 0f;
        positionSlider.maxValue = runtime;
        positionSlider.value = Time.deltaTime;
        isBeingControlledByUser = false;
        timeChanged = false;
    }

    // Update time and UI slider each frame.
    void Update()
    {
        // Reset time if it exceeds the runtime.
        if (currTime >= runtime) ResetTime();
        // Update time and slider if not paused or controlled by user.
        else if (!PauseScript.paused && !isBeingControlledByUser)
        {
            currTime = positionSlider.value;
            deltaTime = currTime - prevTime;
            prevTime = currTime;
            currTime += Time.deltaTime;

            // Update slider position if not controlled by user.
            if (!isBeingControlledByUser)positionSlider.SetValueWithoutNotify(currTime);
        }
    }

    // Handle user interaction start on the slider.IPointerDownHandler interface,Unity uses to handle pointer (mouse or touch) input events.
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("User change in slider...");
        isBeingControlledByUser = true;
    }

    // Handle user interaction end on the slider.the user stops interacting (e.g., releases the click or lift the finger) with the UI element
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log("Slider change stopped...");
        isBeingControlledByUser = false;
        timeChanged = true;
    }

    // Reset current time to zero.
    public void ResetTime()
    {
        currTime = 0f;
    }

    // Reset the slider to a new maximum value. It is used in SceneManip.cs
    public void ResetSlider(float max)
    {
        runtime = max;
        positionSlider.maxValue = max;
        // moves the slider to its starting position, useful for situations where the conditions of the interaction have fundamentally changed (like starting a new timer or resetting a game level).
        positionSlider.value = 0;
    }
}
