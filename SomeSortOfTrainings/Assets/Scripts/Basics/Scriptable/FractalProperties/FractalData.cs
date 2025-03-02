using System.Collections;
using System.Collections.Generic;
using GraphProperties.GraphData;
using UnityEngine;
using Utilities;

namespace GraphProperties.FractalData
{
    [CreateAssetMenu(menuName = "GraphProperties/FractalData")]
    public class FractalData : GraphPropertiesScriptable
    {
        [Space(20)]
        [Header("Fractal Variables")]
        [SerializeField] private Utils.MeshTypes leafMeshType;
        [HideInInspector] public Mesh leafMesh;
        [Range(3, 8)]//min val 3 bc of material col
        public int depth;
        public Material material;
        public Gradient gradientA, gradientB;

        public Color leafColorA, leafColorB;
        [Range(0f, 90f)] public float maxSagAngleA = 15f, maxSagAngleB = 25f;
        [Range(0f, 90f)] public float spinSpeedA = 20f, spinSpeedB = 25f;
        [Range(0f, 1f)] public float reverseSpinChance = 0.25f;
        public override void OnValidate()
        {
            base.OnValidate();
            FindCurrentMeshTypeOnCollection(ref leafMesh, leafMeshType);
        }

    }
}



