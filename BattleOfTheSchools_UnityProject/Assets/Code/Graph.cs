using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Graph : MonoBehaviour {

    [Serializable]
    public class GraphBasicPoints
    {
        public Transform nullPoint;
        public Transform maxVertical;
        public Transform maxHorizontal;
        public GameObject panel;
    }

    [Serializable]
    public class PointHolders
    {
        public Transform pointHolder1;
        public Transform pointHolder2;
        public Transform pointHolder3;
        public Transform pointHolder4;
        public Transform pointHolder5;
    }

    [Serializable]
    public class LineHolders
    {
        public GameObject lineHolder1;
        public GameObject lineHolder2;
        public GameObject lineHolder3;
        public GameObject lineHolder4;
        public GameObject lineHolder5;
    }

    [Serializable]
    public class GraphLength
    {
        public float verticalLength;
        public float horizontalLength;
    }

    [Serializable]
    public class LineInfo
    {
        public float lineWidth;
    }

    public int[,] graphList = new int[5,24];
    public Transform[,] pointList = new Transform[5,24];
    public GraphBasicPoints graphBasicPoints = new GraphBasicPoints();
    public GraphLength graphLength = new GraphLength();
    public PointHolders pointHolders = new PointHolders();
    public LineHolders lineHolders = new LineHolders();
    public LineInfo lineInfo = new LineInfo();

    public GameObject pointImage;
    public Slider timeLineRepresentation;
    public GameObject outsideHolder;

    private LineRenderer line1;
    private LineRenderer line2;
    private LineRenderer line3;
    private LineRenderer line4;
    private LineRenderer line5;

    public bool[] isDisplayed;

    public List<Color> lineColors = new List<Color>();
    public List<GameObject> buttons = new List<GameObject>();

    private void Start()
    {
        for(int p = 0; p < 5; p++)
        {
            for (int y = 0; y < 24; y++)
            {
                int randomInt = UnityEngine.Random.Range(0, 101);
                graphList[p, y] = randomInt;
            }
         //   graphList[vervuiling, uur] = procent vervuiling;
        }
        CreateLines();
        ColorButtons();
    }

    void ColorButtons()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].GetComponent<Button>().image.color = lineColors[i];
        }
    }

    void CreateLines()
    {
        lineHolders.lineHolder1 = Instantiate(new GameObject(), transform.position, Quaternion.identity);
        lineHolders.lineHolder2 = Instantiate(new GameObject(), transform.position, Quaternion.identity);
        lineHolders.lineHolder3 = Instantiate(new GameObject(), transform.position, Quaternion.identity);
        lineHolders.lineHolder4 = Instantiate(new GameObject(), transform.position, Quaternion.identity);
        lineHolders.lineHolder5 = Instantiate(new GameObject(), transform.position, Quaternion.identity);

        lineHolders.lineHolder1.transform.parent = transform;
        lineHolders.lineHolder2.transform.parent = transform;
        lineHolders.lineHolder3.transform.parent = transform;
        lineHolders.lineHolder4.transform.parent = transform;
        lineHolders.lineHolder5.transform.parent = transform;

        /*
        line1 = lineHolders.lineHolder1.AddComponent<LineRenderer>();
        line2 = lineHolders.lineHolder2.AddComponent<LineRenderer>();
        line3 = lineHolders.lineHolder3.AddComponent<LineRenderer>();
        line4 = lineHolders.lineHolder4.AddComponent<LineRenderer>();
        line5 = lineHolders.lineHolder5.AddComponent<LineRenderer>();

        Material redLine = new Material(Shader.Find("Standard"));
        Material blueLine = new Material(Shader.Find("Standard"));
        Material greenLine = new Material(Shader.Find("Standard"));
        Material whiteLine = new Material(Shader.Find("Standard"));
        Material magnetaLine = new Material(Shader.Find("Standard"));


        PrepareLines(line1, 5, Color.red, redLine);
        PrepareLines(line2, 5, Color.blue, blueLine);
        PrepareLines(line3, 5, Color.green, greenLine);
        PrepareLines(line4, 5, Color.white, whiteLine);
        PrepareLines(line5, 5, Color.magenta, magnetaLine);

        */
        CalculateLength();
    }

    void PrepareLines(LineRenderer line, float lineWidth, Color lineColor, Material lineMat)
    {
   //     print(line.ToString());
        line.startWidth = lineWidth;
        lineMat.color = lineColor;
        line.material = lineMat;
        line.positionCount = 46;
    }

    void CalculateLength()
    {
        graphLength.verticalLength = Vector3.Distance(graphBasicPoints.nullPoint.position, graphBasicPoints.maxVertical.position);
        graphLength.horizontalLength = Vector3.Distance(graphBasicPoints.nullPoint.position, graphBasicPoints.maxHorizontal.position);
        SetPoints();
    }

    void SetPoints()
    {
        //Set points to draw lines inbetween
        for (int i = 0; i < 5; i++)
        {
            for(int i2 = 0; i2 < 24; i2++)
            {
                float inputProcent = graphList[i, i2];
                Vector3 pointPos = new Vector3();
                pointPos.x = graphLength.horizontalLength / 24 * (i2 + 1);
                pointPos.y = graphLength.verticalLength / 100 * inputProcent;
                pointPos.z = 0;
          //      print("i = " + i.ToString() + "| i2 = " + i2.ToString() + "| pointPos = " + pointPos.ToString());
                GameObject newPoint = (GameObject)Instantiate(pointImage, graphBasicPoints.nullPoint.position, Quaternion.identity) as GameObject;
                newPoint.transform.name = "i = " + i.ToString() + "| i2 = " + i2.ToString();
                switch (i)
                {
                    case 0:
                        newPoint.transform.parent = pointHolders.pointHolder1;
                        break;
                    case 1:
                        newPoint.transform.parent = pointHolders.pointHolder2;
                        break;
                    case 2:
                        newPoint.transform.parent = pointHolders.pointHolder3;
                        break;
                    case 3:
                        newPoint.transform.parent = pointHolders.pointHolder4;
                        break;
                    case 4:
                        newPoint.transform.parent = pointHolders.pointHolder5;
                        break;
                }
                newPoint.gameObject.GetComponent<Image>().color = lineColors[i];
                newPoint.transform.position += pointPos;
                pointList[i, i2] = newPoint.transform;
            }
        }
        DrawGraphs();
    }

    void DrawGraphs()
    {
        //Draw lines between points  
        for(int i = 0; i < 5; i++)
        {
            int lineId = 0;
            for (int i2 = 0; i2 < 24; i2++)
            {
                int pointId = i2;
        //        print("LineID is " + pointId);
                pointId--;
                if(pointId >= 0 && pointId < 24)
                {
                    LineRenderer line = new LineRenderer();
                    switch (i)
                    {
                        case 0:
                            line = line1;
                            break;
                        case 1:
                            line = line2;
                            break;
                        case 2:
                            line = line3;
                            break;
                        case 3:
                            line = line4;
                            break;
                        case 4:
                            line = line5;
                            break;
                    }
                    
                    /* 
                     * 
                     * //THIS SORT OF WORKS BUT MAKES LINES THIN
                    line.SetPosition(lineId, pointList[i, pointId].position);
                    lineId++;
                    line.SetPosition(lineId, pointList[i, i2].position);
                    lineId++;
                    */

                    GameObject newEmptyLineObject = new GameObject();
                    GameObject newLineObject = Instantiate(newEmptyLineObject, transform.position, Quaternion.identity) as GameObject;
                    newLineObject.transform.parent = transform; //Try adding an outside object and bind the line holders to it
                    LineRenderer testLine = newLineObject.AddComponent<LineRenderer>();
                    Material testMat = new Material(Shader.Find("Standard"));
                    switch (i)
                    {
                        case 0:
                            newLineObject.transform.parent = lineHolders.lineHolder1.transform;
                            break;
                        case 1:
                            newLineObject.transform.parent = lineHolders.lineHolder2.transform;
                            break;
                        case 2:
                            newLineObject.transform.parent = lineHolders.lineHolder3.transform;
                            break;
                        case 3:
                            newLineObject.transform.parent = lineHolders.lineHolder4.transform;
                            break;
                        case 4:
                            newLineObject.transform.parent = lineHolders.lineHolder5.transform;
                            break;
                    }
                    testMat.color = lineColors[i];
                    testLine.material = testMat;

                    testLine.startWidth = lineInfo.lineWidth;
                    testLine.endWidth = lineInfo.lineWidth;
                    testLine.SetPosition(0, GetWorldPosOfPoint(pointList[i, pointId].gameObject));
                   // print(pointList[i, pointId]);
                    lineId++;
                    testLine.SetPosition(1, GetWorldPosOfPoint(pointList[i, i2].gameObject));

                }
            }
        }
    }

    Vector3 GetWorldPosOfPoint(GameObject g)
    {
        Camera c = Camera.main;
        Vector3 newPos = g.transform.position;
        newPos.z = 1;
        newPos = c.ScreenToWorldPoint(newPos);
        return newPos;
    }

    LineRenderer MakeNewLineObject()
    {
        GameObject newLineObj = new GameObject();
        newLineObj.AddComponent<LineRenderer>();
        LineRenderer newLine = newLineObj.GetComponent<LineRenderer>();
        // Set line stuff here.







        return newLine;

    }

    public void ButtonInput(int i)
    {
        if (isDisplayed[i])
        {
            isDisplayed[i] = false;
            ProccesInput(i, false);
        }
        else
        {
            isDisplayed[i] = true;
            ProccesInput(i, true);
        }
    }

    public void ProccesInput(int i, bool b)
    {
        switch (i)
        {
            case 0:
                lineHolders.lineHolder1.SetActive(b);
                pointHolders.pointHolder1.gameObject.SetActive(b);
                break;
            case 1:
                lineHolders.lineHolder2.SetActive(b);
                pointHolders.pointHolder2.gameObject.SetActive(b);
                break;
            case 2:
                lineHolders.lineHolder3.SetActive(b);
                pointHolders.pointHolder3.gameObject.SetActive(b);
                break;
            case 3:
                lineHolders.lineHolder4.SetActive(b);
                pointHolders.pointHolder4.gameObject.SetActive(b);
                break;
            case 4:
                lineHolders.lineHolder5.SetActive(b);
                pointHolders.pointHolder5.gameObject.SetActive(b);
                break;
        }
    }
    public void TimeLine(float f)
    {
        timeLineRepresentation.value = f;
    }
}
