using System.Collections;
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
    public float current, max, acceptable, maxValue, spreadAmount;
}

public class Node
{
    public int x, y;
    //range 0 - 8
    public int dir = -1, processedTurns; //clock, -1 is no direction, 8 is same as 0
    public float spreadRes;
    public InputNode values;
}