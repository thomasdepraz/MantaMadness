using UnityEngine;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;

[System.Serializable]
public class FMODGlobalParamInfo
{
    public string name;
    public float min;
    public float max;
    public float defaultValue;
    public float value;
}

public enum FmodGlobalParamName
{
    G_Player_Drift,
    G_Player_Flying,
    G_Player_Life,
    G_Player_Speed,
    G_Player_StyleState,
    G_Player_TurnAngle,
    G_Player_Underwater,
    G_SecretRoom,
    G_Warping,
    G_Player_Fever
}

public class FmodGlobalParameters : MonoBehaviour
{
    [HideInInspector]public static FmodGlobalParameters instance;

    public List<FMODGlobalParamInfo> globalParameters = new List<FMODGlobalParamInfo>();

    public int selectedIndex = 0;
    public string selectedParameterName
    {
        get
        {
            if (globalParameters == null || globalParameters.Count == 0) return string.Empty;
            return globalParameters[Mathf.Clamp(selectedIndex, 0, globalParameters.Count - 1)].name;
        }
    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {

        LoadGlobalParameters();
    }

    public void LoadGlobalParameters()
    {
        globalParameters.Clear();

        var system = RuntimeManager.StudioSystem;

        system.getParameterDescriptionCount(out int count);

        for (int i = 0; i < count; i++)
        {
            system.getParameterDescriptionList(out PARAMETER_DESCRIPTION[] paramDesc);

            if (paramDesc[i].type == PARAMETER_TYPE.GAME_CONTROLLED)
            {
                globalParameters.Add(new FMODGlobalParamInfo
                {
                    name = paramDesc[i].name,
                    min = paramDesc[i].minimum,
                    max = paramDesc[i].maximum,
                    defaultValue = paramDesc[i].defaultvalue,
                    value = paramDesc[i].defaultvalue,
                });
                //print(globalParameters[i].name);
            }
        }
        if(globalParameters.Count == 0)
        {
            Debug.LogWarning("Aucun paramètre global trouvé dans FMOD.");
        }

        Debug.Log($"Charge {globalParameters.Count} global parameters from FMOD");
    }

    public void ToggleGlobalParameter(FmodGlobalParamName paramName)
    {
        for (int i = 0; i < globalParameters.Count; i++)
        {
            if (globalParameters[i].name == paramName.ToString())
            {
                if (globalParameters[i].value == 0)
                {
                    globalParameters[i].value = 1;
                    SetParameter(globalParameters[i]);
                    //print("Toggle value is" + globalParameters[i].value);
                }
                else if (globalParameters[i].value == 1)
                {
                    globalParameters[i].value = 0;
                    SetParameter(globalParameters[i]);
                    //print("Toggle value is" + globalParameters[i].value);
                }
            }
        }
    }

    public void SetGlobalParameter(FmodGlobalParamName paramName, float value)
    {
        for (int i = 0; i < globalParameters.Count; i++)
        {
            if (globalParameters[i].name == paramName.ToString())
            {
                globalParameters[i].value = value;
                SetParameter(globalParameters[i]);
            }
        }
    }

    void SetParameter(FMODGlobalParamInfo param)
    {
        RuntimeManager.StudioSystem.setParameterByName(param.name, param.value);
        //print(param.name + "Value = " + param.value);
    }
}
