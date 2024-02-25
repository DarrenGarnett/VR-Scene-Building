using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewingUtil : MonoBehaviour
{
    public static GameObject mainCameraMovement;

    void Start()
    {
        mainCameraMovement = GameObject.FindGameObjectWithTag("MainCamera");
    }

    public static void SetMainCameraMovement(bool val)
    {
        mainCameraMovement.SetActive(val);
    }
}
