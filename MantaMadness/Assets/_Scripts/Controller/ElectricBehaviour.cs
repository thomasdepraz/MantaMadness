using System;
using UnityEngine;
using UnityEngine.Rendering;

public class ElectricBehaviour : MonoBehaviour
{
    [Header("Charge Settings")]
    [SerializeField] private float maxCharge = 1f;
    [SerializeField] public float chargeDuration = 2f;
    [SerializeField] private float decayDuration = 5f;
    [SerializeField] private int maxElectricUses = 1;

    public int remainingElectricUses = 0;

    private float currentCharge = 0f;
    private bool isCharged = false;
    private DynamoCharger activeDynamo;

    public bool IsCharged => isCharged;
    public float CurrentCharge => currentCharge;
    public float ChargeRatio => maxCharge <= 0f ? 0f : currentCharge / maxCharge;

    public Action onElectricChargeFull;
    public Action onElectricChargeLost;
    public Action<float> onElectricChargeUpdated;
    public Action electricJumpStart;
    public Action electricJumpEnd;

    public void Tick(bool isSpinning, bool hasDynamoAbility, Vector3 position)
    {
        bool canCharge = CanChargeElectricity(hasDynamoAbility, position);

        if (isSpinning && canCharge)
        {
            Charge();
        }
        else
        {
            Decay();
        }
    }

    private bool CanChargeElectricity(bool hasDynamoAbility, Vector3 position)
    {
        if (activeDynamo != null)
            return true;

        if (hasDynamoAbility)
            return true;

        return false;
    }

    private void Charge()
    {
        float amountPerSecond = maxCharge / chargeDuration;
        currentCharge += amountPerSecond * Time.deltaTime;
        currentCharge = Mathf.Clamp(currentCharge, 0f, maxCharge);

        onElectricChargeUpdated?.Invoke(ChargeRatio);

        if (!isCharged && currentCharge >= maxCharge)
        {
            isCharged = true;
            remainingElectricUses = maxElectricUses;
            onElectricChargeFull?.Invoke();
        }
    }

    private void Decay()
    {
        if (currentCharge <= 0f)
            return;

        float amountPerSecond = maxCharge / decayDuration;
        currentCharge -= amountPerSecond * Time.deltaTime;
        currentCharge = Mathf.Clamp(currentCharge, 0f, maxCharge);

        onElectricChargeUpdated?.Invoke(ChargeRatio);

        if (currentCharge <= 0f)
        {
            currentCharge = 0f;

            if (isCharged)
            {
                isCharged = false;
                onElectricChargeLost?.Invoke();
            }
        }
        else
        {
            if (isCharged && currentCharge < maxCharge)
            {
                // On reste "charged" tant qu'il reste de la charge
                // donc ici on ne perd pas encore l'état électrique.
            }
        }
    }

    public void ConsumeCharge()
    {
        if (!isCharged)
            return;

        currentCharge = 0f;

        if (isCharged)
        {
            isCharged = false;
            onElectricChargeLost?.Invoke();
        }

        onElectricChargeUpdated?.Invoke(0f);
    }

    public void BoostConsumeCharge()
    {
        if (!isCharged)
            return;

        remainingElectricUses--;

        if (remainingElectricUses > 0)
            return;

        ConsumeCharge();
    }

    public void ForceFullCharge()
    {
        currentCharge = maxCharge;

        if (!isCharged)
        {
            isCharged = true;
            onElectricChargeFull?.Invoke();
        }

        onElectricChargeUpdated?.Invoke(1f);
    }

    public void ResetCharge()
    {
        currentCharge = 0f;

        if (isCharged)
        {
            isCharged = false;
            onElectricChargeLost?.Invoke();
        }

        onElectricChargeUpdated?.Invoke(0f);
    }

    public void SetActiveDynamo(DynamoCharger dynamo)
    {
        activeDynamo = dynamo;
    }

    public void ClearActiveDynamo(DynamoCharger dynamo)
    {
        if (activeDynamo == dynamo)
            activeDynamo = null;
    }
}
