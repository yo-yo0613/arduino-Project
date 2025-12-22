using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorSpeed : MonoBehaviour
{
    public Animator animator;
    public float animSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        if (animator != null)
        {
            animator.speed = animSpeed;
        }  
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
