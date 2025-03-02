using UnityEngine;
using GraphProperties;

namespace GraphProperties.GraphData
{
    [CreateAssetMenu(menuName = "GraphProperties/GraphData")]
    public class GraphData : GraphPropertiesScriptable
    {
        public const int maxResolution = 1000;

        public Point pointPrefab;
        public int resolution;
        public Vector3 pointScale;

    }
}

