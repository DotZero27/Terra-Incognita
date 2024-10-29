using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Chase-Run Away", menuName = "Enemy Logic/Chase Logic/Run Away")]
public class EnemyChaseRunAway : EnemyChaseSOBase
{
    [SerializeField] private float _runAwaySpeed = 4f;
    [SerializeField] private float _runAwayDistance = 10f;
    [SerializeField] private float _minDistanceToPlayer = 5f;

    private Vector3 _runAwayDestination;
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        if (enemy.NavMeshAgent != null)
        {
            enemy.NavMeshAgent.isStopped = false;
        }
    }
    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }
    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();

        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer < _minDistanceToPlayer)
        {
            CalculateRunAwayDestination();
        }
        else if (distanceToPlayer > _runAwayDistance)
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
            return;
        }

        if (enemy.NavMeshAgent != null && enemy.NavMeshAgent.isActiveAndEnabled)
        {
            float speedBefore = enemy.NavMeshAgent.speed;

            enemy.NavMeshAgent.speed = _runAwaySpeed;
            enemy.NavMeshAgent.SetDestination(_runAwayDestination);
            enemy.NavMeshAgent.speed = speedBefore;
        }
    }

    private void CalculateRunAwayDestination()
    {
        if (playerTransform == null) return;

        Vector3 directionAwayFromPlayer = (transform.position - playerTransform.position).normalized;
        Vector3 targetPosition = transform.position + directionAwayFromPlayer * _runAwayDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, _runAwayDistance, NavMesh.AllAreas))
        {
            _runAwayDestination = hit.position;
        }
        else
        {
            // If no valid position found, try to find a random point around the current position
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * _runAwayDistance;
            if (NavMesh.SamplePosition(randomPoint, out hit, _runAwayDistance, NavMesh.AllAreas))
            {
                _runAwayDestination = hit.position;
            }
            else
            {
                Debug.LogWarning("Unable to find a valid NavMesh position to run away to.");
                _runAwayDestination = transform.position; // Stay in place if no valid position found
            }
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
