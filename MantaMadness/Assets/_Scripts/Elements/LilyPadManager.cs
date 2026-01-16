using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class LilyPadManager : MonoBehaviour
{
    [SerializeField] private Lilypad[] lilypads;
    [SerializeField] private GameObject[] toDeactivate;
    [SerializeField] private GameObject[] toActivate;
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private CinemachineBlendDefinition blend;
    [SerializeField, Range(1, 100)] private int lilypadsPercentage = 90;
    [SerializeField] private float spawnTime = 2f;
    [SerializeField] private EventReference ClearSound;

    bool endGameStarted = false;
    private int count = 0;

    public UnityEvent onEnd;

    private void Start()
    {
        for (int i = 0; i < lilypads.Length; i++)
        {
            lilypads[i].SetManager(this);
        }

        if (toDeactivate.Length > 0)
        {
            for (int i = 0; i < toDeactivate.Length; i++)
            {
                toDeactivate[i].SetActive(true);
            }
        }

        if (toActivate.Length > 0)
        {
            for (int i = 0; i < toActivate.Length; i++)
            {
                toActivate[i].SetActive(false);
            }
        }
    }

    public void Collect()
    {
        if (endGameStarted)
            return;

        count++;

        if (count >= (lilypadsPercentage * lilypads.Length) / 100)
        {
           EndGame();
        }
    }

    public void EndGame()
    {
        StartCoroutine(EndGameRoutine());
    }

    public IEnumerator EndGameRoutine()
    {
        Game.Instance.player.ForceLock(true);
        Game.Instance.player.RailLock(true);

        //Invoke onEnd for other scripts linked to it
        onEnd?.Invoke();

        //activate camera + play sound
        vcam.enabled = true;
        CameraManager.Instance.BlendToCamera(vcam, blend);
        RuntimeManager.PlayOneShot(ClearSound, vcam.transform.position);
        yield return new WaitForSeconds(1f);

        UIEffectManager.Instance.GoodAction.Invoke();

        for(int i = 0; i < lilypads.Length; i++)
        {
            lilypads[i].AlternateBlooming();
        }

        if (toDeactivate.Length > 0)
        {
            for (int i = 0; i < toDeactivate.Length; i++)
            {
                //DEACTIVCATE OBJECT
                toDeactivate[i].SetActive(false);
            }
        }

        if (toActivate.Length > 0)
        {
            for (int i = 0; i < toActivate.Length; i++)
            {
                //ACTIVATE OBJECT
                toActivate[i].SetActive(true);
                toActivate[i].transform.DOMoveY(toActivate[i].transform.position.y + 5f, 0.2f).SetEase(Ease.OutQuad).SetLoops(2,LoopType.Yoyo);
                yield return new WaitForSeconds(spawnTime / toActivate.Length);
            }
        }
        //unlock player
        Game.Instance.player.ForceLock(false);
        Game.Instance.player.RailLock(false);


        //reset camera
        CameraManager.Instance.ResetCamera(vcam);
        vcam.enabled = false;
    }
}
