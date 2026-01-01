using UnityEngine;

public abstract class ConfirmOption : MonoBehaviour
{
    [SerializeField] private GameObject cursor;

    public virtual void Select()
    {
        cursor.SetActive(true);
    }

    public virtual void Deselect()
    {
        cursor.SetActive(false);
    }

    public abstract void Submit();
}
