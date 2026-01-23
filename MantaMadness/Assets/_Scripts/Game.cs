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
    public bool isHitStop = false;

    //Cinematic / State Points
    //public bool introCinematic = false;
    public TimelineAsset introCinematicTimeline;
    public WorldCheckpoint introCheckpoint;

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
        //player = GameObject.FindWithTag("Player").GetComponent<SimpleController>();

        //Toggle screen effects
        List<ScriptableRendererFeature> scriptableRendererFeatures = RenderFeatureUtility.GetRenderFeatures();
        foreach(var effect in screenEffectData.ScreenEffects)
        {
            ScriptableRendererFeature scriptableRendererFeature = RenderFeatureUtility.GetFeature(scriptableRendererFeatures, effect.featureName);
            scriptableRendererFeature.SetActive(effect.isActive);
        }

        // PLay level Music
        //SoundManager.PlayMusic(Music.THEME_001);

        //m_SpawnPosition = player.transform.position;
        //m_SpawnRotation = player.transform.rotation;
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
                if (introCinematicTimeline != null)
                {
                    CinematicManager.instance.PlayCinematic(introCinematicTimeline);
                }


                //SET POSITION TO FIRST CHECKPOINT POS
                WorldCheckpointManager.Instance.SetStartCheckpoint(introCheckpoint.respawnTransform);
                WorldCheckpointManager.Instance.SetCheckpoint(introCheckpoint.respawnTransform, introCheckpoint.indexName, introCheckpoint.displayAreaName, introCheckpoint.nameToDisplay);
                Vector3 pos = Vector3.zero;
                Quaternion rotation;
                Respawn(out pos, out rotation);
                break;

            //Collect the Frutti wings
            case 1:
                Debug.Log("Ca marche pas ou quoi ? also le state = " + gameState);
                //Set Fisherman to state 1
                NPCManager.instance.UpdateNPCState("FISHERMAN", 1);
                //Set Red moai to state 1
                NPCManager.instance.UpdateNPCState("REDMOAI", 1);
                //Set Pink crab to state 1
                NPCManager.instance.UpdateNPCState("PINKCRAB", 1);

                break;

            //Collect the Missing Hand
            case 2:
                //Set Cat poster to State 1
                NPCManager.instance.UpdateNPCState("CATPOSTER", 1);
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
        UIManager.Instance.transitionScreen.TransitionIn();
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
}
