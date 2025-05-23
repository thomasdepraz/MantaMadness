using System;

public class SceneEventsService : ISceneEventsService
{
    public event Action OnAwake;
    public event Action OnStart;
    public event Action OnClean;
    public event Action OnUpdate;

    public void TriggerOnAwake()
    {
        OnAwake?.Invoke();
    }

    public void TriggerOnClean()
    {
        OnClean?.Invoke();
    }

    public void TriggerOnStart()
    {
        OnStart?.Invoke();
    }

    public void TriggerOnUpdate()
    {
        OnUpdate?.Invoke();
    }
}