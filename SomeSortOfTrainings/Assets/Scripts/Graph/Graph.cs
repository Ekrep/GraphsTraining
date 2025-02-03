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
using UnityEditor.ShaderGraph.Internal;

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
        compiledFunctionPointers.Dispose();
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
        InitializeCompiledFunctionPointers();
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
        float outX;
        float outY;
        float outZ;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == graphProperties.resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            float u = (x + 0.5f) * step - 1f;
            functionMethodsDictionary[currentFunctionType](u, v, Time.time, out outX, out outY, out outZ);
            points[i].SetLocalPosition(new Vector3(outX, outY, outZ));
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
    private NativeHashMap<int, FunctionPointer<FunctionLibrary.FunctionMethod>> compiledFunctionPointers;
    #endregion
    #region Burst
    private void WaveFunctionBurst()
    {
        //Debug.Log(functionPointer.IsCreated);
        //float step = 2f / graphProperties.resolution;
        job.time = Time.time;
        job.resolution = graphProperties.resolution;
        handle = job.Schedule(pointTransformAcesses);
        handle.Complete();
        job.x = 0;
        job.z = 0;
    }
    private void InitalizeJobValues()
    {
        Transform[] transforms = new Transform[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            transforms[i] = points[i].transform;
        }
        float step = 2f / graphProperties.resolution;
        float v = 0.5f * step - 1f;
        pointTransformAcesses = new TransformAccessArray(transforms);
        job = new WaveJob()
        {
            time = Time.time,
            funcPtrs = compiledFunctionPointers,
            resolution = graphProperties.resolution,
            step = step,
            pointsLength = points.Length,
            z = 0,
            x = 0,
            v = v,
            u = 0
        };
        job.funcTypeKey = (int)currentFunctionType;


    }
    private void InitializeCompiledFunctionPointers()
    {
        compiledFunctionPointers = new NativeHashMap<int, FunctionPointer<FunctionLibrary.FunctionMethod>>(functionMethodsDictionary.Count, Allocator.Persistent);
        for (int i = 0; i < functionMethodsDictionary.Count; i++)
        {
            compiledFunctionPointers.Add((int)functionMethodsDictionary.ElementAt(i).Key,
            BurstCompiler.CompileFunctionPointer(functionMethodsDictionary.ElementAt(i).Value));
        }
    }
    private bool IsCurrentFunctionTypeChanged()
    {
        if (currentFunctionType != currentFunctionTypeHolder)
        {
            currentFunctionTypeHolder = currentFunctionType;
            job.funcTypeKey = (int)currentFunctionType;
            return true;
        }
        return false;

    }
    [BurstCompile]
    private struct WaveJob : IJobParallelForTransform
    {
        [ReadOnly] public int funcTypeKey;
        [ReadOnly] public NativeHashMap<int, FunctionPointer<FunctionLibrary.FunctionMethod>> funcPtrs;
        [ReadOnly] public float time;
        [WriteOnly] private float outX;
        [WriteOnly] private float outY;
        [WriteOnly] private float outZ;
        [ReadOnly] public int pointsLength;

        #region InJobValues
        [ReadOnly] public int resolution;
        [ReadOnly] public float step;
        [WriteOnly] public int x;
        [WriteOnly] public int z;
        [WriteOnly] public float v;
        [WriteOnly] public float u;
        #endregion

        public void Execute(int index, TransformAccess transform)
        {
            x++;
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            u = (x + 0.5f) * step - 1f;
            funcPtrs[funcTypeKey].Invoke(u, v, time, out outX, out outY, out outZ);
            transform.position = new Vector3(outX, outY, outZ);

        }
    }

    #endregion
}
