using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputNode : MonoBehaviour {
    [HideInInspector]
    public List<NodeValue> values = new List<NodeValue>();
    public int checkRadius;

    public void Alarm(NodeValue nV)
    {
        transform.GetComponent<SpriteRenderer>().color = Color.red;
    }

    public void UnAlarm()
    {
        transform.GetComponent<SpriteRenderer>().color = Color.white;
    }
}

[Serializable]
public class NodeValue
{
    public float current, threshold, acceptable, maxValue, spreadAmount;

    public void Ran()
    {
        current = UnityEngine.Random.Range(acceptable, threshold);
    }

    public NodeValue(NodeValue other)
    {
        current = other.current;
        threshold = other.threshold;
        acceptable = other.acceptable;
        maxValue = other.maxValue;
        spreadAmount = other.spreadAmount;
    }

    public NodeValue()
    {

    }
}

[Serializable]
public class Node
{
    public int x, y;
    //range 0 - 8
    public int dir = -1, processedTurns; //clock, -1 is no direction, 8 is same as 0
    public float spreadRes = 1;
    public List<NodeValue> values = new List<NodeValue>();

    public Node(Node other)
    {
        x = other.x;
        y = other.y;
        dir = other.dir;
        processedTurns = other.processedTurns;
        spreadRes = other.spreadRes;
        foreach (NodeValue nV in other.values)
            values.Add(new NodeValue(nV));
    }

    public Node()
    {

    }
}