using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickaxeController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private List<string> ownedPickaxes = new List<string>();
    [SerializeField] private List<GameObject> equipButtons = new List<GameObject>();

    [Header("Other")]
    [SerializeField] private List<float> pickaxeMultipliers;
    [SerializeField] private GameObject player;

    public string equippedPickaxe = "Wood";
    public void UpdateCosts()
    {
        foreach (GameObject equipButton in equipButtons)
        {
            foreach (string ownedPickaxe in ownedPickaxes)
            {
                if (equipButton.GetComponent<CostController>() == null) continue;
                if (equipButton.GetComponent<CostController>().costName == ownedPickaxe)
                {
                    if (equipButton.GetComponent<CostController>().costName == equippedPickaxe) equipButton.GetComponentInChildren<TextMeshProUGUI>().text = "Equipped";
                    else equipButton.GetComponentInChildren<TextMeshProUGUI>().text = "Equip";
                }
            }
            if (equipButton.GetComponentInChildren<TextMeshProUGUI>().text == "Purchase")
            {
                equipButton.GetComponent<CostController>().SetEquipCost(false);
            }
        }
    }
    public void EquipPickaxe(string pickaxeType)
    {
        var temp = equippedPickaxe;
        if (equippedPickaxe == pickaxeType) return;
        bool hasPickaxe = false;
        int pickaxeNum1 = 0;
        foreach(string pickaxe in ownedPickaxes)
        {
            if (pickaxe != pickaxeType) pickaxeNum1++;
            else break;
        }
        player.GetComponent<PlayerController>().pickaxeMultiplier = pickaxeMultipliers[pickaxeNum1];
        foreach (string pickaxe in ownedPickaxes)
        {
            if(pickaxe == pickaxeType)
            {
                hasPickaxe = true;
                Debug.Log("HAVE!");
            }
        }
        foreach (var button in equipButtons)
        {
            if (button.GetComponent<CostController>() == null && !hasPickaxe) continue;
            else if(button.GetComponent<CostController>() != null)
            {
                if (button.GetComponent<CostController>().costName != pickaxeType) continue;
                if (!hasPickaxe && button.GetComponent<CostController>().costName == pickaxeType && button.GetComponent<Button>().image.color == Color.green)
                {
                    PurchasePickaxe(pickaxeType, button);
                }
                else if (!hasPickaxe)
                {
                    continue;
                }
            }
            equippedPickaxe = pickaxeType;
            int pickaxeNum = 0;
            while (pickaxeNum < Database.instance.oreNames.Count)
            {
                if (pickaxeType == Database.instance.GetPickaxeName(pickaxeNum)) break;
                pickaxeNum++;
            }
            equipButtons[pickaxeNum].GetComponentInChildren<TextMeshProUGUI>().text = "Equipped";
            foreach(var button2 in equipButtons)
            {
                if(button2 != button && button2.GetComponentInChildren<TextMeshProUGUI>().text == "Equipped")
                {
                    button2.GetComponentInChildren<TextMeshProUGUI>().text = "Equip"; // this took ages cuz im dumb. checks for the previously equipped pickaxe and sets it back to equip
                }
            }
            player.GetComponent<PlayerController>().SetPickaxeSprite(pickaxeNum);
        }
    }

    public void PurchasePickaxe(string pickaxeType, GameObject button)
    {
        ownedPickaxes.Add(pickaxeType);
        Debug.Log("Purchasing "+pickaxeType + ", pressed " + button.name);
        equippedPickaxe = pickaxeType;
        foreach(var button2 in equipButtons)
        {
            if (button2.GetComponent<CostController>() != null)
            {
                if(button2.GetComponentInChildren<TextMeshProUGUI>().text == "Purchase")
                {
                    if(button2.GetComponent<CostController>().costName == pickaxeType) button2.GetComponent<CostController>().SetEquipCost(true);
                    else button2.GetComponent<CostController>().SetEquipCost(false);
                }
            }
        }
        button.GetComponent<Button>().image.color = Color.gray;
    }
}
