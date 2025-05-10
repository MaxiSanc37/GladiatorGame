using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Credits to The Game Dev Cave for the code

public class EnemyAI : MonoBehaviour
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
        //random z value for the new destination
        float z = Random.Range(-range, range);
        //random x value for the new destination
        float x = Random.Range(-range, range);

        //destination point for the agent
        destPoint = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);

        //checks whether the dest point is valid through a raycast that verifies if the hit point is part of the groundLayer
        if (Physics.Raycast(destPoint, Vector3.down, groundLayer))
        {
            //point can now be walked to
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
