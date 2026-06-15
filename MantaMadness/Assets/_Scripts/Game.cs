using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Timeline;

public class Game : MonoBehaviour, IDataPersistence
{
    public static Game Instance;

    private float respawnTimer = -1f;
    private System.Action onTimerFinished;
    private bool isRespawning = false;
    private bool onLoadCheck = false;
    public bool isHitStop = false;

    //Cinematic / State Points
    //public bool introCinematic = false;
    public TimelineAsset introCinematicTimeline;
    public WorldCheckpoint introCheckpoint;
    public InteractableNPC introNpc;
    public InteractableNPC superGoodJoeNpc;

    public EndPortalHolder endPortalHolder;

    public WorldCheckpoint UNSTUCKCheckpoint;
    public int gameState { private set; get; }

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

        Application.targetFrameRate = 60;
        player = GameObject.FindWithTag("Player").GetComponent<SimpleController>();
    }

    public ScreenEffectData screenEffectData;

    [HideInInspector] public SimpleController player;
    public RaceManager raceManager = new RaceManager();
    CameraManager cameraManager = CameraManager.Instance;

    [HideInInspector] public Vector3 m_SpawnPosition;
    [HideInInspector] public Quaternion m_SpawnRotation;

    public void Start()
    {
        List<ScriptableRendererFeature> scriptableRendererFeatures = RenderFeatureUtility.GetRenderFeatures();
        foreach(var effect in screenEffectData.ScreenEffects)
        {
            ScriptableRendererFeature scriptableRendererFeature = RenderFeatureUtility.GetFeature(scriptableRendererFeatures, effect.featureName);
            scriptableRendererFeature.SetActive(effect.isActive);
        }

        player = GameObject.FindWithTag("Player").GetComponent<SimpleController>();

        //Toggle screen effects
        m_SpawnPosition = player.transform.position;
        m_SpawnRotation = player.transform.rotation;

    }

    public void LoadData(GameData data)
    {
        gameState = data.GameState;


        StartCoroutine(DelayLoad());

    }

    private IEnumerator DelayLoad()
    {
        yield return new WaitForSeconds(0.1f);
        StateChange();
        CollectibleAreaManager.RestoreCurrentArea();
    }

    public void SaveData(ref GameData data)
    {
        data.GameState = gameState;
    }

    public void SetGameState(int stateIndex)
    {
        gameState = stateIndex;
        StateChange();
    }

    public void StateChange()
    {
        switch (gameState)
        {
            //Start the game
            case 0:
                //Play intro cinematic
                //if (introCinematicTimeline != null)
                //{
                //    CinematicManager.instance.PlayCinematic(introCinematicTimeline);
                //}

                //PLAY LE DIALOG DU VIEUX
                DialogManager.instance.StartCinematicInteraction(introNpc);

                //SET POSITION TO FIRST CHECKPOINT POS
                WorldCheckpointManager.Instance.SetStartCheckpoint(introCheckpoint.respawnTransform);
                WorldCheckpointManager.Instance.SetCheckpoint(introCheckpoint.respawnTransform, introCheckpoint.indexName, introCheckpoint.displayAreaName, introCheckpoint.nameToDisplay, introCheckpoint.LevelID, introCheckpoint.collectibleAreaID);
                Vector3 pos = Vector3.zero;
                Quaternion rotation;
                Respawn(out pos, out rotation);
                SetGameState(1);
                break;

            case 1:
                break;

            //Collect the Frutti wings
            case 2:
                Debug.Log("Ca marche pas ou quoi ? also le state = " + gameState);
                //Set Fisherman to state 1
                NPCManager.instance.UpdateNPCState("FISHERMAN", 1);
                //Set Red moai to state 1
                NPCManager.instance.UpdateNPCState("REDMOAI", 1);
                //Set Pink crab to state 1
                NPCManager.instance.UpdateNPCState("PINKCRAB", 1);

                break;

            //Go To Sun Altar
            case 3:
                DialogManager.instance.StartCinematicInteraction(superGoodJoeNpc);
                SetGameState(4);
                //UPDATE FISHERMAN
                break;
             
            case 4:
                break;

            case 10:
                NPCManager.instance.UpdateNPCState("SUPERGOODJOE", 1);
                SetGameState(11);
                break;

            case 12:
                NPCManager.instance.UpdateNPCState("SUPERGOODJOE", 2);
                SetGameState(13);
                break;
            default:
                break;
        }
    }

    public void Update()
    {
        if(respawnTimer > 0f)
        {
            respawnTimer -= Time.deltaTime;

            if (respawnTimer <= 0f)
            {
                onTimerFinished?.Invoke();
                onTimerFinished = null;
            }
        }
    }

    public void Respawn(out Vector3 position, out Quaternion rotation)
    {
        if (raceManager.TryGetRespawn(out Transform respawn))
        {
            position = respawn.position;
            rotation = respawn.rotation;
            if(isRespawning == false)
            {
                RespawnBehavior(position, rotation);
            }
            return;
        }

        ChallengeManager.instance.Reset();

        position = m_SpawnPosition;
        rotation = m_SpawnRotation;
        if(isRespawning == false)
        {
            StartCoroutine(RespawnBehavior(position, rotation));
        }
    }

    private IEnumerator RespawnBehavior(Vector3 position, Quaternion rotation)
    {
        isRespawning = true;
        //Action before timer
        if(onLoadCheck != true)
        {
            onLoadCheck = true;
            //UIManager.Instance.transitionScreen.TransitionOnLoad();
        }
        else
        {
            UIManager.Instance.transitionScreen.TransitionIn();
        }

        FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_Life, 1f);
        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.DEATH);
        yield return new WaitForSeconds(1f);
        //Action After timer
        UIManager.Instance.transitionScreen.TransitionOut();
        FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_Life, 0f);
        player.ForcePosition(position, rotation);
        isRespawning = false;
    }

    public void SetRespawnTransform(Transform respawnTransform)
    {
        m_SpawnPosition = respawnTransform.position;
        m_SpawnRotation = respawnTransform.rotation;
    }

    public void CheckEndGame(int checkValue)
    {
        if(checkValue >= 10)
        {
            ActivateEndingScreen();
        }
    }

    public void ActivateEndingScreen()
    {
        //ENDING SCREEN ACTIVATION
        if (CoinManager.Instance.PickupCoinCount == 20)
        {
            // ACTIVATE ENDING SCREEN BEHAVIOR
            endPortalHolder.CouroutinSpawnStart(player);
            SetGameState(10);
        }

        if (CoinManager.Instance.PickupCoinCount == 38)
        {
            SetGameState(12);
            SteamSuccess.instance.ActivateSteamSuccess(SteamSuccessEnum.ACH_EVENT_ALLJOHNNIES);
        }
    }

    public void UnstuckPlayer()
    {
        WorldCheckpointManager.Instance.SetStartCheckpoint(UNSTUCKCheckpoint.respawnTransform);
        WorldCheckpointManager.Instance.SetCheckpoint(UNSTUCKCheckpoint.respawnTransform, UNSTUCKCheckpoint.indexName, UNSTUCKCheckpoint.displayAreaName, UNSTUCKCheckpoint.nameToDisplay, UNSTUCKCheckpoint.LevelID, UNSTUCKCheckpoint.collectibleAreaID);
        Vector3 pos = Vector3.zero;
        Quaternion rotation;
        Respawn(out pos, out rotation);
    }
}
