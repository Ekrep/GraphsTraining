using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using static UnityEngine.Mathf;
using static Utilities.Utils;

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
        return Sin(PI * (x + t));
    }
    [BurstCompile]
    public static float MultiWave(float x, float z, float t)
    {
        float y = Sin(PI * (x + t));
        y += Sin(2f * PI * (x + t)) * (1f / 2f);
        return y * (2f / 3f);

    }
    [BurstCompile]
    public static float MorphingWave(float x, float z, float t)
    {
        float y = Sin(PI * (x + 0.5f * t));
        y += 0.5f * Sin(2f * PI * (x + t));
        return y;
    }
    [BurstCompile]
    public static float Ripple(float x, float z, float t)
    {
        float d = Abs(x);
        float y = Sin(PI * (4f * d - t));
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
