using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utilities.Utils;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
public class Graph : MonoBehaviour
{
    #region Variables
    [SerializeField] private Point pointPrefab;
    [SerializeField] private int resolution;
    [SerializeField] private Vector3 scale;
    private Point[] points;
    [SerializeField] private FunctionTypes currentFunctionType;
    private Dictionary<FunctionTypes, FunctionLibrary.FunctionMethod> functionMethodsDictionary
    = new Dictionary<FunctionTypes, FunctionLibrary.FunctionMethod>();
    #endregion
    #region BurstCompileVariables
    [SerializeField] private bool useBurst = false;
    NativeArray<Vector3> pointPositions;
    NativeArray<float> calculationResults;
    #endregion

    private void OnDisable()
    {
        pointPositions.Dispose();
        calculationResults.Dispose();
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
        InitializeFuncDictionary();
        InitializePoints();
        pointPositions = new NativeArray<Vector3>(points.Length, Allocator.Persistent);
        calculationResults = new NativeArray<float>(points.Length, Allocator.Persistent);
    }

    private void Update()
    {
        if (useBurst)
        {
            WaveFunctionBurst();
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
    #region Burst
    private void WaveFunctionBurst()
    {
        FunctionLibrary.FunctionMethod method = functionMethodsDictionary[currentFunctionType];
        FunctionPointer<FunctionLibrary.FunctionMethod> functionPointer = BurstCompiler.CompileFunctionPointer<FunctionLibrary.FunctionMethod>(method);
        //Debug.Log(functionPointer.IsCreated);
        for (int i = 0; i < points.Length; i++)
        {
            pointPositions[i] = points[i].GetLocalPosition();
        }
        WaveJob job = new WaveJob
        {
            pointPositions = pointPositions,
            functionPtr = functionPointer,
            results = calculationResults,
            time = Time.time


        };
        JobHandle handle = job.Schedule(points.Length, Mathf.Max(1, points.Length / SystemInfo.processorCount));
        handle.Complete();

        for (int i = 0; i < calculationResults.Length; i++)
        {
            Vector3 localPos = points[i].GetLocalPosition();
            points[i].SetLocalPosition(new Vector3(localPos.x, calculationResults[i], localPos.z));
        }
    }
    [BurstCompile]
    private struct WaveJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> pointPositions;
        [ReadOnly] public FunctionPointer<FunctionLibrary.FunctionMethod> functionPtr;
        [WriteOnly] public NativeArray<float> results;
        [ReadOnly] public float time;
        public void Execute(int index)
        {
            results[index] = functionPtr.Invoke(pointPositions[index].x, pointPositions[index].z, time);
        }
    }
    #endregion
}
