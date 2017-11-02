using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeLineCounter : MonoBehaviour {
    public GameObject timeLineElement;
    public int toSpawn;
    public Transform toParrentTo;
    public List<TimelineElement> timeLineElements = new List<TimelineElement>();

	void Start () {
        for (int i = 1; i < toSpawn + 1; i++)
        {
            GameObject newElement = Instantiate(timeLineElement, transform.position, Quaternion.identity);
            newElement.transform.SetParent(toParrentTo);
            newElement.GetComponent<TimelineElement>().ownText.text = i.ToString();
            timeLineElements.Add(newElement.GetComponent<TimelineElement>());
        }
	}
}
