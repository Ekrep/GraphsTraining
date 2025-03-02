using UnityEngine;
using GraphProperties.FractalData;


public class FractalProcedural : MonoBehaviour
{
    [SerializeField] private FractalData fractalData;
    private struct ProceduralFractalPart
    {
        public Vector3 direction, worldPosition;
        public Quaternion rotation, worldRotation;
        public float spinAngle;

    }
    private ProceduralFractalPart[][] parts;
    private Matrix4x4[][] matrices;

    private static Vector3[] directions = {
        Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back
    };
    private static Quaternion[] rotations = {
        Quaternion.identity,
        Quaternion.Euler(0f, 0f, -90f), Quaternion.Euler(0f, 0f, 90f),
        Quaternion.Euler(90f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f)
    };
    protected ComputeBuffer[] matricesBuffers;

    private static readonly int matricesId = Shader.PropertyToID("_Matrices");
    private static MaterialPropertyBlock propertyBlock;

    void OnValidate()
    {
        if (parts != null && enabled)
        {
            OnDisable();
            OnEnable();
        }

    }
    void OnEnable()
    {
        parts = new ProceduralFractalPart[fractalData.depth][];
        matrices = new Matrix4x4[fractalData.depth][];
        matricesBuffers = new ComputeBuffer[fractalData.depth];
        int stride = 16 * 4;
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new ProceduralFractalPart[length];
            matrices[i] = new Matrix4x4[length];
            matricesBuffers[i] = new ComputeBuffer(length, stride);
        }

        parts[0][0] = CreatePart(0);
        //li==LevelIndex
        for (int li = 1; li < parts.Length; li++)
        {
            ProceduralFractalPart[] levelParts = parts[li];
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
        }
        parts = null;
        matrices = null;
        matricesBuffers = null;
    }
    private void Update()
    {
        float spinAngleDelta = 22.5f * Time.deltaTime;
        ProceduralFractalPart rootPart = parts[0][0];
        rootPart.spinAngle += spinAngleDelta;
        rootPart.worldRotation = transform.rotation * (rootPart.rotation * Quaternion.Euler(0f, rootPart.spinAngle, 0f));
        rootPart.worldPosition = transform.position;
        float objectScale = transform.lossyScale.x;
        parts[0][0] = rootPart;
        matrices[0][0] = Matrix4x4.TRS(rootPart.worldPosition, rootPart.worldRotation, objectScale * Vector3.one);
        float scale = objectScale;
        for (int li = 1; li < parts.Length; li++)
        {
            scale *= 0.5f;
            ProceduralFractalPart[] parentParts = parts[li - 1];
            ProceduralFractalPart[] levelParts = parts[li];
            Matrix4x4[] levelMatrices = matrices[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi++)
            {
                ProceduralFractalPart parent = parentParts[fpi / 5];
                ProceduralFractalPart part = levelParts[fpi];
                part.worldRotation = parent.worldRotation * (part.rotation * Quaternion.Euler(0, part.spinAngle, 0f));
                part.worldPosition =
                    parent.worldPosition +
                    parent.worldRotation * (1.5f * scale * part.direction);
                levelParts[fpi] = part;
                levelMatrices[fpi] = Matrix4x4.TRS(part.worldPosition, part.worldRotation, scale * Vector3.one);
            }
        }
        Bounds bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * Vector3.one);
        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            matricesBuffers[i].SetData(matrices[i]);
            ComputeBuffer buffer = matricesBuffers[i];
            buffer.SetData(matrices[i]);
            //fractalData.material.SetBuffer(matricesId, buffer);
            propertyBlock.SetBuffer(matricesId, buffer);
            Graphics.DrawMeshInstancedProcedural(fractalData.meshType, 0, fractalData.material, bounds, buffer.count, propertyBlock);
        }

    }
    private ProceduralFractalPart CreatePart(int childIndex) => new ProceduralFractalPart
    {
        direction = directions[childIndex],
        rotation = rotations[childIndex]
    };

}
