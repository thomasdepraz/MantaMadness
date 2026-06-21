using FMODUnity;
using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopStand : MonoBehaviour, IDataPersistence
{
    [SerializeField] private string standID;

    [Header("Item Sold")]
    public ShopItem item;

    [Header("UI")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI priceText;
    public Image CurrencyIcon;
    [SerializeField] private string dialogSequenceName;

    [Header("Visual")]
    public Transform itemVisualPoint;
    [SerializeField] private GameObject visual;
    public Transform playerPoint;
    public GameObject inRangeVisual;

    [Header("Audio")]
    public EventReference abilitySound;

    [Header("Animation")]
    [SerializeField]private Animator shopCurrencyAnimator;

    [Header("Collectible Rewards")]
    [SerializeField] private List<Collectible> collectibleRewards = new List<Collectible>();
    [SerializeField] private int currentCollectibleIndex = 0;

    private Collider shopCollider;

    private int renewalCount = 0;
    private bool disableShopStand = false;

    public bool IsActive => !disableShopStand && renewalCount < item.itemRenewalLimit;

    public SteamSignal signal;

    private void Awake()
    {
        shopCollider = GetComponent<Collider>();
    }

    public void Start()
    {
        if(item.currency == ShopCurrency.Clam)
        {
            shopCurrencyAnimator.SetInteger("currencyType", 1);
        }
        else
        {
            shopCurrencyAnimator.SetInteger("currencyType", 0);
        }

        if (GetComponent<SteamSignal>() != null)
        {
            signal = GetComponent<SteamSignal>();
        }

        ShopIndicatorToggle(false);

    }

    public void LoadData(GameData data)
    {
        if (data.shopStands.TryGetValue(standID, out ShopStandData standData))
        {
            renewalCount = standData.renewalCount;
            disableShopStand = standData.disabled;
        }
        else
        {
            renewalCount = 0;

            disableShopStand = false;
        }

        if (disableShopStand)
        {
            DisableShop();
        }
        else
        {
            ActivateShop();
        }

        StartCoroutine(DelayLoadData(data));
    }

    public IEnumerator DelayLoadData(GameData data)
    {
        yield return new WaitForSeconds(0.1f);
        if (data.shopStands.TryGetValue(standID, out ShopStandData standData))
        {
            if (item.type == ShopItemType.Sun)
            {
                Debug.Log("Sun count shop is " + renewalCount);
                if (renewalCount == item.itemRenewalLimit)
                {
                    CoinManager.Instance.ForceActivateCoinHolder(item.itemSold);
                }
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data.shopStands.ContainsKey(standID))
        {
            data.shopStands[standID].renewalCount = renewalCount;
            data.shopStands[standID].disabled = disableShopStand;
        }
        else
        {
            data.shopStands.Add(standID, new ShopStandData(renewalCount, disableShopStand));
        }

    }

    void UpdateUI()
    {
        if (itemNameText != null)
            itemNameText.text = item.itemName;

        if (priceText != null)
            priceText.text = item.price.ToString();
    }

    void ToggleUi(bool toggle)
    {
        if (toggle)
        {
            itemNameText.enabled = true;
            priceText.enabled = true;
            CurrencyIcon.enabled = true;
        }
        else
        {
            itemNameText.enabled = false;
            priceText.enabled = false;
            CurrencyIcon.enabled = false;
        }
    }

    public void TryBuy()
    {
        Debug.Log("TryBuy called");

        GameData data = DataPersistenceManager.Instance.gameData;

        bool useCollectibleList =
            collectibleRewards != null &&
            collectibleRewards.Count > 0;

        if (!useCollectibleList &&
            renewalCount >= item.itemRenewalLimit)
        {
            Debug.Log("Renewal limit reached");
            return;
        }

        if (item.currency == ShopCurrency.Clam)
        {
            if (CoinManager.Instance.ClamCollectibleCount >= item.price)
            {
                Debug.Log("Enough money");

                CoinManager.Instance.ClamCollectibleCount -= item.price;

                renewalCount++;
                if (item.type == ShopItemType.Ability)
                {
                    UnlockUpgrade(data, Game.Instance.player);
                }
                else if (item.type == ShopItemType.Item)
                {
                    UnlockItem(Game.Instance.player);
                }
                else if (item.type == ShopItemType.Sun)
                {
                    UnlockSun();
                }
                else if(item.type == ShopItemType.KeyItem)
                {
                    UnlockKeyItem(Game.Instance.player);
                }

                //if (renewalCount >= item.itemRenewalLimit)
                //{
                //    DisableShop();
                //}
                //update clam count
            }
            else
            {
                Debug.Log("Not enough coins");
                //Visual "HAHA ta pas d'argent nullos"
            }
        }

        else if (item.currency == ShopCurrency.Buckie)
        {
            if (CoinManager.Instance.BuckieCollectibleCount >= item.price)
            {
                Debug.Log("Enough money");

                CoinManager.Instance.BuckieCollectibleCount -= item.price;

                renewalCount++;

                if (item.type == ShopItemType.Ability)
                {
                    UnlockUpgrade(data, Game.Instance.player);
                }
                else if (item.type == ShopItemType.Item)
                {
                    UnlockItem(Game.Instance.player);
                }
                else if (item.type == ShopItemType.Sun)
                {
                    UnlockSun();
                }

                if (renewalCount >= item.itemRenewalLimit)
                {
                    DisableShop();
                }
                //update clam count
            }
            else
            {
                Debug.Log("Not enough coins");
                //Visual "HAHA ta pas d'argent nullos"
            }
        }

    }

    void UnlockUpgrade(GameData data, SimpleController player)
    {
        //RuntimeManager.PlayOneShot(pickupAltarSound, Game.Instance.player.transform.position);

        signal?.Trigger();

        string[] abilityTypeNames = Enum.GetNames(typeof(ControllerAbility));

        for (int i = 0; i < abilityTypeNames.Length; i++)
        {
            if (abilityTypeNames[i] == item.itemSold)
            {
                player.UnlockAbility(abilityTypeNames[i]);

                RuntimeManager.PlayOneShot(abilitySound, Game.Instance.player.transform.position);
                player.ForcePosition(playerPoint.position, playerPoint.rotation, resetVelocity: true, forcedState: ControllerState.SURFING);
                if (!string.IsNullOrEmpty(dialogSequenceName))
                    DialogManager.instance.StartSequence(dialogSequenceName);
            }
        }
    }

    void UnlockItem(SimpleController player) 
    {
        // Item classique
        if (item.itemToSpawn != null)
        {
            Instantiate(
                item.itemToSpawn,
                player.transform.position,
                Quaternion.identity
            );
        }

        // Collectible rewards
        if (collectibleRewards.Count > 0)
        {
            if (currentCollectibleIndex >= collectibleRewards.Count)
            {
                Debug.Log("No more collectible rewards");
                return;
            }

            Collectible collectible =
                collectibleRewards[currentCollectibleIndex];

            currentCollectibleIndex++;

            if (collectible != null)
            {
                collectible.ActivateCollectible();

                collectible.transform.position =
                    itemVisualPoint.position;

                collectible.MoveToTarget(player.gameObject);
            }

            // Disable stand if empty
            if (currentCollectibleIndex >= collectibleRewards.Count)
            {
                DisableShop();
            }
        }
    }

    void UnlockSun()
    {
        if (string.IsNullOrEmpty(item.itemSold))
        {
            Debug.LogWarning("Sun shop item has no CoinHolder ID");
            return;
        }
        CoinManager.Instance.ActivateCoinHolder(item.itemSold);
        DisableShop();
    }

    void UnlockKeyItem(SimpleController player)
    {
        // Item classique
        if (item.itemToSpawn != null)
        {
            Instantiate(
                item.itemToSpawn,
                player.transform.position,
                Quaternion.identity
            );
        }

        currentCollectibleIndex++;

        // Disable stand if empty
        if (currentCollectibleIndex >= collectibleRewards.Count)
        {
            DisableShop();
        }
    }

    private void ActivateShop()
    {
        //Activate UI
        if(visual == null)
        {
            visual = Instantiate(item.visual, itemVisualPoint.position, Quaternion.identity);
        }
        else
        {
            visual.SetActive(true);
        }
        ToggleUi(true);
        UpdateUI();
    }

    private void DisableShop()
    {
        disableShopStand = true;

        if (shopCollider != null)
            shopCollider.enabled = false;


        if (visual != null)
        visual.SetActive(false);

        ShopIndicatorToggle(false);
        ToggleUi(false);

    }

    public void ShopIndicatorToggle(bool toggle)
    {
        if (toggle)
        {
            inRangeVisual.SetActive(true);
        }
        else
        {
            inRangeVisual.SetActive(false);
        }
    }
}
