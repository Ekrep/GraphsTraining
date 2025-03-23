using CollectionsTraining.DataVisual.DataNode;
using UnityEngine;
using PoolSystem;
using CollectionsTraining.DataVisual.VisualPointingArrow;
using System.Linq;
using System.Text.RegularExpressions;
using CollectionsTraining.CollectionVisulator.LinkedListVisulator;
using CollectionsTraining.CollectionUtils;
using System.Collections;
using Unity.VisualScripting;

namespace CollectionsTraining.CollectionVisulator.MapCollectionVisulator
{
    public class MapCollectionVisulator : ArrayBasedCollection
    {
        //KEYS ARE ONLY STRING ON THIS TRAINING,AND ONLY DATA NODE CAN BE ASSIGNABLE TO THE DICT.
        private Entry[] buckets;
        private int count = 0;
        private VisulatorPointingArrow pointingArrow;
        private void Start()
        {
            Initialize(nodeCreateAmount, 2, 2);
            InitializeDictionary();
            currentlyUsingBrackets = CreateBrackets(2).ToList();
            SetBracketsPositions(currentlyUsingBrackets[0].transform, currentlyUsingBrackets[1].transform, bracketsStartPosition, buckets.Length);
            pointingArrow = PoolManager.Instance.DequeueItemFromPool<VisulatorPointingArrow>(PointingArrowPool);
        }

        private void InitializeDictionary()
        {
            buckets = new Entry[nodeCreateAmount];
        }
        private int GetBucketIndex(string key)
        {
            int hash = key.GetHashCode();
            return Mathf.Abs(hash % buckets.Length);
        }
        private void Add(string key)
        {
            CustomLinkedListNode linkedNode = new CustomLinkedListNode(PoolManager.Instance.DequeueItemFromPool<DataNode>(DataNodePool, visualsParent.transform), null, null, default);
            linkedNode.dataNode.SetWorldPosition(new Vector3(2f, 2f, 0));
            AdjustNodeScaleByCollectionCount(linkedNode.dataNode);
            StartCoroutine(AddAnim(key, linkedNode));

        }
        private CustomLinkedListNode[] GetDataNodesInEntries()
        {
            CustomLinkedListNode[] nodes = new CustomLinkedListNode[count];
            for (int i = 0; i < buckets.Length; i++)
            {
                if (buckets[i] != null)
                {
                    nodes[i] = buckets[i].Value;
                }
            }
            return nodes;

        }
        private IEnumerator AddAnim(string key, CustomLinkedListNode linkedNode)
        {
            linkedNode.dataNode.SetWorldPosition(new Vector3(2f, 2f, 0));
            AdjustNodeScaleByCollectionCount(linkedNode.dataNode);
            int index = GetBucketIndex(key);
            Entry newEntry = new Entry(key, linkedNode);
            if (buckets[index] == null)
            {
                buckets[index] = newEntry;
                linkedNode.dataNode.MoveByTweening(GetNodePositionByIndex(index), 0.2f, default, CommandConsole.Instance.CommandComplete);
            }
            else
            {
                Entry currentEntry = buckets[index];
                while (currentEntry.Next != null)
                {
                    if (currentEntry.Key.Equals(key))
                    {
                        NullifyDataNodeAnim(linkedNode.dataNode);
                        CommandConsole.Instance.CommandComplete();
                        Debug.LogWarning("KeyContains");
                        yield return null;
                    }
                    currentEntry = currentEntry.Next;
                }
                if (currentEntry.Key.Equals(key))
                {
                    NullifyDataNodeAnim(linkedNode.dataNode);
                    CommandConsole.Instance.CommandComplete();
                    Debug.LogWarning("KeyContains");
                    yield return null;
                }
                currentEntry.Next = newEntry;
                Vector3 currentEntryPos = currentEntry.Value.dataNode.transform.position;
                BoolWrapper wrapper = new BoolWrapper(true);
                StartCoroutine(GetNodeFromDictionaryAnim(currentEntry.Key, wrapper));
                yield return new WaitUntillMultipableMethodsComplete(wrapper);
                linkedNode.dataNode.MoveByTweening(new Vector3(currentEntryPos.x, currentEntryPos.y - 1f), 0.2f, default, () =>
                {
                    CommandConsole.Instance.CommandComplete();
                    currentEntry.BoundEntry();
                });

            }
            count++;
        }
        private IEnumerator GetNodeFromDictionaryAnim(string key, BoolWrapper wrapper)
        {
            pointingArrow.SetWorldPosition(Vector3.zero);
            int index = GetBucketIndex(key);
            Entry currentEntry = buckets[index];
            Vector3 nodePos;
            while (currentEntry != null)
            {
                nodePos = currentEntry.Value.dataNode.transform.position;
                pointingArrow.MoveByTweening(new Vector3(nodePos.x, nodePos.y + 0.5f), 0.2f, default);
                if (currentEntry.Key.Equals(key))
                {
                    // lazynesss!!
                    wrapper.value = false;
                    CommandConsole.Instance.CommandComplete();
                    yield break;
                }
                yield return new WaitUntilNodeReachesTargetPosition(pointingArrow.transform, new Vector3(nodePos.x, nodePos.y + 0.5f));
                currentEntry = currentEntry.Next;
            }
            Debug.LogWarning("Key Not Found");
            wrapper.value = false;
            CommandConsole.Instance.CommandComplete();
            yield return null;

        }
        private IEnumerator GetNodeFromDictionaryAnim(string key)
        {
            pointingArrow.SetWorldPosition(Vector3.zero);
            int index = GetBucketIndex(key);
            Entry currentEntry = buckets[index];
            Vector3 nodePos;
            while (currentEntry != null)
            {
                nodePos = currentEntry.Value.dataNode.transform.position;
                pointingArrow.MoveByTweening(new Vector3(nodePos.x, nodePos.y + 0.5f), 0.2f, default);
                if (currentEntry.Key.Equals(key))
                {
                    // lazynesss!!
                    CommandConsole.Instance.CommandComplete();
                    yield break;
                }
                yield return new WaitUntilNodeReachesTargetPosition(pointingArrow.transform, new Vector3(nodePos.x, nodePos.y + 0.5f));
                currentEntry = currentEntry.Next;
            }
            Debug.LogWarning("Key Not Found");
            CommandConsole.Instance.CommandComplete();
            yield return null;

        }
        private DataNode GetNodeFromDictionary(string key)//make it anim
        {
            int index = GetBucketIndex(key);
            Entry currentEntry = buckets[index];
            while (currentEntry != null)
            {
                if (currentEntry.Key.Equals(key))
                {
                    Vector3 nodePos = currentEntry.Value.dataNode.transform.position;// lazynesss!!
                    pointingArrow.MoveByTweening(new Vector3(nodePos.x, nodePos.y + 0.5f), 0.2f, default);
                    CommandConsole.Instance.CommandComplete();
                    return currentEntry.Value.dataNode;
                }
                currentEntry = currentEntry.Next;
            }
            Debug.LogWarning("Key Not Found");
            CommandConsole.Instance.CommandComplete();
            return null;
        }
        private IEnumerator Remove(string key)
        {
            Vector3 targetPos;
            int index = GetBucketIndex(key);
            Entry currentEntry = buckets[index];
            Entry previous = null;
            while (currentEntry != null)
            {
                targetPos = new Vector3(currentEntry.Value.dataNode.transform.position.x, currentEntry.Value.dataNode.transform.position.y + 0.5f);
                pointingArrow.MoveByTweening(targetPos, 0.2f, default);
                if (currentEntry.Key.Equals(key))
                {
                    if (previous == null)
                    {
                        buckets[index] = currentEntry.Next;
                    }
                    else
                    {
                        previous.Next = currentEntry.Next;
                    }
                    count--;
                    previous.BoundEntry();
                    NullifyDataNodeAnim(currentEntry.Value.dataNode);
                    CommandConsole.Instance.CommandComplete();
                    pointingArrow.SetWorldPosition(Vector3.zero);
                    yield break;
                }
                previous = currentEntry;
                currentEntry = currentEntry.Next;
                yield return new WaitUntilNodeReachesTargetPosition(pointingArrow.transform, targetPos);
            }

            Debug.LogWarning("Key Not Found");
            CommandConsole.Instance.CommandComplete();
        }

        public override string GetCommandList()
        {
            throw new System.NotImplementedException();
        }

        public override void SendCommand(string command)
        {
            if (command.Contains("Dictionary.Add"))
            {
                Match match = Regex.Match(command, @"\((.*?)\)");
                if (match.Success)
                {
                    string value = match.Groups[1].Value;
                    Add(value);
                }

            }
            if (command.Contains("Dictionary.Get"))
            {
                Match match = Regex.Match(command, @"\((.*?)\)");
                if (match.Success)
                {
                    string value = match.Groups[1].Value;
                    StartCoroutine(GetNodeFromDictionaryAnim(value));
                    //GetNodeFromDictionary(value);
                }

            }
            if (command.Contains("Dictionary.Remove"))
            {
                Match match = Regex.Match(command, @"\((.*?)\)");
                if (match.Success)
                {
                    string value = match.Groups[1].Value;
                    StartCoroutine(Remove(value));
                }

            }
        }

        public override void SendResponseToCommand(string response)
        {
            throw new System.NotImplementedException();
        }

        private class Entry
        {
            public string Key;
            public CustomLinkedListNode Value;
            public Entry Next;

            public Entry(string key, CustomLinkedListNode value)
            {
                Key = key;
                Value = value;
                Next = null;
                Value.dataNode.SetLineRenderersPointCount(2);
            }
            public void BoundEntry()
            {
                if (Next != null)
                {
                    Value.BoundNode(null, Next.Value);
                }
                else
                {
                    Value.BoundNode(null, null);
                }

            }


        }
    }

}

