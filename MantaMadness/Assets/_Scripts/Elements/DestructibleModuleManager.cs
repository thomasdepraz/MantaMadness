using DG.Tweening;
using FMODUnity;
using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class DestructibleModuleManager : SpecialDestructibleManager
{
    [SerializeField] private GameObject[] toDeactivate;
    [SerializeField] private GameObject[] toActivate;
    [SerializeField] private float spawnTime = 2f;

    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private CinemachineBlendDefinition blend;

    [SerializeField] private EventReference ClearSound;

    [SerializeField] private bool enableCoin;
    [SerializeField] private string coinName;

    protected override void Start()
    {
        base.Start();

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


    public void StartEndGameRoutine()
    {
        if(EndGameCoroutine == null)
        {
            EndGameCoroutine = StartCoroutine(EndGameRoutine());
        }
    }

    private Coroutine EndGameCoroutine;

    private IEnumerator EndGameRoutine()
    {
        if (!enableCoin)
        {
            Game.Instance.player.ForceLock(true);
            Game.Instance.player.RailLock(true);

            //Invoke onEnd for other scripts linked to it
            //onEnd?.Invoke();

            //activate camera + play sound
            vcam.enabled = true;
            CameraManager.Instance.BlendToCamera(vcam, blend);
            RuntimeManager.PlayOneShot(ClearSound, vcam.transform.position);
            yield return new WaitForSeconds(1f);

            UIEffectManager.Instance.GoodAction.Invoke();

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
                    toActivate[i].transform.DOMoveY(toActivate[i].transform.position.y + 5f, 0.2f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo);
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
        else
        {
            CoinManager.Instance.ActivateCoinHolder(coinName);
            yield return null;
        }

    }
}