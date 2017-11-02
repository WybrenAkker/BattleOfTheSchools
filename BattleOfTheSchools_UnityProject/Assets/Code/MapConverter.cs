using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Linq;

public class MapConverter : MonoBehaviour {

    [SerializeField]
    private Texture2D flowTex;
    [SerializeField]
    private Transform mapPlane;
    private float planeX, planeY;

    [SerializeField]
    private int simulateFramesCount = 86400;

    private void Awake()
    {
        Vector3 vec = mapPlane.GetComponent<Renderer>().bounds.size;
        planeX = vec.x;
        planeY = vec.z;
        inputNodes = GameObject.FindGameObjectsWithTag("Node");
        Simulate();
    }

    [CreateAssetMenu(fileName = "SimFrame", menuName = "Simulation/List", order = 1)]
    public class SimFrame : ScriptableObject
    {
        public List<Node[,]> grids = new List<Node[,]>();
    }

    [SerializeField]
    private SimFrame saveData;

    [SerializeField]
    private List<NodeValue> defaultNode;

    private void Simulate()
    {
        MapData data = GetMapData();

        NodeValue nodeV;
        //randomize starting data
        foreach (Node node in data.grid)
            if(node.dir > -1)
                for(int i = 0; i < defaultNode.Count; i ++)
                {
                    nodeV = new NodeValue();

                    //BARF
                    nodeV.acceptable = defaultNode[i].acceptable;
                    nodeV.maxValue = defaultNode[i].maxValue;
                    nodeV.spreadAmount = defaultNode[i].spreadAmount;
                    nodeV.threshold = defaultNode[i].threshold;

                    nodeV.Ran();               
                    node.values.Add(nodeV);
                }

        saveData.grids.Clear();

        if (calculateFlow != null)
            StopCoroutine(calculateFlow);
        calculateFlow = StartCoroutine(CalculateFlow(data, simulateFramesCount));
    }

    private GameObject[] inputNodes;
    public MapData GetMapData()
    {
        MapData ret = new MapData();
        //convert image to grid with directions
        ret.grid = SendInputDir(flowTex);

        AddFlow(flowTex, ret.grid);

        ret.inputNodes = new InputNodePos[inputNodes.Length];
        
        //calc node positions in grid
        Vector3 pos;
        for (int i = 0; i < inputNodes.Length; i++)
        {
            ret.inputNodes[i] = new InputNodePos();
            pos = inputNodes[i].transform.position;
            ret.inputNodes[i].inputPosX = ConvPercentage(width, planeX, pos.x + planeX / 2);
            ret.inputNodes[i].inputPosY = ConvPercentage(height, planeY, pos.z + planeY / 2);
        }

        return ret;
    }

    public void Resume(Node[,] startPoint, int timeDif)
    {
        if (calculateFlow != null)
            StopCoroutine(calculateFlow);
        MapData temp = new MapData();
        temp.grid = startPoint;
        calculateFlow = StartCoroutine(CalculateFlow(temp, timeDif));
    }

    [SerializeField]
    private int loopsPerFrame = 500;
    private Coroutine calculateFlow;
    public IEnumerator CalculateFlow(MapData data, int timeDif)
    {
        float spread, spreadCalc;
        //convert to new
        Node[,] grid = new Node[data.grid.GetLength(0), data.grid.GetLength(1)];
        for (int x = 0; x < grid.GetLength(0); x++)
            for (int y = 0; y < grid.GetLength(1); y++)
                grid[x, y] = new Node(data.grid[x,y]);

        int timeUntilSnapShot = reqExecutedTurns;
        int timeLeft = timeDif;
        List<Node> corrupted = new List<Node>();
        int loop = 0;
        while (timeLeft > 0)
        {
            print("+1 Frame");

            timeUntilSnapShot--;
            timeLeft--;         
            corrupted.Clear();

            foreach(Node node in grid)
                if(node.dir > -1)
                    foreach(NodeValue nV in node.values)
                        if(nV.current > nV.threshold)
                        {
                            corrupted.Add(node);
                            break;
                        }
            
            Node other;
            foreach (Node corruptNode in corrupted)
            {
                loop++;
                if(loop > loopsPerFrame)
                {
                    yield return null;
                    loop = 0;
                }
                //top
                if (corruptNode.dir == 0 || corruptNode.dir == 1 || corruptNode.dir == 7)
                    if (IsInBounds(corruptNode.x, corruptNode.y + 1, width, height))
                    {
                        other = grid[corruptNode.x, corruptNode.y + 1];
                        if(other.dir > -1)
                            if (!corrupted.Contains(other))
                            {
                                spread = GetDirDifficultyPer(0, corruptNode.dir);
                                for (int nV = 0; nV < corruptNode.values.Count; nV++)
                                {
                                    spreadCalc = corruptNode.values[nV].spreadAmount * spread;
                                    corruptNode.values[nV].current -= spreadCalc;
                                    other.values[nV].current += spreadCalc;
                                    //print(1);
                                }
                            }
                    }
                //bottom
                if (corruptNode.dir == 4 || corruptNode.dir == 5 || corruptNode.dir == 3)
                    if (IsInBounds(corruptNode.x, corruptNode.y - 1, width, height))
                    {
                        other = grid[corruptNode.x, corruptNode.y - 1];
                        if (other.dir > -1)
                            if (!corrupted.Contains(other))
                            {
                                spread = GetDirDifficultyPer(4, corruptNode.dir);
                                for (int nV = 0; nV < corruptNode.values.Count; nV++)
                                {
                                    spreadCalc = corruptNode.values[nV].spreadAmount * spread;
                                    corruptNode.values[nV].current -= spreadCalc;
                                    other.values[nV].current += spreadCalc;
                                    //print(2);
                                }
                            }
                    }
                //right
                if (corruptNode.dir == 2)
                    if (IsInBounds(corruptNode.x + 1, corruptNode.y, width, height))
                    {
                        other = grid[corruptNode.x + 1, corruptNode.y];
                        if (other.dir > -1)
                            if (!corrupted.Contains(other))
                            {
                                spread = GetDirDifficultyPer(2, corruptNode.dir);
                                for (int nV = 0; nV < corruptNode.values.Count; nV++)
                                {
                                    spreadCalc = corruptNode.values[nV].spreadAmount * spread;
                                    corruptNode.values[nV].current -= spreadCalc;
                                    other.values[nV].current += spreadCalc;
                                    //print(3);
                                }
                            }
                    }
                //left
                if (corruptNode.dir == 6)
                    if (IsInBounds(corruptNode.x - 1, corruptNode.y, width, height))
                    {
                        other = grid[corruptNode.x - 1, corruptNode.y];
                        if (other.dir > -1)
                            if (!corrupted.Contains(other))
                            {
                                spread = GetDirDifficultyPer(6, corruptNode.dir);
                                for (int nV = 0; nV < corruptNode.values.Count; nV++)
                                {
                                    spreadCalc = corruptNode.values[nV].spreadAmount * spread;
                                    corruptNode.values[nV].current -= spreadCalc;
                                    other.values[nV].current += spreadCalc;
                                    //print(4);
                                }
                            }
                    }
            }

            if (timeUntilSnapShot <= 0)
            {
                Node[,] newGrid = new Node[width, height];
                for (int w = 0; w < width; w++)
                    for (int h = 0; h < height; h++)
                        newGrid[w, h] = new Node(grid[w,h]);
                saveData.grids.Add(newGrid);
                if(debugVisually)
                    DebugVisuals();
                timeUntilSnapShot = reqExecutedTurns;
            }
        }
    }

    public int reqExecutedTurns = 3600;

    private struct OpenNode
    {
        public Node node;
        public int dis;

        public OpenNode(Node node, int dis)
        {
            this.node = node;
            this.dis = dis;
        }
    }

    public List<Node> GetNodesInArea(int dis, Node center, Node[,] grid)
    {
        List<Node> ret = new List<Node>(); //closed
        List<Node> closed = new List<Node>();
        List<OpenNode> open = new List<OpenNode>() {new OpenNode(center, 0) };
        OpenNode node;
        Node testable;
        int x = grid.GetLength(0), y = grid.GetLength(1);

        while(open.Count > 0)
        {
            node = open[0];
            open.RemoveAt(0);
            closed.Add(node.node);
            //check if path is in dis else continue

            if (node.dis > dis)
                continue;
            if (node.node.dir < 0)
                continue;
            ret.Add(node.node);

            //check adjecent
            #region Check Neighbours

            //top
            if (IsInBounds(node.node.x, node.node.y + 1, x, y))
            {
                testable = grid[node.node.x, node.node.y + 1];
                if (ret.Contains(testable))
                    continue;
                if (closed.Contains(testable))
                    continue;
                foreach (OpenNode oN in open)
                    if (oN.node == testable)
                        continue;
                open.Add(new OpenNode(testable,
                    node.dis + GetDirDifficulty(0, node.node.dir) + 1)); //also calculate spread map
            }
            
            //top right
            if (IsInBounds(node.node.x + 1, node.node.y + 1, x, y))
            {
                testable = grid[node.node.x + 1, node.node.y + 1];
                if (ret.Contains(testable))
                    continue;
                if (closed.Contains(testable))
                    continue;
                foreach (OpenNode oN in open)
                    if (oN.node == testable)
                        continue;
                open.Add(new OpenNode(testable,
                    node.dis + GetDirDifficulty(1, node.node.dir))); //also calculate spread map
            }
            
            //right
            if (IsInBounds(node.node.x + 1, node.node.y, x, y))
            {
                testable = grid[node.node.x + 1, node.node.y];
                if (ret.Contains(testable))
                    continue;
                if (closed.Contains(testable))
                    continue;
                foreach (OpenNode oN in open)
                    if (oN.node == testable)
                        continue;
                open.Add(new OpenNode(testable,
                    node.dis + GetDirDifficulty(2, node.node.dir))); //also calculate spread map
            }

            //down right
            if (IsInBounds(node.node.x + 1, node.node.y - 1, x, y))
            {
                testable = grid[node.node.x + 1, node.node.y - 1];
                if (ret.Contains(testable))
                    continue;
                if (closed.Contains(testable))
                    continue;
                foreach (OpenNode oN in open)
                    if (oN.node == testable)
                        continue;
                open.Add(new OpenNode(testable,
                    node.dis + GetDirDifficulty(3, node.node.dir))); //also calculate spread map
            }

            //down
            if (IsInBounds(node.node.x, node.node.y - 1, x, y))
            {
                testable = grid[node.node.x, node.node.y - 1];
                if (ret.Contains(testable))
                    continue;
                if (closed.Contains(testable))
                    continue;
                foreach (OpenNode oN in open)
                    if (oN.node == testable)
                        continue;
                open.Add(new OpenNode(testable,
                    node.dis + GetDirDifficulty(4, node.node.dir))); //also calculate spread map
            }

            //down left
            if (IsInBounds(node.node.x - 1, node.node.y - 1, x, y))
            {
                testable = grid[node.node.x - 1, node.node.y - 1];
                if (ret.Contains(testable))
                    continue;
                if (closed.Contains(testable))
                    continue;
                foreach (OpenNode oN in open)
                    if (oN.node == testable)
                        continue;
                open.Add(new OpenNode(testable,
                    node.dis + GetDirDifficulty(5, node.node.dir))); //also calculate spread map
            }

            //left
            if (IsInBounds(node.node.x - 1, node.node.y, x, y))
            {
                testable = grid[node.node.x - 1, node.node.y];
                if (ret.Contains(testable))
                    continue;
                if (closed.Contains(testable))
                    continue;
                foreach (OpenNode oN in open)
                    if (oN.node == testable)
                        continue;
                open.Add(new OpenNode(testable,
                    node.dis + GetDirDifficulty(6, node.node.dir))); //also calculate spread map
            }

            //top left
            if (IsInBounds(node.node.x - 1, node.node.y + 1, x, y))
            {
                testable = grid[node.node.x - 1, node.node.y + 1];
                if (ret.Contains(testable))
                    continue;
                if (closed.Contains(testable))
                    continue;
                foreach (OpenNode oN in open)
                    if (oN.node == testable)
                        continue;
                open.Add(new OpenNode(testable,
                    node.dis + GetDirDifficulty(7, node.node.dir))); //also calculate spread map
            }
            
            #endregion
        }

        return ret;
    }

    [SerializeField]
    private bool debugVisually;
    [SerializeField]
    private Material ground;
    public Color defaultCol, debugCol;
    private void DebugVisuals()
    {
        Texture2D tex = new Texture2D(width, height);
        Color c;
        print("Snapshot taken.");
        for (int w = 0; w < width; w++)
            for (int h = 0; h < height; h++)
            {
                c = Color.black;
                if (saveData.grids[saveData.grids.Count - 1][w, h].dir < 0)
                {
                    c.a = 0;
                    tex.SetPixel(w, h, c);
                    continue;
                }
                
                //FILTHY
                c = Color.Lerp(defaultCol, debugCol, saveData.grids[saveData.grids.Count - 1][w, h].values[0].current /
                    saveData.grids[saveData.grids.Count - 1][w, h].values[0].maxValue);
                tex.SetPixel(w, h, c);
            }
        
        tex.Apply();
        ground.mainTexture = tex;
    }

    private float GetDirDifficultyPer(int dir, int flowdir)
    {
        float res = 6 - GetDirDifficulty(dir, flowdir);
        return res / 6;
    }

    private int GetDirDifficulty(int dir, int flowDir)
    {
        int calc1 = dir < 6 ? dir : Mathf.Abs(12 - dir), 
            calc2 = flowDir < 6 ? flowDir : Mathf.Abs(12 - flowDir);
        return Mathf.Abs(calc1 - calc2);
    }

    private bool IsInBounds(int x, int y, int maxX, int maxY)
    {
        if (x < maxX && x > -1 && y < maxY && x > -1)
            return true;
        return false;
    }

    public class MapData
    {
        public Node[,] grid;
        public InputNodePos[] inputNodes;
    }

    public class InputNodePos
    {
        public InputNode node;
        public int inputPosX, inputPosY;
    }

#region Bake

    public int width, height;

	private Node[,] SendInputDir(Texture2D dirMap)
    {
        Node[,] ret = new Node[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                ret[x, y] = new Node();
                ret[x, y].x = x;
                ret[x, y].y = y;

                ret[x, y].dir =
                    ConvertColorToDir(dirMap.GetPixel(
                        ConvPercentage(width, dirMap.width, x),
                        ConvPercentage(height, dirMap.height, y)));
            }
        return ret;
    }

    private void AddFlow(Texture2D flowTex, Node[,] grid)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                grid[x, y].spreadRes =
                    ConvertColorToSpeed(flowTex.GetPixel(
                        ConvPercentage(width, flowTex.width, x),
                        ConvPercentage(height, flowTex.height, y)));
            }
    }

    private float Percentage(float cur, float max)
    {
        return cur / max * 100;
    }

    private int ConvPercentage(float max, float newMax, float cur)
    {
        float per = Percentage(cur, max);
        return Mathf.RoundToInt(newMax / 100 * per);
    }

    private int ConvertColorToDir(Color c)
    {
        float per = c.r * 100 - 50;
        return Mathf.RoundToInt(0.08f * per);
    }

    private float ConvertColorToSpeed(Color c)
    {
        float per = c.a;
        return Mathf.Lerp(0, 1, 0.2f * per);
    }

    #endregion

    public static List<T> CloneList<T>(List<T> clonable)
    {
        List<T> ret = new List<T>();
        foreach (T trans in clonable)
            ret.Add(Clone(trans));
        return ret;
    }

    public static T Clone<T>(T source)
    {
        if (!typeof(T).IsSerializable)
        {
            throw new System.ArgumentException("The type must be serializable.", "source");
        }

        // Don't serialize a null object, simply return the default for that object
        if (Object.ReferenceEquals(source, null))
        {
            return default(T);
        }

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new MemoryStream();
        using (stream)
        {
            formatter.Serialize(stream, source);
            stream.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(stream);
        }
    }
}
