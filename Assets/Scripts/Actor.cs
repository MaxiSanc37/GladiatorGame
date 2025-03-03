using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    //current actor health and max health
    int currentHealth;
    public int maxHealth;
    public float deathDelay = 0.65f;
    Animator animator;

    bool hit = false;

    public const string IDLE = "IdleEnemy";
    public const string HIT = "Hit";

    string currentAnimationState;

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
            ChangeAnimationState(IDLE); // change to idle
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

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
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
