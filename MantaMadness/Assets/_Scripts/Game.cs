using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }


    public Race startRace;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(startRace != null)
            RaceUtility.StartRace(startRace);
    }

    public bool Respawn(out Transform respawn)
    {
        respawn = null;
        if (startRace == null)
            return false;

        respawn = startRace.GetRespawnTransform();
        return true;
    }
}
