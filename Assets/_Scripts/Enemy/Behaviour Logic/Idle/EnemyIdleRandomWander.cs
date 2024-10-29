using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Idle-Random Wander", menuName = "Enemy Logic/Idle Logic/Random Wander")]
public class EnemyIdleRandomWander : EnemyIdleSOBase
{
    [SerializeField] private float randomMovementRange = 5f;
    [SerializeField] private float minIdleTime = 2f;
    [SerializeField] private float maxIdleTime = 5f;
    [SerializeField] private float minWanderTime = 3f;
    [SerializeField] private float maxWanderTime = 7f;

    private Vector3 randomDestination;
    private float wanderTimer;
    private float currentWanderTime;
    private float idleTimer;
    private float currentIdleTime;
    private bool isIdling = true;

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        ResetTimers();
        isIdling = true;
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
        enemy.StopEnemy();
    }

    public override void DoFrameUpdateLogic()
    {
        if (enemy.IsAggroed)
        {
            enemy.StateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        if (isIdling)
        {
            HandleIdleState();
        }
        else
        {
            HandleWanderState();
        }
    }

    private void HandleIdleState()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= currentIdleTime)
        {
            isIdling = false;
            SetNewRandomDestination();
        }
    }

    private void HandleWanderState()
    {
        wanderTimer += Time.deltaTime;
        
        if (wanderTimer >= currentWanderTime || 
            (enemy.NavMeshAgent.remainingDistance < 0.1f && !enemy.NavMeshAgent.pathPending))
        {
            isIdling = true;
            enemy.StopEnemy();
            ResetTimers();
        }
    }

    private void SetNewRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * randomMovementRange;
        randomDirection += enemy.transform.position;
        
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, randomMovementRange, 1))
        {
            randomDestination = hit.position;
            enemy.MoveEnemy(randomDestination);
        }
        else
        {
            isIdling = true;
            ResetTimers();
        }
    }

    private void ResetTimers()
    {
        wanderTimer = 0f;
        currentWanderTime = Random.Range(minWanderTime, maxWanderTime);
        idleTimer = 0f;
        currentIdleTime = Random.Range(minIdleTime, maxIdleTime);
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void ResetValues()
    {
        base.ResetValues();
        wanderTimer = 0f;
        idleTimer = 0f;
        isIdling = true;
    }
}