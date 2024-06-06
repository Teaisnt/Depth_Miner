using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OreCollected : MonoBehaviour
{
    public TextMeshProUGUI oreNameLabel;
    public TextMeshProUGUI oreCollectedLabel;
    public Image oreIcon;
    public void SetUp(int oreType)
    {
        oreIcon.sprite = Database.instance.GetSprite(oreType+1); //this is because of the coin icon taking up the first slot in the Database info stuff
        oreNameLabel.text = Database.instance.GetOreName(oreType);
        GetComponent<Image>().color = Database.instance.GetOreColor(oreType);
        oreNameLabel.color = Database.instance.GetOreColor(oreType);
        if (GetComponentInChildren<SellOre>() != null)
        {
            GetComponentInChildren<SellOre>().oreName = oreNameLabel.text;
        }
    }
    public void Destroy()
    {
        Destroy(this.gameObject);
    }
}
