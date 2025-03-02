using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using GraphProperties.FractalData;
using static Unity.Mathematics.math;
using quartenion = Unity.Mathematics.quaternion;
using Random = UnityEngine.Random;
public class FractalProceduralJobs : MonoBehaviour
{
    [SerializeField] private FractalData fractalData;
    private struct ProceduralFractalPart
    {
        public float3 worldPosition;
        public quartenion rotation, worldRotation;
        public float maxSagAngle, spinAngle, spinVelocity;

    }
    private static quartenion[] rotations = {
        quartenion.identity,
        quartenion.RotateZ(-0.5f * PI), quartenion.RotateZ(0.5f * PI),
        quartenion.RotateX(0.5f * PI), quartenion.RotateX(-0.5f * PI)
    };
    protected ComputeBuffer[] matricesBuffers;

    private static readonly int matricesID = Shader.PropertyToID("_Matrices");
    private static readonly int colorAId = Shader.PropertyToID("_ColorA");
    private static readonly int colorBId = Shader.PropertyToID("_ColorB");
    private static readonly int sequenceNumbersID = Shader.PropertyToID("_SequenceNumbers");
    private static MaterialPropertyBlock propertyBlock;
    private NativeArray<ProceduralFractalPart>[] parts;
    private NativeArray<float3x4>[] matrices;
    private Vector4[] sequenceNumbers;
    void OnEnable()
    {
        parts = new NativeArray<ProceduralFractalPart>[fractalData.depth];
        matrices = new NativeArray<float3x4>[fractalData.depth];
        matricesBuffers = new ComputeBuffer[fractalData.depth];
        sequenceNumbers = new Vector4[fractalData.depth];
        int stride = 12 * 4;
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new NativeArray<ProceduralFractalPart>(length, Allocator.Persistent);
            matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
            matricesBuffers[i] = new ComputeBuffer(length, stride);
            sequenceNumbers[i] = new Vector4(Random.value, Random.value, Random.value, Random.value);
        }

        parts[0][0] = CreatePart(0);
        //li==LevelIndex
        for (int li = 1; li < parts.Length; li++)
        {
            NativeArray<ProceduralFractalPart> levelParts = parts[li];
            //fpi==fractal part iterator
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5)
            {
                //ci==child index
                for (int ci = 0; ci < 5; ci++)
                {
                    levelParts[fpi + ci] = CreatePart(ci);
                }

            }
        }
        propertyBlock ??= new MaterialPropertyBlock();
    }
    void OnDisable()
    {
        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            matricesBuffers[i].Release();
            parts[i].Dispose();
            matrices[i].Dispose();
        }
        parts = null;
        matrices = null;
        matricesBuffers = null;
        sequenceNumbers = null;
    }
    private void Update()
    {
        ProceduralFractalPart rootPart = parts[0][0];
        rootPart.worldRotation = mul(transform.rotation, mul(rootPart.rotation, quartenion.RotateY(rootPart.spinAngle)));
        rootPart.worldPosition = transform.position;
        rootPart.spinAngle += rootPart.spinVelocity * Time.deltaTime;
        float objectScale = transform.lossyScale.x;
        parts[0][0] = rootPart;
        float3x3 r = float3x3(rootPart.worldRotation) * objectScale;
        matrices[0][0] = float3x4(r.c0, r.c1, r.c2, rootPart.worldPosition);
        float scale = objectScale;
        JobHandle jobHandle = default;
        for (int li = 1; li < parts.Length; li++)
        {
            scale *= 0.5f;
            jobHandle = new UpdateFractalJob
            {
                deltaTime = Time.deltaTime,
                scale = scale,
                parents = parts[li - 1],
                parts = parts[li],
                matrices = matrices[li]
            }.ScheduleParallel(parts[li].Length, 1, jobHandle);
        }
        jobHandle.Complete();
        Bounds bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * Vector3.one);
        int leafIndex = matricesBuffers.Length - 1;
        Color colorA, colorB;
        Mesh instanceMesh;
        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            matricesBuffers[i].SetData(matrices[i]);
            ComputeBuffer buffer = matricesBuffers[i];
            buffer.SetData(matrices[i]);
            //fractalData.material.SetBuffer(matricesId, buffer);
            propertyBlock.SetVector(sequenceNumbersID, sequenceNumbers[i]);
            propertyBlock.SetBuffer(matricesID, buffer);
            if (i == leafIndex)
            {
                colorA = fractalData.leafColorA;
                colorB = fractalData.leafColorB;
                instanceMesh = fractalData.leafMesh;
            }
            else
            {
                float gradientInterpolator = i / (matricesBuffers.Length - 2f);
                colorA = fractalData.gradientA.Evaluate(gradientInterpolator);
                colorB = fractalData.gradientA.Evaluate(gradientInterpolator);
                instanceMesh = fractalData.meshType;
            }
            propertyBlock.SetColor(colorAId, colorA);
            propertyBlock.SetColor(colorBId, colorB);
            Graphics.DrawMeshInstancedProcedural(instanceMesh, 0, fractalData.material, bounds, buffer.count, propertyBlock);
        }

    }
    private ProceduralFractalPart CreatePart(int childIndex) => new ProceduralFractalPart
    {
        maxSagAngle = radians(Random.Range(fractalData.maxSagAngleA, fractalData.maxSagAngleB)),
        spinVelocity = (Random.value < fractalData.reverseSpinChance ? 1 : -1) * radians(Random.Range(fractalData.spinSpeedA, fractalData.spinSpeedB)),
        rotation = rotations[childIndex]
    };

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    private struct UpdateFractalJob : IJobFor
    {
        public float deltaTime;
        public float scale;

        [ReadOnly] public NativeArray<ProceduralFractalPart> parents;
        public NativeArray<ProceduralFractalPart> parts;

        [WriteOnly] public NativeArray<float3x4> matrices;

        public void Execute(int index)
        {
            ProceduralFractalPart parent = parents[index / 5];
            ProceduralFractalPart part = parts[index];
            part.spinAngle += part.spinVelocity * deltaTime;

            float3 upAxis = mul(mul(parent.worldRotation, part.rotation), up());
            float3 sagAxis = cross(up(), upAxis);
            float sagMagnitute = length(sagAxis);
            quartenion baseRotation;
            if (sagMagnitute > 0f)
            {
                sagAxis /= sagMagnitute;
                quartenion sagRotation = quartenion.AxisAngle(sagAxis, part.maxSagAngle * sagMagnitute);
                baseRotation = mul(sagRotation, parent.worldRotation);
            }
            else
            {
                baseRotation = parent.worldRotation;
            }
            part.worldRotation = mul(baseRotation, mul(part.rotation, quartenion.RotateY(part.spinAngle)));
            part.worldPosition = parent.worldPosition + mul(part.worldRotation, float3(0f, 1.5f * scale, 0f));
            parts[index] = part;
            float3x3 r = float3x3(part.worldRotation) * scale;
            matrices[index] = float3x4(r.c0, r.c1, r.c2, part.worldPosition);
        }
    }
}
