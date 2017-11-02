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

    private void Awake()
    {
        Vector3 vec = mapPlane.GetComponent<Renderer>().bounds.size;
        planeX = vec.x;
        planeY = vec.z;
        inputNodes = GameObject.FindGameObjectsWithTag("Node");

        CalculateFlow(GetMapData(), 2);
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

    public void CalculateFlow(MapData data, int timeDif)
    {
        bool allNodesDone = false;
        while (!allNodesDone)
        {
            foreach(Node node in data.grid)
            {
                int maxTurns = Mathf.RoundToInt(timeDif / node.spreadRes);
                if (node.processedTurns > maxTurns)
                {
                    allNodesDone = true;
                }
                else
                {
                    allNodesDone = false;
                    break;
                }
            }

            if(allNodesDone)
            {
                break;
            }

            foreach (Node node in data.grid)
            {
                int maxTurns = Mathf.RoundToInt(timeDif / node.spreadRes);

                if (node.processedTurns > maxTurns)
                {
                    continue;
                }

                node.processedTurns += 1;

                List<Node> nodesToCheck = GetNodesFromDir(node.x, node.y, node.dir, data).ToList();

                if (nodesToCheck[0] == node || nodesToCheck.Count == 0)
                    continue;

                int maxX = data.grid.GetLength(0);
                int maxY = data.grid.GetLength(1);
                int dir = node.dir;

                int spreadDir = 0;

                int x = node.x;
                int y = node.y;
                #region Uglyness (Pit of shame)
                if (dir == 0)
                {
                    spreadDir = 1;
                    if (IsInBounds(x + 1, y + 1, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x + 1, y + 1]);
                    }
                    if (IsInBounds(x - 1, y + 1, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x - 1, y + 1]);
                    }
                }
                if (dir == 1)
                {
                    spreadDir = 2;
                    if (IsInBounds(x, y + 1, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x, y + 1]);
                    }
                    if (IsInBounds(x + 1, y, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x + 1, y]);
                    }
                }
                if (dir == 2)
                {
                    spreadDir = 3;
                    if (IsInBounds(x + 1, y + 1, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x + 1, y + 1]);
                    }
                    if (IsInBounds(x + 1, y - 1, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x + 1, y - 1]);
                    }
                }
                if (dir == 3)
                {
                    spreadDir = 4;
                    if (IsInBounds(x, y - 1, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x, y - 1]);
                    }
                    if (IsInBounds(x + 1, y, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x + 1, y]);
                    }
                }
                if (dir == 4)
                {
                    spreadDir = 5;
                    if (IsInBounds(x - 1, y - 1, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x - 1, y - 1]);
                    }
                    if (IsInBounds(x + 1, y - 1, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x + 1, y - 1]);
                    }
                }
                if (dir == 5)
                {
                    spreadDir = 6;
                    if (IsInBounds(x, y - 1, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x, y - 1]);
                    }
                    if (IsInBounds(x - 1, y, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x - 1, y]);
                    }
                }
                if (dir == 6)
                {
                    spreadDir = 7;
                    if (IsInBounds(x - 1, y + 1, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x - 1, y + 1]);
                    }
                    if (IsInBounds(x - 1, y - 1, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x - 1, y - 1]);
                    }
                }
                if (dir == 7)
                {
                    spreadDir = 0;
                    if (IsInBounds(x - 1, y, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x - 1, y]);
                    }
                    if (IsInBounds(x, y + 1, maxX, maxY))
                    {
                        nodesToCheck.Add(data.grid[x, y + 1]);
                    }
                }
                #endregion

                int cost = GetDirDifficulty(spreadDir, dir);

                for (int i = 0; i < nodesToCheck.Count; i++)
                {
                    if (i == 0)
                    {
                        for (int j = 0; j < nodesToCheck[i].values.values.Count; j++)
                        {
                            NodeValue nValue = node.values.values[j];
                            nValue.current = nValue.current * node.spreadRes;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < nodesToCheck[i].values.values.Count; j++)
                        {
                            NodeValue nValue = node.values.values[j];
                            nValue.current = nValue.current * (node.spreadRes / cost);
                        }
                    }
                }
            }
        }
    }

    Node[] GetNodesFromDir(int x, int y, int dir, MapData data)
    {
        //-1 = no dir, 0 = up, 1 = upRight, 2 = right, 3 = downRight, 4 = down, 5 = downLeft, 6 = left, 7 = upLeft

        int maxX = data.grid.GetLength(0);
        int maxY = data.grid.GetLength(1);

        #region more Uglyness
        if (dir == -1)
        {
            Node[] nodesToReturn = new Node[1];
            nodesToReturn[0] = data.grid[x, y];
            return nodesToReturn;
        }
        if(dir == 0)
        {
            List<Node> nodesToReturn = new List<Node>();
            if (IsInBounds(x, y + 1, maxX, maxY))
            {
                nodesToReturn.Add(data.grid[x, y + 1]);
            }
            return nodesToReturn.ToArray();
        }
        if (dir == 1)
        {
            List<Node> nodesToReturn = new List<Node>();
            if (IsInBounds(x + 1, y + 1, maxX, maxY))
            {
                nodesToReturn.Add(data.grid[x + 1, y + 1]);
            }
            return nodesToReturn.ToArray();
        }
        if (dir == 2)
        {
            List<Node> nodesToReturn = new List<Node>();
            if (IsInBounds(x + 1, y, maxX, maxY))
            {
                nodesToReturn.Add(data.grid[x + 1, y]);
            }
            return nodesToReturn.ToArray();
        }
        if (dir == 3)
        {
            List<Node> nodesToReturn = new List<Node>();
            if (IsInBounds(x + 1, y - 1, maxX, maxY))
            {
                nodesToReturn.Add(data.grid[x + 1, y - 1]);
            }
            return nodesToReturn.ToArray();
        }
        if (dir == 4)
        {
            List<Node> nodesToReturn = new List<Node>();
            if (IsInBounds(x, y - 1, maxX, maxY))
            {
                nodesToReturn.Add(data.grid[x, y - 1]);
            }
            return nodesToReturn.ToArray();
        }
        if (dir == 5)
        {
            List<Node> nodesToReturn = new List<Node>();
            if (IsInBounds(x - 1, y - 1, maxX, maxY))
            {
                nodesToReturn.Add(data.grid[x - 1, y - 1]);
            }
            return nodesToReturn.ToArray();
        }
        if (dir == 6)
        {
            List<Node> nodesToReturn = new List<Node>();
            if (IsInBounds(x - 1, y, maxX, maxY))
            {
                nodesToReturn.Add(data.grid[x - 1, y]);
            }
            return nodesToReturn.ToArray();
        }
        if (dir == 7)
        {
            List<Node> nodesToReturn = new List<Node>();
            if (IsInBounds(x - 1, y + 1, maxX, maxY))
            {
                nodesToReturn.Add(data.grid[x - 1, y + 1]);
            }
            return nodesToReturn.ToArray();
        }
        else
        {
            Debug.LogError("Wrong direction number: " + dir.ToString() + " does not exist.");
            Node[] nodesToReturn = new Node[1];
            nodesToReturn[0] = data.grid[x, y];
            return nodesToReturn;
        }
    #endregion
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
        float per = c.g;
        return 0.2f * per;
    }

    #endregion

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
