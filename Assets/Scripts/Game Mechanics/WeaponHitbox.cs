using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] EnemyAI enemyAI;
    [SerializeField] Actor enemy;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            if (enemy.dead) return;
            player.SetHealth(-damage);
            enemyAI.DisableAttack();
        }
    }
}
