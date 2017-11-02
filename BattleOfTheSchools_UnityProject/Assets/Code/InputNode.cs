﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputNode : MonoBehaviour {

    public List<NodeValue> values = new List<NodeValue>();
    public int checkRadius;
}

[Serializable]
public struct NodeValue
{
    public float current, threshold, acceptable, maxValue, spreadAmount;

    public void Ran()
    {
        current = UnityEngine.Random.Range(acceptable, maxValue);
    }

    public NodeValue(NodeValue other)
    {
        current = other.current;
        threshold = other.threshold;
        acceptable = other.acceptable;
        maxValue = other.maxValue;
        spreadAmount = other.spreadAmount;
    }
}

[Serializable]
public class Node
{
    public int x, y;
    //range 0 - 8
    public int dir = -1, processedTurns; //clock, -1 is no direction, 8 is same as 0
    public float spreadRes;
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