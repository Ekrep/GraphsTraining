using System.Collections.Generic;
using UnityEngine;
using static Utilities.Utils;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Linq;
using GraphProperties.GraphData;
using System;
using Unity.Mathematics;

public partial class GraphCPU : Graph
{

    private Point[] points;
    private FunctionLibrary.FunctionMethod transitionFunction;
    private Dictionary<FunctionTypes, FunctionLibrary.FunctionMethod> functionMethodsDictionary
= new Dictionary<FunctionTypes, FunctionLibrary.FunctionMethod>();

    private void OnDisable()
    {
        pointTransformAcesses.Dispose();
        compiledFunctionPointers.Dispose();
    }

    private void Update()
    {
        //HandleFunctionDurationAndChangeNextFunction();
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
            IsCurrentFunctionTypeChanged();
            WaveFunctionBurst();
        }
        else
        {
            if (transitioning)
            {
                UpdateFunctionTransition();
            }
            else
            {
                WaveFunction();
            }

        }
        duration += Time.deltaTime;
        if (transitioning)
        {

            if (duration >= transitionDuration)
            {
                duration -= transitionDuration;
                transitioning = false;
                currentFunctionType = transitionFunctionTypeHolder;

            }
        }
        else if (duration >= functionDuration)
        {
            duration -= functionDuration;
            transitioning = true;
            transitionFunctionTypeHolder = GiveRandomOrNextFunctionType();
            transitionFunction = functionMethodsDictionary[transitionFunctionTypeHolder];
        }
    }
    protected override void Initialize()
    {
        base.Initialize();
        currentFunctionTypeHolder = currentFunctionType;
        InitializeFuncDictionary();
        InitializePoints();
        InitializeCompiledFunctionPointers();
        InitalizeJobValues();
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

    private void UpdateFunctionTransition()
    {
        FunctionLibrary.FunctionMethod currentFunc = functionMethodsDictionary[currentFunctionType];
        float progress = duration / transitionDuration;
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
            points[i].SetLocalPosition(FunctionLibrary.Morph(
                u, v, Time.time, currentFunc, transitionFunction, progress));
        }

    }
}

//MULTITHREADING
public partial class GraphCPU
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
        job.isTransition = transitioning;
        job.progress = duration / transitionDuration;
        job.transitionFunctionIndex = (int)transitionFunctionTypeHolder;
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
            progress = duration / transitionDuration,
            transitionFunctionIndex = 0,
            isTransition = false,
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
        [ReadOnly] public int transitionFunctionIndex;
        [ReadOnly] public float progress;
        [ReadOnly] public bool isTransition;
        [WriteOnly] public int x;
        [WriteOnly] public int z;
        [WriteOnly] public float v;
        [WriteOnly] public float u;
        #endregion

        public void Execute(int index, TransformAccess transform)
        {
            UpdateGraph(transform);
        }
        private void UpdateGraph(TransformAccess transform)
        {
            x++;
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            u = (x + 0.5f) * step - 1f;
            if (!isTransition)
            {
                funcPtrs[funcTypeKey].Invoke(u, v, time, out outX, out outY, out outZ);
                transform.position = new Vector3(outX, outY, outZ);
            }
            else
            {
                transform.position = Morph(u, v, time, funcPtrs[funcTypeKey], funcPtrs[transitionFunctionIndex], progress);
            }



        }
        private Vector3 Morph(float u, float v, float t, FunctionPointer<FunctionLibrary.FunctionMethod> fromFunc, FunctionPointer<FunctionLibrary.FunctionMethod> to, float progress)
        {
            float x1 = 0, y1 = 0, z1 = 0, x2 = 0, y2 = 0, z2 = 0;
            fromFunc.Invoke(u, v, t, out x1, out y1, out z1);
            to.Invoke(u, v, t, out x2, out y2, out z2);
            Vector3 fromVec = new Vector3(x1, y1, z1);
            Vector3 toVec = new Vector3(x2, y2, z2);
            return Vector3.LerpUnclamped(fromVec, toVec, math.smoothstep(0f, 1f, progress));
        }
    }

    #endregion
}
