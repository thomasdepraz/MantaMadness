using Steamworks;
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
}

public class SteamSuccess : MonoBehaviour
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
        switch (success)
        {
            case SteamSuccessEnum.ACH_ABILITY_WINGS:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_ABILITY_WINGS.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_ABILITY_STOMP:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_ABILITY_STOMP.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_ABILITY_DOUBLEJUMP:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_ABILITY_DOUBLEJUMP.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_ABILITY_DYNAMO:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_ABILITY_DYNAMO.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_ABILITY_ALIEN:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_ABILITY_ALIEN.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_ABILITY_LAVARESIST:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_ABILITY_LAVARESIST.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_EVENT_JOVIAL:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_EVENT_JOVIAL.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_EVENT_ALLJOHNNIES:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_EVENT_ALLJOHNNIES.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_AREA_SONKI:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_AREA_SONKI.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_AREA_FRUTTI:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_AREA_FRUTTI.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_AREA_VOLCANINO:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_AREA_VOLCANINO.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_AREA_SUNALTAR:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_AREA_SUNALTAR.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_AREA_SHORES:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_AREA_SHORES.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_AREA_VILLAGE:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_AREA_VILLAGE.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_AREA_MOUNTAINTEMPLE:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_AREA_MOUNTAINTEMPLE.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_AREA_ANCIENTTEMPLE:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_AREA_ANCIENTTEMPLE.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_AREA_CORALLAND:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_AREA_CORALLAND.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_AREA_CORALCELLAR:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_AREA_CORALCELLAR.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_AREA_ALIENFIELD:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_AREA_ALIENFIELD.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_AREA_ALIENSHIP:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_AREA_ALIENSHIP.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_AREA_OUTSKIRT:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_AREA_OUTSKIRT.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_AREA_SEWER:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_AREA_SEWER.ToString());
                SteamUserStats.StoreStats();
                break;

            case SteamSuccessEnum.ACH_AREA_CITY:
                SteamUserStats.SetAchievement(SteamSuccessEnum.ACH_AREA_CITY.ToString());
                SteamUserStats.StoreStats();
                break;

            default:
                break;
        }
    }

    public void ResetSteamSuccess()
    {
        SteamUserStats.ResetAllStats(true);
        SteamUserStats.StoreStats();
    }
}