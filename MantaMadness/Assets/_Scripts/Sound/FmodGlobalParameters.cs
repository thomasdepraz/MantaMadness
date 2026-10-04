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
    private const int PARAM_SLOT_COUNT = 16;
    private const float SPEED_CHANGE_EPSILON = 0.02f;
    private const float DISCRETE_CHANGE_EPSILON = 0.0001f;

    [HideInInspector]public static FmodGlobalParameters instance;

    public List<FMODGlobalParamInfo> globalParameters = new List<FMODGlobalParamInfo>();

    private readonly PARAMETER_ID[] _parameterIds = new PARAMETER_ID[PARAM_SLOT_COUNT];
    private readonly bool[] _hasParameterId = new bool[PARAM_SLOT_COUNT];
    private readonly float[] _lastSentValue = new float[PARAM_SLOT_COUNT];
    private readonly bool[] _hasSentValue = new bool[PARAM_SLOT_COUNT];
    private readonly int[] _parameterListIndex = new int[PARAM_SLOT_COUNT];

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

        for (int slot = 0; slot < PARAM_SLOT_COUNT; slot++)
        {
            _hasParameterId[slot] = false;
            _hasSentValue[slot] = false;
            _parameterListIndex[slot] = -1;
        }

        var system = RuntimeManager.StudioSystem;

        system.getParameterDescriptionCount(out int count);
        system.getParameterDescriptionList(out PARAMETER_DESCRIPTION[] paramDesc);

        if (paramDesc == null)
            count = 0;
        else if (count > paramDesc.Length)
            count = paramDesc.Length;

        for (int i = 0; i < count; i++)
        {
            if (paramDesc[i].type != PARAMETER_TYPE.GAME_CONTROLLED)
                continue;

            int listIndex = globalParameters.Count;
            globalParameters.Add(new FMODGlobalParamInfo
            {
                name = paramDesc[i].name,
                min = paramDesc[i].minimum,
                max = paramDesc[i].maximum,
                defaultValue = paramDesc[i].defaultvalue,
                value = paramDesc[i].defaultvalue,
            });

            if (System.Enum.TryParse(paramDesc[i].name, out FmodGlobalParamName parsedName) == false)
                continue;

            int paramSlot = (int)parsedName;
            if (paramSlot < 0 || paramSlot >= PARAM_SLOT_COUNT)
                continue;

            _parameterIds[paramSlot] = paramDesc[i].id;
            _hasParameterId[paramSlot] = true;
            _parameterListIndex[paramSlot] = listIndex;
            _lastSentValue[paramSlot] = paramDesc[i].defaultvalue;
        }
        if(globalParameters.Count == 0)
        {
            Debug.LogWarning("Aucun paramùtre global trouvù dans FMOD.");
        }

        Debug.Log($"Charge {globalParameters.Count} global parameters from FMOD");
    }

    public void ToggleGlobalParameter(FmodGlobalParamName paramName)
    {
        int slot = (int)paramName;
        if (slot < 0 || slot >= PARAM_SLOT_COUNT)
            return;

        int listIndex = _parameterListIndex[slot];
        if (listIndex < 0 || listIndex >= globalParameters.Count)
            return;

        if (globalParameters[listIndex].value == 0f)
            SetGlobalParameter(paramName, 1f);
        else if (globalParameters[listIndex].value == 1f)
            SetGlobalParameter(paramName, 0f);
    }

    public void SetGlobalParameter(FmodGlobalParamName paramName, float value)
    {
        int slot = (int)paramName;
        if (slot < 0 || slot >= PARAM_SLOT_COUNT)
            return;

        if (_hasParameterId[slot] == false)
            return;

        float epsilon = paramName == FmodGlobalParamName.G_Player_Speed
            ? SPEED_CHANGE_EPSILON
            : DISCRETE_CHANGE_EPSILON;

        if (_hasSentValue[slot] && Mathf.Abs(_lastSentValue[slot] - value) <= epsilon)
            return;

        _lastSentValue[slot] = value;
        _hasSentValue[slot] = true;

        int listIndex = _parameterListIndex[slot];
        if (listIndex >= 0 && listIndex < globalParameters.Count)
            globalParameters[listIndex].value = value;

        RuntimeManager.StudioSystem.setParameterByID(_parameterIds[slot], value);
    }
}
