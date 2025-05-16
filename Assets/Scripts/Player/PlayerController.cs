using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Statistics")]
    public float m_Speed = 10f;
    public float baseSpeed = 10f;
    public float runSpeedBonus = 6f;
    public float m_JumpForce = 3f;
    public float gravity = -9.81f;
    public float health = 100f;
    public float maxHealth = 100f;
    public float healthRegen = 0f;
    public int coins = 0;

    public TMP_Text coinText;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float stamina;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 10f;
    public Slider staminaBar;
    public bool isRunning;
    private bool isRunCooldown = false;
    public float runCooldownDuration = 1.5f;
    private float runCooldownTimer = 0f;


    public TMP_Text staminaText;

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

    [SerializeField] private HealthUI healthBar;

    GameController GC;

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
    public const string ATTACK2 = "Attack 2";

    string currentAnimationState;

    public void ChangeAnimationState(string newState)
    {
        // STOP THE SAME ANIMATION FROM INTERRUPTING WITH ITSELF //
        if (currentAnimationState == newState) return;

        // PLAY THE ANIMATION //
        currentAnimationState = newState;
        animator.speed = attackSpeed; // change speed for attack animation
        animator.CrossFadeInFixedTime(currentAnimationState, 0.3f);
    }

    void SetAnimations()
    {
        if (!attacking && currentAnimationState != IDLE && !animator.GetCurrentAnimatorStateInfo(0).IsName(IDLE))
        {
            animator.speed = 1f;
            ChangeAnimationState(IDLE);
        }
    }

    // ------------------- //
    // ATTACKING BEHAVIOUR //
    // ------------------- //

    [Header("Attacking")] //--> attacking params
    public float baseAttackDuration = 1f;  // original animation time
    public float baseAttackDelay = 0.42f;   // striking point
    public float attackDistance = 3f;
    public float attackSpeed = 1f;
    public float attackDamage = 10f;
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
        healthBar.SetMaxHealth(maxHealth);
        GC = GameObject.Find("Controller").GetComponent<GameController>();
        //stamina configs
        stamina = maxStamina;
        staminaBar.maxValue = maxStamina;
        staminaBar.value = maxStamina;
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

        Running();

        // Health regen
        if (health < maxHealth && healthRegen > 0)
        {
            SetHealth(healthRegen * Time.deltaTime);
        }
    }

    public void Running()
    {
        bool isMoving = Mathf.Abs(m_MovementInputValue) > 0.1f || Mathf.Abs(m_StrafeInputValue) > 0.1f;
        bool shiftHeld = UnityEngine.Input.GetKey(KeyCode.LeftShift);

        // Running logic
        if (shiftHeld && isMoving && stamina > 0f)
        {
            isRunning = true;
            m_Speed = baseSpeed + runSpeedBonus;
            stamina -= staminaDrainRate * Time.deltaTime;

            // Trigger regeneration cooldown if stamina hits zero
            if (stamina <= 0f)
            {
                isRunCooldown = true;
                runCooldownTimer = runCooldownDuration;
            }
        }
        else
        {
            isRunning = false;
            m_Speed = baseSpeed;

            // Cooldown timer before stamina starts regenerating
            if (isRunCooldown)
            {
                runCooldownTimer -= Time.deltaTime;
                if (runCooldownTimer <= 0f)
                {
                    isRunCooldown = false;
                }
            }
            else
            {
                stamina += staminaRegenRate * Time.deltaTime;
            }
        }

        // Clamp and update UI
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        staminaBar.value = stamina;
        staminaText.text = Mathf.RoundToInt(stamina).ToString();
    }

    public void UpdateCoinsUI()
    {
        if (coinText != null)
            coinText.text = coins.ToString();
    }

    //Attack Code
    public void Attack()
    {
        if (!readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        // Changes the animation speed
        animator.speed = attackSpeed;

        float duration = baseAttackDuration / attackSpeed;
        float delay = baseAttackDelay / attackSpeed;

        StartCoroutine(HandleAttack(duration, delay));

        // Alternate between animations
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
    }

    private IEnumerator HandleAttack(float duration, float delay)
    {
        yield return new WaitForSeconds(delay);
        AttackRaycast();

        yield return new WaitForSeconds(duration - delay - 0.02f); // buffer
        ResetAttack();
    }

    //resets the attacking conditions
    private void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
        animator.speed = 1f; // for other animations to play normally
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

    // ------------------- //
    // STATISTICS BEHAVIOUR //
    // ------------------- //

    public void SetHealth(float healthChange)
    {
        //changes the current health
        health += healthChange;
        //makes sure that the health change is legal (> 0 and < maxHealth)
        health = Mathf.Clamp(health,0, maxHealth);
        
        //set the health in health bar UI
        healthBar.SetHealth(health);

        if (health <= 0)
        {
            GC.GameOver();
        }
    }
}
