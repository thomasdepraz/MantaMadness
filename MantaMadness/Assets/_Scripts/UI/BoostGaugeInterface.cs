using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BoostGaugeInterface : MonoBehaviour
{
    public Image boostGauge;

    private int m_Count = 0;

    private void Start()
    {
        UIManager.Instance.boostGaugeInterface = this;
        boostGauge.fillAmount = 0;
    }

    public void SetGauge(int current, int MaxValue)
    {
        m_Count = current;
        float targetCount = (float)m_Count / (float)MaxValue;

        boostGauge.fillAmount = targetCount;
    }
}
