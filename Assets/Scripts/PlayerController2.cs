using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2 : MonoBehaviour
{
    PlayerActions playerInput;
    PlayerActions.MainActions input;

    CharacterController controller;
    Animator animator;

    bool isGrounded;

    [Header("Camera")]
    public Camera cam;

    void Awake()
    { 
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        playerInput = new PlayerActions();
        input = playerInput.Main;
        AssignInputs();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        // Repeat Inputs
        if(input.Attack.IsPressed())
        { Attack(); }

        SetAnimations();
    }

    void OnEnable() 
    { input.Enable(); }

    void OnDisable()
    { input.Disable(); }

    void AssignInputs()
    {
        input.Attack.started += ctx => Attack();
    }

    // ---------- //
    // ANIMATIONS //
    // ---------- //

    public const string IDLE = "Ready Idle";
    //public const string WALK = "Walk";
    public const string ATTACK1 = "Attack 1";
    public const string ATTACK2 = "Attack 2";

    string currentAnimationState;

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
        if(!attacking)
        {
            ChangeAnimationState(IDLE);
        }
    }

    // ------------------- //
    // ATTACKING BEHAVIOUR //
    // ------------------- //

    [Header("Attacking")]
    public float attackDistance = 3f;
    public float attackDelay = 0.4f;
    public float attackSpeed = 1f;
    public int attackDamage = 1;
    public LayerMask attackLayer;

    public GameObject hitEffect;
    //public AudioClip swordSwing;
    //public AudioClip hitSound;

    bool attacking = false;
    bool readyToAttack = true;
    int attackCount;

    public void Attack()
    {
        if(!readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);

        if(attackCount == 0)
        {
            ChangeAnimationState(ATTACK1);
            attackCount++;
        }
        else
        {
            ChangeAnimationState(ATTACK2);
            attackCount = 0;
        }
    }

    void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
    }

    void AttackRaycast()
    {
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        { 
            HitTarget(hit.point);

            if(hit.transform.TryGetComponent<Actor>(out Actor T))
            { T.TakeDamage(attackDamage); }
        } 
    }

    void HitTarget(Vector3 pos)
    {
        GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity);
        Destroy(GO, 20);
    }
}