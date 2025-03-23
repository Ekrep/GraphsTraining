using UnityEngine;
using DG.Tweening;
using System;

namespace ITweenable
{
    public interface ITweenable
    {
        public void MoveByTweening(Vector3 targetPos, float duration, Ease easeType);
        public void MoveByTweening(Vector3 targetPos, float duration, Ease easeType, Action onCompleteMethod);

    }
}
