using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class WorldGenerator : MonoBehaviour
{
    [Header("Chance Stuff")]
    [SerializeField] private List<int> oreVeinChances;

    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    public int smoothness;

    int[,] map;
    int[,] borderedMap;
    public Tilemap tilemap;
    public Tilemap backgroundTilemap;
    public Tilemap propTilemap;
    public Tile stoneTile1;
    public Tile stoneTile2;
    public Tile stoneTile3;
    public List<Tile> oreTiles;
    public Tile backgroundTile1;
    public Tile backgroundTile2;
    public Tile backgroundTile3;
    public Tile stalagmiteTile;
    public GameObject player;

    [Range(0, 100)]
    public int randomFillPercent;

    System.Random pseudoRandom;

    public object psuedoRandom { get; private set; }


    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
        GenerateBackground();
    }
    private void GenerateBackground()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile temp = backgroundTile1;
                float randStone = UnityEngine.Random.Range(1, 4);
                if (randStone == 2) temp = backgroundTile2;
                else if (randStone == 3) temp = backgroundTile3;
                backgroundTilemap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), temp);
            }
        }
    }
    public void GenerateMap()
    {
        tilemap.ClearAllTiles();
        propTilemap.ClearAllTiles();
        map = new int[width, height];

        if (useRandomSeed)
        {
            pseudoRandom = new System.Random();
        }
        else
        {
            pseudoRandom = new System.Random(seed.GetHashCode());
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int level = GetLevel(y);
                randomFillPercent = GetFillPercent(level);
                if (pseudoRandom.Next(0, 100) < randomFillPercent)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = 0;
                }
            }
        }
        for (int i = 0; i < smoothness; i++)
        {
            SmoothMap();
        }

        GenerateOre();
        GenerateProps();

        // tile code
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1)
                {
                    Tile temp = stoneTile1;
                    float randStone = UnityEngine.Random.Range(1, 4);
                    if (randStone == 2) temp = stoneTile2;
                    else if (randStone == 3) temp = stoneTile3;
                    tilemap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), temp);
                }
                else if(map[x, y] > 1)
                {
                    //Debug.Log(map[x, y] - 2);
                    tilemap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), oreTiles[map[x,y]-2]);
                }
            }
        }

        int borderSize = 1;

        borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }
    }
    private void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int level = GetLevel(y);
                //smoothness = 1 - (level / 20);
                if (GetWallCount(x, y) > 4)
                {
                    map[x, y] = 1;
                }
                else if (GetWallCount(x, y) < 4)
                {
                    map[x, y] = 0;
                }
            }
        }
    }
    private int GetWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }
    private void GenerateProps()
    {
        for (int x = 1; x < width; x++)
        {
            for (int y = 1; y < height; y++)
            {
                if (map[x, y] == 0 && map[x,y-1] != 0 && map[x,y-1] != -1)
                {
                    if (1 == UnityEngine.Random.Range(1, 12))
                    {
                        map[x, y] = -1;
                        propTilemap.SetTile(new Vector3Int(-x + width / 2, -y + height / 2, 0), stalagmiteTile);
                    }
                }
            }
        }
    }
    private void GenerateOre()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1)
                {
                    int level = GetLevel(y);
                    int oreType = 0;
                    foreach(OreChance oreChanceData in Database.instance.oreChances)
                    {
                        if(1 == UnityEngine.Random.Range(1, oreChanceData.oreChances[level]))
                        {
                            break;
                        }
                        else oreType++;
                    }
                    if(oreType < Database.instance.oreChances.Count)
                    {
                        oreType += 2;
                        map[x, y] = oreType;
                        for (int z = 0; z < 4; z++)
                        {
                            int veinChance = UnityEngine.Random.Range(1, oreVeinChances[oreType-2]);
                            if (veinChance == 1)
                            {
                                if (x - 1 > 0 && z == 0) map[x - 1, y] = oreType;
                                if (x + 1 < width && z == 1) map[x + 1, y] = oreType;
                                if (y - 1 > 0 && z == 2) map[x, y - 1] = oreType;
                                if (y + 1 < height && z == 3) map[x, y + 1] = oreType;
                            }
                        }
                    }
                }
            }
        }
    }
    int GetFillPercent(int level)
    {
        switch (level)
        {
            case 0:
                return 50;
            case 1:
                return 45;
            case 2:
                return 53;
            case 3:
                return 50;
            case 4:
                return 47;
            case 5:
                return 50;
        }
        return 0;
    }
    int GetLevel(int y)
    {
        if (y < 100)
        {
            return 0;
        }
        else if(y < 250)
        {
            return 1;
        }
        else if (y < 450)
        {
            return 2;
        }
        else if (y < 700)
        {
            return 3;
        }
        else if (y < 1000)
        {
            return 4;
        }
        else if (y < 1500)
        {
            return 5;
        }
        return 0;
    }
    public void BuildBlock(int x, int y)
    {
        //Debug.Log("Building block at " + new Vector3Int(-x - 9 + width / 2, (-y) + height / 2));
        tilemap.SetTile(new Vector3Int(-x-9 + width / 2, (-y) + height / 2, 0), stoneTile2);
    }
    public void CheckForProp(Vector3Int position)
    {
        TileBase tileBelow = propTilemap.GetTile(new Vector3Int(position.x,position.y-1));
        if (tileBelow!=null)
        {
            //Debug.Log(tileBelow.name);
            if (tileBelow.name == "Stalagmite")
            {
                propTilemap.SetTile(new Vector3Int(position.x, position.y - 1), null);
            }
        }
    }
}
