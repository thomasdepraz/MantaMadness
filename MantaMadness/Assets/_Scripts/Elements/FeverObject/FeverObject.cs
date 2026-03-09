using System.Collections;
using UnityEngine;

public enum FeverObjectType
{
    OnRange,
    OnFever,
}

public class FeverObject : MonoBehaviour
{
    protected SimpleController player;

    [SerializeField] FeverObjectType type;

    public FeverObjectType Type => type;

    [Header("Collision Layer")]
    [SerializeField] LayerMask playerMask;

    protected bool feverActive;

    protected virtual void OnEnable()
    {
        StartCoroutine(DelaySetup());
    }

    protected virtual IEnumerator DelaySetup()
    {
        yield return new WaitForSeconds(0.05f);

        player = Game.Instance.player;

        if (type == FeverObjectType.OnFever)
        {
            ComboManager.Instance.OnFeverStarted += OnFeverEnabled;
        }
    }

    protected virtual void OnDisable()
    {
        if (type == FeverObjectType.OnFever && ComboManager.Instance != null)
        {
            ComboManager.Instance.OnFeverStarted -= OnFeverEnabled;
        }
    }

    protected virtual void Update()
    {
        feverActive = ComboManager.Instance.State == ComboState.Fever;
    }

    protected virtual void OnFeverEnabled()
    {

    }

    public virtual void OnFeverRange()
    {

    }

    public virtual void OnFeverReset()
    {

    }
}