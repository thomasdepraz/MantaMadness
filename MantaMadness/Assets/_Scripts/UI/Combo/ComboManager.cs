using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum ComboType
{
    Default,
    GalaxySpin,
}

public enum ComboState
{
    Inactive,
    Active,
    Fever,
    Cinematic,
}

public interface IFeverReactive
{
    void OnFeverActivated();
}

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;

    [Header("Combo Settings")]
    [SerializeField] float mainDuration = 5f;
    [SerializeField] float bonusDuration = 2f;
    [SerializeField] float bonusStartThreshold = 0.25f;

    float mainTimer;
    float bonusTimer;
    float frozenMainTimer;

    bool isBonusPhase;

    public int ComboLevel => Mathf.Clamp(currentComboValue, 0, 4);
    int currentComboValue;

    [SerializeField] private ComboDatabase database;

    ComboActionSO lastAction;

    ComboState currentState = ComboState.Inactive;

    public ComboState State => currentState;
    public int CurrentValue => currentComboValue;
    public float FrozenMainTimer => frozenMainTimer;

    public float TimerNormalized
    {
        get
        {
            float max = (currentState == ComboState.Fever && useSeparateFeverDuration)
                ? feverComboDuration
                : mainDuration;

            if (max <= 0f) return 0f;

            return Mathf.Clamp01(mainTimer / max);
        }
    }

    public List<ComboActionSO> comboMemory = new List<ComboActionSO>(5);
    [SerializeField] private int memorySize = 5;
    [SerializeField] private float freezeDuration = 1f;
    [SerializeField] private float timerBonusDuration = 1.5f;

    float freezeTimer;

    [Header("Fever Settings")]
    [SerializeField] private float feverComboDuration = 6f; // durée plus longue
    [SerializeField] private bool useSeparateFeverDuration = true;

    public event Action<ComboActionSO> OnActionAdded;
    public event Action<int> OnComboValueChanged;
    public event Action OnComboStarted;
    public event Action OnComboEnded;
    public event Action OnFeverStarted;
    public event Action OnFeverEnded;
    public event Action<ComboState> OnStateChanged;
    public event Action<int> OnComboLevelChanged;

    private int lastComboLevel = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (currentState != ComboState.Active &&
        currentState != ComboState.Fever)
            return;

        // BONUS PHASE d'abord : main timer est gelé => UI freeze
        if (isBonusPhase)
        {
            bonusTimer -= Time.deltaTime;

            if (bonusTimer <= 0f)
            {
                bonusTimer = 0f;
                isBonusPhase = false; // main timer peut recommencer à descendre
            }

            return; // IMPORTANT: main timer ne descend pas pendant bonus
        }

        // MAIN PHASE ensuite
        mainTimer -= Time.deltaTime;

        if (mainTimer <= 0f)
        {
            mainTimer = 0f;
            EndCombo();
        }
    }

    public void AddComboAction(ComboID id)
    {
        var action = database.Get(id);

        if (action == null)
        {
            Debug.LogWarning($"Combo action {id} not found in database.");
            return;
        }
        AddComboAction(action);
    }

    public void AddComboAction(ComboActionSO action)
    {
        if (currentState == ComboState.Inactive)
            StartCombo();

        int index = GetMemoryIndex(action);

        int valueToAdd = 0;

        if (index == 0)
        {
            // update text only, no timer changes
            OnActionAdded?.Invoke(action);
            AddToMemory(action);
            return;
        }
        else if (index == 1 || index == 2)
        {
            // bonus half, main unchanged
            isBonusPhase = true;
            ResetBonusTimerHalf();

            valueToAdd = action.value / 4;
        }
        else if (index == 3 || index == 4)
        {
            // bonus full + main +50%
            isBonusPhase = true;
            ResetBonusTimerFull();
            AddHalfMainTimer();

            valueToAdd = action.value / 2;
        }
        else
        {
            ResetMainTimer();
            mainTimer = mainDuration;        
            bonusTimer = bonusDuration;      
            isBonusPhase = true;             

            valueToAdd = action.value;
        }

        currentComboValue += valueToAdd;

        int newLevel = ComboLevel;

        if (newLevel != lastComboLevel)
        {
            lastComboLevel = newLevel;
            OnComboLevelChanged?.Invoke(newLevel);
        }

        Debug.Log("Combo Level: " + ComboLevel);

        AddToMemory(action);
        lastAction = action;

        OnActionAdded?.Invoke(action);
        OnComboValueChanged?.Invoke(currentComboValue);

        CheckFever();
    }

    void StartCombo()
    {
        currentComboValue = 0;

        mainTimer = mainDuration;
        bonusTimer = bonusDuration;

        isBonusPhase = true;

        ChangeState(ComboState.Active);

        OnComboStarted?.Invoke();
    }

    void EndCombo()
    {
        if (currentState == ComboState.Fever)
        {
            OnFeverEnded?.Invoke();
        }

        lastComboLevel = 0;
        OnComboLevelChanged?.Invoke(0);

        currentComboValue = 0;
        comboMemory.Clear();

        isBonusPhase = false;
        frozenMainTimer = 0f;

        isBonusPhase = false;

        ChangeState(ComboState.Inactive);

        OnComboEnded?.Invoke();
    }

    void CheckFever()
    {
        if (currentState == ComboState.Fever)
            return;

        if (ComboLevel >= 4)
            StartFever();
    }

    void StartFever()
    {
        ChangeState(ComboState.Fever);

        if (useSeparateFeverDuration)
        {
            mainTimer = feverComboDuration;
        }
        else
        {
            mainTimer += feverComboDuration;
        }

        ActivateFeverGameplay();

        OnFeverStarted?.Invoke();
    }

    [SerializeField] float feverRadius = 10f;
    [SerializeField] LayerMask feverLayer;

    void ActivateFeverGameplay()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            feverRadius,
            feverLayer);

        foreach (var hit in hits)
        {
            IFeverReactive reactive =
                hit.GetComponent<IFeverReactive>();

            reactive?.OnFeverActivated();
        }
    }

    public void EnterCinematic()
    {
        if (currentState == ComboState.Inactive)
            return;

        ChangeState(ComboState.Cinematic);
    }

    public void ExitCinematic()
    {
        if (currentState != ComboState.Cinematic)
            return;

        // Return to correct state depending on combo value
        if (ComboLevel >= 4)
            ChangeState(ComboState.Fever);
        else
            ChangeState(ComboState.Active);
    }

    void ChangeState(ComboState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        OnStateChanged?.Invoke(newState);
    }

    private int GetMemoryIndex(ComboActionSO action)
    {
        for (int i = 0; i < comboMemory.Count; i++)
        {
            if (comboMemory[i] == action)
                return i;
        }

        return -1;
    }

    private void AddToMemory(ComboActionSO action)
    {
        comboMemory.Add(action);

        if(comboMemory.Count > memorySize)
        {
            comboMemory.RemoveAt(0);
        }
    }

    void ResetMainTimer()
    {
        mainTimer = mainDuration;
    }

    void ResetBonusTimerFull()
    {
        bonusTimer = bonusDuration;
    }

    void ResetBonusTimerHalf()
    {
        bonusTimer = bonusDuration * 0.5f;
    }

    void AddHalfMainTimer()
    {
        mainTimer += mainDuration * 0.5f;

        if (mainTimer > mainDuration)
            mainTimer = mainDuration;
    }
}
