using System.Collections;
using System.Collections.Generic;
using CollectionsTraining.DataVisual.DataNode;
using UnityEngine;
using PoolSystem;
using System;
using CollectionsTraining.DataVisual.BracketVisual;
using CollectionsTraining.DataVisual.VisualPointingArrow;

namespace CollectionsTraining.CollectionVisulator.QueueCollectionVisulator
{
    public class QueueCollectionVisulator : ArrayBasedCollection
    {
        private int head;
        private int tail;
        private int capacity;
        private int count;
        public DataNode[] nodeQueue;
        private VisulatorPointingArrow headPointerArrow;
        private VisulatorPointingArrow tailPointerArrow;
        void Start()
        {
            Initialize(10, 2, 2);
            InitializeArrows();
            InitializeQueue();
            CreateAndAddElementsToQueue();
            InitializeQueueBrackets();
            SetBracketsPositions(currentlyUsingBrackets[0].transform, currentlyUsingBrackets[1].transform, bracketsStartPosition, capacity);
            InitializeNodePositions(nodeQueue, count);
            SetPointerArrowVisualPosition(tailPointerArrow, nodeQueue[tail], 0.2f, default);
            SetPointerArrowVisualPosition(headPointerArrow, nodeQueue[head], 0.2f, default);
            SendResponseToCommand("Green arrow represents <color=green><b>HEAD</b></color>.Red arrow represents <color=red><b>TAIL</b></color>");
        }

        private void InitializeQueue()
        {
            //frontPointerArrow = PoolManager.Instance.DequeueItemFromPool<VisulatorPointingArrow>("PointingArrow");
            //rearPointerArrow = PoolManager.Instance.DequeueItemFromPool<VisulatorPointingArrow>("PotingArrow");
            capacity = nodeCreateAmount;
            nodeQueue = new DataNode[capacity];
            head = -1;
            tail = -1;
            count = 0;
        }
        private void InitializeArrows()
        {
            headPointerArrow = PoolManager.Instance.DequeueItemFromPool<VisulatorPointingArrow>(PointingArrowPool);
            tailPointerArrow = PoolManager.Instance.DequeueItemFromPool<VisulatorPointingArrow>(PointingArrowPool);
            headPointerArrow.visualRenderer.material.color = Color.green;
            tailPointerArrow.visualRenderer.material.color = Color.red;
        }
        #region Queue
        private void CreateAndAddElementsToQueue()
        {
            for (int i = 0; i < nodeCreateAmount; i++)
            {
                DataNode node = PoolManager.Instance.DequeueItemFromPool<DataNode>(DataNodePool, visualsParent.transform);
                AdjustNodeScaleByCollectionCount(node);
                Enqueue(node);
            }

        }

        private void InitializeQueueBrackets()
        {
            for (int i = 0; i < 2; i++)
            {
                currentlyUsingBrackets.Add(PoolManager.Instance.DequeueItemFromPool<VisualBracket>(BracketPool, visualsParent.transform));
            }

        }
        private void Enqueue(DataNode dataNode)
        {
            if (IsFull())
            {
                nodeQueue = ExtendArray(nodeQueue, 2, count);//extends it 
                capacity = nodeQueue.Length;
                tail = count % capacity;
            }
            else
            {
                tail = (tail + 1) % capacity;
            }
            if (IsEmpty())
            {
                head = 0;
            }
            nodeQueue[tail] = dataNode;
            count++;
        }
        private DataNode Dequeue()
        {
            SendResponseToCommand("<color=blue><i>Command recieved processing...</i></color>");
            if (IsEmpty())
            {
                Debug.LogWarning("Queue is Empty");
                return null;
            }
            DataNode item = nodeQueue[head];
            NullifyDataNodeAnim(item);
            nodeQueue[head] = default;//make it default or null for the GC
            if (head == tail)
            {
                head = -1;
                tail = -1;
            }
            else
            {
                head = (head + 1) % capacity;
            }
            SetPointerArrowVisualPosition(headPointerArrow, nodeQueue[head], 0.2f, default);
            count--;
            SendResponseToCommand("<color=green><i>Process Done!</i></color>");
            CommandConsole.Instance.CommandComplete();
            return item;

        }

        private void ClearQueue()
        {
            if (capacity > 0)
            {
                Array.Clear(nodeQueue, 0, capacity);
                head = -1;
                tail = -1;
                capacity = 0;
                count = 0;
            }

        }
        protected override DataNode[] ExtendArray<DataNode>(DataNode[] array, int capacityMultiplier, int count)
        {
            DataNode[] extendedArray = base.ExtendArray(array, capacityMultiplier, count);
            UpdateValuesOnArrayCopied();
            return extendedArray;
        }

        private bool IsEmpty()
        {
            return count == 0;
        }
        private bool IsFull()
        {
            return count == capacity;
        }
        #endregion

        private void EnqueueAnimation()
        {
            //float visualsParentYPosForAnimation = 0f;
            SendResponseToCommand("<color=blue><i>Command recieved processing...</i></color>");
            DataNode dataNode = PoolManager.Instance.DequeueItemFromPool<DataNode>(DataNodePool, visualsParent.transform);
            dataNode.SetWorldPosition(new Vector3(2f, 2f));
            AdjustNodeScaleByCollectionCount(dataNode);
            if (capacity > count)
            {
                Enqueue(dataNode);
                dataNode.MoveByTweening(GetNodePositionByIndex(tail), 0.5f, default, () =>
                {
                    CommandConsole.Instance.CommandComplete();
                    SetPointerArrowVisualPosition(tailPointerArrow, nodeQueue[tail], 0.2f, default);

                });
            }
            else
            {
                Enqueue(dataNode);
                //visualsParentYPosForAnimation = 2f;
                StartCopyArrayAnim(nodeQueue, count, capacity);
            }

        }
        protected override IEnumerator CopyArrayToNewArray(Vector3[] copyPositions, DataNode[] nodes, int nodesCount, float bracketYPos)
        {
            yield return base.CopyArrayToNewArray(copyPositions, nodes, nodesCount, bracketYPos);
            SetElementsToDefaultPosition(nodeQueue, count, () =>
            {
                SetPointerArrowVisualPosition(tailPointerArrow, nodeQueue[tail], 0.2f, default);
                SetPointerArrowVisualPosition(headPointerArrow, nodeQueue[head], 0.2f, default);

            });

        }
        private void UpdateValuesOnArrayCopied()
        {
            head = 0;
            tail = count - 1;
        }

        public override void SendCommand(string command)
        {
            if (command.Contains("Queue.Enqueue"))
            {
                EnqueueAnimation();
            }
            else if (command.Contains("Queue.Dequeue"))
            {
                Dequeue();
            }
            else if (command.Contains("Queue.Trim"))
            {
                if (TrimExcessCollection(nodeQueue, ref capacity, ref count))
                {
                    UpdateValuesOnArrayCopied();
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
            CommandConsole.Instance.RecieveResponse(response, typeof(QueueCollectionVisulator).Name);
        }

        public override string GetCommandList()
        {
            string commandList = "<color=black><b>QueueCommands-></b></color>\n<color=#FF00FF>Queue.Enqueue</color>\n<color=#FF00FF>Queue.Dequeue</color>\n<color=#FF00FF>Queue.Trim</color>";
            commandList += "\n----------------------------";
            return commandList;
        }
    }


}
