using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class FOVController : MonoBehaviour
{   
    public static FOVController instance;

    [SerializeField] private List<CinemachineCamera> playerCameras = new List<CinemachineCamera>();

    public enum FovEffectType
    {
        EXPLOSIF,
        FAST,
        MEDIUM,
        SLOW,
        STOMP,
        BOOST,
        SUPERBOOST,
        STOMPLAND,
    }


    private CinemachineBrain brain;
    private CinemachineCamera current;

    [Header("Parameters")]
    public float maxAvatarSpeed = 10f;
    public float maxFOV = 80f;
    public float speedOnRail = 75f;
    public AnimationCurve FOVProgression;

    private float defaultFOV;
    private float currentFOV;

    private SimpleController controller;
    private bool initialized = false;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

            controller = GetComponent<SimpleController>();
        if (controller == null)
        {
            Debug.LogError("SimpleController manquant sur le GameObject");
        }
    }

    void Start()
    {
        StartCoroutine(InitializeCinemachine());
    }

    bool IsPlayerCamera(CinemachineCamera cam)
    {
        return playerCameras.Contains(cam);
    }

    System.Collections.IEnumerator InitializeCinemachine()
    {
        while (Camera.main == null)
            yield return null;

        brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            Debug.LogError("CinemachineBrain introuvable sur la MainCamera");
            yield break;
        }

        while (brain.ActiveVirtualCamera == null)
            yield return null;

        current = brain.ActiveVirtualCamera as CinemachineCamera;
        if (current == null)
        {
            Debug.LogError("La caméra active n'est pas une CinemachineCamera");
            yield break;
        }

        defaultFOV = current.Lens.FieldOfView;
        currentFOV = defaultFOV;
        initialized = true;
    }

    void Update()
    {
        if (!initialized || controller == null)
            return;

        var cam = brain.ActiveVirtualCamera as CinemachineCamera;
        if (cam == null) return;

        if (!IsPlayerCamera(cam))
            return;

        if (cam != current)
        {
            float previousFov = current != null ? current.Lens.FieldOfView : defaultFOV;

            if (current != null)
                current.Lens.FieldOfView = defaultFOV;

            cam.Lens.FieldOfView = previousFov;

            current = cam;
        }

        if (FovEffectRoutine == null)
        {

            Vector3 horizontalVel = controller.Velocity;
            horizontalVel.y = 0;

            var magnitude = horizontalVel.magnitude;

            if (controller.OnRail)
                magnitude = speedOnRail;

            float speed01 = magnitude / maxAvatarSpeed;

            float targetFOV = Mathf.Lerp(
                defaultFOV,
                maxFOV,
                Mathf.Clamp01(FOVProgression.Evaluate(speed01)));

            currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime);

            if (current != null)
            {
                current.Lens.FieldOfView = currentFOV;
            }
        }
    }

    public void FOVEffect(FovEffectType type)
    {

        var cam = brain.ActiveVirtualCamera as CinemachineCamera;

        if (cam == null || !IsPlayerCamera(cam))
            return;

        if (FovEffectRoutine == null)
        {
            switch (type)
            {
                case FovEffectType.EXPLOSIF:
                    FovEffectRoutine = StartCoroutine(FovEffectCoroutine(120, 1.2f));
                    break;
                case FovEffectType.STOMP:
                    FovEffectRoutine = StartCoroutine(FovEffectCoroutine(90, 0.5f));
                    break;
                case FovEffectType.BOOST:
                    FovEffectRoutine = StartCoroutine(FovEffectCoroutine(120, 1.5f));
                    break;
                case FovEffectType.SUPERBOOST:
                    FovEffectRoutine = StartCoroutine(FovEffectCoroutine(150, 1.5f));
                    break;
                case FovEffectType.STOMPLAND:
                    FovEffectRoutine = StartCoroutine(FovEffectCoroutine(30, 0.5f));
                    break;

            }
        }
    }

    private Coroutine FovEffectRoutine;
    private Tween activeTween;
    private IEnumerator FovEffectCoroutine(float targetFov, float duration)
    {
        float upDuration = duration * 0.2f;
        float downDuration = duration * 0.8f;

        // Sécurité si un tween existe déjà
        if (activeTween != null && activeTween.IsActive())
            activeTween.Kill();

        // PHASE 1 — montée explosive
        activeTween = DOTween.To(
            () => current.Lens.FieldOfView,
            x => current.Lens.FieldOfView = x,
            targetFov,
            upDuration
        ).SetEase(Ease.OutQuad);

        yield return activeTween.WaitForCompletion();

        // PHASE 2 — retour vers le FOV dynamique actuel
        activeTween = DOTween.To(
            () => current.Lens.FieldOfView,
            x => current.Lens.FieldOfView = x,
            currentFOV,
            downDuration
        ).SetEase(Ease.InOutQuad);

        yield return activeTween.WaitForCompletion();

        activeTween = null;
        FovEffectRoutine = null;
    }
}