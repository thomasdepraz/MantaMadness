using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.Timeline;

public class KeyDoor : MonoBehaviour, IDataPersistence
{
    [SerializeField] private CollisionRelay relay;

    [SerializeField] private string doorID;
    [SerializeField] private string requiredPickupName;

    private bool isOpen = false;

    [SerializeField] private GameObject[] deactivateOnOpen;

    [SerializeField] private TimelineAsset doorCinematic;

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
    }

    private void OnDisable()
    {
        relay.HitCollision -= OnPlayerTouchDoor;
    }

    private void OnPlayerTouchDoor(SimpleController player)
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

        CinematicManager.instance.cinematicPlayer.stopped += OnCinematicFinished;
        CinematicManager.instance.PlayCinematic(doorCinematic);

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

    private void OnCinematicFinished(PlayableDirector director)
    {
        CinematicManager.instance.cinematicPlayer.stopped -= OnCinematicFinished;

        foreach (GameObject obj in deactivateOnOpen)
        {
            obj.SetActive(false);
        }

        Game.Instance.player.ForceLock(false);

        CinematicManager.instance.ResetCam();
        MantaCameraController.instance.ActivatePlayerCamera();
    }
}
