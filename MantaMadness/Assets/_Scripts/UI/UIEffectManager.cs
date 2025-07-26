using UnityEngine;
using System.Collections;
using System;

public class UIEffectManager : MonoBehaviour
{
    public static UIEffectManager Instance;

    public Action GoodAction;
    public Action BadAction;
    public Action<string, string> SpecificAction;

    private void Awake()
    {
       if (UIEffectManager.Instance == null)
        {
            UIEffectManager.Instance = this;
        }
    }

}
