using UnityEngine;

public interface IScreen
{
    GameObject Container { get; }

    public void Show() => ScreenUtility.Toggle(this, true);

    public void Hide() => ScreenUtility.Toggle(this, false);
}
