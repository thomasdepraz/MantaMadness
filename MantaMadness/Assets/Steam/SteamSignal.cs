using System.Collections;
using UnityEngine;

public class SteamSignal : MonoBehaviour
{
    [SerializeField] private SteamSuccessEnum successID;

    public void Trigger()
    {
        if (!SteamManager.Initialized)
        {
            Debug.Log("[Steam] Ignoré : Steamworks non initialisé.");
            return;
        }

        SteamSuccess.instance.ActivateSteamSuccess(successID);
    }
}
