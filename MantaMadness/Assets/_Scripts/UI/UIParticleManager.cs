using UnityEngine;
using System.Collections;

public class UIParticleManager : MonoBehaviour
{
    [SerializeField]private VFXData[] uiParticleList;
    [SerializeField] private VFXData[] uiParticleGood;
    [SerializeField] private VFXData[] uiParticleExplosion;

    private bool explosionInCooldown = false;

    private void Start()
    {
        UIEffectManager.Instance.GoodAction += playGoodParticle;
        UIEffectManager.Instance.SpecificAction += playtSpecificParticle;
        UIEffectManager.Instance.ExplosionAction += playExplosionParticle;
    }

    public void playtSpecificParticle(string name, string overload)
    {
        for (int i = 0; i < uiParticleList.Length; i++)
        {
            if (uiParticleList[i].VfxName == name)
            {
                uiParticleList[i].PlayParticle();
                break;
            }
        }
    }

    public void playGoodParticle()
    {
        uiParticleGood[Random.Range(0, uiParticleGood.Length)].PlayParticle();
    }
    
    public void playExplosionParticle(string overload)
    {
        if(explosionInCooldown == false)
        {
            StartCoroutine(explosionParticleCoroutine());
        }
    }

    public IEnumerator explosionParticleCoroutine()
    {
        explosionInCooldown = true;
        uiParticleExplosion[Random.Range(0, uiParticleExplosion.Length)].PlayParticle();
        yield return new WaitForSeconds(3f);
        explosionInCooldown = false;
        yield return null;
    }

}
