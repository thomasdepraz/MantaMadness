using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class ChallengeManager : MonoBehaviour
{
    public static ChallengeManager instance;

    public List<BuoyGame> buoyGamesList;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void Reset()
    {
        foreach(BuoyGame game in buoyGamesList)
        {
            if(game.hasStarted == true && game.Completed == false)
            {
                game.Reset();
            }
        }   
    }
}
