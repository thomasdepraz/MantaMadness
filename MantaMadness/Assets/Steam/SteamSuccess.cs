using Steamworks;
using System.Collections;
using System;
using TMPEffects.Parameters;
using UnityEngine;

public enum SteamSuccessEnum
{
    ACH_ABILITY_WINGS,
    ACH_ABILITY_STOMP,
    ACH_ABILITY_DOUBLEJUMP,
    ACH_ABILITY_DYNAMO,
    ACH_ABILITY_ALIEN,
    ACH_ABILITY_LAVARESIST,
    ACH_EVENT_JOVIAL,
    ACH_EVENT_ALLJOHNNIES,
    ACH_AREA_SONKI,
    ACH_AREA_FRUTTI,
    ACH_AREA_VOLCANINO,
    ACH_AREA_SUNALTAR,
    ACH_AREA_SHORES,
    ACH_AREA_VILLAGE,
    ACH_AREA_MOUNTAINTEMPLE,
    ACH_AREA_ANCIENTTEMPLE,
    ACH_AREA_CORALLAND,
    ACH_AREA_CORALCELLAR,
    ACH_AREA_ALIENFIELD,
    ACH_AREA_ALIENSHIP,
    ACH_AREA_OUTSKIRT,
    ACH_AREA_SEWER,
    ACH_AREA_CITY,
    ACH_AREA_LAVATREASURE,
    ACH_AREA_BACKROOM,
    ACH_AREA_ICELEVEL,
}

public class SteamSuccess : MonoBehaviour, IDataPersistence
{
    public static SteamSuccess instance;

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

    public void LoadData(GameData data)
    {
        StartCoroutine(LoadDelay(data));
    }

    private IEnumerator LoadDelay(GameData data)
    {
        yield return new WaitForSeconds(0.1f);
        SyncAchievements(data);
    }

    public void SaveData(ref GameData data)
    {
        //RIEN
    }

    void Start()
    {
        Debug.Log("SteamManager.Initialized = " + SteamManager.Initialized);

        if (SteamManager.Initialized)
        {
            Debug.Log("Steam User: " + SteamFriends.GetPersonaName());
        }

        Debug.Log("Steam app id= " + SteamUtils.GetAppID().m_AppId);
    }

    public void ActivateSteamSuccess(SteamSuccessEnum success)
    {
        if (!SteamManager.Initialized)
            return;

        SteamUserStats.SetAchievement(success.ToString());
        SteamUserStats.StoreStats();
    }

    public void SyncAchievements(GameData data)
    {
        if (!SteamManager.Initialized || data == null)
            return;
        SimpleController player = Game.Instance.player;

        //Player Abilities
        UnlockIf(player.grindAbility, SteamSuccessEnum.ACH_ABILITY_WINGS);
        UnlockIf(player.stompAbility, SteamSuccessEnum.ACH_ABILITY_STOMP);
        UnlockIf(player.doubleJumpAbility, SteamSuccessEnum.ACH_ABILITY_DOUBLEJUMP);
        UnlockIf(player.dynamoAbility, SteamSuccessEnum.ACH_ABILITY_DYNAMO);
        UnlockIf(player.alienAntennasAbility, SteamSuccessEnum.ACH_ABILITY_ALIEN);
        UnlockIf(player.lavaResistanceAbility, SteamSuccessEnum.ACH_ABILITY_LAVARESIST);

        //Special Events
        //UnlockIf(CoinManager.Instance.PickupCoinCount == 38, SteamSuccessEnum.ACH_EVENT_JOVIAL);

        //Johnnies Count
        UnlockIf(CoinManager.Instance.PickupCoinCount == 38, SteamSuccessEnum.ACH_EVENT_ALLJOHNNIES);

        //Areas Completed
        //Thoses ared hnadled through each AreaIntroManager Respectively

        SteamUserStats.StoreStats();
    }

    private void UnlockIf(bool condition, SteamSuccessEnum achievement)
    {
        if (condition)
        {
            SteamUserStats.SetAchievement(achievement.ToString());
        }
    }

    public void ResetSteamSuccess()
    {
        SteamUserStats.ResetAllStats(true);
        SteamUserStats.StoreStats();
    }


}