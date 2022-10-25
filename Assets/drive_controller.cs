using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drive_controller : MonoBehaviour
{
    Animator ani;
    float xmult = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        ani = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        ani.SetFloat("speed_multi", xmult);
    }
}
