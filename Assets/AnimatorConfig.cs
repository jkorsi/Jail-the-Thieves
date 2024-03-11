using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorConfig : MonoBehaviour
{
    public float speed = 1.0F;
    Animator m_Animator;

    void Start()
    {
        //Get the animator, attached to the GameObject you are intending to animate.
        m_Animator = gameObject.GetComponent<Animator>();
        m_Animator.speed = speed;
    }
}