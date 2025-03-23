using UnityEngine;

namespace CollectionsTraining.DataVisual.DataNode
{
    public class DataNode : DataVisual
    {
        //it'll be dynamic soon
        public Transform[] linkPoints = new Transform[2];
        public LineRenderer nextLineRenderer;
        public LineRenderer previousLineRenderer;

        public void SetLineRenderersPointCount(int count)
        {
            nextLineRenderer.positionCount = count;
            previousLineRenderer.positionCount = count;
        }

        public override void OnCreatedForPool()
        {
            base.OnCreatedForPool();
            nextLineRenderer.material.color = Color.green;
            previousLineRenderer.material.color = Color.red;
            //just in case
            nextLineRenderer.enabled = true;
            previousLineRenderer.enabled = true;
        }
        public override void OnEnqueuePool()
        {
            base.OnEnqueuePool();
            Vector3[] defaultPositions = new Vector3[nextLineRenderer.positionCount];
            nextLineRenderer.SetPositions(defaultPositions);
            previousLineRenderer.SetPositions(defaultPositions);
            nextLineRenderer.enabled = false;
            previousLineRenderer.enabled = false;
        }
        public override void OnDequeuePool()
        {
            base.OnDequeuePool();
            nextLineRenderer.enabled = true;
            previousLineRenderer.enabled = true;
        }

    }

}

