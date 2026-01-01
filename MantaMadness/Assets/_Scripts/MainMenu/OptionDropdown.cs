using TMPro;
using UnityEngine;

public class OptionDropdown : IOptionItem
{
    [SerializeField] private TMP_Dropdown dropdown;

    public override void Select()
    {
        base.Select();
        dropdown.interactable = false;
    }

    public override void Deselect()
    {
        base.Deselect();
        ForceExitEdit();
    }

    public override void Submit()
    {
        if (!isEditing)
        {
            // Entrer en édition
            isEditing = true;
            dropdown.interactable = true;
            dropdown.Show(); // 🔥 OBLIGATOIRE
        }
        else
        {
            // Sortir d’édition
            ExitEdit();
        }
    }

    public override void Cancel()
    {
        if (!isEditing)
            return;

        ExitEdit();
    }

    private void ExitEdit()
    {
        isEditing = false;
        dropdown.Hide();           // 🔥 TRÈS IMPORTANT
        dropdown.interactable = false;
    }

    public override void Increase()
    {

    }

    public override void Decrease()
    {

    }

    public override void ForceExitEdit()
    {
        base.ForceExitEdit();
        dropdown.Hide();
        dropdown.interactable = false;
    }
    public override void OnNavigateUp()
    {
        if (!isEditing) return;
        dropdown.value = Mathf.Max(0, dropdown.value - 1);
    }

    public override void OnNavigateDown()
    {
        if (!isEditing) return;
        dropdown.value = Mathf.Min(dropdown.options.Count - 1, dropdown.value + 1);
    }
}


