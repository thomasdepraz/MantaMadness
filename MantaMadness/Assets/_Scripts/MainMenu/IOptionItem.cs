using UnityEngine;

public abstract class IOptionItem : MonoBehaviour
{
    [Header("Cursor Visual")]
    [SerializeField] private GameObject cursor;

    protected bool isEditing;

    protected virtual void Awake()
    {
        if (cursor != null)
            cursor.SetActive(false);
    }

    public virtual void Select()
    {
        if (cursor != null)
            cursor.SetActive(true);
    }

    public virtual void Deselect()
    {
        if (cursor != null)
            cursor.SetActive(false);
    }
    public virtual bool IsEditing => isEditing;
    public abstract void Increase();
    public abstract void Decrease();

    public abstract void Submit();
    public abstract void Cancel();

    public virtual void ForceExitEdit()
    {
        isEditing = false;
    }

    public virtual void OnNavigateUp() { }
    public virtual void OnNavigateDown() { }
}
