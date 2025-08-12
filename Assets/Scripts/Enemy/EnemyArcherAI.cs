using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyArcherAI : MonoBehaviour, IEnemyAI
{
    GameObject player;
    NavMeshAgent agent;
    Actor actor;

    [SerializeField] LayerMask groundLayer, playerLayer;

    Animator animator;
    public float animationSpeed = 1.0f;

    [Header("Detection")]
    [SerializeField] float sightRange;
    bool playerInSight;

    [Header("Patrol")]
    [SerializeField] float patrolRange;
    Vector3 destPoint;
    bool walkpointSet;

    [Header("Attack")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float attackCooldown = 2f;
    private float cooldownTimer;
    GameObject currentArrow;
    bool isShooting = false;

    private bool disabledAfterDeath = false;

    public void EnableAttack()
    {
        // Nothing needed here, it's just to use it in both EnemyAI and EnemyArcherAI
    }

    public void DisableAttack()
    {
        if (currentArrow != null)
            Destroy(currentArrow);
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player");
        animator = GetComponent<Animator>();
        actor = GetComponent<Actor>();
        cooldownTimer = attackCooldown;
        agent.autoRepath = true;
    }

    void Update()
    {
        if (actor.dead)
        {
            if (!disabledAfterDeath)
            {
                if (agent.enabled)
                {
                    agent.enabled = false;
                }
                
                // Clean enemy up, disable everything
                isShooting = false;
                animator.ResetTrigger("Attack");
                animator.ResetTrigger("AttackOver");
                animator.SetBool("isAiming", false);

                disabledAfterDeath = true;
            }

            return;
        }

        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p;
            return; // Wait till it's assigned
        }

        playerInSight = Physics.CheckSphere(transform.position, sightRange, playerLayer);

        // Prevent walking if about to shoot
        if (playerInSight && cooldownTimer <= attackCooldown * 0.2f)
        {
            animator.SetFloat("Speed", 0f);
            agent.SetDestination(transform.position);
        }

        // If player is no longer in sight but the archer is mid-attack animation, cancel it
        if (!playerInSight && (isShooting || currentArrow != null || animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")))
        {
            CancelShot();
        }

        if (!playerInSight && !isShooting && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            CancelShot();
            animator.SetFloat("Speed", 1f);
        }

        if (playerInSight)
        {
            Attack();

            // Prevent movement animation while aiming
            if (isShooting || animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                animator.SetFloat("Speed", 0f);
                agent.SetDestination(transform.position); // hard stop
            }
        }

        else if (cooldownTimer < attackCooldown * 0.5f)
        {
            // Reset cooldown si se fue muy pronto
            cooldownTimer = attackCooldown;
        }

        else
        {
            Patrol();
        }
    }

    void LateUpdate()
    {
        if (actor.dead) return;

        if ((playerInSight || isShooting))
        {
            // Only rotate if not already attacking and cooldown is about to finish
            Vector3 lookPos = player.transform.position;
            lookPos.y = transform.position.y;

            // Smooth rotation (optional)
            Quaternion targetRot = Quaternion.LookRotation(lookPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
    }

    void Patrol()
    {
        if (!walkpointSet)
            SearchForDest();

        if (walkpointSet && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            agent.SetDestination(destPoint);
            animator.SetFloat("Speed", 1f); // walking animation
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            walkpointSet = false;
        }
    }

    void SearchForDest()
    {
        float x = Random.Range(-patrolRange, patrolRange);
        float z = Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(transform.position.x + x, transform.position.y + 10f, transform.position.z + z);

        if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            destPoint = hit.point;
            walkpointSet = true;
        }
    }

    void Attack()
    {
        if (arrowPrefab == null || firePoint == null) return;

        if (isShooting || animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) return;

        // Always stop movement and look at player
        agent.SetDestination(transform.position);
        animator.SetFloat("Speed", 0f);

        // Aim toward player
        Vector3 lookPos = player.transform.position;
        lookPos.y = transform.position.y;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos - transform.position), Time.deltaTime * 5f);

        // If still waiting cooldown, just stay in "Ready"
        if (cooldownTimer > 0f)
        {
            animator.SetBool("isAiming", true); // Enters Ready state
            cooldownTimer -= Time.deltaTime;
            return;
        }

        // Ready to shoot
        isShooting = true;
        animator.ResetTrigger("AttackOver");
        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack"); // Enters actual shooting animation
        cooldownTimer = attackCooldown;
    }

    public void CancelShot()
    {
        if (currentArrow != null)
        {
            Destroy(currentArrow);
            currentArrow = null;
        }

        isShooting = false;
        cooldownTimer = attackCooldown;

        animator.ResetTrigger("Attack");
        animator.SetTrigger("AttackOver");
        animator.SetBool("isAiming", false);
    }

    public void PrepareArrow()
    {
        // Called when archer grabs the arrow from quiver
        if (arrowPrefab && firePoint)
        {
            currentArrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation, firePoint);
            currentArrow.GetComponent<Arrow>().EnableTimeout(); // enables timeout in case arrow is stuck

        }
    }

    public void Shoot()
    {
        // Cancel shot if player is no longer in sight
        if (!playerInSight)
        {
            CancelShot();
            return;
        }

        // No arrow to shoot
        if (currentArrow == null) return;

        // Called when archer releases the arrow
        if (currentArrow != null)
        {
            currentArrow.transform.parent = null;
            Arrow arrowScript = currentArrow.GetComponent<Arrow>();
            if (arrowScript != null)
            {
                // Force rotation toward player before shooting
                Vector3 lookPos = player.transform.position;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);

                // Target a higher point on the player (like chest or head level)
                Vector3 targetPos = player.transform.position + new Vector3(0, 4.2f, 0); // Desired Y height
                Vector3 shootDirection = (targetPos - firePoint.position).normalized;
                arrowScript.SetShooter(gameObject);
                arrowScript.Fire(shootDirection);
            }

            currentArrow = null;
        }

        // Reset shoot state
        isShooting = false;
        cooldownTimer = attackCooldown;
        animator.SetTrigger("AttackOver");

        // Fallback: force end of attack if needed
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            StartCoroutine(ForceExitAttack());
        }

    }
    private IEnumerator ForceExitAttack()
    {
        yield return new WaitForSeconds(1f);

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            animator.SetBool("isAiming", false);
            isShooting = false;
        }
    }

    public void EndAttack()
    {
        isShooting = false;
        animator.ResetTrigger("Attack");
        animator.SetTrigger("AttackOver");
    }

}
