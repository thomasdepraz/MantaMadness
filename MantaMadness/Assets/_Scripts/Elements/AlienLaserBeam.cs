using DG.Tweening;
using UnityEngine;

public class AlienLaserBeam : MonoBehaviour
{
    private int beatCounter = 0;
    private int beatLaserCounter = 0;
    public int laserBeatDuration = 4;
    public int laserFadeOutOffset = 2;
    public int maxBeatValue = 4;
    public int beatOffset = 0;

    [SerializeField] protected Transform startPoint;
    [SerializeField] protected Transform endPoint;
    [SerializeField] protected float radius = 0.5f;
    [SerializeField] protected LayerMask playerLayer;

    private bool isFiring = false;
    private bool isEnding = false;

    public GameObject laserChargeVisual;
    public GameObject laserBeam;
    public GameObject laserChargeDecal;
    protected Vector3 laserBeamOriginalScale;
    protected Vector3 laserChargeVisualOriginalScale;
    protected Vector3 laserChargeDecalOriginalScale;

    private const float MIN_SCALE_Y = 0.01f;

    protected virtual void Start()
    {
        laserBeamOriginalScale = laserBeam.transform.localScale;
        laserChargeVisualOriginalScale = laserChargeVisual.transform.localScale;
        laserChargeDecalOriginalScale = laserChargeDecal.transform.localScale;

        ResetChargeVisual();
        ResetLaser();

        beatCounter = 0;
        beatLaserCounter = 0;
    }

    protected virtual void OnEnable()
    {
        MusicManager.OnBeat += IncreaseBeatCounter;
    }

    protected virtual void OnDisable()
    {
        MusicManager.OnBeat -= IncreaseBeatCounter;
    }

    private void IncreaseBeatCounter(int bar, int beat, float tempo)
    {
        if (Time.timeScale == 0f)
            return;

        int beatsPerCycle = maxBeatValue + laserBeatDuration + laserFadeOutOffset;

        // Si ton beat commence à 1 au lieu de 0, utilise (beat - 1)
        int globalBeat = bar * 4 + beat;

        // Offset positif = laser en avance
        int cycle = (globalBeat + beatOffset) % beatsPerCycle;

        if (cycle < maxBeatValue)
        {
            beatCounter = cycle;
            beatLaserCounter = 0;

            if (isFiring || isEnding)
            {
                ResetLaser();
                isFiring = false;
                isEnding = false;
            }

            UpdateChargeLaser(beatCounter, tempo);
        }
        else if (cycle < maxBeatValue + laserBeatDuration)
        {
            beatCounter = maxBeatValue;
            beatLaserCounter = cycle - maxBeatValue;

            if (!isFiring)
            {
                ResetChargeVisual();
                FireLaser();
            }
        }
        else
        {
            beatCounter = maxBeatValue;
            beatLaserCounter = cycle - maxBeatValue;

            UpdateLaser(tempo);
        }
    }

    private void UpdateChargeLaser(int beatCounter, float tempo)
    {
        float beatDuration = 60f / tempo;

        float progress = (float)beatCounter / maxBeatValue;
        progress = Mathf.Clamp01(progress);

        Vector3 minScale = new Vector3(1f, 0.01f, 1f);
        Vector3 minScaleDecal = new Vector3(0.1f,0.1f, laserChargeDecalOriginalScale.z);

        Vector3 targetScaleVisual = Vector3.Lerp(minScale, laserChargeVisualOriginalScale, progress);
        Vector3 targetScaleDecal = Vector3.Lerp(minScaleDecal, laserChargeDecalOriginalScale, progress);

        // Activation si nécessaire
        if (!laserChargeVisual.activeSelf)
        {
            laserChargeVisual.SetActive(true);
            laserChargeVisual.transform.localScale = minScale;
        }

        if (!laserChargeDecal.activeSelf)
        {
            laserChargeDecal.SetActive(true);
            laserChargeDecal.transform.localScale = minScale;
        }

        // Kill tweens
        laserChargeVisual.transform.DOKill();
        laserChargeDecal.transform.DOKill();

        // Tween
        laserChargeVisual.transform
            .DOScale(targetScaleVisual, beatDuration)
            .SetEase(Ease.OutQuad);

        laserChargeDecal.transform
            .DOScale(targetScaleDecal, beatDuration)
            .SetEase(Ease.OutQuad);
    }

    private void UpdateLaser(float tempo)
    {
        float beatDuration = 60f / tempo;

        if (!isEnding)
        {
            isEnding = true;

            laserBeam.transform.DOKill();

            laserBeam.transform
                .DOScale(new Vector3(0.1f, laserBeamOriginalScale.y, 0.2f), beatDuration * laserFadeOutOffset)
                .SetEase(Ease.Linear);
        }

        if (beatLaserCounter >= laserBeatDuration + laserFadeOutOffset - 1)
        {
            ResetLaser();
            ResetCounters();
        }
    }

    private void ResetChargeVisual()
    {
        laserChargeVisual.transform.DOKill();
        laserChargeDecal.transform.DOKill();

        laserChargeVisual.SetActive(false);
        laserChargeDecal.SetActive(false);
    }

    private void FireLaser()
    {
        isFiring = true;
        isEnding = false;

        laserBeam.transform.DOKill();
        laserBeam.SetActive(true);

        laserBeam.transform.localScale = new Vector3(
            laserBeamOriginalScale.x,
            MIN_SCALE_Y,
            laserBeamOriginalScale.z
        );

        laserBeam.transform
            .DOScaleY(laserBeamOriginalScale.y, 0.1f)
            .SetEase(Ease.Linear);
    }

    private void ResetLaser()
    {
        laserBeam.transform.DOKill();
        laserBeam.SetActive(false);
        laserBeam.transform.localScale = laserBeamOriginalScale;
    }

    private void ResetCounters()
    {
        isFiring = false;
        isEnding = false;
        beatCounter = 0;
        beatLaserCounter = 0;
        hasKilledThisCycle = false;
    }

    private bool hasKilledThisCycle = false;


    private void Update()
    {
        CheckPlayerInLaser();
    }
    protected virtual void CheckPlayerInLaser()
    {
        if (!isFiring || hasKilledThisCycle || isEnding) return;

        Collider[] hits = Physics.OverlapCapsule(startPoint.position, endPoint.position, radius, playerLayer);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out SimpleController controller))
            {
                hasKilledThisCycle = true;
                Game.Instance.player.Kill(DeathType.ELECTROCUTED);
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (startPoint == null || endPoint == null) return;

        Gizmos.color = Color.cyan;

        Vector3 start = startPoint.position;
        Vector3 end = endPoint.position;

        // Sphères aux extrémités
        Gizmos.DrawWireSphere(start, radius);
        Gizmos.DrawWireSphere(end, radius);

        // Direction du laser
        Vector3 direction = (end - start).normalized;

        // Trouver un vecteur perpendiculaire pour dessiner les côtés
        Vector3 offset = Vector3.Cross(direction, Vector3.up) * radius;

        // Si direction ~ vertical, fallback
        if (offset == Vector3.zero)
            offset = Vector3.Cross(direction, Vector3.right) * radius;

        // Lignes de la capsule
        Gizmos.DrawLine(start + offset, end + offset);
        Gizmos.DrawLine(start - offset, end - offset);

        // Optionnel : autre axe pour meilleure lisibilité
        Vector3 offset2 = Vector3.Cross(direction, offset).normalized * radius;

        Gizmos.DrawLine(start + offset2, end + offset2);
        Gizmos.DrawLine(start - offset2, end - offset2);
    }
}
