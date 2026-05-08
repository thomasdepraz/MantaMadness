using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public enum ModuleState
{
    Disabled,
    Enabled
}

public class LilyPadManager : MonoBehaviour, IDataPersistence
{
    [SerializeField]
    private ModuleState moduleState = ModuleState.Disabled;

    [Header("Save")]
    [SerializeField] private string moduleID;

    [SerializeField] private Lilypad[] lilypads;
    [SerializeField] private GameObject[] toDeactivate;
    [SerializeField] private GameObject[] toActivate;

    [Header("Collectible Rewards")]
    [SerializeField] private Collectible[] collectibleRewards;

    [SerializeField] private bool enableCoin;
    [SerializeField] private string coinName;
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private CinemachineBlendDefinition blend;
    [SerializeField, Range(1, 100)] private int lilypadsPercentage = 90;
    [SerializeField] private float spawnTime = 2f;
    [SerializeField] private EventReference ClearSound;


    bool endGameStarted = false;
    private int count = 0;

    public UnityEvent onEnd;

#if UNITY_EDITOR
    [ContextMenu("Generate GUID")]
    private void GenerateGUID()
    {
        moduleID = System.Guid.NewGuid().ToString();
    }
#endif

    public void LoadData(GameData data)
    {
        if (data.lilyPadModules.TryGetValue(moduleID, out ModuleState savedState))
        {
            moduleState = savedState;
        }

        ApplyModuleState();
    }

    public void SaveData(ref GameData data)
    {
        if (data.lilyPadModules.ContainsKey(moduleID))
        {
            data.lilyPadModules[moduleID] = moduleState;
        }
        else
        {
            data.lilyPadModules.Add(moduleID, moduleState);
        }
    }

    private void Start()
    {
        for (int i = 0; i < lilypads.Length; i++)
        {
            lilypads[i].SetManager(this);
        }
    }

    public void Collect()
    {
        if (endGameStarted || moduleState == ModuleState.Enabled)
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
        if (!enableCoin)
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

            for (int i = 0; i < lilypads.Length; i++)
            {
                lilypads[i].AlternateBlooming();
            }

            if (!enableCoin)
            {
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

                if (collectibleRewards.Length > 0)
                {
                    for (int i = 0; i < collectibleRewards.Length; i++)
                    {
                        Collectible collectible = collectibleRewards[i];

                        if (collectible == null)
                            continue;

                        // IMPORTANT :
                        // seulement les collectibles Activable
                        // peuvent être activés par le puzzle
                        if (collectible.State == CollectibleState.Activable)
                        {
                            collectible.ActivateCollectible();

                            collectible.transform.DOMoveY(
                                collectible.transform.position.y + 5f,
                                0.2f
                            )
                            .SetEase(Ease.OutQuad)
                            .SetLoops(2, LoopType.Yoyo);

                            yield return new WaitForSeconds(
                                spawnTime / collectibleRewards.Length
                            );
                        }
                    }
                }

                moduleState = ModuleState.Enabled;
                DataPersistenceManager.Instance.SaveGame();


                yield return new WaitForSeconds(0.8f);
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

    private void ApplyModuleState()
    {
        // MODULE PAS FINI
        if (moduleState == ModuleState.Disabled)
        {
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

            return;
        }

        // MODULE FINI

        RestoreLilypads();

        if (toDeactivate.Length > 0)
        {
            for (int i = 0; i < toDeactivate.Length; i++)
            {
                toDeactivate[i].SetActive(false);
            }
        }

        if (toActivate.Length > 0)
        {
            for (int i = 0; i < toActivate.Length; i++)
            {
                toActivate[i].SetActive(true);
            }
        }
    }

    private void RestoreLilypads()
    {
        for (int i = 0; i < lilypads.Length; i++)
        {
            lilypads[i].RestoreBloomedState();
        }
    }
}
