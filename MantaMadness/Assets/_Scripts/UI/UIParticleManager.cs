using UnityEngine;

public class UIParticleManager : MonoBehaviour
{
    [SerializeField]private VFXData[] uiParticleList;
    [SerializeField] private VFXData[] uiParticleGood;

    private void Start()
    {
        UIEffectManager.Instance.GoodAction += playGoodParticle;
        UIEffectManager.Instance.SpecificAction += playtSpecificParticle;
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

}
