using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseScript : MonoBehaviour
{
    public static Button pauseButton;
    public Button PBCopy;

    // Boolean to allow other scripts to see when the animation is paused
    public static bool paused;

    // UI Controls for the main camera
    private GameObject mainCameraControls;

    private void Start()
    {
        mainCameraControls = GameObject.FindGameObjectWithTag("MainCameraMovement");
        paused = false;
        pauseButton = PBCopy;
    }

    
    public static void PauseFunction()
    {
        // Case where animation is playing
        if(paused == false)
        {
            paused = true;
            // Set the time scale to 0
            // Time.timeScale = 0;
            // Change the text on the button to say "Resume"
            pauseButton.GetComponentInChildren<Text>().text = "Play";
            // Hide the main camera controls since the camera can't move if the time scale is 0
            // mainCameraControls.SetActive(false);
        }
        // Case where animation is paused
        else
        {
            paused = false;
            // Return the time scale to the current playback speed
            // Time.timeScale = PlaybackSpeedScript.currPlaySpeed;
            // Change the text on the button to say "Pause" again
            pauseButton.GetComponentInChildren<Text>().text = "Pause";
            // Show the main camera controls again
            // mainCameraControls.SetActive(true);
        }
    }
}
