using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SellController : MonoBehaviour
{
    [Header("Groups")]
    [SerializeField] private List<GameObject> oreCollectedGroups;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Buttons")]
    [SerializeField] private List<Button> sellButtons;

    [Header("Other")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject oreCollectedPrefab;
    [SerializeField] private GameObject sellButtonPrefab;
    [SerializeField] private GameObject inventory;

    private float sellAmount = 1;
    private float oreTypesCollected;

    public void SetupInventory()
    {
        PlayerController temp = player.GetComponent<PlayerController>();
        int groupNum = 0;
        int blockNum = 0;
        foreach (GameObject group in temp.oreCollectedGroups)
        {
            blockNum = 0;
            foreach (string oreName in Database.instance.oreNames)
            {
                if (group.GetComponent<OreCollected>().oreNameLabel.text == oreName) break;
                blockNum++;
            }
            //Debug.Log("Setting up group " + groupNum + ", has block "+blockNum);
            GameObject orePrefab = Instantiate(oreCollectedPrefab, inventory.transform);
            orePrefab.GetComponent<RectTransform>().localPosition = new Vector3(-130, 100 - 60 * oreTypesCollected, 0);
            GameObject sellButton = Instantiate(sellButtonPrefab, orePrefab.transform);
            orePrefab.GetComponent<OreCollected>().SetUp(blockNum);
            //Debug.Log(groupNum);
            float oreAmount = temp.oreCollected[blockNum];
            orePrefab.GetComponent<OreCollected>().oreCollectedLabel.text = temp.oreCollected[blockNum].ToString();
            if (orePrefab.GetComponent<OreCollected>().oreCollectedLabel.text != "0") oreTypesCollected++;
            groupNum++;
            oreCollectedGroups.Add(orePrefab);
            sellButton.GetComponent<RectTransform>().localPosition = new Vector3(275, 0, 0);
            if (sellAmount == 0.01f)
            {
                float cost = Mathf.Round(Database.instance.GetOreValue(blockNum)); //calculate original cost WITHOUT adding the sell upgrade
                float costAdded = Mathf.Round(Database.instance.GetOreValue(blockNum) * ((player.GetComponent<PlayerController>().sellLevel - 1) / 10)); //calculate the coins that would be added to that cost with that upgrade
                //Debug.Log("Cost: " + cost + ", Cost added: " + costAdded);
                sellButton.GetComponent<SellOre>().sellAmountText.text = (cost + costAdded).ToString();
            }
            else
            {
                float cost = Mathf.Round(player.GetComponent<PlayerController>().oreCollected[blockNum] * sellAmount) * Database.instance.GetOreValue(blockNum);
                float costAdded = Mathf.Round(Mathf.Round(player.GetComponent<PlayerController>().oreCollected[blockNum] * sellAmount) * (Database.instance.GetOreValue(blockNum) * ((player.GetComponent<PlayerController>().sellLevel - 1) / 10)));
                //Debug.Log("Cost: " + cost + ", Cost added: " + costAdded);
                sellButton.GetComponent<SellOre>().sellAmountText.text = (cost+costAdded).ToString();
            }
        }
        coinsText.text = "Coins: " + player.GetComponent<PlayerController>().coinsCollected.ToString();
    }
    public void RefreshOreTypes()
    {
        //Wipe all current ores, then call SetupInventory() to refresh.
        oreTypesCollected = 0;
        foreach (var oreGroup in oreCollectedGroups)
        {
            oreGroup.GetComponent<OreCollected>().Destroy();
        }
        oreCollectedGroups.Clear();
        SetupInventory();
    }
    public void SellOre(string oreType)
    {
        float oreSold;
        int oreNum = 0;
        int groupNum = 0;
        while (oreNum < Database.instance.oreNames.Count)
        {
            if (oreType == Database.instance.GetOreName(oreNum)) break;
            oreNum++;
        }
        foreach (GameObject group in oreCollectedGroups)
        {
            if (oreType == group.GetComponent<OreCollected>().oreNameLabel.text) break;
            groupNum++;
        }
        int oreValue = Database.instance.GetOreValue(oreNum);
        if (sellAmount == 0.01f)
        {
            oreSold = 1;
            player.GetComponent<PlayerController>().oreCollected[oreNum] -= 1;
        }
        else player.GetComponent<PlayerController>().oreCollected[oreNum] -= oreSold = Mathf.Round(player.GetComponent<PlayerController>().oreCollected[oreNum] * sellAmount);
        oreCollectedGroups[groupNum].GetComponent<OreCollected>().oreCollectedLabel.text = player.GetComponent<PlayerController>().oreCollected[oreNum].ToString();
        if (player.GetComponent<PlayerController>().oreCollected[oreNum] <= 0)
        {
            player.GetComponent<PlayerController>().RemoveGroup(oreNum);
        }
        RefreshOreTypes();
        GainCoins(oreSold, oreValue);
    }

    public void SwitchSellAmount(float sellNum)
    {
        foreach (var button in sellButtons)
        {
            button.GetComponent<Image>().color = Color.gray;
        }
        switch (sellNum)
        {
            case 1:
                sellButtons[0].GetComponent<Image>().color = Color.green;
                break;
            case 10:
                sellButtons[1].GetComponent<Image>().color = Color.green;
                break;
            case 25:
                sellButtons[2].GetComponent<Image>().color = Color.green;
                break;
            case 50:
                sellButtons[3].GetComponent<Image>().color = Color.green;
                break;
            case 100:
                sellButtons[4].GetComponent<Image>().color = Color.green;
                break;
        }
        sellAmount = sellNum/100;
        RefreshOreTypes();
    }
    public void GainCoins(float oreSold, float multiplier)
    {
        float cost = Mathf.Round(oreSold * sellAmount) * multiplier;
        float costAdded = Mathf.Round(Mathf.Round(oreSold * sellAmount) * (multiplier * ((player.GetComponent<PlayerController>().sellLevel - 1) / 10)));
        if (oreSold == 1)
        {
            cost = Mathf.Round(multiplier);
            costAdded = Mathf.Round(multiplier * ((player.GetComponent<PlayerController>().sellLevel - 1) / 10));
        }
        player.GetComponent<PlayerController>().coinsCollected += (cost+costAdded);
        coinsText.text = "Coins: " + player.GetComponent<PlayerController>().coinsCollected.ToString();
    }
}