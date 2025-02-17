using UnityEngine;
using static Utilities.Utils;
using GraphProperties.GraphData;
using System;


public class Graph : MonoBehaviour
{
    #region Variables
    [SerializeField] protected GraphPropertiesScriptable graphProperties;

    [SerializeField] protected FunctionTypes currentFunctionType;
    protected FunctionTypes currentFunctionTypeHolder;

    [SerializeField, Min(0f)] protected float functionDuration = 1f, transitionDuration = 1f;
    protected float duration;
    [SerializeField] protected bool useRandomFunctionSelect = false;
    [SerializeField] protected bool transitioning = false;
    protected FunctionTypes transitionFunctionTypeHolder;
    protected int totalFunctionsCount;
    #endregion

    private void Start()
    {
        Initialize();
    }
    protected virtual void Initialize()
    {
        totalFunctionsCount = FunctionLibrary.GetAllMehtodsCount();
    }
    protected FunctionTypes GiveRandomOrNextFunctionType()
    {
        return (FunctionTypes)(Convert.ToInt16(!useRandomFunctionSelect) * GetNextFuncIndex())
        + Convert.ToInt16(useRandomFunctionSelect) * CheckFunctionIndexMatch((int)currentFunctionType, UnityEngine.Random.Range(0, totalFunctionsCount));
    }
    protected int CheckFunctionIndexMatch(int currentIndex, int randomIndex)
    {
        if (currentIndex == randomIndex)
        {
            return randomIndex + 1 % totalFunctionsCount;
        }
        else
        {
            return randomIndex;
        }

    }
    protected int GetNextFuncIndex()
    {
        return ((int)currentFunctionType + 1) % totalFunctionsCount;
    }
}

