using DG.Tweening;
using System;
using UnityEditor;
using UnityEngine;

public class BuoyVisuals : MonoBehaviour
{
    public GameObject model;
    public MeshRenderer renderer;
    private Material defaultMaterial;
    public Material completedMaterial;

    private void Start()
    {
        Buoy buoy = GetComponentInParent<Buoy>();
        buoy.onCollect += OnCollect;
        buoy.onReset += OnReset;
        defaultMaterial = renderer.materials[0];
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
