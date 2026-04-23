using DG.Tweening;
using UnityEngine;

public class AlienLaserBeam : MonoBehaviour
{
    //ALIEN LASER BEAM SCRIPT:
    //Le script doit faire les choses suivante:
    //
    //Lorsqu'il est actif
    //Au rythme de la music un counteur augment en permanence (public int)
    //AUSSI il faut un parameters pour gerer un potentiel offset du premier beat (pattern alternatif)
    //
    //Lorsque le compteur est plein, le laser doit ensuite sustain pendant X beat (nouveau parametre)
    //
    //Si le joueur rentre en contact avec le laser actif > mort electrocuté
    //
    //Ensuite le laser se desactive
    //
    //Il faut un son / visuel au moment de:
    //Charge des laser (boule qui scale a chaque beat)
    //Tir des lasers
    //Laser qui disparais
    //
    //
    //

    private int beatCounter = 0;
    private int beatLaserCounter = 0;
    public int laserBeatDuration = 4;
    public int laserFadeOutOffset = 2;
    public int maxBeatValue = 4;
    public int beatOffset = 0;

    private bool isFiring = false;
    private bool isEnding = false;

    public GameObject laserChargeVisual;
    public GameObject laserBeam;
    private Vector3 laserBeamOriginalScale;

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
                laserChargeVisual.transform.localScale = Vector3.zero;
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
                .DOScale(new Vector3(0, laserBeamOriginalScale.y, 0), beatDuration * laserFadeOutOffset)
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
            0,
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
    }
}
