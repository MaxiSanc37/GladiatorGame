using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;
using UnityEngine.AI;

public class Actor : MonoBehaviour
{
    //current actor health and max health
    int currentHealth;
    public int maxHealth;
    Animator animator;
    private Rigidbody m_Rigidbody;

    bool hit = false;
    public bool dead = false;

    public const string WALK = "Walk";
    public const string HIT = "Hit";
    public const string DEATH = "Death";

    string currentAnimationState;

    WaveSystem spawner;

    [SerializeField] GameObject ragdollRoot;

    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        DisableRagdoll();
    }

    private void Update()
    {
        //if (dead) return;
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
        if (!hit && !dead)
        {
            ChangeAnimationState(WALK); // change to idle
        }
    }

    public void Hit()
    {
        //Checks that the enemy hasn't been hit or that it isn't dead
        if (hit || dead) return;

        hit = true;

        // Cancel current attack if it's an archer
        IEnemyAI ai = GetComponent<IEnemyAI>();
        if (ai is EnemyArcherAI archerAI)
        {
            archerAI.CancelShot();
        }

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
            //call death function
            Death(); 
        }
    }

    //activates the ragdoll
    void ActivateRagdoll()
    {
        IEnemyAI ai = GetComponent<IEnemyAI>();
        if (ai != null)
        {
            ai.DisableAttack();
        }

        foreach (var rb in ragdollBodies)
        {
            if (rb != m_Rigidbody)
            {
                rb.isKinematic = false;
                rb.mass = 30f;
                rb.drag = 0f;
                rb.angularDrag = 6f;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }   
        }

        foreach (var col in ragdollColliders)
        {
            if (col != GetComponent<Collider>())
                col.enabled = true;
        }

        //turn off physics
        m_Rigidbody.isKinematic = true;
        GetComponent<Collider>().enabled = false;

        //gets the hips of the enemy
        var hips = ragdollBodies.FirstOrDefault(rb => rb.name.Contains("mixamorig:Hips"));

        //if the hips are found
        if (hips != null)
        {
            //get a random vector
            Vector3 randomDirection = Random.insideUnitSphere;
            //give a bit of height in the y axis so it's not plain
            randomDirection.y = 0.2f;
            //add the force to the hips
            hips.AddForce(randomDirection.normalized * 200f, ForceMode.Impulse);
            //adds torque for random spin
            hips.AddTorque(Random.onUnitSphere * 200f, ForceMode.Impulse);
        }

        //turn off animator
        animator.applyRootMotion = false;
        animator.enabled = false;
    }

    //deactivates the ragdoll
    void DisableRagdoll()
    {
        foreach (var rb in ragdollBodies)
        {
            if (rb != m_Rigidbody)
                rb.isKinematic = true;
        }

        foreach (var col in ragdollColliders)
        {
            if (col != GetComponent<Collider>())
                col.enabled = false;
        }
    }

    //wait 1 frame and activate ActivateRagdoll()
    private IEnumerator DelayedRagdoll()
    {
        yield return null; // espera 1 frame
        ActivateRagdoll();
    }

    // Death function
    void Death()
    {
        dead = true;

        //turn off NavMesh
        var agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false; 
        }

        //Turn off animator and root motion
        animator.applyRootMotion = false;
        animator.enabled = false;

        // Turn off main rigidbody and collision
        m_Rigidbody.isKinematic = true;
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;

        StartCoroutine(DelayedRagdoll());

        //if there's a spawner
        if (spawner != null)
        {
            //removes enemy from the spawner
            spawner.currEnemies.Remove(this.gameObject);
        }

        // Give coins to the player when enemy dies
        var player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        var waveSystem = spawner; // spawner ya lo tenés asignado

        if (player != null && waveSystem != null)
        {
            int baseReward = 10; // initial coin drop per enemy
            int waveIndex = waveSystem.GetCurrentWaveIndex();
            float multiplier = 1f + waveIndex * 0.15f; // how many more coins you get every new round
            int reward = Mathf.RoundToInt(baseReward * multiplier);
            player.coins += reward;
            player.UpdateCoinsUI();
        }

        Invoke(nameof(Despawn), 20f);
    }

    // Despawns the gameObject
    void Despawn()
    {
        //Destroys the game object with the deathDelay in mind
        Destroy(gameObject);
    }

    public void SetSpawner(WaveSystem _spawner)
    {
        spawner = _spawner;
    }
}
