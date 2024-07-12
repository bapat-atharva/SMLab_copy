using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;

public class SmallMultiplesCurvedStatic : MonoBehaviour
{
    private GameObject mainAxes;
    private int gridRows = 3;
    private int gridCols = 3;

    private int spacing = 4; // Spacing between small multiples
    private int gridDim = 3; // Spacing between small multiples

    private List<GameObject> dataPoints = new List<GameObject>();
    Dictionary<int, List<GameObject>> datapointTrailMap = new Dictionary<int, List<GameObject>>(); //trail
    Dictionary<int, List<Vector3>> datapointPositionsList = new Dictionary<int, List<Vector3>>();
    Dictionary<int, Vector3> datapointBasePos = new Dictionary<int, Vector3>();

    SortedSet<float> yearSet = new SortedSet<float>();

    private int currentInd = 0;
    private bool play = false;
    private Coroutine intervalCoroutine;
    private int numValues;
    private float anglex = -15.0f;
    private int pointDebughash;
    Quaternion globalRotation = Quaternion.Euler(0, 15.0f, 0); // Örneðin Z ekseni etrafýnda 45 derece

    private Dictionary<int, string> dpTextureMap = new Dictionary<int, string> {
            {0, "argentina" },
            {1, "brazil" },
            {2, "france" },
            {3, "germany" },
            {4, "italy" },
            {5, "japan" },
            {6, "turkey" },
            {7, "usa" },
            {8, "russia" },
        };

    void Start()
    {
        CreateMainAxes();
        CreateSmallMultiples();

        CSVReader reader = new CSVReader();
        var data = reader.ReadData();
        int col_counter = 0;
        float anglexx = -25.0f;
        for (int i = 0; i < gridCols * gridRows; i++)
        {
            var dKey = data.Keys.ElementAt(i);
            var hMap = dataPoints.ElementAt(i).GetHashCode();

            List<Vector3> positions = new List<Vector3>();
            List<GameObject> trails = new List<GameObject>();

            foreach (var v in data[dKey])
            {
                var year = (float)Math.Round(v.Year, 0); // * gridCols;
                var life = (float)Math.Round(v.LifeExpectancy, 2) * gridCols;
                var infant = (float)Math.Round(v.InfantDeaths, 2) * gridCols;

                if (i == 0 || i == 3 || i == 6)
                    anglexx = -25.0f;
                if (i == 1 || i == 4 || i == 7)
                    anglexx = 0.0f;
                if (i == 2 || i == 5 || i == 8)
                    anglexx = 25.0f;
                //Debug.Log(i);

                globalRotation = Quaternion.Euler(0, anglexx, 0);
                Vector3 position = datapointBasePos[hMap] + globalRotation * new Vector3(year, life, infant);
                positions.Add(position);

                GameObject trail = CreateTrail(position);
                trails.Add(trail);


            }

            datapointPositionsList.Add(hMap, positions);
            datapointTrailMap.Add(hMap, trails);

            // Set final position
            dataPoints.ElementAt(i).transform.position = positions.Last();
            foreach (GameObject trail in trails)
            {
                trail.SetActive(true); // Activate all trail points
            }
        }
    }


    private void Update()
    {


    }

    private void OnDestroy()
    {

    }


    void CreateMainAxes()
    {
        mainAxes = new GameObject("MainAxes");
        mainAxes.transform.localScale = Vector3.one;

        //Debug.Log(mainAxes.transform.localScale);

        //  CreateAxis(Vector3.right, Color.red, "XAxis");
        //  CreateAxis(Vector3.up, Color.green, "YAxis");
        //  CreateAxis(Vector3.forward, Color.blue, "ZAxis");


    }

    void CreateAxis(Vector3 direction, Color color, string name)
    {
        GameObject axis = new GameObject(name);
        axis.transform.parent = mainAxes.transform;

        LineRenderer lineRenderer = axis.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, direction * gridDim * spacing); // Adjust length as needed
        lineRenderer.transform.localScale = Vector3.one;

        lineRenderer.useWorldSpace = false;

        //CreateAxisLabel(axis.transform, direction * gridDim * spacing, "labelText");
    }

    void CreateSmallMultiples()
    {
        for (int row = 0; row < gridRows; row++)
        {
            anglex = -25.0f;
            for (int col = 0; col < gridCols; col++)
            {
                if (col == 0)
                {
                    anglex = anglex + 0.0f;  // Eðer sütun 0 ise temp'e 0 ata
                }
                else if (col == 1)
                {
                    anglex = anglex + 25.0f; // Eðer sütun 1 ise temp'e -1 ata
                }
                else
                {
                    anglex = anglex + 50.0f;  // Diðer durumlar için temp'e -2 ata
                }
                Vector3 basePosition = new Vector3(col * spacing, row * spacing, 0);
                CreateSmallMultiple(row, col, basePosition);
            }
        }
    }

    void CreateSmallMultiple(int row, int col, Vector3 basePosition)
    {

        CreateAxes(col * spacing + 1, row * spacing + 1, anglex);

        Vector3 position = new Vector3(col * spacing + 1, row * spacing + 1);
        GameObject dp = CreateDataPoint(position);

        datapointBasePos.Add(dp.GetHashCode(), position); // new Vector3(col * 4 + 1, row * 4 + 1, 0));
    }

    void CreateAxes(int basex, int basey, float angleZ)
    {
        Quaternion rotationZ = Quaternion.Euler(0, angleZ, 0);

        var p1 = new Vector3(basex, basey, 0);
        var p2 = new Vector3(basex + gridDim, basey, 0);
        p2 = rotationZ * (p2 - p1) + p1;
        CreateAxis(Vector3.right, Color.red, "XAxis", p1, p2);

        p1 = new Vector3(basex, basey, 0);
        p2 = new Vector3(basex, basey + gridDim, 0);
        p2 = rotationZ * (p2 - p1) + p1;
        CreateAxis(Vector3.up, Color.green, "YAxis", p1, p2);

        p1 = new Vector3(basex, basey, 0);
        p2 = new Vector3(basex, basey, gridDim);
        p2 = rotationZ * (p2 - p1) + p1;
        CreateAxis(Vector3.forward, Color.blue, "ZAxis", p1, p2);
    }


    GameObject CreateAxis(Vector3 direction, Color color, string name, Vector3 p1, Vector3 p2)
    {
        GameObject axis = new GameObject(name);
        axis.transform.parent = mainAxes.transform;

        LineRenderer lineRenderer = axis.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, p1);
        lineRenderer.SetPosition(1, p2); // Adjust length as needed
        lineRenderer.useWorldSpace = false;

        return axis;

    }


    GameObject CreateDataPoint(Vector3 position)
    {
        GameObject dataPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Vector3 rotatedPosition = position; // Rotasyon uygula
        dataPoint.transform.position = rotatedPosition;
        dataPoint.transform.localScale = Vector3.one * 0.7f; // Boyutu ayarla
        dataPoint.transform.parent = mainAxes.transform;

        Renderer renderer = dataPoint.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        renderer.material.color = Color.yellow;

        dataPoints.Add(dataPoint);

        return dataPoint;
    }

    GameObject CreateTrail(Vector3 oldPos)
    {
        // Ýlk olarak pozisyona global rotasyonu uygula
        Vector3 rotatedPosition = oldPos;
        GameObject dataPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dataPoint.transform.position = rotatedPosition;
        dataPoint.transform.localScale = Vector3.one * 0.1f;  // Boyutu ayarla
        dataPoint.transform.parent = mainAxes.transform;  // Ebeveyni ayarla

        Renderer renderer = dataPoint.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        renderer.material.color = Color.white;

        return dataPoint;
    }




}