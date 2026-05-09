using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    //GAME STATE
    public int GameState;

    //Currencies Counters
    public int clamCount;
    public int buckieCount;
    public SerializableDictionnary<string, bool> coinsCollected;

    public SerializableDictionnary<string, CollectibleState> collectibleStates;
    public string currentCollectibleAreaID;

    //Checkpoints
    public SerializableDictionnary<string, bool> checkpoints;

    //Altar
    public SerializableDictionnary<string, bool> abilityAltars;

    //Special Pickups
    public SerializableDictionnary<string, bool> specialPickups;

    //NPC Dialogs
    public SerializableDictionnary<string, NPCDialogData> npcDialogData;

    //Doors
    public SerializableDictionnary<string, bool> doorsOpened;

    //Shop Standss
    public SerializableDictionnary<string, ShopStandData> shopStands;

    // Puzzle destructibles
    public SerializableDictionnary<string, bool> puzzleElements;

    // Lilypad Manager
    public SerializableDictionnary<string, ModuleState> lilyPadModules;


    //Player Upgrades Tracking
    public bool doubleJump;
    public bool chargeBoost;
    public bool stomp;
    public bool lavaResistance;
    public bool alienAntennas;
    public bool grind;
    public bool cat;
    public bool dynamo;



    public MUSICS gameStartMusic;
    public AMBIENT gameStartAmbient;
    public WeatherType weatherCondition;
    public WeatherType mainMenuWeatherCondition;
    public FogState fogState;

    // The values defined in this constructor will be the default values
    // Start with this when there's no data to load
    public GameData()
    {
        GameState = 0;

        this.clamCount = 0;
        this.buckieCount = 0;
        this.gameStartMusic = MUSICS.MUSIC_LEVEL01;
        this.gameStartAmbient = AMBIENT.AMB_BEACH;
        this.weatherCondition = WeatherType.Shores;
        this.mainMenuWeatherCondition = weatherCondition;
        this.fogState = FogState.enabled;
        coinsCollected = new SerializableDictionnary<string, bool>();
        checkpoints = new SerializableDictionnary<string, bool>();
        abilityAltars = new SerializableDictionnary<string, bool>();
        npcDialogData = new SerializableDictionnary<string, NPCDialogData>();
        specialPickups = new SerializableDictionnary<string, bool>();
        doorsOpened = new SerializableDictionnary<string, bool>();
        shopStands = new SerializableDictionnary<string, ShopStandData>();
        puzzleElements = new SerializableDictionnary<string, bool>();
        collectibleStates = new SerializableDictionnary<string, CollectibleState>();
        lilyPadModules = new SerializableDictionnary<string, ModuleState>();
        currentCollectibleAreaID = "SONKI";

        doubleJump = false;
        chargeBoost = false;
        stomp = false;
        lavaResistance = false;
        alienAntennas = false;
        grind = false;
        cat = false;
        dynamo = false;

    }
}

[System.Serializable]
public class ShopStandData
{
    public int renewalCount;
    public bool disabled;

    public ShopStandData(int renewalCount, bool disabled)
    {
        this.renewalCount = renewalCount;
        this.disabled = disabled;
    }
}
