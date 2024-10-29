using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack-Straight-Single Projectile", menuName = "Enemy Logic/Attack Logic/Straight Single Projectile")]
public class EnemyAttackSingleStraightProjectile : EnemyAttackSOBase
{
    [SerializeField] private float _attackTimer;
    [SerializeField] private float _timeBetweenAttacks = 2f;

    [SerializeField] private float _exitTimer;
    [SerializeField] private float _timeTillExit = 3f;
    [SerializeField] private float _attackRange = 10f;
    [SerializeField] private float _optimalAttackDistance = 4f;

    [SerializeField] private float _bulletSpeed = 10f;

    [SerializeField] private Rigidbody BulletPrefab;

    private bool isAttacking = false;
    private float attackAnimationDuration = 0.5f;
    private float currentAttackAnimTime = 0f;

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        _attackTimer = 0f;
        _exitTimer = 0f;
        enemy.NavMeshAgent.stoppingDistance = _optimalAttackDistance;
        isAttacking = false;
        currentAttackAnimTime = 0f;
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
        enemy.NavMeshAgent.stoppingDistance = 0f;
        isAttacking = false;
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();
        float distanceToPlayer = Vector3.Distance(playerTransform.position, enemy.transform.position);

        // Handle attack animation timing
        if (isAttacking)
        {
            currentAttackAnimTime += Time.deltaTime;
            if (currentAttackAnimTime >= attackAnimationDuration)
            {
                isAttacking = false;
                currentAttackAnimTime = 0f;
            }
        }

        if (distanceToPlayer <= _attackRange)
        {
            enemy.StopEnemy();

            // Rotate to face the player
            Vector3 directionToPlayer = (playerTransform.position - enemy.transform.position).normalized;
            enemy.transform.rotation = Quaternion.LookRotation(directionToPlayer);

            _attackTimer += Time.deltaTime;
            if (_attackTimer >= _timeBetweenAttacks && !isAttacking)
            {
                AttackPlayer();
                _attackTimer = 0f;
            }
            _exitTimer = 0f;
        }
        else
        {
            if (!isAttacking)
            {
                enemy.MoveEnemy(playerTransform.position);
            }

            _exitTimer += Time.deltaTime;
            if (_exitTimer >= _timeTillExit)
            {
                enemy.StateMachine.ChangeState(enemy.ChaseState);
            }
        }
    }

    private void AttackPlayer()
    {
        isAttacking = true;
        currentAttackAnimTime = 0f;
        enemy.StopEnemy();

        enemy.TriggerAttackAnimation();

        Vector3 directionToPlayer = (playerTransform.position - enemy.transform.position).normalized;
        Vector3 shootPosition = enemy.transform.position + Vector3.up;

        Rigidbody bullet = Instantiate(BulletPrefab, shootPosition, Quaternion.LookRotation(directionToPlayer));
        bullet.velocity = directionToPlayer * _bulletSpeed;

        Debug.Log("Enemy attacked player");
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
        isAttacking = false;
        currentAttackAnimTime = 0f;
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }
}