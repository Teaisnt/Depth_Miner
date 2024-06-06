using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Runtime.CompilerServices;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.VFX;
using System.Reflection;

public class PlayerController : MonoBehaviour
{ 
    [Header("Audio")]
    public AudioSource audioSource;
    [SerializeField] private List<AudioClip> blockBreakClips = new List<AudioClip>();
    public AudioClip blockPlaceClip;
    public AudioClip boomClip;

    [Header("Variables")]
    public float miningSpeed;
    public float baseMiningSpeed;
    public float pickaxeMultiplier;
    public float reachLevel;
    public float sellLevel;
    public float mineStrengthLevel;
    public float mineLevel = 1;

    [Header("Visual Effects")]
    [SerializeField] private List<VisualEffect> oreVFX;
    [SerializeField] private VisualEffect burstVFX;
    public Volume volume;
    public ColorAdjustments colorAdjustments;

    [Header("UI Stuff")]
    public List<GameObject> oreCollectedGroups;
    public GameObject oreCollectedPrefab;
    public GameObject oreCollectedParent;
    [SerializeField] private TextMeshProUGUI depthText;
    [SerializeField] private TextMeshProUGUI totalOreCollectedText;
    [SerializeField] private GameObject oreCollectedBackground;
    [SerializeField] private GameObject inGameOverlay;
    [SerializeField] private GameObject hubOverlay;
    [SerializeField] private GameObject darkFadeOverlay;
    [SerializeField] private TextMeshProUGUI warningLabel;

    [Header("Other")]
    public GameObject miningBlockOverlay;
    public GameObject currMiningBlockOverlay;
    public WorldGenerator worldGenerator;
    public Tilemap tileMap;
    public List<float> oreCollected;
    public float coinsCollected;
    public float oreMax;
    public SpriteRenderer pickaxeSprite;
    [SerializeField] private List<Sprite> pickaxeSprites = new List<Sprite>();
    [SerializeField] private MenuController menuController;
    public UnityEngine.Transform light2D;
    private FieldInfo _LightCookieSprite = typeof(Light2D).GetField("m_LightCookieSprite", BindingFlags.NonPublic | BindingFlags.Instance);
    public List<Sprite> lightSprites;

    private float leftDistance;
    private float rightDistance;
    private Coroutine blockBreakCoroutine;
    private Animator animator;
    private TileBase tileBelow;
    private TileBase tileRight;
    private TileBase tileLeft;
    private Coroutine miningCoroutine;
    private bool isMining;
    private bool miningSoundPlaying;
    private RaycastHit2D hitBelow;
    private RaycastHit2D hitLeft;
    private RaycastHit2D hitRight;
    private LayerMask groundLayer;
    private float miningDepth;
    private float oreTypesCollected;
    private bool isGrounded;
    private bool isBuilding;
    private bool inGame = true;
    private Rigidbody2D rb2d;
    public enum StateType
    {
        Idle,
        Mining,
        Falling,
        Building,
        Teleporting
    }
    public StateType stateType;

    private static readonly int IdleAnim = Animator.StringToHash("Idle");
    private static readonly int MiningAnim = Animator.StringToHash("Mining");
    private static readonly int FallAnim = Animator.StringToHash("Fall");
    private static readonly int BuildAnim = Animator.StringToHash("Build");
    private static readonly int TeleportAnim = Animator.StringToHash("Teleporting");

    private void Awake()
    {
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        currMiningBlockOverlay = Instantiate(miningBlockOverlay);
        currMiningBlockOverlay.SetActive(false);
        volume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
    }
    private void Start()
    {
        RefreshOreTypes();
    }
    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale != 1) return;
        if (Input.GetKeyDown(KeyCode.Space) && stateType != StateType.Teleporting)
        {
            stateType = StateType.Teleporting;
            if (inGame)
            {
                TeleportAnimation();
                Invoke("OpenShop", 2f);
            }
            else
            {
                rb2d.constraints = RigidbodyConstraints2D.None;
                darkFadeOverlay.SetActive(true);
                darkFadeOverlay.GetComponent<Animator>().SetTrigger("FadeOut");
                Invoke("OpenShop", 0.7f);
            }
        }
        if (!inGame) return;
        RaycastHit2D temp = Physics2D.Raycast(transform.position - new Vector3(0, 0.6f, 0), -Vector2.up, groundLayer);
        if (temp.collider != null) isGrounded = true;
        else isGrounded = false;
        CalculateDepth();
        if (stateType == StateType.Teleporting) return;
        if (isBuilding)
        {
            float step = 10 * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(0, transform.position.y + 1), step);
        }
        if(stateType != StateType.Mining && isMining)
        {
            //Debug.Log("NOOO!!!!");
            isMining = false;
            StopCoroutine(miningCoroutine);
            if(miningSoundPlaying)StopCoroutine(blockBreakCoroutine);
            miningSoundPlaying = false;
        }
        if (!isGrounded && stateType != StateType.Falling && stateType != StateType.Building)
        {
            //Debug.Log("Falling!");
            animator.CrossFade(FallAnim, 0);
            stateType = StateType.Falling;
            isMining = false;
        }
        else if(isGrounded && stateType != StateType.Idle && stateType != StateType.Mining && stateType != StateType.Building)
        {
            //Debug.Log("Idle!");
            animator.CrossFade(IdleAnim, 0);
            stateType = StateType.Idle;
            isMining = false;
        }
        DetectBlocks();
        if (stateType == StateType.Falling || !isGrounded) return;
        if((mineLevel-1>mineStrengthLevel || GetTotalOre() >= oreMax) && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)))
        {
            if (warningLabel.color.a != 0) return;
            if (mineLevel-1 > mineStrengthLevel) WarningMessage("Your mining strength is too low!\n(Press 'Space' to open the shop).");
            else WarningMessage("Your inventory is full!\n(Press 'Space' to open the shop).");
            return;
        }
        if (Input.GetKey(KeyCode.W) && !isMining && stateType != StateType.Building && oreCollected[0] > 0 && miningDepth != 0)
        {
            gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            StartCoroutine(Ext.WaitAndDo(() => {
                isBuilding = true;
            }, 0.11f, () => {
                isBuilding = false;
            }));
            animator.CrossFade(BuildAnim, 0f);
            stateType = StateType.Building;
        }
        else if (Input.GetKey(KeyCode.A) && tileLeft != null && stateType == StateType.Idle)
        {
            CalculateMiningSpeed(baseMiningSpeed, tileLeft);
            transform.localScale = new Vector3(-1, 1, 1);
            miningCoroutine = StartCoroutine(MineBlock(1, miningSpeed));
        }
        else if (Input.GetKey(KeyCode.S) && tileBelow != null && stateType == StateType.Idle)
        {
            CalculateMiningSpeed(baseMiningSpeed, tileBelow);
            miningCoroutine = StartCoroutine(MineBlock(2, miningSpeed));
        }
        else if (Input.GetKey(KeyCode.D) && tileRight != null && stateType == StateType.Idle)
        {
            CalculateMiningSpeed(baseMiningSpeed, tileRight);
            transform.localScale = new Vector3(1, 1, 1);
            miningCoroutine = StartCoroutine(MineBlock(3, miningSpeed));
        }
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D))
        {
            Destroy(currMiningBlockOverlay);
            if (isGrounded)
            {
                if(miningCoroutine != null)StopCoroutine(miningCoroutine);
                if (miningSoundPlaying) StopCoroutine(blockBreakCoroutine);
                miningSoundPlaying = false;
                animator.CrossFade(IdleAnim, 0);
                stateType = StateType.Idle;
                isMining = false;
            }
            else if(stateType != StateType.Idle && stateType != StateType.Mining)
            {
                stateType = StateType.Idle;
                animator.CrossFade(IdleAnim, 0);
            }
        }
    }
    private void DetectBlocks()
    {
        float temp = 0;
        while (temp < reachLevel)
        {
            temp++;
            leftDistance = temp;
            hitLeft = Physics2D.Raycast(transform.position - new Vector3(temp, 0, 0), -Vector2.left, groundLayer);
            if (hitLeft.collider == null) continue;
            else break;
        }
        temp = 0;
        while (temp < reachLevel)
        {
            temp++;
            rightDistance = temp;
            hitRight = Physics2D.Raycast(transform.position - new Vector3(-temp, 0, 0), -Vector2.right, groundLayer);
            if (hitRight.collider == null) continue;
            else break;
        }
        hitBelow = Physics2D.Raycast(transform.position - new Vector3(0, 0.6f, 0), -Vector2.up, groundLayer);
        if (hitBelow.collider == null || hitBelow.transform.gameObject.name == "Player") return;
        TileBase belowTile = tileMap.GetTile(hitBelow.transform.gameObject.GetComponent<Tilemap>().WorldToCell(hitBelow.point));
        TileBase leftTile = tileMap.GetTile(hitBelow.transform.gameObject.GetComponent<Tilemap>().WorldToCell(hitLeft.point));
        TileBase rightTile = tileMap.GetTile(hitBelow.transform.gameObject.GetComponent<Tilemap>().WorldToCell(hitRight.point));

        tileBelow = belowTile;
        tileLeft = leftTile;
        tileRight = rightTile;
    }
    public IEnumerator MineBlock(int direction, float duration)
    {
        //Debug.Log("Mining at " + miningDepth + " depth!");
        stateType = StateType.Mining;
        isMining = true;    
        if (currMiningBlockOverlay != null)currMiningBlockOverlay.SetActive(false);
        currMiningBlockOverlay = Instantiate(miningBlockOverlay);
        switch (direction)
        {
            case 1:
                ProgressBar(tileLeft);
                currMiningBlockOverlay.transform.position = new Vector2(-1 * leftDistance, gameObject.transform.position.y - 0.055f);
                break;
            case 2:
                ProgressBar(tileBelow);
                currMiningBlockOverlay.transform.position = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y - 1.055f);
                break;
            case 3:
                ProgressBar(tileRight);
                currMiningBlockOverlay.transform.position = new Vector2(1 * rightDistance, gameObject.transform.position.y - 0.055f);
                break;
        }
        currMiningBlockOverlay.GetComponent<Animator>().speed = 1 / duration;
        animator.CrossFade(MiningAnim, 0); 
        yield return new WaitForSeconds(duration);
        isMining = false;
        switch (direction)
        {
            case 1:
                CollectBlock(tileLeft, currMiningBlockOverlay);
                tileMap.SetTile(hitLeft.transform.gameObject.GetComponent<Tilemap>().WorldToCell(hitLeft.point), null);
                worldGenerator.CheckForProp(hitLeft.transform.gameObject.GetComponent<Tilemap>().WorldToCell(hitLeft.point));
                break;
            case 2:
                CollectBlock(tileBelow, currMiningBlockOverlay);
                tileMap.SetTile(hitBelow.transform.gameObject.GetComponent<Tilemap>().WorldToCell(hitBelow.point), null);
                worldGenerator.CheckForProp(hitBelow.transform.gameObject.GetComponent<Tilemap>().WorldToCell(hitBelow.point));
                break;
            case 3:
                CollectBlock(tileRight, currMiningBlockOverlay);
                tileMap.SetTile(hitRight.transform.gameObject.GetComponent<Tilemap>().WorldToCell(hitRight.point), null);
                worldGenerator.CheckForProp(hitRight.transform.gameObject.GetComponent<Tilemap>().WorldToCell(hitRight.point));
                break;
        }
        //Debug.Log("BREAK BLOCK!");
        Destroy(currMiningBlockOverlay);
        animator.CrossFade(IdleAnim, 0);
        stateType = StateType.Idle;
    }
    private void CalculateMiningSpeed(float newMiningSpeed, TileBase tile)
    {
        int blockNum = 0;
        while (blockNum < Database.instance.oreNames.Count)
        {
            if (tile.name == Database.instance.GetOreName(blockNum)) break;
            blockNum++;
        }
        miningSpeed = newMiningSpeed * (Database.instance.GetMiningSpeed(blockNum)/pickaxeMultiplier);
    }
    private void CollectBlock(TileBase tile, GameObject position)
    {
        if(tile == null)
        {
            return;
        }
        VisualEffect temp = Instantiate(oreVFX[0]);
        temp.gameObject.transform.position = position.transform.position;
        int blockNum = 0;
        int groupNum = 0;
        while (blockNum < Database.instance.oreNames.Count)
        {
            if (tile.name == Database.instance.GetOreName(blockNum)) break;
            blockNum++;
        }
        // doing this because the player may not obtain each ore in the same order as they are created (for example, coal may be ore #2 but if the user obtains iron 2nd it will get confused
        foreach (GameObject group in oreCollectedGroups)
        {
            if (tile.name == group.GetComponent<OreCollected>().oreNameLabel.text) break; 
            groupNum++;
        }
        if (blockNum > 0)
        {
            temp = Instantiate(oreVFX[blockNum]);
            temp.gameObject.transform.position = position.transform.position;
        }
        if (oreCollected[blockNum] == 0) AddNewOreType(blockNum);
        oreCollected[blockNum] += 1;
        oreCollectedGroups[groupNum].GetComponent<OreCollected>().oreCollectedLabel.text = oreCollected[blockNum].ToString();
        temp.gameObject.transform.position = position.transform.position;

        GetTotalOre();
        oreCollectedBackground.GetComponent<RectTransform>().localPosition = new Vector3(960, -540 + 60 * oreTypesCollected, 0);
    }
    private void AddNewOreType(int oreType)
    {
        GameObject orePrefab = Instantiate(oreCollectedPrefab, oreCollectedParent.transform);
        orePrefab.GetComponent<OreCollected>().SetUp(oreType);
        oreCollectedGroups.Add(orePrefab);
        orePrefab.GetComponent<OreCollected>().oreCollectedLabel.text = oreCollected[oreType].ToString();
        oreTypesCollected += 1;
        orePrefab.GetComponent<RectTransform>().localPosition = new Vector3(781, -566 + 58 * oreTypesCollected, 0);
        oreCollectedBackground.GetComponent<RectTransform>().localPosition = new Vector3(960, -540 + 60 * oreTypesCollected, 0);
    }
    private float GetTotalOre()
    {
        float oreCount = 0;
        for(int i = 0; i < oreCollected.Count; i++)
        {
            oreCount += oreCollected[i];
        }
        totalOreCollectedText.text = oreCount.ToString() + "/" + oreMax;
        if (oreCount >= oreMax) totalOreCollectedText.color = Color.red;
        else totalOreCollectedText.color = Color.white;
        return oreCount;
    }
    public void RefreshOreTypes()
    {
        oreTypesCollected = 0;
        foreach(GameObject group in oreCollectedGroups)
        {
            group.GetComponent<OreCollected>().Destroy();
        }
        oreCollectedGroups.Clear();
        int oreNum = 0;
        foreach(float oreAmount in oreCollected)
        {
            if (oreAmount != 0)
            {
                AddNewOreType(oreNum);
            }
            oreNum++;
        }
        GetTotalOre();
        oreCollectedBackground.GetComponent<RectTransform>().localPosition = new Vector3(960, -540 + 60 * oreTypesCollected, 0);
    }
    private void ProgressBar(TileBase tile)
    {
        int blockNum = 0;
        while (blockNum < Database.instance.oreNames.Count)
        {
            if (tile.name == Database.instance.GetOreName(blockNum)) break;
            blockNum++;
        }
        currMiningBlockOverlay.GetComponent<MineBlockController>().ChangeColor(Database.instance.GetOreColor(blockNum));
    }
    private void CalculateDepth()
    {
        int temp = ((int)mineLevel);
        float postExposure = 0;
        float contrast = 0;
        Color colorFilter = Color.white;
        float saturation = 0;
        depthText.text = "Depth: " + (miningDepth = Mathf.Abs(Mathf.Round(transform.position.y))).ToString();
        if(miningDepth < 100 && mineLevel != 1)
        {
            mineLevel = 1;
            GameController.instance.ChangeSong(1);
        }
        else if(miningDepth > 100 && miningDepth < 250 && mineLevel != 2)
        {
            mineLevel = 2;
            contrast = 10;
            colorFilter = new Color(0.86f, 0.86f, 0.86f, 1);
            GameController.instance.ChangeSong(2);
        }
        else if(miningDepth > 250 && miningDepth < 450 && mineLevel != 3)
        {
            mineLevel = 3;
            postExposure = 0.5f;
            contrast = 15;
            colorFilter = new Color(0.55f, 0.65f, 1f, 1);
            saturation = 70;
            GameController.instance.ChangeSong(3);
        }
        else if (miningDepth > 450 && miningDepth < 700 && mineLevel != 4)
        {
            mineLevel = 4;
            postExposure = -0.5f;
            contrast = 40;
            colorFilter = new Color(0.87f, 0.93f, 0.87f, 1);

            GameController.instance.ChangeSong(4);
        }
        else if (miningDepth > 700 && miningDepth < 1000 && mineLevel != 5)
        {
            mineLevel = 5;
            GameController.instance.ChangeSong(5);
            contrast = 80;
            colorFilter = new Color(1, 0.26f, 0.26f, 1);
        }
        else if (miningDepth > 1000 && miningDepth < 1500 && mineLevel != 6)
        {
            mineLevel = 6;
            contrast = 80;
            colorFilter = new Color(0.51f, 0.35f, 1, 1);
            saturation = -35;
            GameController.instance.ChangeSong(6);
        }
        else { return; }
        if(temp < mineLevel) PlaySound(boomClip);
        colorAdjustments.postExposure.value = postExposure;
        colorAdjustments.contrast.value = contrast;
        colorAdjustments.colorFilter.value = colorFilter;
        colorAdjustments.saturation.value = saturation;

        baseMiningSpeed = 0.8f + 0.3f * mineLevel - 0.1f*mineStrengthLevel;
    }
    private void PlaySound(AudioClip sound)
    {
        audioSource.PlayOneShot(sound);
    }
    private void PlayBlockBreakSound()
    {
        int blockNum = 0;
        audioSource.pitch = UnityEngine.Random.Range(0.85f, 1.15f);
        TileBase tile;
        if (Input.GetKey(KeyCode.A)) tile = tileLeft;
        else if (Input.GetKey(KeyCode.S)) tile = tileBelow;
        else tile = tileRight;
        while(blockNum< Database.instance.oreNames.Count)
        {
            if (tile.name == Database.instance.GetOreName(blockNum)) break;
            blockNum++;
        }
        if(blockNum != 0)audioSource.PlayOneShot(blockBreakClips[blockNum]);
        audioSource.PlayOneShot(blockBreakClips[0]);
    }
    public IEnumerator BlockBreak(float duration)
    {
        miningSoundPlaying = true;
        yield return new WaitForSeconds(duration);
        miningSoundPlaying = false;
        blockBreakCoroutine = StartCoroutine(BlockBreak(duration));
    }
    public void BuildBlock()
    {
        oreCollected[0]--;
        RefreshOreTypes();
        int y = (int)Mathf.Round(transform.position.y);
        worldGenerator.BuildBlock(0, -y);
        PlaySound(blockPlaceClip);
    }
    public void SetIdle()
    {
        stateType = StateType.Idle;
    }
    public void SetPickaxeSprite(int pickaxeNum)
    {
        pickaxeSprite.sprite = pickaxeSprites[pickaxeNum];
    }
    void TeleportAnimation()
    {
        rb2d.constraints = RigidbodyConstraints2D.FreezePositionY;
        darkFadeOverlay.SetActive(true);
        Invoke("DarkFadeOut", 1.5f);
        animator.CrossFade(TeleportAnim, 0);
        /*float currTimePassed = 0;
        while(currTimePassed < 100f)
        {
            transform.position = new Vector3(0, transform.position.y + 0.00001f, 0);
            currTimePassed += Time.deltaTime/1000;
        }*/
    }
    public void BurstVFX()
    {
        Instantiate(burstVFX.gameObject, transform);
    }
    void DarkFadeIn()
    {
        darkFadeOverlay.GetComponent<Animator>().SetTrigger("FadeIn");
        Invoke("ToggleDarkFadeEnabled", 0.5f);
    }
    void DarkFadeOut()
    {
        darkFadeOverlay.GetComponent<Animator>().SetTrigger("FadeOut");
    }
    void ToggleDarkFadeEnabled()
    {
        darkFadeOverlay.SetActive(false);
    }
    void OpenShop()
    {
        hubOverlay.SetActive(!hubOverlay.activeSelf);
        inGameOverlay.SetActive(!inGameOverlay.activeSelf);
        stateType = StateType.Idle;
        animator.CrossFade(IdleAnim,0);
        if (!inGame)
        {
            GameController.instance.ChangeSong(1);
            gameObject.transform.position = new Vector3(0, 0, 0);
            worldGenerator.GenerateMap();
            inGame = true;
            RefreshOreTypes();
            darkFadeOverlay.GetComponent<Animator>().SetTrigger("FadeIn");
        }
        else
        {
            GameController.instance.ChangeSong(0);
            menuController.OpenShop();
            inGame = false;
            Invoke("DarkFadeIn", 0.5f);
        }
    }
    void WarningMessage(string warnText)
    {
        warningLabel.text = warnText;
        warningLabel.GetComponent<Animator>().SetTrigger("Warn");
    }
    public void RemoveGroup(int oreNum)
    {
        int blockNum = 0;
        GameObject groupToRemove = oreCollectedGroups[0];
        foreach(GameObject group in oreCollectedGroups)
        {
            blockNum = 0;
            while (blockNum < Database.instance.oreNames.Count)
            {
                if (group.GetComponent<OreCollected>().oreNameLabel.text.Replace(":", "") == Database.instance.GetOreName(oreNum))
                {
                    //Debug.Log("Removing " + group.GetComponent<OreCollected>().oreNameLabel.text +"group.");
                    groupToRemove = group;
                    break;
                }
                blockNum++;
            }
        }
        oreCollectedGroups.Remove(groupToRemove);
        Destroy(groupToRemove);
    }
    public bool HasOre(int oreNum)
    {
        //Debug.Log(oreNum);
        foreach(GameObject group in oreCollectedGroups)
        {
            if(Database.instance.GetOreName(oreNum) == group.GetComponent<OreCollected>().oreNameLabel.text) //check if the group has the same ore as the one we are checking
            {
                if (int.Parse(group.GetComponent<OreCollected>().oreCollectedLabel.text) != 0) // if so, check if the ore amount is greater than 0, if so we have it.
                {
                    return true;
                }
            }
        }
        return false;
    }
    public void UpdateCookieSprite(Sprite sprite)
    {
        _LightCookieSprite.SetValue(light2D.GetComponent<Light2D>(), sprite);
    }
}
