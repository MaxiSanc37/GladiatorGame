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
        //fades into any other animation with a timed transition
        animator.CrossFadeInFixedTime(currentAnimationState, 0.35f);
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

        //mess with the second float to shorten the hit animation duration
        Invoke(nameof(ResetHit), 0.5f);
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
