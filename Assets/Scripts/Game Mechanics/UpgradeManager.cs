using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public List<Upgrade> allUpgrades; // Assigned via Inspector
    public Transform upgradesContainer; // The UI which will contain the upgrades
    public GameObject upgradePanel;
    public GameObject upgradePrefab; // Prefab with icon, description, and button
    public PlayerController player;
    public GameObject notEnoughCoinsText;

    public void ShowRandomUpgrades(int count)
    {
        upgradePanel.SetActive(true);
        Time.timeScale = 0f;

        foreach (Transform child in upgradesContainer)
            Destroy(child.gameObject);

        List<Upgrade> chosen = new List<Upgrade>();
        while (chosen.Count < count)
        {
            var upgrade = allUpgrades[Random.Range(0, allUpgrades.Count)];
            if (!chosen.Contains(upgrade)) chosen.Add(upgrade);
        }

        foreach (var upg in chosen)
        {
            GameObject go = Instantiate(upgradePrefab, upgradesContainer);
            go.GetComponent<UpgradeUI>().Setup(upg, player, this);
        }
    }

    public void ContinueGame()
    {
        Time.timeScale = 1f;
        upgradePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ShowNotEnoughCoinsMessage(float duration = 2f)
    {
        StopAllCoroutines(); // In case it's already active
        StartCoroutine(ShowMessageRoutine(duration));
    }

    private IEnumerator ShowMessageRoutine(float duration)
    {
        notEnoughCoinsText.SetActive(true);
        yield return new WaitForSecondsRealtime(duration);
        notEnoughCoinsText.SetActive(false);
    }

    public void RerollUpgrades()
    {
        //checks that the player has enough coins
        if (player.coins < 30)
        {
            ShowNotEnoughCoinsMessage();
            return;
        }

        player.coins -= 30;
        player.UpdateCoinsUI();
        ShowRandomUpgrades(3); // Re-rolls upgrades!
    }

}
