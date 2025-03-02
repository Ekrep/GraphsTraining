using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProperties.FractalData;
using UnityEditor;

public class Fractal : MonoBehaviour
{
    private struct FractalPart
    {
        public Vector3 direction;
        public Quaternion rotation;
        public Transform transform;

    }
    private FractalPart[][] parts;
    [SerializeField] private FractalData fractalData;

    private static Vector3[] directions = {
        Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back
    };
    private static char[] directionNames ={
        'U','R','L','F','B'
    };
    private static Quaternion[] rotations = {
        Quaternion.identity,
        Quaternion.Euler(0f, 0f, -90f), Quaternion.Euler(0f, 0f, 90f),
        Quaternion.Euler(90f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f)
    };

    void Awake()
    {
        parts = new FractalPart[fractalData.depth][];
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new FractalPart[length];
        }
        float scale = 1f;
        parts[0][0] = CreatePart(0, 0, scale);
        //li==LevelIndex
        for (int li = 1; li < parts.Length; li++)
        {
            scale *= 0.5f;
            FractalPart[] levelParts = parts[li];
            //fpi==fractal part iterator
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5)
            {
                //ci==child index
                for (int ci = 0; ci < 5; ci++)
                {
                    levelParts[fpi + ci] = CreatePart(li, ci, scale);
                }

            }
        }
    }
    private void Update()
    {
        Quaternion deltaRotation = Quaternion.Euler(0f, 22.5f * Time.deltaTime, 0f);
        FractalPart rootPart = parts[0][0];
        rootPart.rotation *= deltaRotation;
        rootPart.transform.localRotation = rootPart.rotation;
        parts[0][0] = rootPart;
        for (int li = 1; li < parts.Length; li++)
        {
            FractalPart[] parentParts = parts[li - 1];
            FractalPart[] levelParts = parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi++)
            {
                Transform parentTransform = parentParts[fpi / 5].transform;
                FractalPart part = levelParts[fpi];
                part.rotation *= deltaRotation;
                part.transform.localRotation = parentTransform.localRotation * part.rotation;
                part.transform.localPosition = parentTransform.localPosition + 1.5f * part.transform.localScale.x * part.direction;
                levelParts[fpi] = part;
            }
        }
    }
    private FractalPart CreatePart(int levelIndex, int childIndex, float scale)
    {
        GameObject go = new GameObject("Fractal Part" + levelIndex + "[" + directionNames[childIndex] + "]" + " C" + childIndex);
        go.transform.localScale = Vector3.one * scale;
        go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>().mesh = fractalData.meshType;
        go.AddComponent<MeshRenderer>().material = fractalData.material;
        return new FractalPart
        {
            direction = directions[childIndex],
            rotation = rotations[childIndex],
            transform = go.transform
        };

    }
}
