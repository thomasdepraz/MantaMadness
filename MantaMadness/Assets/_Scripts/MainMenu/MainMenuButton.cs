using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class MainMenuButton : MonoBehaviour
{
    [SerializeField] private GameObject[] subVisuals;
    [Header("Default Parameters")]
    [SerializeField] private float defaultMatAmplitude = 0.25f;
    [SerializeField] private float defaultMatSpeed = 1f;
    [SerializeField] private float defaultMatFrequency = 1f;
    [Header("Enabled Parameters")]
    [SerializeField] private float matAmplitude = 0.25f;
    [SerializeField] private float matSpeed = 12.5f;
    [SerializeField] private float matFrequency = 2.5f;



    private void Start()
    {
        //foreach (GameObject sub in subVisuals)
        //{
        //    sub.SetActive(false);
        //}
    }

    public void EnableButton()
    {
        //Modify visual mat settings
        GetComponent<MeshRenderer>().material.SetFloat("_Amplitude", matAmplitude);
        GetComponent<MeshRenderer>().material.SetFloat("_Speed", matSpeed);
        GetComponent<MeshRenderer>().material.SetFloat("_Frequency", matFrequency);

        //ACtivate and modify sub visuals mat settings
        foreach (GameObject sub in subVisuals)
        {
            sub.SetActive(true);
            sub.GetComponent<MeshRenderer>().material.SetFloat("_Amplitude", matAmplitude);
            sub.GetComponent<MeshRenderer>().material.SetFloat("_Speed", matSpeed);
            sub.GetComponent<MeshRenderer>().material.SetFloat("_Frequency", matFrequency);
        }

    }

    public void ResetButton()
    {
        GetComponent<MeshRenderer>().material.SetFloat("_Amplitude", defaultMatAmplitude);
        GetComponent<MeshRenderer>().material.SetFloat("_Speed", defaultMatSpeed);
        GetComponent<MeshRenderer>().material.SetFloat("_Frequency", defaultMatFrequency);

        foreach (GameObject sub in subVisuals)
        {
            sub.SetActive(false);
        }
    }
}
