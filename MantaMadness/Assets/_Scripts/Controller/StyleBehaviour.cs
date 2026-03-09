using UnityEngine;

public class StyleBehaviour : MonoBehaviour
{
    public LayerMask layerMask;
    private ControllerData controllerData;

    public void Initialize(ControllerData data)
    {
        controllerData = data;
    }

    public void StyleTrigger(Vector3 origin, int combo)
    {
        var colliders = Physics.OverlapSphere(origin, controllerData.styleTriggerRadius, layerMask, QueryTriggerInteraction.UseGlobal);
        if (colliders == null)
            return;

        foreach (var collider in colliders)
        {
            if(collider.gameObject.TryGetComponent(out StyleTriggerable component))
                component.Trigger(combo);
        }

        ComboManager.Instance.AddComboAction(ComboID.RailStyle);
    }
}
