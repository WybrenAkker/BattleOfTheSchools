using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapConverter : MonoBehaviour {

    [SerializeField]
    private Texture2D directionTex;
    [SerializeField]
    private Transform mapPlane;
    private float planeX, planeY;

    private void Awake()
    {
        Vector3 vec = mapPlane.GetComponent<Renderer>().bounds.size;
        planeX = vec.x;
        planeY = vec.z;
        GetMapData();
    }

    public MapData GetMapData()
    {
        MapData ret = new MapData();
        //convert image to grid with directions
        ret.grid = SendInputDir(directionTex);
        GameObject[] inputNodes = GameObject.FindGameObjectsWithTag("Node");
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

    public int width, height;

	private Node[,] SendInputDir(Texture2D dirMap)
    {
        Node[,] ret = new Node[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                ret[x, y] = new Node();
                ret[x, y].dir =
                    ConvertColorToDir(dirMap.GetPixel(
                        ConvPercentage(width, dirMap.width, x),
                        ConvPercentage(height, dirMap.height, y)));
            }
        return ret;
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
        float per = c.b * 100 - 50;
        return Mathf.RoundToInt(0.08f * per);
    }
}
