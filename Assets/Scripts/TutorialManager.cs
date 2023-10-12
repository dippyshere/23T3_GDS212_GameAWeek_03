using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject tutorialUI1;
    [SerializeField] private GameObject tutorialUI2;

    // Start is called before the first frame update
    void Start()
    {
        tutorialUI1.SetActive(true);
        tutorialUI2.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TutorialPhase2()
    {
        tutorialUI1.SetActive(false);
        tutorialUI2.SetActive(true);
    }

    public void TutorialPhase3()
    {
        tutorialUI1.SetActive(false);
        tutorialUI2.SetActive(false);
        playerController.ResetRotation();
    }
}
