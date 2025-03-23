using System.Collections;
using System.Collections.Generic;
using CollectionsTraining.DataVisual.DataNode;
using UnityEngine;
using PoolSystem;
using DG.Tweening;
using CollectionsTraining.DataVisual.BracketVisual;
using System.Text.RegularExpressions;
using CollectionsTraining.CollectionUtils;

namespace CollectionsTraining.CollectionVisulator.ListCollectionVisulator
{
    public class ListCollectionVisulator : ArrayBasedCollection
    {
        private List<DataNode> nodes;
        // [SerializeField] private CorutineHelper helper;
        private void Start()
        {
            Initialize(nodeCreateAmount * 2, 4, 0);
            InitializeNodes();
            IntializeBrackets();
            InitializeNodePositions(nodes.ToArray(), nodes.Count);
        }

        private void InitializeNodes()
        {
            nodes = new List<DataNode>(nodeCreateAmount);
            for (int i = 0; i < nodeCreateAmount; i++)
            {
                nodes.Add(PoolManager.Instance.DequeueItemFromPool<DataNode>("DataNode", visualsParent.transform));
                AdjustNodeScaleByCollectionCount(nodes[i]);
            }
        }
        private void IntializeBrackets()
        {
            for (int i = 0; i < 2; i++)
            {
                currentlyUsingBrackets.Add(PoolManager.Instance.DequeueItemFromPool<VisualBracket>("VisualBracket", visualsParent.transform));
            }
            SetBracketsPositions(currentlyUsingBrackets[0].transform, currentlyUsingBrackets[1].transform, bracketsStartPosition, nodes.Count);
        }

        private void RemoveFromList(int index)
        {
            Vector3 nodePos = nodes[index].transform.position;
            Vector3[] positions = GetCollectionNodePositions(nodes.ToArray(), nodes.Count);
            nodes[index].MoveByTweening(new Vector3(nodePos.x, nodePos.y + 2f, nodePos.z), 1f, Ease.Flash, () =>
            {
                StartCoroutine(AjdustListPositionsAndRemoveFromList(index, positions));
            });
        }

        private IEnumerator AjdustListPositionsAndRemoveFromList(int removedIndex, Vector3[] positionsBeforeRemove)
        {
            SendResponseToCommand("<color=blue><i>Command recieved processing...</i></color>");
            Transform t;
            for (int i = removedIndex; i < nodes.Count - 1; i++)
            {
                t = nodes[i + 1].transform;
                nodes[i + 1].MoveByTweening(positionsBeforeRemove[i], 0.4f, default);
                yield return new WaitUntilNodeReachesTargetPosition(t, positionsBeforeRemove[i]);
            }
            PoolManager.Instance.EnqueueItemToPool("DataNode", nodes[removedIndex]);
            nodes.RemoveAt(removedIndex);
            SendResponseToCommand("<color=green><i>Process Done!</i></color>");
            CommandConsole.Instance.CommandComplete();
        }
        private void AddDataNodeToList(int amount)
        {
            StartCoroutine(AddDataNodeToListCoroutine(amount));
        }
        private IEnumerator AddDataNodeToListCoroutine(int amount)
        {
            BoolWrapper wrapper = new BoolWrapper();
            //float visualsParentYPosForAnimation = 0f;
            SendResponseToCommand("<color=blue><i>Command recieved processing...</i></color>");
            for (int i = 0; i < amount; i++)
            {
                wrapper.value = true;
                DataNode dataNode = PoolManager.Instance.DequeueItemFromPool<DataNode>("DataNode", visualsParent.transform);
                dataNode.SetWorldPosition(new Vector3(2f, 2f));
                AdjustNodeScaleByCollectionCount(dataNode);
                if (nodes.Capacity > nodes.Count)
                {
                    nodes.Add(dataNode);
                    dataNode.MoveByTweening(new Vector3(nodes[nodes.Count - 2].transform.position.x + (nodes[0].GetScale().x * baseNodeDistance), currentlyUsingBrackets[0].transform.position.y, 0), 0.5f, default, () =>
                    {
                        wrapper.value = false;
                    });

                }
                else
                {
                    nodes.Add(dataNode);
                    //visualsParentYPosForAnimation = 2f;
                    VisualBracket[] createdBrackets = CreateBrackets(2);
                    SetBracketsPositions(createdBrackets[0].transform, createdBrackets[1].transform, new Vector3(currentlyUsingBrackets[0].transform.position.x,
                    currentlyUsingBrackets[0].transform.position.y - 2f, 0f), nodes.Capacity);
                    Vector3[] targetCopyPositions = GetCollectionNodePositions(nodes.ToArray(), nodes.Count);
                    targetCopyPositions[targetCopyPositions.Length - 1] = new Vector3(nodes[nodes.Count - 2].transform.position.x + (nodes[nodes.Count - 2].GetScale().x * baseNodeDistance), 0, 0);
                    garbageBrackects[0] = currentlyUsingBrackets[0];
                    garbageBrackects[1] = currentlyUsingBrackets[1];
                    currentlyUsingBrackets[0] = createdBrackets[0];
                    currentlyUsingBrackets[1] = createdBrackets[1];
                    StartCoroutine(CopyArrayToNewArray(targetCopyPositions, nodes.ToArray(), nodes.Count, currentlyUsingBrackets[0].transform.position.y, wrapper));
                }
                yield return new WaitUntillMultipableMethodsComplete(wrapper);
            }
            wrapper.value = false;
            SetElementsToDefaultPosition(nodes.ToArray(), nodes.Count);


        }
        protected override IEnumerator CopyArrayToNewArray(Vector3[] copyPositions, DataNode[] nodes, int nodesCount, float bracketYPos, BoolWrapper conditionWrapper)
        {
            yield return base.CopyArrayToNewArray(copyPositions, nodes, nodesCount, bracketYPos, conditionWrapper);
            conditionWrapper.value = false;

        }
        protected override IEnumerator CopyArrayToNewArray(Vector3[] copyPositions, DataNode[] nodes, int nodesCount, float bracketYPos)
        {
            yield return base.CopyArrayToNewArray(copyPositions, nodes, nodesCount, bracketYPos);
            SetElementsToDefaultPosition(nodes, nodesCount);
        }
        private void TrimExcessList()
        {
            if (nodes.Count < nodes.Capacity * 0.9f)//TrimExcess does not reduce capacity due to the 90% threshold.
            {
                SendResponseToCommand("<color=blue><i>Command recieved processing...</i></color>");
                nodes.TrimExcess();
                VisualBracket[] createdBrackets = CreateBrackets(2);
                SetBracketsPositions(createdBrackets[0].transform, createdBrackets[1].transform, new Vector3(currentlyUsingBrackets[0].transform.position.x, currentlyUsingBrackets[0].transform.position.y - 2f, 0f), nodes.Capacity);
                Vector3[] targetCopyPositions = GetCollectionNodePositions(nodes.ToArray(), nodes.Count);
                targetCopyPositions[targetCopyPositions.Length - 1] = new Vector3(nodes[nodes.Count - 2].transform.position.x + (nodes[nodes.Count - 2].GetScale().x * baseNodeDistance), 0, 0);
                garbageBrackects[0] = currentlyUsingBrackets[0];
                garbageBrackects[1] = currentlyUsingBrackets[1];
                currentlyUsingBrackets[0] = createdBrackets[0];
                currentlyUsingBrackets[1] = createdBrackets[1];
                StartCoroutine(CopyArrayToNewArray(targetCopyPositions, nodes.ToArray(), nodes.Count, currentlyUsingBrackets[0].transform.position.y));
            }
            else
            {
                SendResponseToCommand("There is no trimable Collection!!");
                CommandConsole.Instance.CommandComplete();
            }

        }


        public override void SendCommand(string command)
        {
            if (command.Contains("List.Add"))
            {
                Match match = Regex.Match(command, @"\((\d+)\)");
                if (match.Success)
                {
                    int value = int.Parse(match.Groups[1].Value);
                    AddDataNodeToList(value);

                }

            }
            else if (command.Contains("List.Remove"))
            {
                Match match = Regex.Match(command, @"\((\d+)\)");
                if (match.Success)
                {
                    int value = int.Parse(match.Groups[1].Value);
                    RemoveFromList(value);
                }

            }
            else if (command.Contains("List.Trim"))
            {
                TrimExcessList();
            }
            else
            {
                SendResponseToCommand("<color=red><b>Invalid Command</b></color>");
                CommandConsole.Instance.CommandComplete();
            }

        }
        public override void SendResponseToCommand(string response)
        {
            CommandConsole.Instance.RecieveResponse(response, typeof(ListCollectionVisulator).Name);
        }
        public override string GetCommandList()
        {
            string commandList = "<color=black><b>ListCommands-></b></color>\n<color=#FF00FF>List.Add</color>(<color=green><i>amount</i></color>)\n<color=#FF00FF>List.Remove</color>(<color=green><i>index</i></color>)\n<color=#FF00FF>List.Trim</color>";
            commandList += "\n----------------------------";
            return commandList;
        }
    }


}
