using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BoostGaugeInterface : MonoBehaviour
{
    public Image boostGauge;
    public GameObject speedGauge;
    public GameObject speedNeedle;
    public GameObject[] speedStatesVisuals;
    public GameObject speedStatesTween;

    private SimpleController player;

    private int m_Count = 0;
    private float needleStartRotationZ = 180;
    private bool overdrive = false;
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<SimpleController>();
    }

    private void Start()
    {
        UIManager.Instance.boostGaugeInterface = this;
        boostGauge.fillAmount = 0;

        foreach(GameObject state in speedStatesVisuals)
        {
            if (state == speedStatesVisuals[0])
            {
                state.SetActive(true);
            }
            else
            {
                state.SetActive(false);
            }
        }
        speedNeedle.transform.Rotate(new Vector3(0, 0, needleStartRotationZ));
    }

    private void FixedUpdate()
    {
        if(player.HorizontalVelocity. magnitude <= 10f && speedStatesVisuals[0].activeSelf == false)
        {
            StateChange(0);
        }

        else if (player.HorizontalVelocity.magnitude > 10f && player.HorizontalVelocity.magnitude < player.controllerData.maxSpeed && speedStatesVisuals[1].activeSelf == false)
        {
            StateChange(1);
        }

        else if (player.HorizontalVelocity.magnitude >= player.controllerData.maxSpeed + 1 && speedStatesVisuals[2].activeSelf == false)
        {
            StateChange(2);
        }

        if (player.HorizontalVelocity.magnitude <= player.controllerData.maxSpeed)
        {
            NeedleRotation();
        }
        else if (player.HorizontalVelocity.magnitude > player.controllerData.maxSpeed + 1 && overdrive == false)
        {
            NeedleOverdrive();
        }
    }

    private void StateChange(int index)
    {
        foreach (GameObject state in speedStatesVisuals)
        {
            state.SetActive(false);
        }
        speedStatesVisuals[index].SetActive(true);
    }

    private void NeedleRotation()
    {
        if (overdrive == true)
        {
            overdrive = false;
            speedNeedle.transform.DOKill();
        }
        float clampedVelocity = Mathf.Clamp(player.HorizontalVelocity.magnitude, 0f, 40f);
        float needleZ = Mathf.SmoothStep(180f, 30f , clampedVelocity / player.controllerData.maxSpeed);
        speedNeedle.transform.rotation = Quaternion.Euler(0f, 0f, needleZ);
    }

    private void NeedleOverdrive()
    {
        overdrive = true;
        speedNeedle.transform.DOLocalRotate(new Vector3(0f,0f, -10f),0.05f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
    }

    public void SetGauge(int current, int MaxValue)
    {
        m_Count = current;
        float targetCount = (float)m_Count / (float)MaxValue;

        boostGauge.fillAmount = targetCount;
    }
}
