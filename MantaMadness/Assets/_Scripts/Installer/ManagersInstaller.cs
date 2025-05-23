using System;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "ManagersInstaller", menuName = "Installers/ManagersInstaller")]
public class ManagersInstaller : ScriptableObjectInstaller<ManagersInstaller>
{
    public override void InstallBindings()
    {
        Container.Bind<ISceneEventsService>().To<SceneEventsService>().AsSingle();
    }
}

public interface IGameService
{

}

public class GameService : IGameService
{

    //Cache
    private ISceneEventsService m_SceneEventsService;


    [Inject]
    public void Construct(ISceneEventsService sceneEventsService)
    {
        m_SceneEventsService = sceneEventsService;
        sceneEventsService.OnAwake += OnAwake;
        sceneEventsService.OnStart += OnStart;

        Application.targetFrameRate = 60;
    }

    private void OnAwake()
    {
        throw new NotImplementedException();
    }

    private void OnStart()
    {
        throw new NotImplementedException();
    }

        
}

public interface ISaveService
{
    public void Save();
    public void Load();
}

public class SaveService : ISaveService
{
    //save data path
    const string c_RaceDataPath = "/RaceData.json";
    const string c_MiniGameDataPath = "/MiniGameData.json";

    //cache
    ISceneEventsService m_SceneEventService;

    [Inject]
    public void Construct(ISceneEventsService sceneEventsService)
    {
        m_SceneEventService = sceneEventsService;
        m_SceneEventService.OnAwake += OnAwake;
    }

    private void OnAwake()
    {
        
    }

    public void Load()
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        
    }
}

public interface ISaveData<T>
{
    public T GetSaveData();
}

[System.Serializable]
public class RaceData : ISaveData<RaceData>
{
    public RaceData GetSaveData()
    {
        //fill with data
        return new RaceData();
    }
}

[System.Serializable]
public class MiniGameData : ISaveData<MiniGameData>
{
    public MiniGameData GetSaveData()
    {
        //fill with data
        return new MiniGameData();
    }
}
