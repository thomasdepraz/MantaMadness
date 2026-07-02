using FMODUnity;
using UnityEngine;
using System.Collections;

public class SpecialButton : Button
{
    protected SpecialButtonManager manager;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            controller.Bump(new Vector3(0, 1, 1));
            if (isActivated == false && routine == null)
            {
                routine = StartCoroutine(ActivationCoroutine(controller));
            }
        }
    }

    public void SetManager(SpecialButtonManager newManager)
    {
        manager = newManager;

    }
    protected override IEnumerator ActivationCoroutine(SimpleController controller)
    {
        //manager.RegisterDestruction(this);
        isActivated = true;
        buttonMesh.material = activatedMaterial;

        RuntimeManager.PlayOneShot(buttonStinger, Camera.main.transform.position);
        yield return null;
    }
}
