using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public GameData Load()
    {
        // use Path.Combine to account to different OS's having different path separators
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                //Load serialized data from file
                string dataToLoad = "";
                using (FileStream stream = new  FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }

                }

                //Deserialize the data from Json back into C# object
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch(Exception e)
            {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + " \n" + e);
            }
        }
        return loadedData;

    }

    public void Save(GameData data)
    {
        // use Path.Combine to account to different OS's having different path separators
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            // Create the directory path if it doesn't exist yet
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            //Serialize the C# game data object into a Json
            string dataToStore = JsonUtility.ToJson(data, true);

            //write the serialized data to the file
            using(FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using(StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine(dataToStore);
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogError("Error Occured when trying to save data to file:" + fullPath + "\n" + e);
        }
    }
}
