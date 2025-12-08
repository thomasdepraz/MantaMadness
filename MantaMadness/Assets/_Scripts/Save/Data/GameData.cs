using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    //Game State
    public bool introCinematic;



    //Currencies Counters
    public int clamCount;
    public SerializableDictionnary<string, bool> coinsCollected;

    //Checkpoints
    public SerializableDictionnary<string, bool> checkpoints;

    //Altar
    public SerializableDictionnary<string, bool> abilityAltars;

    //Player Upgrades Tracking
    public bool doubleJump;
    public bool chargeBoost;
    public bool stomp;
    public bool lavaResistance;
    public bool alienAntennas;
    public bool grind;

    // The values defined in this constructor will be the default values
    // Start with this when there's no data to load
    public GameData()
    {
        introCinematic = false;

        this.clamCount = 0;
        coinsCollected = new SerializableDictionnary<string, bool>();
        checkpoints = new SerializableDictionnary<string, bool>();
        abilityAltars = new SerializableDictionnary<string, bool>();

        doubleJump = true;
        chargeBoost = false;
        stomp = false;
        lavaResistance = false;
        alienAntennas = false;
        grind = false;
    }
}
