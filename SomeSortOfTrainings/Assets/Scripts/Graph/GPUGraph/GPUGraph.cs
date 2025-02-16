using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProperties.GraphData;
using static Utilities.Utils;
using UnityEditor;
using System;


public class GPUGraph : MonoBehaviour
{
    #region Regular Variables
    [SerializeField] private GraphPropertiesScriptable graphProperties;
    [SerializeField] private FunctionTypes currentFunctionType;

    [SerializeField, Min(0f)] private float functionDuration = 1f, transitionDuration = 1f;
    private float duration;
    [SerializeField] private bool useRandomFunctionSelect = false;
    [SerializeField] private bool transitioning = false;
    private FunctionTypes transitionFunctionTypeHolder;

    [SerializeField] private int totalFunctionsCount;
    #endregion
    #region Compute Shader Varaibles
    ComputeBuffer positionsBuffer;
    [SerializeField] private ComputeShader computeShader;

    private static readonly int positionsId = Shader.PropertyToID("_Positions");
    private static readonly int resolutionId = Shader.PropertyToID("_Resolution");
    private readonly int stepId = Shader.PropertyToID("_Step");
    private readonly int timeId = Shader.PropertyToID("_Time");
    private readonly int transitionProgressId = Shader.PropertyToID("_TransitionProgress");

    [SerializeField] private Material material;

    #endregion

    void OnEnable()
    {
        positionsBuffer = new ComputeBuffer(GraphPropertiesScriptable.maxResolution * GraphPropertiesScriptable.maxResolution, 3 * 4);
        totalFunctionsCount = FunctionLibrary.GetAllMehtodsCount();
    }
    void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
    }
    void Update()
    {
        UpdateFunctionOnGPU();
    }
    private void UpdateFunctionOnGPU()
    {
        duration += Time.deltaTime;
        if (transitioning)
        {
            if (duration >= transitionDuration)
            {
                duration -= transitionDuration;
                transitioning = false;
                currentFunctionType = transitionFunctionTypeHolder;

            }
            computeShader.SetFloat(transitionProgressId, Mathf.SmoothStep(0, 1f, duration / transitionDuration));
        }
        else if (duration >= functionDuration)
        {
            duration -= functionDuration;
            transitionFunctionTypeHolder = GiveRandomOrNextFunctionType();
            transitioning = true;

        }
        float step = 2f / graphProperties.resolution;
        computeShader.SetInt(resolutionId, graphProperties.resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);
        int kernelIndex = (int)currentFunctionType + (int)(transitioning ? (int)transitionFunctionTypeHolder : (int)currentFunctionType) * totalFunctionsCount;
        computeShader.SetBuffer(kernelIndex, positionsId, positionsBuffer);
        int groups = Mathf.CeilToInt(graphProperties.resolution / 8f);
        computeShader.Dispatch(kernelIndex, groups, groups, 1);
        material.SetBuffer(positionsId, positionsBuffer);
        material.SetFloat(stepId, step);
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / graphProperties.resolution));
        Graphics.DrawMeshInstancedProcedural(graphProperties.meshType, 0, material, bounds, graphProperties.resolution * graphProperties.resolution);


    }
    private FunctionTypes GiveRandomOrNextFunctionType()
    {
        return (FunctionTypes)(Convert.ToInt16(!useRandomFunctionSelect) * GetNextFuncIndex())
        + Convert.ToInt16(useRandomFunctionSelect) * CheckFunctionIndexMatch((int)currentFunctionType, UnityEngine.Random.Range(0, totalFunctionsCount));
    }
    private int GetNextFuncIndex()
    {
        return ((int)currentFunctionType + 1) % totalFunctionsCount;
    }
    private int CheckFunctionIndexMatch(int currentIndex, int randomIndex)
    {
        if (currentIndex == randomIndex)
        {
            return randomIndex + 1 % totalFunctionsCount;
        }
        else
        {
            return randomIndex;
        }

    }
}
