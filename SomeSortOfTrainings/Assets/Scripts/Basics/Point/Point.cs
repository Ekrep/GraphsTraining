using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class Point : MonoBehaviour
{
    public MeshRenderer pointRenderer;
    public MeshFilter pointMeshFilter;
   
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }
    public void SetLocalPosition(Vector3 position)
    {
        transform.localPosition = position;
    }
    public Vector3 GetLocalPosition()
    {
        return transform.localPosition;
    }
    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;

    }
    public Vector3 GetLocalScale()
    {
        return transform.localScale;
    }
    public void SetParent(Transform parentObj)
    {
        transform.SetParent(parentObj, false);
    }
    public void SetParent(Transform parentObj, bool worldPosStays)
    {
        transform.SetParent(parentObj, worldPosStays);
    }
    public Transform GetParent()
    {
        return transform.parent;
    }

}
