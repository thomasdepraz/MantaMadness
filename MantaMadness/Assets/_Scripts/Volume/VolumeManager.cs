using UnityEngine;

public class VolumeManager : MonoBehaviour
{
    public static VolumeManager Instance;

    [SerializeField] private GameObject defaultVolume;
    [SerializeField] private GameObject underwatertVolume;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if(defaultVolume.activeSelf == false && defaultVolume != null)
        {
            defaultVolume.SetActive(true);
        }

        if (underwatertVolume.activeSelf == true && underwatertVolume != null)
        {
            underwatertVolume.SetActive(false);
        }

    }

    public void toggleUnderwater(bool toggleValue)
    {
        if(toggleValue == false)
        {
            underwatertVolume.SetActive(false);
            UIParticleManager.Instance.stopSpecificParticle(UiWordsParticles.BUBBLE, "");
        }
        else
        {
            underwatertVolume.SetActive(true);
            UIParticleManager.Instance.playtSpecificParticle(UiWordsParticles.BUBBLE, "");
        }
    }
}
