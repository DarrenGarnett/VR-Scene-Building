using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaybackSpeedScript : MonoBehaviour
{
    // This float will act as a way for other scripts to access the current playback speed
    public static float currPlaySpeed;

    private void Awake()
    {
        // Initialize currPlaySpeed to be 1
        currPlaySpeed = 1;
    }

    public void UpdatePlaybackSpeed(int selectedVal)
    {
        // Set the playback speed to the appropriate value based on the selected item in the dropdown
        switch(selectedVal)
        {
            case 0:
                currPlaySpeed = 0.25f;
                break;
            case 1:
                currPlaySpeed = 0.5f;
                break;
            case 2:
                currPlaySpeed = 0.75f;
                break;
            case 3:
                currPlaySpeed = 1f;
                break;
            case 4:
                currPlaySpeed = 1.25f;
                break;
            case 5:
                currPlaySpeed = 1.5f;
                break;
            case 6:
                currPlaySpeed = 1.75f;
                break;
            case 7:
                currPlaySpeed = 2f;
                break;
        }

        /*
         * Update the time scale to the correct playback speed
         * However, only do so if the animation isn't paused
         */
        if(Time.timeScale != 0)
        {
            Time.timeScale = currPlaySpeed;
        }
    }

}
