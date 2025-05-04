using System;
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


    [HideInInspector] public SimpleController player;
    public RaceManager raceManager = new RaceManager();

    public void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<SimpleController>();
    }

    public bool Respawn(out Transform respawn)
    {
        respawn = null;
        if (raceManager.TryGetRespawn(out respawn))
            return true;

        return false;
    }
}
