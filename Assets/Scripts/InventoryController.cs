using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    [Header("Player Groups")]
    [SerializeField] private List<GameObject> oreCollectedGroups1;

    [Header("Deposited Groups")]
    [SerializeField] private List<GameObject> oreCollectedGroups2;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI inventoryOreTotalLabel;
    [SerializeField] private TextMeshProUGUI depositedOreTotalLabel;

    [Header("Buttons")]
    [SerializeField] private List<Button> swapButtons;

    [Header("Other")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject oreCollectedPrefab;
    [SerializeField] private GameObject playerInventory;
    [SerializeField] private GameObject depositedInventory;
    [SerializeField] private int maxOre;

    private float swapAmount = 1;
    private float oreTypesCollected;
    private float depositedOreTypesCollected;
    private float inventoryOreTotal;
    private float depositedOreTotal;

    void Start()
    {
        depositedOreTotalLabel.text = "0/" + maxOre.ToString();
        RefreshOreTypes();
    }
    public void SetupInventory()
    {
        PlayerController temp = player.GetComponent<PlayerController>();
        int groupNum = 0;
        int blockNum = 0;
        inventoryOreTotal = 0;
        foreach (GameObject group in temp.oreCollectedGroups)
        {
            blockNum = 0;
            foreach (string oreName in Database.instance.oreNames)
            {
                if (group.GetComponent<OreCollected>().oreNameLabel.text == oreName) break;
                blockNum++;
            }
            //Debug.Log("Setting up group " + groupNum + ", has block "+blockNum);
            GameObject orePrefab = Instantiate(oreCollectedPrefab, playerInventory.transform);
            orePrefab.GetComponent<RectTransform>().localPosition = new Vector3(-148, 159 - 60 * oreTypesCollected, 0);
            BetterButtonEvents button = orePrefab.GetComponent<BetterButtonEvents>();
            button.OnClick += () => {
                DepositOre(orePrefab.GetComponent<OreCollected>().oreNameLabel.text);
            };
            orePrefab.GetComponent<OreCollected>().SetUp(blockNum);
            //Debug.Log(groupNum);
            float oreAmount = temp.oreCollected[blockNum];
            orePrefab.GetComponent<OreCollected>().oreCollectedLabel.text = temp.oreCollected[blockNum].ToString();
            if (orePrefab.GetComponent<OreCollected>().oreCollectedLabel.text != "0") oreTypesCollected++;
            groupNum++;
            oreCollectedGroups1.Add(orePrefab);
            inventoryOreTotal += int.Parse(orePrefab.GetComponent<OreCollected>().oreCollectedLabel.text);
        }
        inventoryOreTotalLabel.text = inventoryOreTotal.ToString() + "/" + player.GetComponent<PlayerController>().oreMax.ToString();
    }
    public void RefreshOreTypes()
    {
        oreTypesCollected = 0;
        foreach (var oreGroup in oreCollectedGroups1)
        {
            oreGroup.GetComponent<OreCollected>().Destroy();
        }
        oreCollectedGroups1.Clear();
        SetupInventory();
    }
    public void DepositOre(string oreType)
    {
        int oreNum = 0;
        foreach (string oreName in Database.instance.oreNames)
        {
            if (oreType == oreName) break;
            oreNum++;
        }
        float oreCollected = player.GetComponent<PlayerController>().oreCollected[oreNum];
        float oreToSwap = Mathf.Round(oreCollected * swapAmount);
        if (swapAmount == 0.01f)
        {
            oreToSwap = 1;
        }
        if (depositedOreTotal == maxOre) return; //do NOT deposit anything if deposited inventory is full.
        if (depositedOreTotal + oreToSwap >= maxOre)
        {
            oreToSwap -= depositedOreTotal + oreToSwap - maxOre;
            depositedOreTotalLabel.color = Color.red;
        }
        else
        {
            depositedOreTotalLabel.color = Color.white;
        }
        oreCollected -= oreToSwap;
        //Debug.Log("Depositing " + oreToSwap + " " +oreType);
        foreach (GameObject ore in oreCollectedGroups1)
        {
            if (ore.GetComponent<OreCollected>().oreNameLabel.text == oreType)
            {
                player.GetComponent<PlayerController>().oreCollected[oreNum] -= (float.Parse(ore.GetComponent<OreCollected>().oreCollectedLabel.text) - oreToSwap);
            }
        }
        bool isAdding = false;
        int invIndex = 0;
        foreach (GameObject ore in oreCollectedGroups2)
        {
            if (ore.GetComponent<OreCollected>().oreNameLabel.text == oreType)
            {
                ore.GetComponent<OreCollected>().oreCollectedLabel.text = (float.Parse(oreCollectedGroups2[invIndex].GetComponent<OreCollected>().oreCollectedLabel.text) + oreToSwap).ToString();
                isAdding = true;
                break;
            }
            invIndex++;
        }
        if (isAdding == false) 
        {
            PlayerController temp = player.GetComponent<PlayerController>();
            GameObject orePrefab = Instantiate(oreCollectedPrefab, depositedInventory.transform);
            orePrefab.GetComponent<RectTransform>().localPosition = new Vector3(-150, 159 - 60 * depositedOreTypesCollected, 0);
            BetterButtonEvents button = orePrefab.GetComponent<BetterButtonEvents>();
            button.OnClick += () => {
                WithdrawOre(orePrefab.GetComponent<OreCollected>().oreNameLabel.text);
            };
            orePrefab.GetComponent<OreCollected>().SetUp(oreNum);
            //Debug.Log(groupNum);
            float oreAmount = temp.oreCollected[oreNum];
            orePrefab.GetComponent<OreCollected>().oreCollectedLabel.text = temp.oreCollected[oreNum].ToString();
            depositedOreTypesCollected++;
            oreCollectedGroups2.Add(orePrefab);
        }
        //Debug.Log(oreNum+", "+oreType);
        //Debug.Log(player.GetComponent<PlayerController>().oreCollected[oreNum]);
        UpdateOres(oreType, oreCollected);
        if (player.GetComponent<PlayerController>().oreCollected[oreNum] <= 0)
        {
            player.GetComponent<PlayerController>().RemoveGroup(oreNum);
        }
        depositedOreTotal += oreToSwap;
        inventoryOreTotal -= oreToSwap;
        RefreshOreTypes();
        inventoryOreTotalLabel.text = inventoryOreTotal.ToString() + "/" + player.GetComponent<PlayerController>().oreMax.ToString();
        depositedOreTotalLabel.text = depositedOreTotal.ToString() + "/" + maxOre.ToString();
        if (inventoryOreTotal < player.GetComponent<PlayerController>().oreMax) inventoryOreTotalLabel.color = Color.white;
    }
    public void WithdrawOre(string oreType)
    {
        //Debug.Log(oreType);
        int oreNum = 0;
        foreach (string oreName in Database.instance.oreNames)
        {
            if (oreType == oreName) break;
            oreNum++;
        }
        float oreCollected = 0;
        foreach (GameObject ore in oreCollectedGroups2)
        {
            if (ore.GetComponent<OreCollected>().oreNameLabel.text == oreType)
            {
                //Debug.Log("ye");
                oreCollected = float.Parse(ore.GetComponent<OreCollected>().oreCollectedLabel.text);
            }
        }
        float oreToSwap = Mathf.Round(oreCollected * swapAmount);
        if (swapAmount == 0.01f)
        {
            oreToSwap = 1;
        }
        if (inventoryOreTotal == player.GetComponent<PlayerController>().oreMax) return; //do NOT withdraw anything if inventory is full.
        if (inventoryOreTotal + oreToSwap >= player.GetComponent<PlayerController>().oreMax)
        {
            oreToSwap -= inventoryOreTotal + oreToSwap - player.GetComponent<PlayerController>().oreMax;
            inventoryOreTotalLabel.color = Color.red;
        }
        else
        {
            inventoryOreTotalLabel.color = Color.white;
        }
        //Debug.Log("Withdrawing " + oreToSwap + " " +oreType);
        int invIndex = 0;
        bool foundOre = false;
        foreach (GameObject ore in oreCollectedGroups2)
        {
            if (ore.GetComponent<OreCollected>().oreNameLabel.text == oreType)
            {
                //Debug.Log("Found ore!!!!! now swapping " + oreToSwap + " ore");
                //Debug.Log("Ore num: " + oreNum);
                ore.GetComponent<OreCollected>().oreCollectedLabel.text = (float.Parse(oreCollectedGroups2[invIndex].GetComponent<OreCollected>().oreCollectedLabel.text) - oreToSwap).ToString();
                foundOre = true;
                player.GetComponent<PlayerController>().oreCollected[oreNum] += oreToSwap;
                player.GetComponent<PlayerController>().RefreshOreTypes();
                if (float.Parse(ore.GetComponent<OreCollected>().oreCollectedLabel.text) <= 0)
                {
                    RemoveDepositedOre(ore);
                    depositedOreTypesCollected--;
                    UpdateDepositedOrePositions();
                    break;
                }
            }
            invIndex++;
        }
        if (!foundOre) Debug.Log("If you are seeing this, something went very wrong trying to find the ore!!");
        //Debug.Log(oreNum+", "+oreType);
        //Debug.Log(player.GetComponent<PlayerController>().oreCollected[oreNum]);
        //UpdateOres(oreType, oreCollected);
        depositedOreTotal -= oreToSwap;
        inventoryOreTotal += oreToSwap;
        RefreshOreTypes();
        inventoryOreTotalLabel.text = inventoryOreTotal.ToString() + "/" + player.GetComponent<PlayerController>().oreMax.ToString();
        depositedOreTotalLabel.text = depositedOreTotal.ToString() + "/"+maxOre.ToString();
        if (depositedOreTotal < player.GetComponent<PlayerController>().oreMax) depositedOreTotalLabel.color = Color.white;
    }
    public void SwitchSwapAmount(float swapNum)
    {
        foreach(var button in swapButtons)
        {
            button.GetComponent<Image>().color = Color.gray;
        }

        switch (swapNum)
        {
            case 1:
                swapButtons[0].GetComponent<Image>().color = Color.green;
                break;
            case 10:
                swapButtons[1].GetComponent<Image>().color = Color.green;
                break;
            case 25:
                swapButtons[2].GetComponent<Image>().color = Color.green;
                break;
            case 50:
                swapButtons[3].GetComponent<Image>().color = Color.green;
                break;
            case 100:
                swapButtons[4].GetComponent<Image>().color = Color.green;
                break;
        }

        swapAmount = swapNum / 100;
    }
    public void UpdateOres(string oreType, float oreCollected)
    {
        int oreNum = 0;
        while (oreNum < Database.instance.oreNames.Count)
        {
            if (oreType == Database.instance.GetOreName(oreNum)) break;
            oreNum++;
        }
        player.GetComponent<PlayerController>().oreCollected[oreNum] = oreCollected;
    }
    private void RemoveDepositedOre(GameObject ore)
    {
        oreCollectedGroups2.Remove(ore);
        Destroy(ore);
    }
    private void UpdateDepositedOrePositions() // i refuse to make this function name any shorter
    {
        depositedOreTypesCollected = 0;
        foreach (GameObject ore in oreCollectedGroups2)
        {
            ore.GetComponent<RectTransform>().localPosition = new Vector3(-151, 159 - 60 * depositedOreTypesCollected, 0);
            depositedOreTypesCollected++;
        }
    }
}
