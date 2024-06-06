using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject optionsScreen;
    [SerializeField] private GameObject sellScreen;
    [SerializeField] private GameObject upgradeScreen;
    [SerializeField] private GameObject pickaxeScreen;
    [SerializeField] private GameObject inventoryScreen;
    private string previousScreen;
    private Coroutine disableCoroutine;
    private bool transitioning;
    private Animator animator;
    private void Awake()
    {
        if(this.gameObject.name == "Canvas")animator = GetComponent<Animator>();
    }
    public void PlayButton()
    {
        SceneManager.LoadScene("In-Game");
    }
    public void OptionsButton()
    {
        if (transitioning) return;
        animator.SetTrigger("OpenOptions");
    }
    public void CreditsButton()
    {
        if (transitioning) return;
        animator.SetTrigger("OpenCredits");
    }
    public void QuitButton()
    {
        Application.Quit();
    }
    public void OptionsBackButton()
    {
        if (transitioning) return;
        animator.SetTrigger("OpenTitleFromOptions");
    }
    public void CreditsBackButton()
    {
        if (transitioning) return;
        animator.SetTrigger("OpenTitleFromCredits");
    }
    public void PauseQuitButton()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1.0f;
    }
    public void OpenShop()
    {
        sellScreen.SetActive(true);
        upgradeScreen.SetActive(false);
        pickaxeScreen.SetActive(false);
        inventoryScreen.SetActive(false);
        SwitchTabs("Sell");
        sellScreen.GetComponent<SellController>().RefreshOreTypes();
    }
    public void SwitchTabs(string paramater)
    {
        if (disableCoroutine != null) StopCoroutine(disableCoroutine);
        if (paramater == previousScreen) return;
        switch (previousScreen)
        {
            case "Sell":
                {
                    disableCoroutine = StartCoroutine(DisableMenu(sellScreen, previousScreen));
                    sellScreen.GetComponent<Animator>().SetTrigger("Disappear");
                    break;
                }
            case "Upgrade":
                {
                    disableCoroutine=StartCoroutine(DisableMenu(upgradeScreen, previousScreen));
                    upgradeScreen.GetComponent<Animator>().SetTrigger("Disappear");
                    break;
                }
            case "Pickaxe":
                {
                    disableCoroutine=StartCoroutine(DisableMenu(pickaxeScreen, previousScreen));
                    pickaxeScreen.GetComponent<Animator>().SetTrigger("Disappear");
                    break;
                }
            case "Inventory":
                {
                    disableCoroutine=StartCoroutine(DisableMenu(inventoryScreen, previousScreen));
                    inventoryScreen.GetComponent<Animator>().SetTrigger("Disappear");
                    break;
                }
        }
        switch (paramater)
        {
            case "Sell":
                {
                    EnableMenu(sellScreen);
                    sellScreen.GetComponent<Animator>().SetTrigger("Appear");
                    sellScreen.GetComponent<SellController>().RefreshOreTypes();
                    break;
                }
            case "Upgrade":
                {
                    EnableMenu(upgradeScreen);
                    upgradeScreen.GetComponent<Animator>().SetTrigger("Appear");
                    upgradeScreen.GetComponent<UpgradeController>().UpdateCosts();
                    break;
                }
            case "Pickaxe":
                {
                    EnableMenu(pickaxeScreen);
                    pickaxeScreen.GetComponent<Animator>().SetTrigger("Appear");
                    pickaxeScreen.GetComponent<PickaxeController>().UpdateCosts();
                    break;
                }
            case "Inventory":
                {
                    EnableMenu(inventoryScreen);
                    inventoryScreen.GetComponent<Animator>().SetTrigger("Appear");
                    inventoryScreen.GetComponent<InventoryController>().RefreshOreTypes();
                    break;
                }
        }
        previousScreen = paramater;
    }
    IEnumerator DisableMenu(GameObject menu, string previousScreen)
    {
        yield return new WaitForSeconds(0.5f);
        menu.SetActive(false);
    }
    void EnableMenu(GameObject menu)
    {
        menu.SetActive(true);
    }
    public void SetTransitioning(string isTransitioning)
    {
        //TODO: ask unity why the fuck animation events dont allow bool as input
        if (isTransitioning == "true") transitioning = true;
        else transitioning = false;
    }
}
