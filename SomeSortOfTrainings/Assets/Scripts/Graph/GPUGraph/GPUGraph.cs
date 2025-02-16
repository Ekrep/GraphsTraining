using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProperties.GraphData;
using static Utilities.Utils;
using UnityEditor;


public class GPUGraph : MonoBehaviour
{
    #region Regular Variables
    [SerializeField] private GraphPropertiesScriptable graphProperties;
    private Point[] points;
    [SerializeField] private FunctionTypes currentFunctionType;
    private FunctionTypes currentFunctionTypeHolder;
    private Dictionary<FunctionTypes, FunctionLibrary.FunctionMethod> functionMethodsDictionary
    = new Dictionary<FunctionTypes, FunctionLibrary.FunctionMethod>();

    [SerializeField, Min(0f)] private float functionDuration = 1f, transitionDuration = 1f;
    private float duration;
    [SerializeField] private bool useRandomFunctionSelect = false;
    [SerializeField] private bool transitioning = false;
    private FunctionLibrary.FunctionMethod transitionFunction;
    private FunctionTypes transitionFunctionTypeHolder;
    #endregion
    #region Compute Shader Varaibles
    ComputeBuffer positionsBuffer;
    [SerializeField] private ComputeShader computeShader;

    private static readonly int positionsId = Shader.PropertyToID("_Positions");
    private static readonly int resolutionId = Shader.PropertyToID("_Resolution");
    private readonly int stepId = Shader.PropertyToID("_Step");
    private readonly int timeId = Shader.PropertyToID("_Time");

    [SerializeField] private Material material;

    #endregion

    void OnEnable()
    {
        positionsBuffer = new ComputeBuffer(graphProperties.resolution * graphProperties.resolution, 3 * 4);
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
        float step = 2f / graphProperties.resolution;
        computeShader.SetInt(resolutionId, graphProperties.resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);
        computeShader.SetBuffer(0, positionsId, positionsBuffer);
        int groups = Mathf.CeilToInt(graphProperties.resolution / 8f);
        computeShader.Dispatch(0, groups, groups, 1);
        material.SetBuffer(positionsId, positionsBuffer);
        material.SetFloat(stepId, step);
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / graphProperties.resolution));
        Graphics.DrawMeshInstancedProcedural(graphProperties.meshType, 0, material, bounds, positionsBuffer.count);

    }
}
