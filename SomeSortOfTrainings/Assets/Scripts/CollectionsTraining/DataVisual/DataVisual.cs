using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PoolSystem.Poolable;
using UnityEngine;

namespace CollectionsTraining.DataVisual
{
    public class DataVisual : MonoBehaviour, ITweenable.ITweenable, IPoolable
    {
        public MeshRenderer visualRenderer;
        public void SetScale(Vector3 scaleVec)
        {
            transform.localScale = scaleVec;
        }
        public Vector3 GetScale()
        {
            return transform.localScale;
        }
        public void SetWorldPosition(Vector3 positionVec)
        {
            transform.position = positionVec;
        }
        public void SetLocalPosition(Vector3 positionVec)
        {
            transform.localPosition = positionVec;
        }
        public virtual void MoveByTweening(Vector3 targetPos, float duration, Ease easeType)
        {
            transform.DOMove(targetPos, duration).SetEase(easeType);//addcomplex
            CommandConsole.Instance.IncreaseComplexityCount();
        }

        public virtual void MoveByTweening(Vector3 targetPos, float duration, Ease easeType, Action onCompleteMethod)
        {
            transform.DOMove(targetPos, duration).SetEase(easeType).OnComplete(() =>
               {
                   onCompleteMethod();
               });
            CommandConsole.Instance.IncreaseComplexityCount();
        }

        public virtual void OnAssignPool()
        {

        }

        public virtual void OnCreatedForPool()
        {
            gameObject.SetActive(false);
        }

        public virtual void OnDeletePool()
        {

        }

        public virtual void OnEnqueuePool()
        {
            transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InOutBounce).OnComplete(() =>
           {
               gameObject.SetActive(false);
           });
            CommandConsole.Instance.IncreaseComplexityCount();
        }

        public virtual void OnDequeuePool()
        {
            gameObject.SetActive(true);
            CommandConsole.Instance.IncreaseComplexityCount();
        }
    }


}
