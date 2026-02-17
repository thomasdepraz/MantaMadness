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
    [SerializeField] public MUSICS musicToPlay = MUSICS.NULL;
    [SerializeField] public AMBIENT ambientToPlay = AMBIENT.NULL;

    [SerializeField] private MeshRenderer[] visuals;
    [SerializeField] private GameObject flag;
    [SerializeField] private Material enableMat;
    [SerializeField] private Material disableMat;
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private EventReference soundToPlay;


    private void Awake()
    {
        //if(gameObject.GetComponent<MeshRenderer>().enabled == true)
        //{
        //    gameObject.GetComponent<MeshRenderer>().enabled = false;
        //}
    }

    private void Start()
    {
        if (!WorldCheckpointManager.Instance.checkpoints.Contains(this))
        {
            WorldCheckpointManager.Instance.checkpoints.Add(this);
            print(indexName + " Has been added to checkpoint list");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out SimpleController controller))
        {
            WorldCheckpointManager.Instance.SetCheckpoint(respawnTransform, indexName, displayAreaName, nameToDisplay);
            if (musicToPlay != MUSICS.NULL)
            {
                MusicManager.Instance.PlayMusic(musicToPlay);
            }
            if(ambientToPlay != AMBIENT.NULL)
            {
                MusicManager.Instance.PlayAmbient(ambientToPlay);
            }
        }
    }

    public void EnableMat()
    {
        //Enable Flag
        flag.SetActive(true);
        //Change mat to enablemat
        foreach(MeshRenderer renderer in visuals)
        {
            renderer.material = enableMat;
        }
        //Play SFX
        particle.Play();
        //Play Particle
        RuntimeManager.PlayOneShot(soundToPlay, transform.position);
        //Play Checkpoint UI particle
        UIParticleManager.Instance.playSpecificUIParticle(UiParticles.CHECKPOINT, "");
    }
    
    public void DisableMat()
    {
        //Disable Flag
        flag.SetActive(false);
        //Change Mat to disableMat
        foreach (MeshRenderer renderer in visuals)
        {
            renderer.material = disableMat;
        }
    }
}
