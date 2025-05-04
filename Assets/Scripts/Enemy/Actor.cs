using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;

public class Actor : MonoBehaviour
{
    //current actor health and max health
    int currentHealth;
    public int maxHealth;
    public float deathDelay = 0.65f;
    Animator animator;
    private Rigidbody m_Rigidbody;

    bool hit = false;

    public const string WALK = "Walk";
    public const string HIT = "Hit";

    string currentAnimationState;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;   
    }

    private void Update()
    {
        SetAnimations();
    }

    public void ChangeAnimationState(string newState)
    {
        // STOP THE SAME ANIMATION FROM INTERRUPTING WITH ITSELF //
        if (currentAnimationState == newState) return;

        // PLAY THE ANIMATION //
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }

    void SetAnimations()
    {
        // If player is not attacking
        if (!hit)
        {
            ChangeAnimationState(WALK); // change to idle
        }
    }

    public void Hit()
    {
        if (hit) return;

        hit = true;

        Invoke(nameof(ResetHit), 1f);
        ChangeAnimationState(HIT);
    }

    //resets the attacking conditions
    void ResetHit()
    {
        hit = false;
    }

    //code for taking damage
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        //die when health is 0 or less
        if(currentHealth <= 0)
        { 
            Death(); 
        }
    }

    void Death()
    {
        // Death function
        // TEMPORARY: Destroy Object
        Destroy(gameObject, deathDelay);
    }
}
