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

    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private LayerMask playerLayer;

    private bool isFiring = false;
    private bool isEnding = false;

    public GameObject laserChargeVisual;
    public GameObject laserBeam;
    private Vector3 laserBeamOriginalScale;

    private const float MIN_SCALE_Y = 0.01f;

    private void Start()
    {
        laserBeamOriginalScale = laserBeam.transform.localScale;

        ResetChargeVisual();
        ResetLaser();

        beatCounter = 0;
        beatLaserCounter = 0;
    }

    private void OnEnable()
    {
        MusicManager.OnBeat += IncreaseBeatCounter;
    }

    private void OnDisable()
    {
        MusicManager.OnBeat -= IncreaseBeatCounter;
    }

    private void IncreaseBeatCounter(int bar, int beat, float tempo)
    {
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

        if (beatCounter >= maxBeatValue)
        {
            ResetChargeVisual();
            FireLaser();
        }
        else
        {
            if (laserChargeVisual.activeSelf)
            {
                laserChargeVisual.transform.DOKill();
                laserChargeVisual.transform
                    .DOScale(laserChargeVisual.transform.localScale * 1.5f, beatDuration / 2)
                    .SetEase(Ease.OutQuad);
            }
            else
            {
                laserChargeVisual.SetActive(true);
                laserChargeVisual.transform.localScale = new Vector3(1,0.01f,1f);
                laserChargeVisual.transform
                    .DOScale(Vector3.one, beatDuration)
                    .SetEase(Ease.OutQuad);
            }
        }
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
        laserChargeVisual.SetActive(false);
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
    private void CheckPlayerInLaser()
    {
        if (!isFiring || hasKilledThisCycle) return;

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
