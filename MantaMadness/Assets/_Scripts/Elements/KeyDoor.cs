using DG.Tweening;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public enum DoorType
{
    Clam,
    Key,
}

public class KeyDoor : MonoBehaviour, IDataPersistence
{
    [SerializeField] private CollisionRelay relay;

    [SerializeField] private string doorID;
    [SerializeField] private string requiredPickupName;
    [SerializeField] private int requiredClamAmount;

    private bool isOpen = false;

    [SerializeField] private GameObject[] deactivateOnOpen;

    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private CinemachineBlendDefinition blend;
    [SerializeField] private GameObject keyVisual;
    [SerializeField] private Transform lockPosition;
    [SerializeField] private ParticleSystem particles;

    public DoorType doorType = DoorType.Key;

    [SerializeField] private TextMeshProUGUI clamText;

    public void LoadData(GameData data)
    {
        data.doorsOpened.TryGetValue(doorID, out isOpen);

        if (isOpen)
        {
            ApplyOpenState();
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data.doorsOpened.ContainsKey(doorID))
        {
            data.doorsOpened.Remove(doorID);
        }

        data.doorsOpened.Add(doorID, isOpen);
    }

    private void Start()
    {
        relay.HitCollision += OnPlayerTouchDoor;

        keyVisual.SetActive(false);

        if(doorType == DoorType.Clam)
        {
            SetClamPriceText();
        }
    }

    private void OnDisable()
    {
        relay.HitCollision -= OnPlayerTouchDoor;
    }

    private void OnPlayerTouchDoor(SimpleController player)
    {
        if(doorType == DoorType.Key)
        {
            if (HasKey() && !isOpen)
            {
                OpenDoor();
            }
            else
            {
                Debug.Log("Door locked, missing key : " + requiredPickupName);
            }
        }

        else if(doorType == DoorType.Clam)
        {
            if (CoinManager.Instance.ClamCollectibleCount >= requiredClamAmount)
            {
                OpenDoor();
                CoinManager.Instance.ClamCollectibleCount -= requiredClamAmount;
            }
            else
            {
                Debug.Log("Not enough clams, you need : " + requiredClamAmount + " clams");
            }
        }

    }

    private bool HasKey()
    {

        bool hasKey = false;
        DataPersistenceManager.Instance.gameData.specialPickups.TryGetValue(requiredPickupName, out hasKey);

        return hasKey;
    }

    private void OpenDoor()
    {
        //foreach(GameObject obj in deactivateOnOpen)
        //{
        //    obj.SetActive(false);
        //}

        //CinematicManager.instance.cinematicPlayer.stopped += OnCinematicFinished;
        //CinematicManager.instance.PlayCinematic(doorCinematic);

        StartCoroutine(OpenDoorSequence());
        Game.Instance.player.ForceLock(true);

        isOpen = true;
    }

    private void ApplyOpenState()
    {
        foreach (GameObject obj in deactivateOnOpen)
        {
            obj.SetActive(false);
        }
    }

    private IEnumerator OpenDoorSequence()
    {
        Vector3 camStartPos = vcam.transform.position;
        Vector3 startPos = keyVisual.transform.position;
        Vector3 backPos = startPos + Vector3.back * 1f;      // recule de 1 unité
        Vector3 impactPos = startPos + Vector3.forward * 3f; // avance rapidement

        //Activate cam
        CameraManager.Instance.BlendToCamera(vcam, blend);

        //Tween cam movement
        Tween camTween = vcam.transform.DOMove(new Vector3(camStartPos.x, camStartPos.y, camStartPos.z + 5f), 2f).SetEase(Ease.OutQuad);

        yield return camTween.WaitForCompletion();

        keyVisual.SetActive(true);
        keyVisual.transform.localScale = Vector3.zero;

        Tween keyScaleTween = keyVisual.transform.DOScale(Vector3.one, 0.75f).SetEase(Ease.OutElastic);

        yield return keyScaleTween.WaitForCompletion();

        Sequence seq = DOTween.Sequence();

        seq.Append(
            keyVisual.transform.DOMove(
                keyVisual.transform.position + Vector3.back * 0.5f,
                0.25f
            ).SetEase(Ease.OutBack)
        );

        seq.Append(
            keyVisual.transform.DOMove(lockPosition.position, 0.2f)
                .SetEase(Ease.InExpo)
        );

        yield return seq.WaitForCompletion();

        keyVisual.SetActive(false);
        particles.Play();

        foreach (GameObject obj in deactivateOnOpen)
        {
            obj.SetActive(false);
        }

        yield return new WaitForSeconds(1.1f);

        OnCinematicFinished();

        yield return null;
    }

    private void OnCinematicFinished()
    {
        foreach (GameObject obj in deactivateOnOpen)
        {
            obj.SetActive(false);
        }

        Game.Instance.player.ForceLock(false);

        CameraManager.Instance.ResetCamera(vcam);
        //MantaCameraController.instance.ActivatePlayerCamera();
    }

    private void SetClamPriceText()
    {
        clamText.text = requiredClamAmount.ToString() + " clams";
    }
}
