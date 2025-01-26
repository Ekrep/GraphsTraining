using System.Collections.Generic;
using UnityEngine;
using static Utilities.Utils;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using System.Linq;

public partial class Graph : MonoBehaviour
{
    #region Variables
    [SerializeField] private Point pointPrefab;
    [SerializeField] private int resolution;
    [SerializeField] private Vector3 scale;
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
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 localPosition = points[i].GetLocalPosition();
            //localPosition.y = FunctionLibrary.MorphingWave(localPosition.x, Time.time);
            localPosition.y = functionMethodsDictionary[currentFunctionType](localPosition.x, localPosition.z, Time.time);
            points[i].SetLocalPosition(localPosition);
        }

    }
    private void InitializePoints()
    {
        points = new Point[resolution * resolution];
        float step = 2f / resolution;
        Vector3 scale = this.scale * step;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
            }
            points[i] = Instantiate(pointPrefab);
            points[i].SetPosition(new Vector3((x + 0.5f) * step - 1f, 0, (z + 0.5f) * step - 1f));
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
    private NativeHashMap<int, FunctionPointer<FunctionLibrary.FunctionMethod>> compiledFunctionPointers;
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
            currentFunctionKeyValue = (int)currentFunctionType,
            functionPtrs = compiledFunctionPointers
        };

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
            job.currentFunctionKeyValue = (int)currentFunctionType;
            return true;
        }
        return false;

    }
    [BurstCompile]
    private struct WaveJob : IJobParallelForTransform
    {
        [ReadOnly] public int currentFunctionKeyValue;
        [ReadOnly] public NativeHashMap<int, FunctionPointer<FunctionLibrary.FunctionMethod>> functionPtrs;
        [ReadOnly] public float time;
        public void Execute(int index, TransformAccess transform)
        {
            transform.position = new float3(transform.position.x, functionPtrs[currentFunctionKeyValue].Invoke(transform.position.x, transform.position.y, time), transform.position.z);
        }
    }

    #endregion
}
