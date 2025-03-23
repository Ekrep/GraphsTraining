using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PoolSystem;
using CollectionsTraining.DataVisual.DataNode;
using CollectionsTraining.DataVisual.BracketVisual;
using Consoleable;
using CollectionsTraining.CollectionUtils;
using System;
using DG.Tweening;
using CollectionsTraining.DataVisual.VisualPointingArrow;
namespace CollectionsTraining.CollectionVisulator
{
    public class CollectionVisulator : MonoBehaviour, IConsolable
    {
        public const string DataNodePool = "DataNode";
        public const string BracketPool = "VisualBracket";
        public const string PointingArrowPool = "PointingArrow";
        [SerializeField] protected DataNode nodePrefab;
        [SerializeField] protected VisulatorPointingArrow pointingArrowPrefab;
        [SerializeField] protected int nodeCreateAmount;
        [SerializeField] protected float baseNodeDistance;
        protected GameObject visualsParent;

        protected void AdjustNodeScaleByCollectionCount(DataNode node)
        {
            node.SetScale(new Vector3(1f / nodeCreateAmount, 0.5f, 0.5f));
        }

        protected float GetNodeScaleRatio()
        {
            return 1f / nodeCreateAmount;
        }

        protected void NullifyDataNodeAnim(DataNode node)
        {
            PoolManager.Instance.EnqueueItemToPool(DataNodePool, node);

        }
        protected void SetPointerArrowVisualPosition(VisulatorPointingArrow arrow, DataNode node, float moveDuration, Ease ease)
        {
            Vector3 destinatedPos = new Vector3(node.transform.position.x, node.transform.position.y + 0.5f);
            arrow.MoveByTweening(destinatedPos, moveDuration, ease);
        }

        public virtual void SendCommand(string command)
        {
            throw new NotImplementedException();
        }

        public virtual void SendResponseToCommand(string response)
        {
            throw new NotImplementedException();
        }

        public virtual string GetCommandList()
        {
            throw new NotImplementedException();
        }
    }


}
