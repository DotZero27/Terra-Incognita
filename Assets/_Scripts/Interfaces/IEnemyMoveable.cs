using UnityEngine;
using UnityEngine.AI;

public interface IEnemyMoveable
{
    NavMeshAgent NavMeshAgent { get; }
    void MoveEnemy(Vector3 destination);
    void StopEnemy();
}