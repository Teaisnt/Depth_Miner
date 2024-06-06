using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SellOre : MonoBehaviour
{
    public string oreName;
    public TextMeshProUGUI sellAmountText;
    public void Sell()
    {
        //Replace is used because the code gets the name from the text in the Ore Collected group, that being for example "Stone:"
        GetComponentInParent<SellController>().SellOre(oreName.Replace(":", ""));
    }
}
