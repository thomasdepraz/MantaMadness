using UnityEngine;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    public GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    public static DataPersistenceManager Instance { get; private set; }
    private FileDataHandler dataHandler;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one Data persistence in the scene.");
        }
        Instance = this;
    }

    private void Start()
    {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
        Debug.Log(Application.persistentDataPath);
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public void NewGame()
    {
        //IF SOME GAME DATA ALLREADY
        //TODO - Ask if player wants to delete it

        //If no game data already
        this.gameData = new GameData();
    }

    public void LoadGame()
    {
        //Load any saved data from a file using the data handler
        this.gameData = dataHandler.Load();

        // if no data can be loaded, initialize to a new game
        if(this.gameData == null)
        {
            Debug.Log("No data was found, Init to default");
            NewGame();
        }
        //push the data to all scripts that needs it

        foreach(IDataPersistence dataPersistObj in dataPersistenceObjects)
        {
            dataPersistObj.LoadData(gameData);
        }

        Debug.Log("Loaded clam count= " + gameData.clamCount);
    }

    public void SaveGame()
    {
        //pass the data to other script so they can update it
        foreach (IDataPersistence dataPersistObj in dataPersistenceObjects)
        {
            dataPersistObj.SaveData(ref gameData);
        }

        //save that data to a file using the data handler
        dataHandler.Save(gameData);
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID).OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}
