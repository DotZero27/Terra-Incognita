using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Idle-Stand Still", menuName = "Enemy Logic/Idle Logic/Stand")]
public class EnemyIdleStandStill : EnemyIdleSOBase
{
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
    }
    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }
    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();
    }
    public override void DoPhysicsLogic()
    {
        base.DoPhysicsLogic();
    }
    public override void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }
    public override void ResetValues()
    {
        base.ResetValues();
    }
    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

}
