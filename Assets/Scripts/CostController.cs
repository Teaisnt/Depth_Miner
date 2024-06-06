using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class CostController : MonoBehaviour
{
    [Header("Cost values")]
    public string costName;
    [SerializeField] private List<Cost> costs = new List<Cost>();

    [Header("Cost visuals")]
    [SerializeField] private Sprite blankSprite;
    [SerializeField] private List<Image> costImages;
    [SerializeField] private List<TextMeshProUGUI> costTexts;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Other")]
    public GameObject player;
    public bool upgradable = true;
    public Database database;

    public void SetCost(int level, bool purchasing)
    {
        if(purchasing)BuyUpgrade(level);
        if (level >= 5) return;
        level--; // to help work with lists n shit
        int currCost = 0;
        upgradable = true;
        while(currCost < costs[level].costAmounts.Count)
        {
            //Debug.Log(currCost);
            bool canAfford = false;
            string oreName = "Coins: ";
            if (currCost != 0) oreName = Database.instance.GetOreName(costs[0].costTypes[currCost-1]) + ": ";
            float oreAmount = 0;
            if (currCost != 0)
            {
                if (player.GetComponent<PlayerController>().HasOre(currCost))
                {
                    //Debug.Log("YES!");
                    oreAmount = player.GetComponent<PlayerController>().oreCollected[costs[0].costTypes[currCost-1]];
                }
            }
            //Debug.Log("NO!");
            if (currCost == 0 && player.GetComponent<PlayerController>().coinsCollected >= costs[level].costAmounts[0])
            {
                canAfford = true;
            }
            else if (currCost != 0 && costs[level].costAmounts[currCost] <= oreAmount)
            {
                canAfford = true;
            }
            if (!canAfford) upgradable = false;
            SetText(costs[level].costAmounts[currCost], oreName, currCost, canAfford);
            SetImage(currCost,costs[level].costTypes[currCost], costs[level].costAmounts[currCost]);
            currCost++;
        }
        while(currCost < 5)
        {
            SetText(0, "",currCost, false);
            SetImage(currCost, 0, 0);
            currCost++;
        }
        if (upgradable) gameObject.GetComponent<Button>().image.color = Color.green;
        else gameObject.GetComponent<Button>().image.color = Color.gray;
    }
    public void SetImage(int currImage, int imageNum, int cost)
    {
        //Debug.Log("Setting sprite #" + imageNum + " to " + currImage);
        if (cost == 0)
        {
            costImages[currImage].sprite = blankSprite;
        }
        else costImages[imageNum].sprite = database.GetSprite(currImage);
    }
    public void SetText(int cost, string text, int currText, bool canAfford)
    {
        if (cost == 0)
        {
            costTexts[currText].text = "";
        }
        else
        {
            costTexts[currText].text = text + cost.ToString();
            if (canAfford) costTexts[currText].color = Color.green;
            else costTexts[currText].color = Color.red;
        }
    }
    public void BuyUpgrade(int level)
    {
        level-=2;
        int currCost = 0;
        while (currCost < costs[level].costAmounts.Count)
        {
            if (currCost == 0) player.GetComponent<PlayerController>().coinsCollected -= costs[level].costAmounts[0];
            else player.GetComponent<PlayerController>().oreCollected[costs[level].costTypes[currCost-1]] -= costs[level].costAmounts[currCost];
            currCost++;
        }
        coinsText.text = "Coins: " + player.GetComponent<PlayerController>().coinsCollected.ToString();
    }
    public void SetMaxed()
    {
        foreach(var text in costTexts)
        {
            text.text = "";
        }
        foreach(var image in costImages)
        {
            image.sprite = blankSprite;
        }
        gameObject.GetComponent<Button>().image.color = Color.gray;
    }
    public void SetEquipCost(bool purchasing)
    {
        if (purchasing) BuyPickaxe();
        int currCost = 0;
        upgradable = true;
        while (currCost < costs[0].costAmounts.Count)
        {
            bool canAfford = false;
            string oreName = "Coins: ";
            float oreAmount = 0;
            if (currCost != 0)
            {
                oreName = Database.instance.GetOreName(costs[0].costTypes[currCost-1]) + ": ";
                if (player.GetComponent<PlayerController>().HasOre(currCost-1))
                {
                    oreAmount = player.GetComponent<PlayerController>().oreCollected[costs[0].costTypes[currCost-1]];
                    //Debug.Log("Player has " + oreAmount + " of ore " + (costs[0].costTypes[currCost]-1));
                }
            }
            if (currCost == 0 && player.GetComponent<PlayerController>().coinsCollected >= costs[0].costAmounts[0])
            {
                canAfford = true;
            }
            else if (currCost != 0 && costs[0].costAmounts[currCost] <= oreAmount)
            {
                canAfford = true;
            }
            if (!canAfford) upgradable = false; //if any of the costs do NOT match the players ore count, can NOT upgrade
            if (purchasing)
            {
                SetText(0, "", currCost, false);
                SetImage(currCost, 0, 0);
            }
            else
            {
                SetText(costs[0].costAmounts[currCost], oreName, currCost, canAfford);
                SetImage(costs[0].costTypes[currCost], currCost, costs[0].costAmounts[currCost]);
            }
            currCost++;
        }
        if (upgradable) gameObject.GetComponent<Button>().image.color = Color.green;
        else gameObject.GetComponent<Button>().image.color = Color.gray;
    }
    void BuyPickaxe()
    {
        int currCost = 0;
        while (currCost < costs[0].costAmounts.Count)
        {
            if (currCost == 0) player.GetComponent<PlayerController>().coinsCollected -= costs[0].costAmounts[0];
            else player.GetComponent<PlayerController>().oreCollected[costs[0].costTypes[currCost]-1] -= costs[0].costAmounts[currCost];
            currCost++;
        }
        coinsText.text = "Coins: " + player.GetComponent<PlayerController>().coinsCollected.ToString();
    }
}
