using System.Collections.Generic;
using UnityEngine;


namespace GraphProperties.GraphData
{
    [CreateAssetMenu(menuName = "GraphProperties/GraphData")]
    public class GraphPropertiesScriptable : ScriptableObject
    {
        [System.Serializable]
        public struct MeshTypeDatas
        {
            public Mesh mesh;
            public Utilities.Utils.MeshTypes meshType;

        }
        public Point pointPrefab;
        public int resolution;
        public Vector3 pointScale;
        [HideInInspector] public Mesh meshType;
        [SerializeField] private Utilities.Utils.MeshTypes prefabMeshType;

        public List<MeshTypeDatas> meshTypeDataCollection;


        private void OnValidate()
        {
            meshType = FindCurrentMeshTypeOnCollection();
        }
        private Mesh FindCurrentMeshTypeOnCollection()
        {
            for (int i = 0; i < meshTypeDataCollection.Count; i++)
            {
                if (meshTypeDataCollection[i].meshType == prefabMeshType)
                {
                    return meshTypeDataCollection[i].mesh;
                }
            }
            Debug.LogError("NullRef on mesh collection");
            return null;
        }

    }
}
