using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class PlayerController : MonoBehaviour
{
    public float m_Speed = 12f;
    public float m_JumpForce = 3f;
    public float gravity = -9.81f;

    //credit to Brackeys and Pogle on youtube
    public CharacterController controller;
    public Animator animator;
    Vector3 velocity;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;
    public float animationSpeed = 1.7f;

    PlayerActions playerInput;
    PlayerActions.MainActions input;

    [Header("Camera")]
    public Camera cam;

    private string m_MovementAxisName;
    private string m_StrafeAxisName;
    private string m_JumpAxisName;
    private Rigidbody m_Rigidbody;
    private float m_MovementInputValue;
    private float m_StrafeInputValue;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();

        playerInput = new PlayerActions();
        input = playerInput.Main;
        AssignInputs();
    }


    private void OnEnable()
    {
        m_Rigidbody.isKinematic = false;
        m_MovementInputValue = 0f;
        m_StrafeInputValue = 0f;
        input.Enable();
    }


    private void OnDisable()
    {
        m_Rigidbody.isKinematic = true;
        input.Disable();
    }

    //call attack function once the input for the attack is pressed
    void AssignInputs()
    {
        input.Attack.started += ctx => Attack();
    }

    //Animations
    public const string IDLE = "Ready Idle";
    public const string ATTACK1 = "Attack 1";

    string currentAnimationState;

    public void ChangeAnimationState(string newState)
    {
        // STOP THE SAME ANIMATION FROM INTERRUPTING WITH ITSELF //
        if (currentAnimationState == newState) return;

        // PLAY THE ANIMATION //
        currentAnimationState = newState;
        animator.speed = animationSpeed; // change speed for attack animation
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }

    void SetAnimations()
    {
        // If player is not attacking
        if (!attacking)
        {
            animator.speed = 1; //set speed back to normal
            ChangeAnimationState(IDLE); // change to idle
           
        }
    }

    // ------------------- //
    // ATTACKING BEHAVIOUR //
    // ------------------- //

    [Header("Attacking")] //--> attacking params
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

    private void Start()
    {
        //set axis names
        m_MovementAxisName = "Vertical";
        m_StrafeAxisName = "Horizontal";
        m_JumpAxisName = "Jump";
        animator.speed = animationSpeed;
    }

    private void Update()
    {
        // Store the player's input.
        m_MovementInputValue = UnityEngine.Input.GetAxis(m_MovementAxisName);
        m_StrafeInputValue = UnityEngine.Input.GetAxis(m_StrafeAxisName);
        // Move the player and also jump.
        // Adjust the rigidbodies position and orientation in Update. 
        MoveJump();

        //invoke the attack function when the attack input is pressed
        if (input.Attack.WasPressedThisFrame())
        {
            Attack();
        }

        SetAnimations();
    }

    //Attack Code
    public void Attack()
    {
        if (!readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);
        
        ChangeAnimationState(ATTACK1);
        //combo for two attack animations
        /*
        if (attackCount == 0)
        {
            ChangeAnimationState(ATTACK1);
            attackCount++;
        }
        else
        {
            ChangeAnimationState(ATTACK2);
            attackCount = 0;
        }
        */
    }

    //resets the attacking conditions
    void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
    }

    void AttackRaycast()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            HitTarget(hit.point);

            if (hit.transform.TryGetComponent<Actor>(out Actor T))
            { 
                T.TakeDamage(attackDamage);
                T.Hit();
            }
        }
    }

    //Hit effect instantiation (blood, sword hit, etc.)
    
    void HitTarget(Vector3 pos)
    {
        //instantiates effect
        GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity);
        Destroy(GO, 0.3f);
    }

    //credits to Brackeys on youtube
    private void MoveJump()
    {
        // Adjust the position of the player based on the player's input.
        
        //checks wether the player is on the ground,
        //turns true when the sphere below the player touches the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        //resets the velocity so that falling isn't instanteneous
        if(isGrounded && velocity.y < 0)
        {
            //limits the slope the player can climb
            controller.slopeLimit = 45.0f;
            velocity.y = -2f;
        }

        //movement vector
        Vector3 move = transform.right * m_StrafeInputValue + transform.forward * m_MovementInputValue;

        //move at a certain speed
        controller.Move(move * m_Speed * Time.deltaTime);

        //build up velocity depending on gravity
        velocity.y += gravity * Time.deltaTime;

        //fall with the applied velocity
        controller.Move(velocity * Time.deltaTime);

        //if the player is pressing jump and it's on the ground
        if (UnityEngine.Input.GetButtonDown(m_JumpAxisName) && isGrounded)
        {
            //increases the maximum slope that can be climbed
            controller.slopeLimit = 100.0f;
            //apply jumping force (sqrt(x * -2 * gravity) is the jump equation)
            velocity.y = Mathf.Sqrt(m_JumpForce * -2f * gravity);
        }
    }
}
