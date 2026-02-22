using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject tutorialUI1;
    [SerializeField] private GameObject tutorialUI2;
    [SerializeField] private GameObject tutorialUI3;

    // Start is called before the first frame update
    void Start()
    {
        if (Application.isMobilePlatform)
        {
            Application.targetFrameRate = Mathf.CeilToInt((float)Screen.currentResolution.refreshRateRatio.value);
        }
        else
        {
            Application.targetFrameRate = -1;
        }

        InputSystem.settings.SetInternalFeatureFlag("USE_OPTIMIZED_CONTROLS", true);
        InputSystem.settings.SetInternalFeatureFlag("USE_READ_VALUE_CACHING", true);
        tutorialUI1.SetActive(true);
        tutorialUI2.SetActive(false);
    }

    public void TutorialPhase2()
    {
        tutorialUI1.SetActive(false);
        tutorialUI2.SetActive(true);
    }

    public void TutorialGyroCheck()
    {
        try
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
            if (AttitudeSensor.current.attitude.ReadValue() == new Quaternion(0f, 0f, 0f, 0f))
            {
                tutorialUI1.SetActive(false);
                tutorialUI2.SetActive(false);
                tutorialUI3.SetActive(true);
            }
            else
                TutorialPhase3();
        }
        catch (Exception)
        {
            tutorialUI1.SetActive(false);
            tutorialUI2.SetActive(false);
            tutorialUI3.SetActive(true);
        }
    }

    public void TutorialPhase3()
    {
        tutorialUI1.SetActive(false);
        tutorialUI2.SetActive(false);
        tutorialUI3.SetActive(false);
        playerController.ResetRotation();
    }
}
