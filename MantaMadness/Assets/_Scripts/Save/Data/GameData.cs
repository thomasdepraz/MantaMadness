using UnityEngine;

[System.Serializable]
public class GameData
{
    public int deathCount;

    public int clamCount;
    public Vector3 playerPosition;

    // The values defined in this constructor will be the default values
    // Start with this whenn there's no data to load
    public GameData()
    {
        this.deathCount = 0;
        this.clamCount = 0;
        playerPosition = Vector3.zero;

    }
}
