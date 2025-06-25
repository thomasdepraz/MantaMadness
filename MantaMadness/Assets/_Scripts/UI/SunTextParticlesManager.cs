using UnityEngine;

public class SunTextParticlesManager : MonoBehaviour
{


    public ParticleSystem[] goodParticles;
    public ParticleSystem[] wackParticles;
    public ParticleSystem[] generalParticles;

    public void PlayGoodParticle()
    {
        goodParticles[Random.Range(0, goodParticles.Length)].Play();
    }

    public void PlayBadParticle()
    {
        wackParticles[Random.Range(0, wackParticles.Length)].Play();
    }

    public void PlayParticle(int index)
    {
        generalParticles[index].Play();
    }
}
