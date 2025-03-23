using System;
using UnityEngine;
using CollectionsTraining.CollectionVisulator;
using CollectionsTraining.DataVisual.VisualPointingArrow;
using PoolSystem;
using CollectionsTraining.DataVisual.DataNode;
using System.Text.RegularExpressions;
using System.Collections;
using CollectionsTraining.CollectionUtils;
using Unity.VisualScripting.Dependencies.NCalc;

namespace CollectionsTraining.CollectionVisulator.LinkedListVisulator
{
    //there is no index value in linked list but for the visualition i need one.
    public class LinkedListCollectionVisulator : LinkBasedCollection
    {
        private VisulatorPointingArrow pointerArrow;
        private CustomLinkedListNode firstNode;
        private CustomLinkedListNode lastNode;
        private Vector3[] grid;
        private int curentGridIndex = 0;
        void Start()
        {
            Init();
        }
        private void Init()
        {
            Initialize(nodeCreateAmount, 1);
            grid = CreateGrid(4, 4);
            pointerArrow = PoolManager.Instance.DequeueItemFromPool<VisulatorPointingArrow>(PointingArrowPool);
            DataNode node = PoolManager.Instance.DequeueItemFromPool<DataNode>(DataNodePool);
            AdjustNodeScaleByCollectionCount(node);
            node.SetWorldPosition(GetAvailablePosFromGrid());
            firstNode = new CustomLinkedListNode(node, null, null, 0);
            lastNode = firstNode;
        }

        private IEnumerator AddNode()
        {
            DataNode dataNode = PoolManager.Instance.DequeueItemFromPool<DataNode>(DataNodePool);
            pointerArrow.SetWorldPosition(new Vector3(firstNode.dataNode.transform.position.x, firstNode.dataNode.transform.position.y + 0.5f));
            AdjustNodeScaleByCollectionCount(dataNode);
            dataNode.MoveByTweening(GetAvailablePosFromGrid(), 0.2f, default);
            CustomLinkedListNode currentNode = firstNode;
            Vector3 targetPos = new Vector3(currentNode.dataNode.transform.position.x, currentNode.dataNode.transform.position.y + 0.5f);
            pointerArrow.MoveByTweening(targetPos, 0.2f, default);
            yield return new WaitUntilNodeReachesTargetPosition(pointerArrow.transform, targetPos);
            while (currentNode.nextNode != null)
            {
                currentNode = currentNode.nextNode;
                targetPos = new Vector3(currentNode.dataNode.transform.position.x, currentNode.dataNode.transform.position.y + 0.5f);
                pointerArrow.MoveByTweening(targetPos, 0.2f, default);
                yield return new WaitUntilNodeReachesTargetPosition(pointerArrow.transform, targetPos);

            }
            yield return new WaitForSeconds(0.5f);
            CustomLinkedListNode newLinkedNode = new CustomLinkedListNode(dataNode, null, currentNode, currentNode.index + 1);
            newLinkedNode.AutoBound();
            currentNode.nextNode = newLinkedNode;
            currentNode.AutoBound();
            lastNode = newLinkedNode;
            UpdateIndexValuesOfNodes();
            CommandConsole.Instance.CommandComplete();
        }
        private Vector3 GetAvailablePosFromGrid()
        {
            Vector3 pos = grid[curentGridIndex];
            curentGridIndex++;
            return pos;

        }
        private Vector3[] CreateGrid(int sizeX, int sizeY)
        {
            Vector3[] grid = new Vector3[sizeX * sizeY];
            int index = 0;
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    grid[index] = new Vector3(i * baseNodeDistance, j * baseNodeDistance);
                    index++;
                }
            }
            return grid;
        }
        private IEnumerator AddAfter(int index)
        {
            DataNode dataNode = PoolManager.Instance.DequeueItemFromPool<DataNode>(DataNodePool);
            AdjustNodeScaleByCollectionCount(dataNode);
            dataNode.MoveByTweening(GetAvailablePosFromGrid(), 0.2f, default);
            CustomLinkedListNode indexNode = FindNodeByIndex(index);
            CustomLinkedListNode newNode;
            if (indexNode.nextNode == null)
            {
                //it means lastNode
                newNode = new CustomLinkedListNode(dataNode, null, indexNode, indexNode.index + 1);
                lastNode = newNode;
            }
            else
            {
                newNode = new CustomLinkedListNode(dataNode, indexNode.nextNode, indexNode, indexNode.index + 1);
            }

            newNode.AutoBound();
            indexNode.nextNode = newNode;
            indexNode.AutoBound();
            //previousNodeSame
            UpdateIndexValuesOfNodes();
            CommandConsole.Instance.CommandComplete();
            yield return null;
        }
        private IEnumerator AddBefore(int index)
        {
            DataNode dataNode = PoolManager.Instance.DequeueItemFromPool<DataNode>(DataNodePool);
            AdjustNodeScaleByCollectionCount(dataNode);
            dataNode.MoveByTweening(GetAvailablePosFromGrid(), 0.2f, default);
            CustomLinkedListNode indexNode;
            CustomLinkedListNode newNode;
            if (index == 0 || index == 1)
            {
                indexNode = firstNode;
                newNode = new CustomLinkedListNode(dataNode, indexNode, null, 0);
                firstNode = newNode;

            }
            else
            {
                indexNode = FindNodeByIndex(index);
                newNode = new CustomLinkedListNode(dataNode, indexNode, indexNode.previousNode, indexNode.index - 1);
            }
            newNode.AutoBound();
            indexNode.previousNode = newNode;
            indexNode.AutoBound();
            //previousNodeSame
            UpdateIndexValuesOfNodes();
            CommandConsole.Instance.CommandComplete();
            yield return null;

        }
        private IEnumerator Remove(int index)
        {
            CustomLinkedListNode indexNode = FindNodeByIndex(index);
            //bound them
            if (indexNode.previousNode != null && indexNode.previousNode.nextNode != null)
            {
                indexNode.previousNode.nextNode = indexNode.nextNode;
                indexNode.previousNode.AutoBound();
            }
            else if (indexNode.previousNode == null)
            {
                firstNode = indexNode.nextNode;

            }
            if (indexNode.nextNode != null && indexNode.nextNode.previousNode != null)
            {
                indexNode.nextNode.previousNode = indexNode.previousNode;
                indexNode.nextNode.AutoBound();
            }
            else if (indexNode.nextNode == null)
            {
                lastNode = indexNode.previousNode;
            }
            //dissappear anim
            NullifyDataNodeAnim(indexNode.dataNode);
            pointerArrow.SetWorldPosition(Vector3.zero);
            //avoid to memoryleak! just in case
            indexNode.nextNode = null;
            indexNode.previousNode = null;
            UpdateIndexValuesOfNodes();
            CommandConsole.Instance.CommandComplete();
            yield return null;
        }
        private void AddFirst()
        {
            DataNode dataNode = PoolManager.Instance.DequeueItemFromPool<DataNode>(DataNodePool);
            dataNode.MoveByTweening(GetAvailablePosFromGrid(), 0.2f, default);
            AdjustNodeScaleByCollectionCount(dataNode);
            if (firstNode == null)
            {
                CustomLinkedListNode newLinkedNode = new CustomLinkedListNode(dataNode, null, null, 0);
                firstNode = newLinkedNode;
            }
            else
            {
                CustomLinkedListNode newLinkedNode = new CustomLinkedListNode(dataNode, firstNode, null, 0);
                firstNode.previousNode = newLinkedNode;
                firstNode.AutoBound();
                newLinkedNode.AutoBound();
                firstNode = newLinkedNode;
            }
            UpdateIndexValuesOfNodes();
            CommandConsole.Instance.CommandComplete();

        }
        private void AddLast()
        {
            DataNode dataNode = PoolManager.Instance.DequeueItemFromPool<DataNode>(DataNodePool);
            dataNode.MoveByTweening(GetAvailablePosFromGrid(), 0.2f, default);
            AdjustNodeScaleByCollectionCount(dataNode);
            if (firstNode.nextNode == null && lastNode == null)
            {
                CustomLinkedListNode newLinkedNode = new CustomLinkedListNode(dataNode, null, firstNode, firstNode.index + 1);
                lastNode = newLinkedNode;
                newLinkedNode.AutoBound();
            }
            else
            {
                CustomLinkedListNode newLinkedNode = new CustomLinkedListNode(dataNode, null, lastNode, lastNode.index + 1);
                lastNode.nextNode = newLinkedNode;
                lastNode.AutoBound();
                newLinkedNode.AutoBound();
                lastNode = newLinkedNode;
            }
            UpdateIndexValuesOfNodes();
            CommandConsole.Instance.CommandComplete();

        }
        private void PointIndex(int index)
        {
            CustomLinkedListNode lNode = FindNodeByIndex(index);
            pointerArrow.MoveByTweening(new Vector3(lNode.dataNode.transform.position.x, lNode.dataNode.transform.position.y + 0.5f), 0.2f, default);
            CommandConsole.Instance.CommandComplete();

        }
        private IEnumerator PointIndexAnimAndStartNextProcess(int index, IEnumerator nextProcess)
        {
            CustomLinkedListNode node = firstNode;
            Vector3 targetPos;
            while (node.index != index && node.nextNode != null)
            {
                targetPos = new Vector3(node.dataNode.transform.position.x, node.dataNode.transform.position.y + 0.5f);
                pointerArrow.MoveByTweening(targetPos, 0.2f, default);
                yield return new WaitUntilNodeReachesTargetPosition(pointerArrow.transform, targetPos);
                node = node.nextNode;
            }
            targetPos = new Vector3(node.dataNode.transform.position.x, node.dataNode.transform.position.y + 0.5f);
            pointerArrow.MoveByTweening(targetPos, 0.2f, default);
            StartCoroutine(nextProcess);

        }
        private IEnumerator PointIndexAnim(int index)
        {
            CustomLinkedListNode node = firstNode;
            Vector3 targetPos;
            while (node.index != index && node.nextNode != null)
            {
                targetPos = new Vector3(node.dataNode.transform.position.x, node.dataNode.transform.position.y + 0.5f);
                pointerArrow.MoveByTweening(targetPos, 0.2f, default);
                yield return new WaitUntilNodeReachesTargetPosition(pointerArrow.transform, targetPos);
                node = node.nextNode;
            }
            targetPos = new Vector3(node.dataNode.transform.position.x, node.dataNode.transform.position.y + 0.5f);
            pointerArrow.MoveByTweening(targetPos, 0.2f, default);
            CommandConsole.Instance.CommandComplete();

        }
        private CustomLinkedListNode FindNodeByIndex(int index)
        {
            CustomLinkedListNode node = firstNode;
            while (node.index != index && node.nextNode != null)
            {
                node = node.nextNode;
            }
            return node;
        }
        private void UpdateIndexValuesOfNodes()
        {
            int index = 0;
            CustomLinkedListNode node = firstNode;
            while (node.nextNode != null)
            {
                node.index = index;
                index++;
                node = node.nextNode;
            }
            //after null
            index++;
            node.index = index;
        }

        public override string GetCommandList()
        {
            return "";
        }

        public override void SendCommand(string command)
        {
            if (command.Equals("LinkedList.Add"))
            {
                StartCoroutine(AddNode());
            }
            else if (command.Equals("LinkedList.AddFirst"))
            {
                AddFirst();
            }
            else if (command.Equals("LinkedList.AddLast"))
            {
                AddLast();
            }
            else if (command.Contains("LinkedList.AddBefore"))
            {
                Match match = Regex.Match(command, @"\((\d+)\)");
                if (match.Success)
                {
                    int value = int.Parse(match.Groups[1].Value);
                    //AddBefore(value);
                    StartCoroutine(PointIndexAnimAndStartNextProcess(value, AddBefore(value)));

                }
            }
            else if (command.Contains("LinkedList.AddAfter"))
            {
                Match match = Regex.Match(command, @"\((\d+)\)");
                if (match.Success)
                {
                    int value = int.Parse(match.Groups[1].Value);
                    //AddAfter(value);
                    StartCoroutine(PointIndexAnimAndStartNextProcess(value, AddAfter(value)));

                }
            }
            else if (command.Contains("LinkedList.Remove"))
            {
                Match match = Regex.Match(command, @"\((\d+)\)");
                if (match.Success)
                {
                    int value = int.Parse(match.Groups[1].Value);
                    //Remove(value);
                    StartCoroutine(PointIndexAnimAndStartNextProcess(value, Remove(value)));

                }
            }
            else if (command.Contains("Point"))
            {
                Match match = Regex.Match(command, @"\((\d+)\)");
                if (match.Success)
                {
                    int value = int.Parse(match.Groups[1].Value);
                    StartCoroutine(PointIndexAnim(value));

                }
            }
            else
            {
                SendResponseToCommand("<color=red><b>Invalid Command</b></color>");
                CommandConsole.Instance.CommandComplete();
            }
        }

        public override void SendResponseToCommand(string response)
        {

        }


    }
    public class CustomLinkedListNode
    {
        public DataNode dataNode;
        public CustomLinkedListNode nextNode;
        public CustomLinkedListNode previousNode;
        public int index = -1;
        public CustomLinkedListNode(DataNode node, CustomLinkedListNode nextNode, CustomLinkedListNode previousNode, int currentIndex)
        {
            dataNode = node;
            this.nextNode = nextNode;
            this.previousNode = previousNode;
            index = currentIndex;
        }
        public void AutoBound()
        {
            if (nextNode != null)
            {
                dataNode.nextLineRenderer.SetPosition(0, dataNode.linkPoints[1].position);
                dataNode.nextLineRenderer.SetPosition(1, new Vector3(dataNode.linkPoints[1].position.x + 0.5f, dataNode.linkPoints[1].position.y));
                dataNode.nextLineRenderer.SetPosition(2, new Vector3(nextNode.dataNode.linkPoints[0].position.x - 0.5f, nextNode.dataNode.linkPoints[1].position.y));
                dataNode.nextLineRenderer.SetPosition(3, nextNode.dataNode.linkPoints[0].position);
                CommandConsole.Instance.IncreaseComplexityCount();
            }
            else
            {
                dataNode.nextLineRenderer.SetPosition(0, Vector3.zero);
                dataNode.nextLineRenderer.SetPosition(1, Vector3.zero);
                dataNode.nextLineRenderer.SetPosition(2, Vector3.zero);
                dataNode.nextLineRenderer.SetPosition(3, Vector3.zero);
                CommandConsole.Instance.IncreaseComplexityCount();
            }
            if (previousNode != null)
            {
                dataNode.previousLineRenderer.SetPosition(0, dataNode.linkPoints[0].position);
                dataNode.previousLineRenderer.SetPosition(1, new Vector3(dataNode.linkPoints[0].position.x - 0.5f, dataNode.linkPoints[0].position.y));
                dataNode.previousLineRenderer.SetPosition(2, new Vector3(previousNode.dataNode.linkPoints[1].position.x + 0.5f, previousNode.dataNode.linkPoints[0].position.y));
                dataNode.previousLineRenderer.SetPosition(3, previousNode.dataNode.linkPoints[1].position);
                CommandConsole.Instance.IncreaseComplexityCount();
            }
            else
            {
                dataNode.previousLineRenderer.SetPosition(0, Vector3.zero);
                dataNode.previousLineRenderer.SetPosition(1, Vector3.zero);
                dataNode.previousLineRenderer.SetPosition(2, Vector3.zero);
                dataNode.previousLineRenderer.SetPosition(3, Vector3.zero);
                CommandConsole.Instance.IncreaseComplexityCount();
            }
        }
        public void BoundNode(CustomLinkedListNode previousNode, CustomLinkedListNode nextNode)
        {
            if (nextNode != null)
            {
                dataNode.nextLineRenderer.SetPosition(0, dataNode.linkPoints[1].position);
                dataNode.nextLineRenderer.SetPosition(1, nextNode.dataNode.linkPoints[0].position);
                CommandConsole.Instance.IncreaseComplexityCount();
            }
            else
            {
                dataNode.nextLineRenderer.SetPosition(0, Vector3.zero);
                dataNode.nextLineRenderer.SetPosition(1, Vector3.zero);
                CommandConsole.Instance.IncreaseComplexityCount();
            }
            if (previousNode != null)
            {
                dataNode.previousLineRenderer.SetPosition(0, dataNode.linkPoints[0].position);
                dataNode.previousLineRenderer.SetPosition(1, previousNode.dataNode.linkPoints[1].position);
                CommandConsole.Instance.IncreaseComplexityCount();
            }
            else
            {
                dataNode.previousLineRenderer.SetPosition(0, Vector3.zero);
                dataNode.previousLineRenderer.SetPosition(1, Vector3.zero);
                CommandConsole.Instance.IncreaseComplexityCount();
            }

        }

    }



}

