using System.Collections;
using System.Collections.Generic;
using PoolSystem.Poolable;
using UnityEngine;
using DG.Tweening;
using System;

namespace CollectionsTraining.DataVisual.BracketVisual
{
    public class VisualBracket : DataVisual
    {
        public override void OnEnqueuePool()
        {
            Vector3 firstScale = transform.localScale;
            transform.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
         {
             gameObject.SetActive(false);
             transform.localScale = firstScale;
             transform.rotation = Quaternion.Euler(0, 0, 0);
         });
        }
        public override void OnDequeuePool()
        {
            gameObject.SetActive(true);
        }
        public override void MoveByTweening(Vector3 targetPos, float duration, Ease easeType)
        {
            transform.DOMove(targetPos, duration).SetEase(easeType);
        }
        public override void MoveByTweening(Vector3 targetPos, float duration, Ease easeType, Action onCompleteMethod)
        {
            transform.DOMove(targetPos, duration).SetEase(easeType).OnComplete(() =>
               {
                   onCompleteMethod();
               });
        }
    }


}
