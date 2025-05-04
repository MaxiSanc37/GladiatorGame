using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Credits to The Game Dev Cave for the code

public class EnemyAI : MonoBehaviour
{
    GameObject player;
    NavMeshAgent agent;

    [SerializeField] LayerMask groundLayer, playerLayer;

    //patrol
    Vector3 destPoint;
    bool walkpointSet;
    [SerializeField] float range;

    //state change
    [SerializeField] float sightRange, attackRange;
    bool playerInSight;
    bool playerInAttackRange;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        playerInSight = Physics.CheckSphere(transform.position, sightRange, playerLayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

        if (!playerInSight && !playerInAttackRange)
        {
            Patrol();
        }

        if (playerInSight && !playerInAttackRange)
        {
            Chase();
        }
        
        if (playerInAttackRange)
        {
            Attack();
        }
    }

    void Chase()
    {
        agent.SetDestination(player.transform.position);
    }

    void Attack()
    {

    }

    void Patrol()
    {
        //if there's no walkpoint set
        if (!walkpointSet)
        {
            //search for a destination to go to
            SearchForDest();
        }
        //if there's a set walking point
        if (walkpointSet)
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
}
