using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Database : MonoBehaviour
{
    public static Database instance;

    public List<Sprite> sprites;
    public List<string> oreNames;
    public List<string> pickaxeNames;
    public List<Color> oreColors;
    public List<int> oreValues;
    public List<OreChance> oreChances;
    public List<float> oreMiningSpeeds;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
        else
        {
            instance = this;
        }
    }
    public Sprite GetSprite(int imageNum)
    {
        return sprites[imageNum];
    }
    public string GetOreName(int oreNum)
    {
        return oreNames[oreNum];
    }
    public string GetPickaxeName(int oreNum)
    {
        return pickaxeNames[oreNum];
    }
    public Color GetOreColor(int oreNum)
    {
        return oreColors[oreNum];
    }
    public int GetOreValue(int oreNum)
    {
        return oreValues[oreNum];
    }
    public float GetMiningSpeed(int oreNum)
    {
        return oreMiningSpeeds[oreNum];
    }
}
