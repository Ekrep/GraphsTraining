using UnityEngine;
using CollectionsTraining.CollectionVisulator;
using PoolSystem;
using CollectionsTraining.DataVisual.BracketVisual;
using CollectionsTraining.DataVisual.DataNode;
using CollectionsTraining.CollectionUtils;
using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
public class ArrayBasedCollection : CollectionVisulator
{
    [SerializeField] protected VisualBracket bracketPrefab;
    [SerializeField] protected Vector3 bracketsStartPosition;
    protected List<VisualBracket> currentlyUsingBrackets = new List<VisualBracket>();
    protected VisualBracket[] garbageBrackects = new VisualBracket[2];
    protected virtual void Initialize(int dataNodeCreationAmount, int bracketCreationAmount, int pointingArrowCreationAmount)
    {
        PoolManager.Instance.CreatePool("DataNode", nodePrefab, dataNodeCreationAmount, transform);
        PoolManager.Instance.CreatePool("VisualBracket", bracketPrefab, bracketCreationAmount, transform);
        PoolManager.Instance.CreatePool("PointingArrow", pointingArrowPrefab, pointingArrowCreationAmount, transform);
        visualsParent = new GameObject("VisualsParent");
        CommandConsole.Instance.AssignConsolable(this);
    }
    protected void InitializeNodePositions(DataNode[] nodes, int collectionCount)
    {
        nodes[0].SetWorldPosition(new Vector3(currentlyUsingBrackets[0].transform.position.x + (nodes[0].GetScale().x * baseNodeDistance), 0, 0));
        for (int i = 1; i < collectionCount; i++)
        {
            nodes[i].SetWorldPosition(new Vector3(nodes[i - 1].transform.position.x + (nodes[i].GetScale().x * baseNodeDistance), 0, 0));
        }
    }
    protected VisualBracket[] CreateBrackets(int amount)
    {
        VisualBracket[] createdBrackets = new VisualBracket[amount];
        for (int i = 0; i < amount; i++)
        {
            createdBrackets[i] = PoolManager.Instance.DequeueItemFromPool<VisualBracket>("VisualBracket", visualsParent.transform);
        }
        return createdBrackets;
    }
    private float CalculateDistanceBasedOnCapacity(int capacity)
    {
        float nodeScaleRatio = GetNodeScaleRatio();
        return capacity * (nodeScaleRatio * baseNodeDistance) + (nodeScaleRatio * baseNodeDistance);
    }
    protected void SetBracketsPositions(Transform leftBracket, Transform rightBracket, Vector3 bracketsStartPosition, int collectionCapacity)
    {
        leftBracket.position = bracketsStartPosition;
        rightBracket.position = new Vector3(bracketsStartPosition.x + CalculateDistanceBasedOnCapacity(collectionCapacity), leftBracket.position.y, leftBracket.position.z);
        rightBracket.rotation = Quaternion.Euler(0, 180, 0);
    }
    protected virtual void StartCopyArrayAnim(DataNode[] nodes, int count, int capacity)
    {
        VisualBracket[] createdBrackets = CreateBrackets(2);
        SetBracketsPositions(createdBrackets[0].transform, createdBrackets[1].transform, new Vector3(currentlyUsingBrackets[0].transform.position.x,
            currentlyUsingBrackets[0].transform.position.y - 2f, 0f), capacity);
        Vector3[] targetCopyPositions = GetCopyPositions(count);
        garbageBrackects[0] = currentlyUsingBrackets[0];
        garbageBrackects[1] = currentlyUsingBrackets[1];
        currentlyUsingBrackets[0] = createdBrackets[0];
        currentlyUsingBrackets[1] = createdBrackets[1];
        StartCoroutine(CopyArrayToNewArray(targetCopyPositions, nodes, count, currentlyUsingBrackets[0].transform.position.y));

    }
    protected virtual IEnumerator CopyArrayToNewArray(Vector3[] copyPositions, DataNode[] nodes, int nodesCount, float bracketYPos, BoolWrapper conditionWrapper)
    {
        for (int i = 0; i < nodesCount; i++)
        {
            Vector3 targetPos = new Vector3(copyPositions[i].x, bracketYPos);
            nodes[i].MoveByTweening(targetPos, 0.3f, Ease.Flash);
            Transform t = nodes[i].transform;
            yield return new WaitUntilNodeReachesTargetPosition(t, targetPos);
        }
        //make them a method
        BoolWrapper boolWrapper = new BoolWrapper(true);
        DestroyOldListBrackets(boolWrapper);
        yield return new WaitUntillMultipableMethodsComplete(boolWrapper);
    }
    // fix here delete ref!
    protected virtual IEnumerator CopyArrayToNewArray(Vector3[] copyPositions, DataNode[] nodes, int nodesCount, float bracketYPos)
    {
        int index = 0;
        for (int i = 0; index < copyPositions.Length; i++)
        {
            if (nodes[i] != null)
            {
                Vector3 targetPos = new Vector3(copyPositions[index].x, bracketYPos);
                nodes[i].MoveByTweening(targetPos, 0.3f, Ease.Flash);
                Transform t = nodes[i].transform;
                index++;
                yield return new WaitUntilNodeReachesTargetPosition(t, targetPos);

            }

        }
        //make them a method
        BoolWrapper boolWrapper = new BoolWrapper(true);
        DestroyOldListBrackets(boolWrapper);
        yield return new WaitUntillMultipableMethodsComplete(boolWrapper);
    }
    protected bool TrimExcessCollection(DataNode[] nodes, ref int capacity, ref int count)
    {
        if (count < capacity * 0.9f)//TrimExcess does not reduce capacity due to the 90% threshold.
        {
            Trim(nodes, ref capacity, ref count);
            SendResponseToCommand("<color=blue><i>Command recieved processing...</i></color>");
            StartCopyArrayAnim(nodes, count, capacity);
            return true;
        }
        else
        {
            SendResponseToCommand("There is no trimable Collection!!");
            CommandConsole.Instance.CommandComplete();
            return false;
        }

    }
    protected virtual T[] ExtendArray<T>(T[] array, int capacityMultiplier, int count)
    {
        T[] extendedArray = new T[array.Length * capacityMultiplier];
        for (int i = 0; i < count; i++)
        {
            extendedArray[i] = array[i];
        }
        //UpdateValuesOnArrayCopied();
        return extendedArray;
    }
    private DataNode[] Trim(DataNode[] nodes, ref int capacity, ref int count)//currently only works on DataNodeArrays
    {
        DataNode[] trimmedArrray = new DataNode[count];
        int index = 0;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                trimmedArrray[index] = nodes[i];
                index++;
            }

        }
        nodes = trimmedArrray;
        capacity = count;
        count = trimmedArrray.Length;
        return nodes;
    }
    protected void SetElementsToDefaultPosition(DataNode[] nodes, int dataNodesCount, Action onComplete = null)
    {
        visualsParent.transform.DOMoveY(2f, 0.5f).OnComplete(() =>
       {
           SetAllVisualsPositionsDefault(nodes, dataNodesCount);
           SendResponseToCommand("<color=green><i>Process Done!</i></color>");
           if (onComplete != null)
           {
               onComplete();
           }
           CommandConsole.Instance.CommandComplete();
       });

    }
    private void SetAllVisualsPositionsDefault(DataNode[] nodes, int dataNodesCount)
    {
        visualsParent.transform.position = Vector3.zero;
        for (int i = 0; i < dataNodesCount; i++)
        {
            nodes[i].SetLocalPosition(new Vector3(nodes[i].transform.position.x, 0));
        }
        for (int i = 0; i < currentlyUsingBrackets.Count; i++)
        {
            currentlyUsingBrackets[i].SetLocalPosition(new Vector3(currentlyUsingBrackets[i].transform.position.x, 0));
        }

    }
    protected void DestroyOldListBrackets(BoolWrapper boolWrapper = null)
    {
        Vector3 middlePoint = Vector3.Lerp(garbageBrackects[0].transform.position, garbageBrackects[1].transform.position, 0.5f);
        Vector3 leftBracketTargetPoint = new Vector3(middlePoint.x - 0.3f, middlePoint.y, middlePoint.z);
        Vector3 rightBracketTargetPoint = new Vector3(middlePoint.x + 0.3f, middlePoint.y, middlePoint.z);
        //for maybe?
        garbageBrackects[1].MoveByTweening(rightBracketTargetPoint, 0.5f, default, () =>
        {
            PoolManager.Instance.EnqueueItemToPool("VisualBracket", garbageBrackects[1]);

        });
        garbageBrackects[0].MoveByTweening(leftBracketTargetPoint, 0.5f, default, () =>
       {
           PoolManager.Instance.EnqueueItemToPool("VisualBracket", garbageBrackects[0]);
           boolWrapper.value = false;
           Array.Clear(garbageBrackects, 0, garbageBrackects.Length);//GC works here i think?
       });

    }
    protected Vector3 GetNodePositionByIndex(int index)
    {
        float xPos = CalculateDistanceBasedOnCapacity(index);
        return new Vector3(xPos, currentlyUsingBrackets[0].transform.position.y, 0);

    }
    protected Vector3[] GetCollectionNodePositions(DataNode[] nodes, int count)
    {
        Vector3[] positionsArray = new Vector3[count];
        int index = 0;
        for (int i = 0; i < count; i++)
        {
            if (nodes[i] != null)
            {
                positionsArray[index] = nodes[i].transform.position;
                index++;
            }

        }
        return positionsArray;
    }
    protected Vector3[] GetCopyPositions(int copyArrayCount)
    {
        Vector3[] copyPositions = new Vector3[copyArrayCount];
        for (int i = 0; i < copyArrayCount; i++)
        {
            copyPositions[i] = GetNodePositionByIndex(i);
        }
        return copyPositions;

    }
}
