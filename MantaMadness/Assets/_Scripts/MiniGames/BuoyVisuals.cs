using DG.Tweening;
using UnityEngine;

public class BuoyVisuals : MonoBehaviour
{
    public GameObject model;
    public new MeshRenderer renderer;
    public Material defaultMaterial;
    public Material completedMaterial;

    private void Start()
    {
        Buoy buoy = GetComponentInParent<Buoy>();
        buoy.onCollect += OnCollect;
        buoy.onReset += OnReset;
    }

    public void SetCompleted(bool completed)
    {
        renderer.material = completed ? completedMaterial : defaultMaterial;
    }

    private void OnReset()
    {
        renderer.material = defaultMaterial;
    }

    private void OnCollect()
    {
        renderer.material = completedMaterial;
    }

    private void OnCollisionEnter(Collision collision)
    {
        model.transform.DOPunchScale(Vector3.one, 0.7f, 8, 1);
    }
}
