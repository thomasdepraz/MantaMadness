using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInventoryManager : MonoBehaviour, IDataPersistence
{
    public static UIInventoryManager Instance;

    private SimpleController player;
    public List<UIInventoryItems> inventoryItems;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void LoadData(GameData data)
    {
        StartCoroutine(LoadDelay(data));
    }

    private IEnumerator LoadDelay(GameData data)
    {
        yield return new WaitForSeconds(0.1f);
        player = Game.Instance.player;
        UpdateInventory();
    }

    public void SaveData(ref GameData data)
    {
        //No save
    }

    private bool HasKey(UIInventoryItems item)
    {

        bool hasKey = false;
        DataPersistenceManager.Instance.gameData.specialPickups.TryGetValue(item.id, out hasKey);

        return hasKey;
    }

    private bool HasAbility(UIInventoryItems item)
    {
        if (player == null)
            return false;

        switch (item.id)
        {
            case nameof(ControllerAbility.DOUBLEJUMP):
                return player.doubleJumpAbility;

            case nameof(ControllerAbility.CHARGEBOOST):
                return player.chargeBoostAbility;

            case nameof(ControllerAbility.STOMP):
                return player.stompAbility;

            case nameof(ControllerAbility.LAVARESIST):
                return player.lavaResistanceAbility;

            case nameof(ControllerAbility.ALIEN):
                return player.alienAntennasAbility;

            case nameof(ControllerAbility.GRIND):
                return player.grindAbility;

            case nameof(ControllerAbility.CAT):
                return player.catAbility;

            case nameof(ControllerAbility.DYNAMO):
                return player.dynamoAbility;

            default:
                Debug.LogWarning($"Ability inconnue : {item.id}");
                return false;
        }
    }

    public void UpdateInventory()
    {
        foreach(UIInventoryItems item in inventoryItems)
        {
            if(item.type == UiInventoryType.Key)
            {
                UpdateKeyItem(item);
            }
            else if(item.type == UiInventoryType.Ability)
            {
                UpdateAbilityItem(item);
            }
        }
    }

    private void UpdateKeyItem(UIInventoryItems item)
    {
        if (HasKey(item))
        {
            item.EnableVisual();
        }
        else
        {
            item.DisableVisual();
        }
    }

    private void UpdateAbilityItem(UIInventoryItems item)
    {
        if (HasAbility(item))
        {
            item.EnableVisual();
        }
        else
        {
            item.DisableVisual();
        }
    }

}
