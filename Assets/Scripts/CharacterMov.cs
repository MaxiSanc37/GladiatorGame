using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class CharacterMov : MonoBehaviour
{
    public float m_Speed = 12f;
    public float m_JumpForce = 3f;
    public float gravity = -9.81f;

    //credit to Brackeys on youtube
    public CharacterController controller;
    Vector3 velocity;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;

    private string m_MovementAxisName;
    private string m_StrafeAxisName;
    private string m_JumpAxisName;
    private Rigidbody m_Rigidbody;
    private float m_MovementInputValue;
    private float m_StrafeInputValue;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }


    private void OnEnable()
    {
        m_Rigidbody.isKinematic = false;
        m_MovementInputValue = 0f;
        m_StrafeInputValue = 0f;
    }


    private void OnDisable()
    {
        m_Rigidbody.isKinematic = true;
    }


    private void Start()
    {
        m_MovementAxisName = "Vertical";
        m_StrafeAxisName = "Horizontal";
        m_JumpAxisName = "Jump";
    }

    private void Update()
    {
        // Store the player's input.
        m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
        m_StrafeInputValue = Input.GetAxis(m_StrafeAxisName);
        // Move the player.
        // Adjust the rigidbodies position and orientation in FixedUpdate. 
        MoveJump();
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
        if (Input.GetButtonDown(m_JumpAxisName) && isGrounded)
        {
            //increases the maximum slope that can be climbed
            controller.slopeLimit = 100.0f;
            //apply jumping force (sqrt(x * -2 * gravity) is the jump equation)
            velocity.y = Mathf.Sqrt(m_JumpForce * -2f * gravity);
        }
    }
}
