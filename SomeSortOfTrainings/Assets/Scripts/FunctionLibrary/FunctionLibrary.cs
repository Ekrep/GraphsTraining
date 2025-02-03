using System.Collections.Generic;
using Unity.Burst;
using static Utilities.Utils;
using Unity.Mathematics;


[BurstCompile]
public static class FunctionLibrary
{
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
        y = math.sin(math.PI * (u + v + t));
        z = v;
    }
    [BurstCompile]
    private static void MultiWave(float u, float v, float t, out float x, out float y, out float z)
    {
        x = u;
        y = (math.sin(math.PI * (u + 0.5f * t)) + 0.5f * math.sin(2f * math.PI * (v + t)) * 0.25f) * (2f / 3f);
        z = v;
    }
    [BurstCompile]
    private static void MorphingWave(float u, float v, float t, out float x, out float y, out float z)
    {
        x = u;
        y = math.sin(math.PI * (u + 0.5f * t)) + 0.5f * math.sin(2f * math.PI * (u + v + t));
        z = v;
    }
    [BurstCompile]
    private static void Ripple(float u, float v, float t, out float x, out float y, out float z)
    {
        float d = math.sqrt(u * u + v * v);
        x = u;
        y = math.sin(math.PI * (4f * d - t)) / (1 + 10f * d);
        z = v;
    }
    [BurstCompile]
    private static void Sphere(float u, float v, float t, out float x, out float y, out float z)
    {
        float r = 0.9f + 0.1f * math.sin(math.PI * (6f * u + 4f * v + t));
        float s = r * math.cos(0.5f * math.PI * v);
        x = s * math.sin(math.PI * u);
        y = r * math.sin(math.PI * 0.5f * v);
        z = s * math.cos(math.PI * u);
    }
    public static List<MethodFunctionData> GetAllMehtods()
    {
        List<MethodFunctionData> floatTypeFuncs = new List<MethodFunctionData>(){
            new MethodFunctionData(FunctionTypes.Wave,Wave),
            new MethodFunctionData(FunctionTypes.MultiWave,MultiWave),
            new MethodFunctionData(FunctionTypes.MorphingWave,MorphingWave),
            new MethodFunctionData(FunctionTypes.Ripple,Ripple),
            new MethodFunctionData(FunctionTypes.Sphere,Sphere)
        };
        return floatTypeFuncs;

    }
}

