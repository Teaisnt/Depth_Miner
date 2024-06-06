using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;

public class UpgradeController : MonoBehaviour
{
    [Header("Upgrade Stuff")]
    public int strengthUpgradeLevel = 1;
    public int reachUpgradeLevel = 1;
    public int inventoryUpgradeLevel = 1;
    public int sellUpgradeLevel = 1;
    public GameObject upgradeButton1;
    public GameObject upgradeButton2;
    public GameObject upgradeButton3;
    public GameObject upgradeButton4;
    [SerializeField] private List<float> inventoryLevels;
    [SerializeField] private TextMeshProUGUI strengthCurrentText;
    [SerializeField] private TextMeshProUGUI strengthUpgradedText;
    [SerializeField] private TextMeshProUGUI reachCurrentText;
    [SerializeField] private TextMeshProUGUI reachUpgradedText;
    [SerializeField] private TextMeshProUGUI inventoryCurrentText;
    [SerializeField] private TextMeshProUGUI inventoryUpgradedText;
    [SerializeField] private TextMeshProUGUI sellCurrentText;
    [SerializeField] private TextMeshProUGUI sellUpgradedText;
    [SerializeField] private TextMeshProUGUI strengthButtonText;
    [SerializeField] private TextMeshProUGUI reachButtonText;
    [SerializeField] private TextMeshProUGUI inventoryButtonText;
    [SerializeField] private TextMeshProUGUI sellButtonText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private GameObject player;

    public void UpdateCosts()
    {
        upgradeButton1.GetComponent<CostController>().SetCost(strengthUpgradeLevel, false);
        upgradeButton2.GetComponent<CostController>().SetCost(reachUpgradeLevel, false);
        upgradeButton3.GetComponent<CostController>().SetCost(inventoryUpgradeLevel, false);
        upgradeButton4.GetComponent<CostController>().SetCost(sellUpgradeLevel, false);
        coinsText.text = "Coins: " + player.GetComponent<PlayerController>().coinsCollected.ToString();
    }
    public void UpgradeButton(CostController costController)
    {
        switch (costController.costName)
        {
            case "Mining Strength":
                {
                    if (!upgradeButton1.GetComponent<CostController>().upgradable || strengthUpgradeLevel > 5) return;
                    strengthUpgradeLevel++;
                    strengthCurrentText.text = "Current: " + strengthUpgradedText.text;
                    if (strengthUpgradeLevel > 5)
                    {
                        strengthUpgradedText.text = "MAX";
                        strengthButtonText.text = "MAX";
                        upgradeButton1.GetComponent<CostController>().SetMaxed();
                    }
                    else
                    {
                        strengthUpgradedText.text = (strengthUpgradeLevel + 1).ToString();
                    }
                    upgradeButton1.GetComponent<CostController>().SetCost(strengthUpgradeLevel, true);
                    player.GetComponent<PlayerController>().baseMiningSpeed -= 0.1f;
                    player.GetComponent<PlayerController>().mineStrengthLevel += 1f;
                    break;
                }
            case "Mining Reach":
                {
                    if (!upgradeButton2.GetComponent<CostController>().upgradable || reachUpgradeLevel > 5) return;
                    reachUpgradeLevel++;
                    reachCurrentText.text = "Current: " + reachUpgradedText.text;
                    if (reachUpgradeLevel > 5)
                    {
                        reachUpgradedText.text = "MAX";
                        reachButtonText.text = "MAX";
                        upgradeButton2.GetComponent<CostController>().SetMaxed();
                    }
                    else
                    {
                        reachUpgradedText.text = (reachUpgradeLevel + 1).ToString();
                    }
                    upgradeButton2.GetComponent<CostController>().SetCost(reachUpgradeLevel, true);
                    player.GetComponent<PlayerController>().reachLevel += 1f;
                    player.GetComponent<PlayerController>().UpdateCookieSprite(player.GetComponent<PlayerController>().lightSprites[reachUpgradeLevel - 1]);
                    break;
                }
            case "Inventory Size":
                {
                    if (!upgradeButton3.GetComponent<CostController>().upgradable || inventoryUpgradeLevel > 5) return;
                    inventoryUpgradeLevel++;
                    inventoryCurrentText.text = "Current: " + inventoryUpgradedText.text;
                    if (inventoryUpgradeLevel > 5)
                    {
                        inventoryUpgradedText.text = "MAX";
                        inventoryButtonText.text = "MAX";
                        upgradeButton3.GetComponent<CostController>().SetMaxed();
                    }
                    else
                    {
                        inventoryUpgradedText.text = inventoryLevels[inventoryUpgradeLevel].ToString();
                    }
                    upgradeButton3.GetComponent<CostController>().SetCost(inventoryUpgradeLevel, true);
                    player.GetComponent<PlayerController>().oreMax = inventoryLevels[inventoryUpgradeLevel - 1];
                    break;
                }
            case "Sell Amount":
                {
                    if (!upgradeButton4.GetComponent<CostController>().upgradable || sellUpgradeLevel > 5) return;
                    sellUpgradeLevel++;
                    sellCurrentText.text = "Current: " + sellUpgradedText.text;
                    if (sellUpgradeLevel > 5)
                    {
                        sellUpgradedText.text = "MAX";
                        sellButtonText.text = "MAX";
                        upgradeButton4.GetComponent<CostController>().SetMaxed();
                    }
                    else
                    {
                        sellUpgradedText.text = (sellUpgradeLevel * 10).ToString() + "%";
                    }
                    upgradeButton4.GetComponent<CostController>().SetCost(sellUpgradeLevel, true);
                    player.GetComponent<PlayerController>().sellLevel = sellUpgradeLevel;
                    break;
                }
        }
        if (strengthUpgradeLevel < 6) upgradeButton1.GetComponent<CostController>().SetCost(strengthUpgradeLevel, false);
        if (reachUpgradeLevel < 6) upgradeButton2.GetComponent<CostController>().SetCost(reachUpgradeLevel, false);
        if (inventoryUpgradeLevel < 6) upgradeButton3.GetComponent<CostController>().SetCost(inventoryUpgradeLevel, false);
        if (sellUpgradeLevel < 6) upgradeButton4.GetComponent<CostController>().SetCost(sellUpgradeLevel, false);
    }
}
