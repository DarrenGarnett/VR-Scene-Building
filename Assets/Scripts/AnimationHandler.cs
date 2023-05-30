using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    private Animator animator;
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        speed = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(PauseScript.paused) animator.speed = 0;
        else animator.speed = speed;
    }
}
