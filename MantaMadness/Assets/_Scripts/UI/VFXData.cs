using UnityEngine;

public class VFXData : MonoBehaviour
{
    [SerializeField] public string VfxName;

    public void PlayParticle()
    {
        GetComponent<ParticleSystem>().Play();
    }

    public void StopParticle()
    {

    }
}
