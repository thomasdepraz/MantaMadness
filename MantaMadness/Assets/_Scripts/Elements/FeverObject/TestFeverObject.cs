using System.Collections;
using UnityEngine;

public class TestFeverObject : FeverObject
{
    [SerializeField] private UnityEngine.GameObject defaultVisual;
    [SerializeField] private UnityEngine.GameObject feverVisual;

    private void Start()
    {
        defaultVisual.SetActive(true);
        feverVisual.SetActive(false);
    }

    public override void OnFeverRange()
    {
        feverVisual.SetActive(true);
        defaultVisual.SetActive(false);
    }
}
