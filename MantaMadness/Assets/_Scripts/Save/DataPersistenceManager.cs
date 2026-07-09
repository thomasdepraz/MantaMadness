using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    public GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    public static DataPersistenceManager Instance { get; private set; }
    private FileDataHandler dataHandler;

    public bool HasGameData()
    {
        return dataHandler.Load() != null;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            //Debug.LogError("More than one Data persistence in the scene.");
            Destroy(this);
        }
        Instance = this;
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
        //Debug.Log(Application.persistentDataPath);
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    private void Start()
    {
        Debug.Log("Persistent data path is" + Application.persistentDataPath);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveGame();
        }
    }

    public void NewGame(bool forceNewGame = false)
    {
        if (HasGameData() && !forceNewGame)
        {
            Debug.Log("Save exists, confirmation required.");
            return;
        }

        Debug.Log("Creating new game data.");
        this.gameData = new GameData();
    }

    public void LoadGame()
    {
        //Load any saved data from a file using the data handler
        this.gameData = dataHandler.Load();

        // if no data can be loaded, initialize to a new game
        if (this.gameData == null)
        {
            Debug.Log("No data was found, Init to default");
            NewGame();
        }
        //push the data to all scripts that needs it

        dataPersistenceObjects = FindAllDataPersistenceObjects();

        foreach (IDataPersistence dataPersistObj in dataPersistenceObjects)
        {
            dataPersistObj.LoadData(gameData);
        }

        Debug.Log("Loaded clam count= " + gameData.clamCount);
    }

    public void SaveGame()
    {
        dataPersistenceObjects = FindAllDataPersistenceObjects();

        foreach (IDataPersistence dataPersistObj in dataPersistenceObjects)
        {
            dataPersistObj.SaveData(ref gameData);
        }

        dataHandler.Save(gameData);
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None).OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    public void DeleteSave()
    {
        dataHandler.Delete();
        gameData = null;
    }
}
