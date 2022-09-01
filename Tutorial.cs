using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Tutorial : MonoBehaviour
{
    [SerializeField] Manager manager;

    [SerializeField] GameObject[] screens;

    [SerializeField] RectTransform[] sections;

    [SerializeField] Transform button, mainUI, tutorial;

    [SerializeField] PostProcessProfile main;

    int idx = 0;

    Vector3 startPos;

    bool inTutorial;

    void Start()
    {
        inTutorial = true;
        main.GetSetting<ColorGrading>().saturation.value = 0;

        screens[idx].SetActive(true);

        sections[idx].SetParent(tutorial);

        startPos = button.position;
        Vector3 highlightedPos = startPos;
        highlightedPos.z = -9f;
        button.position = highlightedPos;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && inTutorial)
        {
            button.position = startPos;

            screens[idx].SetActive(false);

            if(idx <= 3)
            {
                sections[idx].SetParent(mainUI);
            }
            
            idx++;

            if(idx <= 4)
            {
                screens[idx].SetActive(true);

                if(idx <= 3)
                {
                    sections[idx].SetParent(tutorial);
                }
            }
            else
            {
                tutorial.gameObject.SetActive(false);
                manager.enabled = true;
                inTutorial = false;
            }
        }
    }
}
