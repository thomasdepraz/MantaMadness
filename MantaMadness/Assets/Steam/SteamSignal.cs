using System.Collections;
using UnityEngine;

public class SteamSignal : MonoBehaviour
{
    [SerializeField] private SteamSuccessEnum successID;

    public void Trigger()
    {
        SteamSuccess.instance.ActivateSteamSuccess(successID);
    }
}
