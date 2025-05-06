public class ScreenUtility
{
    public static void Toggle(IScreen screen, bool toggle)
    {
        screen.Container.SetActive(toggle);
    }
}
