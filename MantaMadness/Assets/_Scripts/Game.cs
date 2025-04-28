using UnityEngine;

public class Game : MonoBehaviour
{
    public Race startRace;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RaceUtility.StartRace(startRace);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
