using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timeline : MonoBehaviour
{
    public float maxSnapDif;
    public Slider slider;
    GameObject gameManager;
    MapConverter mc;
    int imaginarySecondsBetweenHours;
    public GameObject graph;

    public void Start()
    {
        gameManager = GameObject.Find("GameManager");
        mc = gameManager.GetComponent<MapConverter>();
        imaginarySecondsBetweenHours = mc.reqExecutedTurns;
    }

    public void OnValueChanged()
    {
        int nearestValue = Mathf.RoundToInt(slider.value);

        float dif = nearestValue - slider.value;

        if(dif < maxSnapDif && dif > 0)
        {
            slider.value = nearestValue;
        }
        else if(dif < 0 && dif > maxSnapDif)
        {
            slider.value = nearestValue;
        }

        int timeSnippet = Mathf.FloorToInt(slider.value);

        float timeDif = slider.value - timeSnippet;

        int simTime = Mathf.RoundToInt(imaginarySecondsBetweenHours * timeDif);

        //mc.UpdateSim(timeSnippet, simTime);
        //graph.GetComponent<Graph>().TimeLine(slider.value);
    }
}
