using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Utilities;

namespace GraphProperties
{
    [CreateAssetMenu(menuName = "GraphProperties/GraphData")]
    public class GraphPropertiesScriptable : ScriptableObject
    {
        [System.Serializable]
        public struct MeshTypeDatas
        {
            public Mesh mesh;
            public Utils.MeshTypes meshType;

        }
        [HideInInspector] public Mesh meshType;
        [SerializeField] private Utils.MeshTypes prefabMeshType;

        public List<MeshTypeDatas> meshTypeDataCollection;


        public virtual void OnValidate()
        {
            EnsureUniqueMeshTypeData();
            FindCurrentMeshTypeOnCollection(ref meshType, prefabMeshType);
        }
        protected void FindCurrentMeshTypeOnCollection(ref Mesh mType, Utils.MeshTypes kindOfMesh)
        {
            if (meshTypeDataCollection.Count < 1)
            {
                Debug.LogWarning("Please add item to the collection!!");
                return;
            }
            for (int i = 0; i < meshTypeDataCollection.Count; i++)
            {
                if (meshTypeDataCollection[i].mesh != null && meshTypeDataCollection[i].meshType == kindOfMesh)
                {
                    mType = meshTypeDataCollection[i].mesh;
                    return;
                }
            }
            Debug.LogError("NullRef on mesh collection");
            prefabMeshType = 0;
            mType = meshTypeDataCollection[0].mesh;
        }
        protected void EnsureUniqueMeshTypeData()
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
