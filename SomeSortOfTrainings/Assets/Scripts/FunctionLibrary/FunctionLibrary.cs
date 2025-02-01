using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using static UnityEngine.Mathf;
using static Utilities.Utils;
using Unity.Mathematics;

public static partial class FunctionLibrary
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

    public delegate Vector3 FunctionMethod(float u, float v, float t);

    public static Vector3 Wave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = math.sin(math.PI * (u + v + t));
        p.z = v;
        return p;
    }
    public static Vector3 MultiWave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = (math.sin(math.PI * (u + 0.5f * t)) + 0.5f * math.sin(2f * math.PI * (v + t)) * 0.25f) * (2f / 3f);
        p.z = v;
        return p;

    }
    public static Vector3 MorphingWave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = math.sin(math.PI * (u + 0.5f * t)) + 0.5f * math.sin(2f * math.PI * (u + v + t));
        p.z = v;
        return p;
    }
    public static Vector3 Ripple(float u, float v, float t)
    {
        float d = math.sqrt(u * u + v * v);
        Vector3 p;
        p.x = u;
        p.y = math.sin(math.PI * (4f * d - t)) / (1 + 10f * d);
        p.z = v;
        return p;
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
//FOR BURST
public static partial class FunctionLibrary
{
    //NOT DYNAMIC, DO SOMETHING ABOUT IT LATER
    //Type casting can be used for to make it more dynamic but it will increase the cost i think?
    //NativeHashMaps can be more efficent i think.
    [BurstCompile]
    public struct WaveFunction : IFunction
    {
        public int functionType;
        public float3 Evaluate(float u, float v, float t)
        {
            switch (functionType)
            {
                case 0://Wave
                    return new float3(u, math.sin(math.PI * (u + v + t)), v);
                case 1://MultiWave
                    return new float3(u, (math.sin(math.PI * (u + 0.5f * t)) + 0.5f * math.sin(2f * math.PI * (v + t)) * 0.25f) * (2f / 3f), v);
                case 2://Morphing
                    return new float3(u, math.sin(math.PI * (u + 0.5f * t)) + 0.5f * math.sin(2f * math.PI * (u + v + t)), v);
                case 3://Ripple
                    float d = math.sqrt(u * u + v * v);
                    return new float3(u, math.sin(math.PI * (4f * d - t)) / (1 + 10f * d), v);
                default:
                    break;
            }
            return new float3(55, 55, 55);

        }
    }
}
