using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using CustomHashes;
public class HashVisualization : MonoBehaviour
{

    private static int hashesId = Shader.PropertyToID("_Hashes");
    private static int configId = Shader.PropertyToID("_Config");
    //private static int rotationId = Shader.PropertyToID("_Rotation");
    //private static int scaleId = Shader.PropertyToID("_Scale");
    //private static int TRSId = Shader.PropertyToID("_TRS");
    private static int positionsId = Shader.PropertyToID("_Positions");
    private static int normalsId = Shader.PropertyToID("_Normals");
    [SerializeField] private Mesh instanceMesh;
    [SerializeField] private Material material;
    [SerializeField, Range(1, 512)] private int resolution = 16;
    private NativeArray<uint4> hashes;
    private NativeArray<float3x4> positions, normals;
    private ComputeBuffer hashesBuffer, positionsBuffer, normalsBuffer;
    private MaterialPropertyBlock propertyBlock;
    //[Range(0, 2)] public float scale = 1;
    [SerializeField] private int seed = 0;
    [SerializeField, Range(-2f, 2f)] private float verticalOffset;
    [SerializeField] private SpaceTRS domain = new SpaceTRS { scale = 8f };
    [SerializeField, Range(-0.5f, 0.5f)] private float displacement = 0.1f;
    private Bounds bounds;
    private bool isDirty;
    void OnValidate()
    {
        if (hashesBuffer != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }
    private void OnEnable()
    {
        isDirty = true;
        int length = resolution * resolution;
        length /= 4 + (length & 1);
        hashes = new NativeArray<uint4>(length, Allocator.Persistent);
        hashesBuffer = new ComputeBuffer(length * 4, 4);
        positions = new NativeArray<float3x4>(length, Allocator.Persistent);
        positionsBuffer = new ComputeBuffer(length * 4, 4 * 3);
        normals = new NativeArray<float3x4>(length, Allocator.Persistent);
        normalsBuffer = new ComputeBuffer(length * 4, 4 * 3);
        hashesBuffer.SetData(hashes);
        positionsBuffer.SetData(positions);
        propertyBlock ??= new MaterialPropertyBlock();
        propertyBlock.SetBuffer(hashesId, hashesBuffer);
        propertyBlock.SetVector(configId, new Vector4(resolution, 1f / resolution, verticalOffset / resolution, displacement));
        propertyBlock.SetBuffer(positionsId, positionsBuffer);
        propertyBlock.SetBuffer(normalsId, normalsBuffer);

    }
    private void OnDisable()
    {
        hashes.Dispose();
        positions.Dispose();
        normals.Dispose();
        positionsBuffer.Release();
        hashesBuffer.Release();
        normalsBuffer.Release();
        hashesBuffer = null;
        positionsBuffer = null;
        normalsBuffer = null;
    }
    private void Update()
    {
        if (isDirty || transform.hasChanged)
        {
            isDirty = false;
            transform.hasChanged = false;
            bounds = new Bounds(transform.position, float3(2f * cmax(abs(transform.lossyScale)) + displacement));
            JobHandle handle = Shapes.Job.ScheduleParallel(
                positions, normals, resolution, transform.localToWorldMatrix, default
            );

            new HashJob
            {
                positions = positions,
                hashes = hashes,
                hash = SmallXXHash.Seed(seed),
                domainTRS = domain.Matrix
            }.ScheduleParallel(hashes.Length, resolution, handle).Complete();

            hashesBuffer.SetData(hashes.Reinterpret<uint>(4 * 4));
            positionsBuffer.SetData(positions.Reinterpret<float3>(3 * 4 * 4));
            normalsBuffer.SetData(normals.Reinterpret<float3>(3 * 4 * 4));

        }
        //propertyBlock.SetFloat(scaleId, scale);
        //propertyBlock.SetVector(rotationId, float4(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w));
        //Matrix4x4 m = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one * scale);
        //propertyBlock.SetMatrix(TRSId, m);
        Graphics.DrawMeshInstancedProcedural(instanceMesh, 0, material, bounds, resolution * resolution, propertyBlock);
    }


    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    private struct HashJob : IJobFor
    {
        [ReadOnly] public NativeArray<float3x4> positions;
        [WriteOnly] public NativeArray<uint4> hashes;
        public SmallXXHash4 hash;
        public float3x4 domainTRS;

        public void Execute(int index)
        {
            float4x3 p = TransformPositions(domainTRS, transpose(positions[index]));

            int4 u = (int4)floor(p.c0);
            int4 v = (int4)floor(p.c1);
            int4 w = (int4)floor(p.c2);

            hashes[index] = hash.Eat(u).Eat(v).Eat(w);
        }
        private float4x3 TransformPositions(float3x4 trs, float4x3 p)
        {
            return float4x3(trs.c0.x * p.c0 + trs.c1.x * p.c1 + trs.c2.x * p.c2 + trs.c3.x,
            trs.c0.y * p.c0 + trs.c1.y * p.c1 + trs.c2.y * p.c2 + trs.c3.y,
            trs.c0.z * p.c0 + trs.c1.z * p.c1 + trs.c2.z * p.c2 + trs.c3.z);
        }
    }
}
