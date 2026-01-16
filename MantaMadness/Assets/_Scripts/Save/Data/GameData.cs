using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    //GAME STATE
    public int GameState;

    //Currencies Counters
    public int clamCount;
    public SerializableDictionnary<string, bool> coinsCollected;

    //Checkpoints
    public SerializableDictionnary<string, bool> checkpoints;

    //Altar
    public SerializableDictionnary<string, bool> abilityAltars;

    //NPC Dialogs
    public SerializableDictionnary<string, NPCDialogData> npcDialogData;

    //Player Upgrades Tracking
    public bool doubleJump;
    public bool chargeBoost;
    public bool stomp;
    public bool lavaResistance;
    public bool alienAntennas;
    public bool grind;

    public MUSICS gameStartMusic;
    public AMBIENT gameStartAmbient;

    // The values defined in this constructor will be the default values
    // Start with this when there's no data to load
    public GameData()
    {
        GameState = 0;

        this.clamCount = 0;
        this.gameStartMusic = MUSICS.MUSIC_LEVEL01;
        this.gameStartAmbient = AMBIENT.AMB_BEACH;
        coinsCollected = new SerializableDictionnary<string, bool>();
        checkpoints = new SerializableDictionnary<string, bool>();
        abilityAltars = new SerializableDictionnary<string, bool>();
        npcDialogData = new SerializableDictionnary<string, NPCDialogData>();

        doubleJump = false;
        chargeBoost = false;
        stomp = false;
        lavaResistance = false;
        alienAntennas = false;
        grind = false;
    }
}
