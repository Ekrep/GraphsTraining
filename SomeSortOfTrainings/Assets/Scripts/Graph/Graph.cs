using System.Collections.Generic;
using UnityEngine;
using static Utilities.Utils;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using System.Linq;
using GraphProperties.GraphData;

public partial class Graph : MonoBehaviour
{
    #region Variables
    [SerializeField] private GraphPropertiesScriptable graphProperties;
    private Point[] points;
    [SerializeField] private FunctionTypes currentFunctionType;
    private FunctionTypes currentFunctionTypeHolder;
    private Dictionary<FunctionTypes, FunctionLibrary.FunctionMethod> functionMethodsDictionary
    = new Dictionary<FunctionTypes, FunctionLibrary.FunctionMethod>();
    #endregion


    private void OnDisable()
    {
        pointTransformAcesses.Dispose();
    }

    void Start()
    {
        #region y=x^3
        // float step = 2f / resolution;
        // Vector3 position = Vector3.zero;
        // Vector3 scale = this.scale * step;
        // for (int i = 0; i < resolution; i++)
        // {
        //     Point point = Instantiate(pointPrefab);
        //     position.x = (i + 0.5f) * step - 1f;
        //     point.SetLocalPosition(new Vector3(position.x, position.x * position.x * position.x, 0));
        //     point.SetScale(scale);
        //     point.SetParent(this.transform, false);
        // }
        #endregion
        #region  y=x^2 Training
        // for (int i = -pointAmount; i < pointAmount; i++)
        // {
        //     Point point=Instantiate(pointPrefab);
        //     float positionX = (i + 0.5f) / 5f - 1f;
        //     Vector3 pointPosition = new Vector3(positionX, positionX * positionX);
        //     point.SetPosition(pointPosition);
        //     point.SetScale(Vector3.one / 5);
        // }
        #endregion
        currentFunctionTypeHolder = currentFunctionType;
        InitializeFuncDictionary();
        InitializePoints();
        InitalizeJobValues();
    }

    private void Update()
    {
        //FOR FPS COUNTER(BUILD) MESSY I KNOW, WILL REFACTOR LATER
        if (Input.GetKeyDown(KeyCode.B))
        {
            useBurst = true;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            useBurst = false;
        }
        if (useBurst)
        {
            WaveFunctionBurst();
            IsCurrentFunctionTypeChanged();
        }
        else
        {
            WaveFunction();
        }
    }
    private void WaveFunction()
    {
        //v means z axis, u means x axis
        float step = 2f / graphProperties.resolution;
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == graphProperties.resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            float u = (x + 0.5f) * step - 1f;
            points[i].SetLocalPosition(functionMethodsDictionary[currentFunctionType](u, v, Time.time));
        }

    }
    private void InitializePoints()
    {
        points = new Point[graphProperties.resolution * graphProperties.resolution];
        float step = 2f / graphProperties.resolution;
        Vector3 scale = graphProperties.pointScale * step;
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = Instantiate(graphProperties.pointPrefab);
            points[i].pointMeshFilter.mesh = graphProperties.meshType;
            points[i].SetScale(scale);
            points[i].SetParent(transform, false);
        }

    }
    private void InitializeFuncDictionary()
    {
        List<FunctionLibrary.MethodFunctionData> functions = FunctionLibrary.GetAllMehtods();
        for (int i = 0; i < functions.Count; i++)
        {
            functionMethodsDictionary.Add(functions[i].type, functions[i].func);
        }

    }
}

//MULTITHREADING
public partial class Graph
{
    #region BurstCompileVariables
    [SerializeField] private bool useBurst = false;
    private WaveJob job = new WaveJob();
    private TransformAccessArray pointTransformAcesses;
    private JobHandle handle;
    #endregion
    #region Burst
    private void WaveFunctionBurst()
    {
        //Debug.Log(functionPointer.IsCreated);
        job.time = Time.time;
        handle = job.Schedule(pointTransformAcesses);
        handle.Complete();
    }
    private void InitalizeJobValues()
    {
        Transform[] transforms = new Transform[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            transforms[i] = points[i].transform;
        }
        pointTransformAcesses = new TransformAccessArray(transforms);
        job = new WaveJob()
        {
            time = Time.time,
            currentFunc = new FunctionLibrary.WaveFunction(),
        };
        job.currentFunc.functionType = (int)currentFunctionType;

    }
    private bool IsCurrentFunctionTypeChanged()
    {
        if (currentFunctionType != currentFunctionTypeHolder)
        {
            currentFunctionTypeHolder = currentFunctionType;
            job.currentFunc.functionType = (int)currentFunctionType;
            return true;
        }
        return false;

    }
    [BurstCompile]
    private struct WaveJob : IJobParallelForTransform
    {
        [ReadOnly] public FunctionLibrary.WaveFunction currentFunc;
        [ReadOnly] public float time;
        public void Execute(int index, TransformAccess transform)
        {
            transform.position = currentFunc.Evaluate(transform.position.x, transform.position.z, time);
        }
    }

    #endregion
}
