using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade : MonoBehaviour
{
    public string u_name;
    public string description;
    public Sprite icon;
    public int cost;
    public UpgradeType type;
    public float value;
    public bool notEnoughCoins = false;

    public PlayerController playerStats;

    public void ApplyUpgrade(Upgrade upgrade)
    {
        playerStats.coins -= upgrade.cost;

        switch (upgrade.type)
        {
            case UpgradeType.Speed:
                playerStats.m_Speed += upgrade.value; playerStats.baseSpeed += upgrade.value; break; // --> speed upgrade
            case UpgradeType.AttackSpeed:
                playerStats.attackSpeed += upgrade.value; break; // --> atk speed upgrade
            case UpgradeType.Damage:
                playerStats.attackDamage *= upgrade.value; break; // --> atk damage upgrade
            case UpgradeType.MaxHealth:
                playerStats.maxHealth += upgrade.value; playerStats.SetHealth(upgrade.value);
                playerStats.healthUI.SetMaxHealth(playerStats.maxHealth); // Update HealthUI
                playerStats.healthUI.SetHealth(playerStats.health);
                break; // --> max health upgrade (also heals you by the amount changing the HealthBar UI)
            case UpgradeType.MaxStamina:
                playerStats.maxStamina += upgrade.value; // --> max stamina upgrade
                playerStats.staminaBar.maxValue = playerStats.maxStamina; // --> update max value for staminaBar
                playerStats.staminaBar.value = playerStats.stamina;
                playerStats.staminaText.text = Mathf.RoundToInt(playerStats.stamina).ToString();
                break; 
            case UpgradeType.HealthRegen:
                playerStats.healthRegen += upgrade.value; break; // --> health regen upgrade
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (playerStats.coinText != null)
        {
            playerStats.coinText.text = playerStats.coins.ToString();
        }
    }

    public enum UpgradeType
    {
        Speed,
        AttackSpeed,
        Damage,
        MaxHealth,
        MaxStamina,
        HealthRegen
    }

}
