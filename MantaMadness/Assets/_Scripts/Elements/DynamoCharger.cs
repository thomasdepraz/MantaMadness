using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class DynamoCharger : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 5f;

    [Header("Visuals")]
    [SerializeField] private Transform gear;
    [SerializeField] private float spinDuration = 0.3f;
    [SerializeField] private float spinSpeed = 360f;
    [SerializeField] private ParticleSystem vfx;

    [SerializeField] private Transform chargePoint;

    [SerializeField] private Transform chargeVisual;
    [SerializeField] private Vector3 chargedScale = Vector3.one * 2f;
    [SerializeField] private float cooldownDuration = 5f;

    private float chargedDuration;
    private Vector3 baseScale;

    private SimpleController player;
    private Tween gearTween;
    private bool active;

    private void Start()
    {
        if (chargeVisual != null)
            baseScale = new Vector3(1f, 0f, 1f);

        chargeVisual.localScale = baseScale;

        chargedDuration = Game.Instance.player.electricBehaviour.chargeDuration;
    }

    void Update()
    {
        if (chargeRoutine != null)
            return;

        DetectPlayer();

        if (player != null && player.IsSpinning)
        {
            chargeRoutine = StartCoroutine(ChargePlayer(player));
        }
    }

    void DetectPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);

        player = null;

        foreach (var hit in hits)
        {
            SimpleController controller = hit.GetComponent<SimpleController>();

            if (controller != null)
            {
                player = controller;
                break;
            }
        }
    }

    void StartDynamo()
    {
        if (active) return;

        active = true;

        if (gear != null)
        {
            gearTween = gear
                .DORotate(new Vector3(0, spinSpeed, 0), spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1);
        }

        if (vfx != null)
            vfx.Play();
    }

    void StopDynamo()
    {
        if (!active) return;

        active = false;

        gearTween?.Kill();

        if (vfx != null)
            vfx.Stop();
    }

    private void OnDisable()
    {
        StopDynamo();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    public Coroutine chargeRoutine;

    private IEnumerator ChargePlayer(SimpleController player)
    {
        player.electricBehaviour.SetActiveDynamo(this);

        player.ForceLock(true);

        player.transform.position = chargePoint.position;
        player.transform.rotation = chargePoint.rotation;

        player.SetForcedSpin(true);

        StartDynamo();

        while (!player.electricBehaviour.IsCharged)
        {
            if (chargeVisual != null)
            {
                chargeVisual.localScale = Vector3.Lerp(
                    baseScale,
                    chargedScale,
                    player.electricBehaviour.ChargeRatio
                );
            }

            yield return null;
        }

        StopDynamo();

        player.CancelSpinFromDynamo();
        player.electricBehaviour.ClearActiveDynamo(this);

        yield return new WaitForSeconds(chargedDuration);

        //if (chargeVisual != null)
        //    chargeVisual.localScale = baseScale;

        yield return new WaitForSeconds(cooldownDuration);

        if (chargeVisual != null)
            chargeVisual.localScale = baseScale;

        chargeRoutine = null;
    }
}