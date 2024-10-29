using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Chase-Direct Chase", menuName = "Enemy Logic/Chase Logic/Direct Chase")]
public class EnemyChaseDirectToPlayer : EnemyChaseSOBase
{
    [SerializeField] private float _chaseSpeed = 3.5f;

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        enemy.NavMeshAgent.speed = _chaseSpeed;
    }
    public override void DoExitLogic()
    {
        base.DoExitLogic();
        enemy.StopEnemy();
    }
    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();

        enemy.MoveEnemy(playerTransform.position);

        if (enemy.IsWithinStrikingDistance)
        {
            enemy.StateMachine.ChangeState(enemy.AttackState);
        }
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
