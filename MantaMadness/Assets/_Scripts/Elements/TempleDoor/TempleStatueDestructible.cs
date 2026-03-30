using UnityEngine;
using System.Collections;

public enum TempleStatueType
{
    Blue,
    Red,
    Green,
    Yellow,
}

public class TempleStatueDestructible : SpecialDestructible, IDataPersistence
{

    [SerializeField] public TempleStatueType type;
    [SerializeField] private string id;

    public override void Start()
    {
        if (!isBroken)
        {
            if (visual.activeSelf == false)
                visual.SetActive(true);
        }

        if (remain != null)
        {
            if (remain.activeSelf == true)
                remain.SetActive(false);
        }
    }

    public override IEnumerator DestructionRoutine(Vector3 point)
    {
        yield return StartCoroutine(base.DestructionRoutine(point));

        yield return new WaitForSeconds(0.75f);

        if (manager is TempleDoorManager templeManager)
        {
            templeManager.OnDestructibleDestroyed(this);
        }
    }

    public void LoadData(GameData data)
    {
        if (data.puzzleElements.TryGetValue(id, out bool state))
        {
            isBroken = state;

            if (isBroken)
            {
                DisableDestructible();
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data.puzzleElements.ContainsKey(id))
            data.puzzleElements[id] = isBroken;
        else
            data.puzzleElements.Add(id, isBroken);
    }

    public override void DisableDestructible()
    {
        base.DisableDestructible();
    }
}
