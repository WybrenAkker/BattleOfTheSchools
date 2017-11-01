using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputNode : MonoBehaviour {

    public List<NodeValue> values = new List<NodeValue>();
}

[Serializable]
public struct NodeValue
{
    public float current, max;
}

public class Node
{
    //range 0 - 8
    public int dir = -1;
    public InputNode values;
}