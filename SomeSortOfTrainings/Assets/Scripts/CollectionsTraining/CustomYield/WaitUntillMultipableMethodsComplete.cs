using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CollectionsTraining.CollectionUtils;

public class WaitUntillMultipableMethodsComplete : CustomYieldInstruction
{

    private BoolWrapper wait;
    public WaitUntillMultipableMethodsComplete(BoolWrapper boolWrapper)
    {
        wait = boolWrapper;
    }
    public override bool keepWaiting => wait.value;
}
