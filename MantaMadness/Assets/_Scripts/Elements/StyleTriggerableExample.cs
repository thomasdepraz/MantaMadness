using UnityEngine;

public class StyleTriggerableExample : StyleTriggerable
{
    public override void Trigger(int combo)
    {
        Debug.Log($"{gameObject.name} trigger by style. combo is {combo}");

        //Do something custom
    }
}
