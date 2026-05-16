using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;

public class WorldCheckpoint : MonoBehaviour
{
    [SerializeField] public Transform respawnTransform;
    [SerializeField] public string indexName;
    [SerializeField] public bool displayAreaName;
    [SerializeField] public string nameToDisplay;

    [SerializeField] private MeshRenderer[] visuals;
    [SerializeField] private GameObject flag;
    [SerializeField] private Material enableMat;
    [SerializeField] private Material disableMat;
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private EventReference soundToPlay;

    [Header("Level")]
    [SerializeField] protected string levelID;
    public string LevelID => levelID;

    protected virtual void Start()
    {
        if (!WorldCheckpointManager.Instance.checkpoints.Contains(this))
        {
            WorldCheckpointManager.Instance.checkpoints.Add(this);
            print(indexName + " Has been added to checkpoint list");
        }
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out SimpleController controller))
        {
            WorldCheckpointManager.Instance.SetCheckpoint(respawnTransform, indexName, displayAreaName, nameToDisplay, LevelID);
        }
    }

    public virtual void EnableMat()
    {
        //Enable Flag
        if(flag != null)
        {
            flag.SetActive(true);
        }

        //Change mat to enablemat
        foreach(MeshRenderer renderer in visuals)
        {
            renderer.material = enableMat;
        }
        //Play SFX
        if(particle != null)
        {
            particle.Play();
        }

        //Play Particle
        if(!soundToPlay.IsNull)
        {
            RuntimeManager.PlayOneShot(soundToPlay, transform.position);
        }

        //Play Checkpoint UI particle
        UIParticleManager.Instance.playSpecificUIParticle(UiParticles.CHECKPOINT, "");
    }
    
    public virtual void DisableMat()
    {
        if(flag != null)
        {
            //Disable Flag
            flag.SetActive(false);
        }

        //Change Mat to disableMat
        foreach (MeshRenderer renderer in visuals)
        {
            renderer.material = disableMat;
        }
    }
}
