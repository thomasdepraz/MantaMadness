using FMODUnity;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HauntedTunnel : MonoBehaviour
{

    bool wasInTunnel;

    [SerializeField] private Transform center;
    [SerializeField] private Vector3 size;
    [SerializeField] private LayerMask playerLayer;

    [Header("Settings")]
    [SerializeField] private float checkInterval = 0.2f;

    private MUSICS lastMusic;
    private AMBIENT lastAmbient;

    private Coroutine checkCoroutine;

    [Header("Puzzle Components")]
    [SerializeField] private int state = 0;
    [SerializeField] private List<HauntedPuzzleElement> elements;
    [SerializeField] private ParticleSystem dark_001, dark_002, dark_003;
    private float endTimer;
    [SerializeField] private float timerDuration;
    [SerializeField] private GameObject deadFish;
    [SerializeField] private Transform northEntrance, southEntrance;

    [SerializeField] private CoinHolder CoinToDeac;
    [SerializeField] private GameObject Coco;

    public EventReference bellStinger;
    public EventReference laugh;

    private void Start()
    {
        OnInteraction(0);
    }
    private void OnEnable()
    {
        checkCoroutine = StartCoroutine(CheckTunnelRoutine());

        foreach(HauntedPuzzleElement element in elements)
        {
            element.tunnel = this;
        }

        ResetTimer();

        Coco.SetActive(false);
    }
    private void OnDisable()
    {
        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
            checkCoroutine = null;
        }
    }

    private IEnumerator CheckTunnelRoutine()
    {

        CheckTunnel();

        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        while (true)
        {
            yield return wait;
            CheckTunnel();
        }
    }

    public void CheckTunnel()
    {
        bool isInTunnel = Physics.CheckBox(center.position, size / 2f, center.rotation, playerLayer);

        if (isInTunnel)
        {
            UIManager.Instance.ToggleBaseInterface(false);
        }

        if (isInTunnel == wasInTunnel)
            return;

        wasInTunnel = isInTunnel;

        if (isInTunnel)
        {
            // ENTER TUNNEL => DEACTIVATE MUSIC + UI
            lastMusic = MusicManager.Instance.currentMusic;
            lastAmbient = MusicManager.Instance.currentAmb;
            MusicManager.Instance.PlayMusic(MUSICS.MUSIC_WHY);
            MusicManager.Instance.PlayAmbient(AMBIENT.NULL);
            VolumeManager.Instance.toggleTunnel(true);
        }
        else
        {
            if (Game.Instance.why == true)
                return;
            //REACTIVATE MUSIC + UI
            MusicManager.Instance.PlayMusic(lastMusic);
            MusicManager.Instance.PlayAmbient(lastAmbient);
            UIManager.Instance.ToggleBaseInterface(true);
            VolumeManager.Instance.toggleTunnel(false);
        }
    }

    public void OnInteraction(int newState)
    {

        //SETUP PUZZLE
        for (int i = 0; i < elements.Count; i++)
        {
            if (i == newState)
            {
                elements[i].gameObject.SetActive(true);
            }
            else
            {
                elements[i].gameObject.SetActive(false);
            }
        }

        state = newState;

        switch (state)
        {
            case 0:
                //Nothing
                break;
            case 1:
                //Enable ParticleSystem
                dark_001.Play();
                RuntimeManager.PlayOneShot(laugh, Camera.main.transform.position);
                break;
            case 2:
                //Enable ParticleSystem
                dark_002.Play();
                RuntimeManager.PlayOneShot(laugh, Camera.main.transform.position);
                break;
            case 3:
                //Launch Last Coroutine
                dark_003.Play();
                elements[2].gameObject.SetActive(true);
                break;
        }
    }

    public void ReduceTimer()
    {
        endTimer -= Time.fixedDeltaTime;
        if (endTimer > 0)
        {
            Debug.Log(endTimer);
        }
        else if(endTimer <= 0 && ThisIsTheEndRoutine == null)
        {
            ThisIsTheEndRoutine = StartCoroutine(ThisIsTheEnd());
        }
    }

    public void ResetTimer()
    {
        if(endTimer != timerDuration)
            endTimer = timerDuration;

        Debug.Log("Reset Timer");
    }

    private Coroutine ThisIsTheEndRoutine;

    private IEnumerator ThisIsTheEnd()
    {


        OnInteraction(3);
        RuntimeManager.PlayOneShot(bellStinger, Camera.main.transform.position);
        WeatherManager.instance.SetNewWeather(WeatherType.Why);
        SunPositionManager.instance.SetSunState(SunState.Why);

        Game.Instance.why = true;

        yield return new WaitForSeconds(25f);
        switch (CalculateOptimumPosition())
        {
            case 0:
                //SPAWN SOUTH
                GameObject dead = Instantiate(deadFish, southEntrance.position, Quaternion.identity);
                dead.GetComponent<DeadFish>().tunnel = this;
                break;
            case 1:
                //SPAWN NORTH
                GameObject dead2 = Instantiate(deadFish, northEntrance.position, Quaternion.identity);
                dead2.GetComponent<DeadFish>().tunnel = this;
                break;
        }

    }

    public void SpawnAnomaly()
    {
        RuntimeManager.PlayOneShot(laugh, Camera.main.transform.position);
        Coco.SetActive(true);
        CoinToDeac.gameObject.SetActive(false);
    }

    private int CalculateOptimumPosition()
    {
        float distanceToNorth = Vector3.Distance(northEntrance.position, Game.Instance.player.transform.position);
        float distanceToSouth = Vector3.Distance(southEntrance.position, Game.Instance.player.transform.position);

        if (distanceToNorth < distanceToSouth)
            return 0; // SPAWN SOUTH
        else if (distanceToSouth < distanceToNorth)
            return 1; //SPAWN NORTH
        else
            return 0; //DEFAULT SPAWN SOUTH
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (center == null)
            return;

        Gizmos.matrix = Matrix4x4.TRS(
            center.position,
            center.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, size);
    }
#endif
}
