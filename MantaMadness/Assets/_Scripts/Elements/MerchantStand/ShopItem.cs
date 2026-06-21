using System;
using System.Collections.Generic;
using UnityEngine;


public enum ShopItemType
{
    Ability,
    Item,
    Sun,
    KeyItem,
}

public enum ShopCurrency
{
    Clam,
    Buckie,
}

[CreateAssetMenu(menuName = "Shop/Shop Item")]
public class ShopItem : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    [TextArea]
    public string description;

    [Header("Item Granted")]
    public ShopItemType type;
    public string itemSold;

    [Header("Price")]
    public ShopCurrency currency;
    public int price;
    public int itemRenewalLimit = 1;

    [Header("Item Visual")]
    public GameObject visual;

    [Header("If is An Item")]
    public GameObject itemToSpawn;
}
