using UnityEngine;
using DG.Tweening;
using System.Collections;
using TMPro;
using EasyTextEffects;

public class AreaNameDisplay : MonoBehaviour
{
    [SerializeField] private RectTransform startPosition;
    [SerializeField] private RectTransform endPosition;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextEffect textEffects;
    public IEnumerator displayCoroutine(string name)
    {
        text.text = name;
        yield return null;
    }
}
