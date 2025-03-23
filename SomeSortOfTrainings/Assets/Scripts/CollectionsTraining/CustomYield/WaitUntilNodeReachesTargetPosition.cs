using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitUntilNodeReachesTargetPosition : CustomYieldInstruction
{
    private Transform _firstPos;
    private Vector3 _secondPos;

    /// <summary>
    /// Make sure that the positions can actually become equal when using this
    /// </summary>
    public WaitUntilNodeReachesTargetPosition(Transform firstPos, Vector3 secondPos)
    {
        _firstPos = firstPos;
        _secondPos = secondPos;
    }
    public override bool keepWaiting => _firstPos.position != _secondPos;
}
