using System.Collections.Generic;
using Unity.Burst;
using static Utilities.Utils;
using Unity.Mathematics;
using UnityEngine;


[BurstCompile]
public static class FunctionLibrary
{
    private const float PI = math.PI;
    public struct MethodFunctionData
    {
        public MethodFunctionData(FunctionTypes types, FunctionMethod twoParamFloatTypeFunc)
        {
            type = types;
            func = twoParamFloatTypeFunc;
        }
        public FunctionTypes type;
        public FunctionMethod func;

    }
    public delegate void FunctionMethod(float u, float v, float t, out float x, out float y, out float z);

    [BurstCompile]
    private static void Wave(float u, float v, float t, out float x, out float y, out float z)
    {
        x = u;
        y = math.sin(PI * (u + v + t));
        z = v;
    }
    [BurstCompile]
    private static void MultiWave(float u, float v, float t, out float x, out float y, out float z)
    {
        x = u;
        y = (math.sin(PI * (u + 0.5f * t)) + 0.5f * math.sin(2f * PI * (v + t)) * 0.25f) * (2f / 3f);
        z = v;
    }
    [BurstCompile]
    private static void MorphingWave(float u, float v, float t, out float x, out float y, out float z)
    {
        x = u;
        y = math.sin(PI * (u + 0.5f * t)) + 0.5f * math.sin(2f * PI * (u + v + t));
        z = v;
    }
    [BurstCompile]
    private static void Ripple(float u, float v, float t, out float x, out float y, out float z)
    {
        float d = math.sqrt(u * u + v * v);
        x = u;
        y = math.sin(PI * (4f * d - t)) / (1 + 10f * d);
        z = v;
    }
    [BurstCompile]
    private static void Sphere(float u, float v, float t, out float x, out float y, out float z)
    {
        float r = 0.9f + 0.1f * math.sin(PI * (6f * u + 4f * v + t));
        float s = r * math.cos(0.5f * PI * v);
        x = s * math.sin(PI * u);
        y = r * math.sin(PI * 0.5f * v);
        z = s * math.cos(PI * u);
    }
    [BurstCompile]
    private static void Torus(float u, float v, float t, out float x, out float y, out float z)
    {
        #region Sphere pulled apart
        // float r = 1f;
        // float s = 0.5f + r * math.cos(0.5f * PI * v);
        // x = s * math.sin(PI * u);
        // y = r * math.sin(0.5f * PI * v);
        // z = s * math.cos(PI * u);
        #endregion
        #region Self-intersecting spindle torus
        // float r = 1f;
        // float s = 0.5f + r * math.cos(PI * v);
        // x = s * math.sin(PI * u);
        // y = r * math.sin(PI * v);
        // z = s * math.cos(PI * u);
        #endregion
        #region Ring torus
        // float r1 = 0.75f;
        // float r2 = 0.25f;
        // float s = r1 + r2 * math.cos(PI * v);
        // x = s * math.sin(PI * u);
        // y = r2 * math.sin(PI * v);
        // z = s * math.cos(PI * u);
        #endregion
        #region Twisting torus
        float r1 = 0.7f + 0.1f * math.sin(PI * (6f * u + 0.5f * t));
        float r2 = 0.15f + 0.05f * math.sin(PI * (8f * u + 4f * v + 2f * t));
        float s = r1 + r2 * math.cos(PI * v);
        x = s * math.sin(PI * u);
        y = r2 * math.sin(PI * v);
        z = s * math.cos(PI * u);
        #endregion
    }

    public static Vector3 Morph(float u, float v, float t, FunctionMethod fromFunc, FunctionMethod to, float progress)
    {
        float x1 = 0, y1 = 0, z1 = 0, x2 = 0, y2 = 0, z2 = 0;
        fromFunc(u, v, t, out x1, out y1, out z1);
        to(u, v, t, out x2, out y2, out z2);
        Vector3 fromVec = new Vector3(x1, y1, z1);
        Vector3 toVec = new Vector3(x2, y2, z2);
        return Vector3.LerpUnclamped(fromVec, toVec, Mathf.SmoothStep(0f, 1f, progress));
    }
    public static List<MethodFunctionData> GetAllMehtods()
    {
        List<MethodFunctionData> floatTypeFuncs = new List<MethodFunctionData>(){
            new MethodFunctionData(FunctionTypes.Wave,Wave),
            new MethodFunctionData(FunctionTypes.MultiWave,MultiWave),
            new MethodFunctionData(FunctionTypes.MorphingWave,MorphingWave),
            new MethodFunctionData(FunctionTypes.Ripple,Ripple),
            new MethodFunctionData(FunctionTypes.Sphere,Sphere),
            new MethodFunctionData(FunctionTypes.Torus,Torus),
        };
        return floatTypeFuncs;

    }
}

