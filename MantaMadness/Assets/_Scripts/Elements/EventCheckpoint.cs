using DG.Tweening;
using UnityEngine;
using System.Collections;

public enum ZoneState
{
    Inactive,
    Active,
    Disabled
}

public class EventCheckpoint : WorldCheckpoint, IDataPersistence
{
    [SerializeField] private string zoneID;

    [SerializeField] private GameObject[] toDeactivate;
    [SerializeField] private GameObject[] toActivate;
    [SerializeField] private float spawnDelay = 0.2f;
    [SerializeField] private float moveOffset = 5f;

    private ZoneState state = ZoneState.Inactive;

    protected override void Start()
    {
        base.Start();
        ApplyState();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (state != ZoneState.Inactive) return;

        if (other.TryGetComponent(out SimpleController controller))
        {
            base.OnTriggerEnter(other);
            StartCoroutine(ActivationRoutine());
        }
    }

    private IEnumerator ActivationRoutine()
    {
        state = ZoneState.Active;

        Game.Instance.player.ForceLock(true);
        Game.Instance.player.RailLock(true);

        foreach (var obj in toDeactivate)
            if (obj != null) obj.SetActive(false);

        foreach (var obj in toActivate)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                obj.transform.DOMoveY(obj.transform.position.y + moveOffset, 0.2f)
                    .SetEase(Ease.OutQuad)
                    .SetLoops(2, LoopType.Yoyo);
            }

            yield return new WaitForSeconds(spawnDelay);
        }


        Game.Instance.player.ForceLock(false);
        Game.Instance.player.RailLock(false);
    }

    public void LoadData(GameData data)
    {
        if (data.puzzleElements.ContainsKey(zoneID))
        {
            state = data.puzzleElements[zoneID] ? ZoneState.Active : ZoneState.Disabled;
        }
        else
        {
            state = ZoneState.Inactive;
            data.puzzleElements.Add(zoneID, false);
        }

        ApplyState();
    }

    public void SaveData(ref GameData data)
    {
        if (data.puzzleElements.ContainsKey(zoneID))
            data.puzzleElements[zoneID] = (state == ZoneState.Active);
        else
            data.puzzleElements.Add(zoneID, (state == ZoneState.Active));
    }

    private void ApplyState()
    {
        if (state == ZoneState.Active)
        {
            foreach (var obj in toActivate)
                if (obj != null) obj.SetActive(true);

            foreach (var obj in toDeactivate)
                if (obj != null) obj.SetActive(false);
        }
        else
        {
            foreach (var obj in toActivate)
                if (obj != null) obj.SetActive(false);

            foreach (var obj in toDeactivate)
                if (obj != null) obj.SetActive(true);
        }
    }

    public void DisableZone()
    {
        state = ZoneState.Disabled;
        ApplyState();
        DataPersistenceManager.Instance.SaveGame();
    }

    public override void EnableMat() { }
}