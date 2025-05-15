using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Credits to The Game Dev Cave for the code

public class EnemyAI : MonoBehaviour, IEnemyAI
{
    GameObject player;
    NavMeshAgent agent;
    Actor enem;

    [SerializeField] LayerMask groundLayer, playerLayer;

    Animator animator;
    public float animationSpeed = 1.0f;

    [SerializeField] BoxCollider weaponCollider;

    //patrol
    Vector3 destPoint;
    bool walkpointSet;
    [SerializeField] float range;

    //state change
    [SerializeField] float sightRange, attackRange;
    bool playerInSight;
    bool playerInAttackRange;

    public float damage = 10f;

    private bool disabledAfterDeath = false;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player");
        animator = GetComponent<Animator>();
        enem = GetComponent<Actor>();
    }

    // Update is called once per frame
    void Update()
    {
        //checks that the enemy is dead
        if (enem.dead)
        {
            // Deactivate once
            if (!disabledAfterDeath)
            {
                DisableAttack();

                // turn off agent
                if (agent.enabled)
                    agent.enabled = false;

                disabledAfterDeath = true;
            }

            return; //turns off AI logic
        }

        playerInSight = Physics.CheckSphere(transform.position, sightRange, playerLayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

        //Patrol when the player is not in sight and in the attack range
        if (!playerInSight && !playerInAttackRange)
        {
            Patrol();
        }

        //Chase when the player is in sight but not in the attack range
        if (playerInSight && !playerInAttackRange)
        {
            Chase();
        }
        
        //Attack when player is in the attack range
        if (playerInAttackRange)
        {
            Attack();
        }
    }

    void Chase()
    {
        //make the enemy target and follow the player
        agent.SetDestination(player.transform.position);
    }

    void Attack()
    {
        //checks that the attack animation is not the one currently playing
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Enemy Attack"))
        {
            //triggers the attack animation
            animator.speed = animationSpeed;
            animator.SetTrigger("Attack");
            //makes the agent stop walking when it attacks
            agent.SetDestination(transform.position);
            agent.transform.rotation.Equals(player);
        }
    }

    void Patrol()
    {
        //if there's no walkpoint set
        if (!walkpointSet)
        {
            //search for a destination to go to
            SearchForDest();
        }
        //if there's a set walking point and that the attackins animation isn't playing
        if (walkpointSet && !animator.GetCurrentAnimatorStateInfo(0).IsName("Enemy Attack"))
        {
            //send the agent to the destination point
            agent.SetDestination(destPoint);
        }
        if (Vector3.Distance(transform.position, destPoint) < 10)
        {
            walkpointSet = false;
        }
        
    }

    void SearchForDest()
    {
        // Random X and Z within range
        float x = Random.Range(-range, range);
        float z = Random.Range(-range, range);

        // Set origin at a height to make sure raycast reaches ground
        Vector3 randomPoint = new Vector3(transform.position.x + x, transform.position.y + 10f, transform.position.z + z);

        // Cast ray downwards to find ground
        if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            destPoint = hit.point; // Set point on ground
            walkpointSet = true;
        }
    }

    public void EnableAttack()
    {
        //enables the weapon collider to attack
        weaponCollider.enabled = true;
    }

    public void DisableAttack()
    {
        //disables the weapon collider to walk
        weaponCollider.enabled = false;
    }
}
