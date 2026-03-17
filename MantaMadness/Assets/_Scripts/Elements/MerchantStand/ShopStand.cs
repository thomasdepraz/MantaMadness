using FMODUnity;
using System;
using System.Collections;
using TMPro;
using UnityEditor.Animations;
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
    private GameObject visual;
    public Transform playerPoint;
    public GameObject inRangeVisual;

    [Header("Audio")]
    public EventReference abilitySound;

    [Header("Animation")]
    [SerializeField]private Animator shopCurrencyAnimator;

    private int renewalCount = 0;
    private bool disableShopStand = false;

    public bool IsActive => !disableShopStand && renewalCount < item.itemRenewalLimit;

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

        if (renewalCount >= item.itemRenewalLimit)
        {
            Debug.Log("Renewal limit reached");
            return;
        }

        if(item.currency == ShopCurrency.Clam)
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
        if(item.itemToSpawn != null)
        {
            Instantiate(item.itemToSpawn, player.transform.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("No Item sets! just wasted money");
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

        CameraTargetDetection.Instance.validShopTargets.Remove(GetComponent<Collider>());

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
