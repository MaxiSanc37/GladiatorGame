using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text costText;
    public Image icon;
    public Button button;
    public Image coin;

    private Upgrade upgrade;
    public UpgradeManager upgradeManager;
    private PlayerController player;

    public void Setup(Upgrade upgradeData, PlayerController playerRef, UpgradeManager managerRef)
    {
        upgrade = upgradeData;
        player = playerRef;
        upgradeManager = managerRef;

        nameText.text = upgrade.u_name;
        descriptionText.text = upgrade.description;
        costText.text = upgrade.cost.ToString();
        icon.sprite = upgrade.icon;

        button.onClick.AddListener(ApplyUpgrade);
    }

    public void ApplyUpgrade()
    {
        // Checks that the player has enough coins for the upgrade
        if (player.coins < upgrade.cost)
        {
            upgradeManager.ShowNotEnoughCoinsMessage();
            return;
        }

        upgrade.playerStats = player; // Assign player to upgrade object
        upgrade.ApplyUpgrade(upgrade);

    }
}
