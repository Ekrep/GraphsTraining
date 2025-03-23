using System;
using System.Collections;
using CollectionsTraining.DataVisual.BracketVisual;
using CollectionsTraining.DataVisual.DataNode;
using CollectionsTraining.DataVisual.VisualPointingArrow;
using PoolSystem;
using UnityEngine;

namespace CollectionsTraining.CollectionVisulator.StackCollectionVisulator
{
    public class StackCollectionVisulator : ArrayBasedCollection
    {
        [SerializeField] private int top;
        [SerializeField] private int capacity;
        [SerializeField] private int count;
        private DataNode[] nodeStack;
        private VisulatorPointingArrow topPointerArrow;

        //Actually huge similarity with Queue but i didn't want to create another parent class for this structures
        void Start()
        {
            Initialize(10, 2, 1);
            InitializeArrows();
            InitializeStack();
            CreateAndAddElementsToStack();
            InitializeStackBrackets();
            SetBracketsPositions(currentlyUsingBrackets[0].transform, currentlyUsingBrackets[1].transform, bracketsStartPosition, capacity);
            InitializeNodePositions(nodeStack, count);
            SetPointerArrowVisualPosition(topPointerArrow, nodeStack[top], 0.2f, default);
            SendResponseToCommand("Green arrow represents <color=green><b>TOP</b></color> of the <color=black><b>STACK</b></color>");
        }

        private void InitializeStack()
        {
            capacity = nodeCreateAmount;
            nodeStack = new DataNode[capacity];
            top = -1;
            count = 0;
        }
        private void InitializeArrows()
        {
            topPointerArrow = PoolManager.Instance.DequeueItemFromPool<VisulatorPointingArrow>(PointingArrowPool);
            topPointerArrow.visualRenderer.material.color = Color.green;
        }
        #region Queue
        private void CreateAndAddElementsToStack()
        {
            for (int i = 0; i < nodeCreateAmount; i++)
            {
                DataNode node = PoolManager.Instance.DequeueItemFromPool<DataNode>(DataNodePool, visualsParent.transform);
                AdjustNodeScaleByCollectionCount(node);
                Push(node);
            }

        }

        private void InitializeStackBrackets()
        {
            for (int i = 0; i < 2; i++)
            {
                currentlyUsingBrackets.Add(PoolManager.Instance.DequeueItemFromPool<VisualBracket>(BracketPool, visualsParent.transform));
            }

        }
        private void Push(DataNode dataNode)
        {
            if (IsFull())
            {
                nodeStack = ExtendArray(nodeStack, 2, count);//extends it 
                capacity = nodeStack.Length;
            }
            if (IsEmpty())
            {
                top = -1;
            }
            top++;
            nodeStack[top] = dataNode;
            count++;
        }
        private DataNode Pop()
        {
            SendResponseToCommand("<color=blue><i>Command recieved processing...</i></color>");
            if (IsEmpty())
            {
                Debug.LogWarning("Stack is Empty");
                return null;
            }
            DataNode item = nodeStack[top];
            NullifyDataNodeAnim(item);
            nodeStack[top] = default;//make it default or null for the GC
            if (top == capacity)
            {
                top = 0;
            }
            else
            {
                top--;
            }
            SetPointerArrowVisualPosition(topPointerArrow, nodeStack[top], 0.2f, default);
            count--;
            SendResponseToCommand("<color=green><i>Process Done!</i></color>");
            CommandConsole.Instance.CommandComplete();
            return item;

        }

        private void ClearStack()
        {
            if (capacity > 0)
            {
                Array.Clear(nodeStack, 0, capacity);
                top = -1;
                capacity = 0;
                count = 0;
            }

        }
        protected override DataNode[] ExtendArray<DataNode>(DataNode[] array, int capacityMultiplier, int count)
        {
            return base.ExtendArray(array, capacityMultiplier, count);
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

        private void PushAnimation()
        {
            //float visualsParentYPosForAnimation = 0f;
            SendResponseToCommand("<color=blue><i>Command recieved processing...</i></color>");
            DataNode dataNode = PoolManager.Instance.DequeueItemFromPool<DataNode>(DataNodePool, visualsParent.transform);
            dataNode.SetWorldPosition(new Vector3(2f, 2f));
            AdjustNodeScaleByCollectionCount(dataNode);
            if (capacity > count)
            {
                Push(dataNode);
                dataNode.MoveByTweening(GetNodePositionByIndex(top), 0.5f, default, () =>
                {
                    SetPointerArrowVisualPosition(topPointerArrow, dataNode, 0.2f, default);
                    CommandConsole.Instance.CommandComplete();

                });
            }
            else
            {
                Push(dataNode);
                //visualsParentYPosForAnimation = 2f;
                StartCopyArrayAnim(nodeStack, count, capacity);
            }

        }
        protected override IEnumerator CopyArrayToNewArray(Vector3[] copyPositions, DataNode[] nodes, int nodesCount, float bracketYPos)
        {
            yield return base.CopyArrayToNewArray(copyPositions, nodes, nodesCount, bracketYPos);
            SetElementsToDefaultPosition(nodeStack, count, () =>
            {
                SetPointerArrowVisualPosition(topPointerArrow, nodeStack[top], 0.2f, default);

            });

        }

        public override string GetCommandList()
        {
            return "null";
        }

        public override void SendCommand(string command)
        {
            if (command.Contains("Stack.Push"))
            {
                PushAnimation();
            }
            else if (command.Contains("Stack.Pop"))
            {
                Pop();
            }
            else if (command.Contains("Stack.Trim"))
            {
                TrimExcessCollection(nodeStack, ref capacity, ref count);
            }
            else
            {
                SendResponseToCommand("<color=red><b>Invalid Command</b></color>");
                CommandConsole.Instance.CommandComplete();
            }
        }

        public override void SendResponseToCommand(string response)
        {
            CommandConsole.Instance.RecieveResponse(response, typeof(StackCollectionVisulator).Name);
        }
    }

}

