using System;
using UnityEngine;

public class PoulpsRelay : MonoBehaviour
{
    public Action AnimationTriggerAction;

    private void AnimationTrigger()
    {
        AnimationTriggerAction.Invoke();
    }
}
