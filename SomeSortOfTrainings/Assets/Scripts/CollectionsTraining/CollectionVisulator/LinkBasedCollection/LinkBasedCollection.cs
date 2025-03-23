using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CollectionsTraining.CollectionVisulator;
using PoolSystem;

public class LinkBasedCollection : CollectionVisulator
{
    protected virtual void Initialize(int dataNodeCreationAmount, int pointingArrowCreationAmount)
    {
        PoolManager.Instance.CreatePool("DataNode", nodePrefab, dataNodeCreationAmount, transform);
        PoolManager.Instance.CreatePool("PointingArrow", pointingArrowPrefab, pointingArrowCreationAmount, transform);
        visualsParent = new GameObject("VisualsParent");
        CommandConsole.Instance.AssignConsolable(this);
    }
}
