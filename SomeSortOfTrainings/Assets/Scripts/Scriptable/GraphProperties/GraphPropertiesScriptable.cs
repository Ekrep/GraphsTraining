using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Utilities;


namespace GraphProperties.GraphData
{
    [CreateAssetMenu(menuName = "GraphProperties/GraphData")]
    public class GraphPropertiesScriptable : ScriptableObject
    {
        public const int maxResolution = 1000;
        [System.Serializable]
        public struct MeshTypeDatas
        {
            public Mesh mesh;
            public Utils.MeshTypes meshType;

        }
        public Point pointPrefab;
        public int resolution;
        public Vector3 pointScale;
        [HideInInspector] public Mesh meshType;
        [SerializeField] private Utils.MeshTypes prefabMeshType;

        public List<MeshTypeDatas> meshTypeDataCollection;


        private void OnValidate()
        {
            EnsureUniqueMeshTypeData();
            FindCurrentMeshTypeOnCollection();
        }
        private void FindCurrentMeshTypeOnCollection()
        {
            if (meshTypeDataCollection.Count < 1)
            {
                Debug.LogWarning("Please add item to the collection!!");
                return;
            }
            for (int i = 0; i < meshTypeDataCollection.Count; i++)
            {
                if (meshTypeDataCollection[i].mesh != null && meshTypeDataCollection[i].meshType == prefabMeshType)
                {
                    meshType = meshTypeDataCollection[i].mesh;
                    return;
                }
            }
            Debug.LogError("NullRef on mesh collection");
            prefabMeshType = 0;
            meshType = meshTypeDataCollection[0].mesh;
        }
        private void EnsureUniqueMeshTypeData()
        {
            if (meshTypeDataCollection.Count > 1)
            {
                MeshTypeDatas lastItem = meshTypeDataCollection[meshTypeDataCollection.Count - 1];
                MeshTypeDatas secondLastItem = meshTypeDataCollection[meshTypeDataCollection.Count - 2];
                if (lastItem.mesh == secondLastItem.mesh && lastItem.meshType == secondLastItem.meshType)
                {

                    lastItem.mesh = null;
                    lastItem.meshType = Utils.MeshTypes.Null;
                    meshTypeDataCollection[meshTypeDataCollection.Count - 1] = lastItem;
                    Debug.LogWarning("Duplicate dedected nullifying");
                }

            }
        }
    }
}
