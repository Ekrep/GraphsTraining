using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using static UnityEngine.Mathf;
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

    public delegate float FunctionMethod(float a, float b, float c);

    [BurstCompile]
    public static float Wave(float x, float z, float t)
    {
        return math.sin(math.PI * (x + z + t));
    }
    [BurstCompile]
    public static float MultiWave(float x, float z, float t)
    {
        float y = math.sin(math.PI * (x + t));
        y += math.sin(2f * math.PI * (x + z + t)) * (1f / 2f);
        return y * (2f / 3f);

    }
    [BurstCompile]
    public static float MorphingWave(float x, float z, float t)
    {
        float y = math.sin(math.PI * (x + 0.5f * t));
        y += 0.5f * math.sin(2f * math.PI * (x + z + t));
        return y;
    }
    [BurstCompile]
    public static float Ripple(float x, float z, float t)
    {
        float d = math.abs(x);
        float f = math.abs(z);
        float y = math.sin(math.PI * (4f * (d + f) - t));
        return y / (1f + 10f * d);
    }
    public static List<MethodFunctionData> GetAllMehtods()
    {
        List<MethodFunctionData> twoRefFloatTypeFuncs = new List<MethodFunctionData>(){
            new MethodFunctionData(FunctionTypes.Wave,Wave),
            new MethodFunctionData(FunctionTypes.MultiWave,MultiWave),
            new MethodFunctionData(FunctionTypes.MorphingWave,MorphingWave),
            new MethodFunctionData(FunctionTypes.Ripple,Ripple)
        };
        return twoRefFloatTypeFuncs;

    }
}
