using UnityEngine;

public class VFXData : MonoBehaviour
{
    public void PlayParticle()
    {
        GetComponent<ParticleSystem>().Play();
    }

    public void StopParticle()
    {
        GetComponent<ParticleSystem>().Stop();
    }
}
