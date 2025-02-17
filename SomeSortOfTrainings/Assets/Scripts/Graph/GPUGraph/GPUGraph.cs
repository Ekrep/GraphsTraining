using UnityEngine;
using GraphProperties.GraphData;


public class GPUGraph : Graph
{

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
}
