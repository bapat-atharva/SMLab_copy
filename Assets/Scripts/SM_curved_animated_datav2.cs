using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using Button = UnityEngine.UI.Button;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
using Slider = UnityEngine.UI.Slider;
using TMPro;

public class SmallMultiplesController_inter_curved_datav2 : MonoBehaviour
{
    public Button moveButtonNext;
    public Button moveButtonBack;
    public Button playPauseButton;
    //public Text playPauseButtonText;
    public Slider slider;
    public TMP_Text yearText;

    private GameObject mainAxes;
    private int gridRows = 3;
    private int gridCols = 3;

    private int spacing = 4; // Spacing between small multiples
    private int gridDim = 3; // Spacing between small multiples

    private List<GameObject> dataPoints = new List<GameObject>();
    Dictionary<int, GameObject> dataPointsMap = new Dictionary<int, GameObject>();
    Dictionary<int, List<GameObject>> datapointTrailMap = new Dictionary<int, List<GameObject>>(); //trail
    Dictionary<int, List<Vector3>> datapointPositionsList = new Dictionary<int, List<Vector3>>();
    Dictionary<int, Vector3> datapointBasePos = new Dictionary<int, Vector3>();
    Dictionary<int, List<float>> datapointSizeList = new Dictionary<int, List<float>>();

    private Dictionary<int, string> dpTextureMap = new Dictionary<int, string>();

    SortedSet<float> yearSet = new SortedSet<float>();

    private int currentInd = 0;
    private bool play = false;
    private Coroutine intervalCoroutine;
    private int numValues;
    private float anglex = -15.0f;
    Quaternion globalRotation = Quaternion.Euler(0, 15.0f, 0);
    private int pointDebughash;
    void Start()
    {
        CreateMainAxes();
        CreateSmallMultiples();

        moveButtonNext.onClick.AddListener(MoveDataPointNext);
        moveButtonBack.onClick.AddListener(MoveDataPointBack);
        playPauseButton.onClick.AddListener(PlayOrPause);
        slider.onValueChanged.AddListener(OnSliderValueChanged);

        CSVReaderv2 reader = new CSVReaderv2();

        var data = reader.ReadData();

        float anglexx = -25.0f;

        for (int i = 0; i < gridCols * gridRows; i++)
        {
            var dKey = data.Keys.ElementAt(i);
            var hMap = dataPoints.ElementAt(i).GetHashCode();

            dataPointsMap.Add(hMap, dataPoints.ElementAt(i));

            List<Vector3> t = new List<Vector3>();
            List<GameObject> trails = new List<GameObject>();
            List<float> t2 = new List<float>();
            mainAxes.transform.localScale = Vector3.one;


            foreach (var v in data[dKey])
            {
                var year = (float)Math.Round(v.Year, 0);
                var life = (float)Math.Round(v.LifeExpectancy, 2) * gridCols;
                var infant = (float)Math.Round(v.InfantDeaths, 2) * gridCols;
                var birth = (float)Math.Round(v.BirthRate, 2) * gridCols;

                var population = (float)Math.Round(v.Population, 2);

                if (i == 0 || i == 3 || i == 6)
                    anglexx = -25.0f;
                if (i == 1 || i == 4 || i == 7)
                    anglexx = 0.0f;
                if (i == 2 || i == 5 || i == 8)
                    anglexx = 25.0f;

                globalRotation = Quaternion.Euler(0, anglexx, 0);
                t.Add(datapointBasePos[hMap] + globalRotation * new Vector3(life, infant, birth));
                t2.Add(population);
                var trail = CreateTrail(datapointBasePos[hMap] + globalRotation * new Vector3(life, infant, birth));

                trails.Add(trail);

                yearSet.Add(year);

            }

            dpTextureMap.Add(hMap, data[dKey][0].Country.ToLower());

            datapointPositionsList.Add(hMap, t);
            datapointSizeList.Add(hMap, t2);
            datapointTrailMap.Add(hMap, trails);
        }

        numValues = datapointPositionsList[dataPoints[0].GetHashCode()].Count;

        //playPauseButtonText.text = "Play";

        mainAxes.transform.localScale = Vector3.one;

        foreach (var dp in dataPoints)
        {
            dp.transform.position = getNextPoint(dp.GetHashCode());
            dp.transform.localScale = getNextSize(dp.GetHashCode());

        }

        pointDebughash = dataPoints.ElementAt(0).GetHashCode();

        mainAxes.transform.localScale = Vector3.one;

        changeTrail();
        ApplyTexture();

    }

    private void Update()
    {


    }

    private void OnDestroy()
    {
        moveButtonBack.onClick.RemoveAllListeners();
        moveButtonNext.onClick.RemoveAllListeners();
        playPauseButton.onClick.RemoveAllListeners();

        slider.onValueChanged.RemoveAllListeners();
    }

    void CreateMainAxes()
    {
        mainAxes = new GameObject("MainAxes");
        mainAxes.transform.localScale = Vector3.one;

        //Debug.Log(mainAxes.transform.localScale);

        // CreateAxis(Vector3.right, Color.red, "XAxis");
        // CreateAxis(Vector3.up, Color.green, "YAxis");
        // CreateAxis(Vector3.forward, Color.blue, "ZAxis");

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
        dataPoint.transform.position = position;
        dataPoint.transform.localScale = Vector3.one * 0.7f; // Adjust size as needed
        dataPoint.transform.parent = mainAxes.transform;

        Renderer renderer = dataPoint.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        renderer.material.color = Color.yellow;

        dataPoints.Add(dataPoint);

        return dataPoint;
    }

    void ApplyTexture()
    {
        foreach (GameObject g in dataPoints)
        {
            var hc = g.GetHashCode();

            Texture2D texture = Resources.Load<Texture2D>("Textures/" + dpTextureMap[hc]);

            Renderer renderer = g.GetComponent<Renderer>();
            // Apply the texture to the material
            if (texture != null)
            {
                //Debug.Log(texture);
                Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.mainTexture = texture;
                renderer.material = material;
            }
            else
            {
                Debug.LogWarning("Texture not found!");
                renderer.material.color = Color.yellow; // Default color if texture is not found
            }
        }
    }


    void MoveDataPoint(int mul)
    {
        foreach (GameObject point in dataPoints)
        {
            Vector3 vector3 = getNextPoint(point.GetHashCode());

            //Vector3 newPosition = point.transform.position + new Vector3(Random.Range(0, .3f), Random.Range(0, .3f), Random.Range(0, .3f)) * mainAxes.transform.localScale.y / 5;
            Vector3 newPosition = vector3; // point.transform.position + ; // * mainAxes.transform.localScale.y / 5;

            point.transform.position = newPosition;
            point.transform.localScale = getNextSize(point.GetHashCode());

            yearText.text = yearSet.ElementAt(currentInd).ToString();

        }
    }

    GameObject CreateTrail(Vector3 oldPos)
    {
        GameObject dataPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dataPoint.transform.position = oldPos;
        dataPoint.transform.localScale = Vector3.one * 0.1f; // Adjust size as needed
        dataPoint.transform.parent = mainAxes.transform;

        Renderer renderer = dataPoint.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        renderer.material.color = Color.white;

        return dataPoint;
    }

    Vector3 getNextPoint(int hash)
    {
        var l = datapointPositionsList[hash];

        var v = l[currentInd];
        //if (hash == dataPoints.ElementAt(0).GetHashCode())
        //Debug.Log(v[0]+""+ v[1] +""+ v[2]);

        return v;
    }


    Vector3 getNextSize(int hash)
    {
        var l = datapointSizeList[hash];

        var ratio = dataPointsMap[hash].transform.localScale.magnitude / mainAxes.transform.localScale.magnitude;

        return l[currentInd] * 0.5f * Vector3.one;
    }

    void MoveDataPointNext()
    {
        if (currentInd < numValues - 1)
            currentInd++;
        MoveDataPoint(1);

        changeTrail();
    }

    void MoveDataPointBack()
    {
        if (currentInd >= 1)
            currentInd--;
        MoveDataPoint(-1);

        changeTrail();
    }

    void changeTrail()
    {
        foreach (var dp in dataPoints)
        {
            var trail = datapointTrailMap[dp.GetHashCode()];
            for (int i = 0; i < trail.Count; i++)
            {
                if (i < currentInd)
                    trail[i].SetActive(true);
                else trail[i].SetActive(false);
            }

        }
    }

    void OnSliderValueChanged(float value)
    {
        int v = (int)(value * (numValues - 1));

        currentInd = v;
        MoveDataPoint(1);
        changeTrail();
    }

    void PlayOrPause()
    {
        if (play) // means now pause
        {
            play = false;
            //playPauseButtonText.text = "Play";
            if (intervalCoroutine != null)
            {
                StopCoroutine(intervalCoroutine);
                intervalCoroutine = null;
            }
        }
        else
        {
            play = true;

            //playPauseButtonText.text = "Pause";

            intervalCoroutine = StartCoroutine(IntervalCoroutine());
        }
    }

    IEnumerator IntervalCoroutine()
    {
        while (true)
        {
            // Call the function you want to run at intervals
            //MoveDataPointNext();
            ppRoutine();
            yield return new WaitForSeconds(1);
        }
    }

    void ppRoutine()
    {
        if (currentInd >= numValues - 1 && intervalCoroutine != null)
        {
            StopCoroutine(intervalCoroutine);
            intervalCoroutine = null;
        }
        else

            MoveDataPointNext();

    }
}