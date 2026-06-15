using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적의 이동을 제어하는 스크립트입니다.
/// NavMeshAgent를 사용하여 목표(Candy)를 향해 이동하며, 일정 거리 이내에서 공격을 수행합니다.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Navigation Settings")]
    [Tooltip("적이 추적할 목표 오브젝트의 Transform입니다.")]
    public Transform candyTarget;

    [Tooltip("공격을 시작할 거리 (미터)")]
    public float attackRange = 1.0f;

    [Tooltip("NavMesh 스냅을 시도할 최대 반경")]
    public float navMeshSampleRadius = 2.0f;

    private NavMeshAgent agent;

    public EnemySpawner spawner;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (agent == null)
        {
            Debug.LogError($"{gameObject.name}: NavMeshAgent 컴포넌트가 필요합니다.");
            enabled = false;
        }
    }

    void Start()
    {
        if (spawner == null)
        {
            spawner = Object.FindAnyObjectByType<EnemySpawner>();
        }

        if (candyTarget == null)
        {
            GameObject candyObj = GameObject.Find("Candy");
            if (candyObj != null)
            {
                candyTarget = candyObj.transform;
                Debug.Log($"{gameObject.name}: candyTarget이 비어있어 자동으로 'Candy' 오브젝트를 할당했습니다.");
            }
            else
            {
                Debug.LogError($"{gameObject.name}: candyTarget이 비어있고 씬에서 'Candy' 오브젝트를 찾을 수 없습니다!");
            }
        }

        // 4. NavMesh.SamplePosition을 사용하여 가장 가까운 NavMesh 위로 위치를 보정(Warp)
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            // agent.Warp는 transform.position 설정보다 안전하며 즉시 NavMesh에 등록시킵니다.
            agent.Warp(hit.position);
            
            // 1. 적이 NavMesh 위에 있는지 확인하는 디버그 로그
            Debug.Log($"{gameObject.name}: NavMesh 위로 성공적으로 배치되었습니다. (좌표: {hit.position})");
        }
        else
        {
            // 1. NavMesh를 찾지 못했을 때의 디버그 로그
            Debug.LogWarning($"{gameObject.name}: {navMeshSampleRadius}m 이내에서 NavMesh를 찾을 수 없습니다. " +
                           "스폰 위치가 NavMesh 영역 안에 있는지 확인하세요.");
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (candyTarget == null)
        {
            GameObject candyObj = GameObject.Find("Candy");
            if (candyObj != null)
            {
                candyTarget = candyObj.transform;
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        if (spawner == null)
        {
            spawner = Object.FindAnyObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
    }
#endif

    void Update()
    {
        // 3. agent.isOnNavMesh 체크를 추가하여 "IsStopped" 에러 방지
        if (agent == null || !agent.isOnNavMesh)
        {
            return;
        }

        if (candyTarget == null)
        {
            return;
        }

        MoveToTarget();
        CheckAttackDistance();
    }

    private void MoveToTarget()
    {
        // 이미 위에서 isOnNavMesh 체크를 수행했으므로 안전합니다.
        agent.SetDestination(candyTarget.position);
    }

    private void CheckAttackDistance()
    {
        float distance = Vector3.Distance(transform.position, candyTarget.position);

        if (distance <= attackRange)
        {
            // 3. 에러 방지를 위해 isStopped 설정 전 상태를 다시 한번 확인
            if (agent.isOnNavMesh && !agent.isStopped)
            {
                agent.isStopped = true;
            }
            
            AttackTarget();
            
            // 보스 에너미는 Candy를 공격할 때 10의 데미지를, 일반 에너미는 1의 데미지를 줍니다.
            int damageAmount = gameObject.name.StartsWith("Boss_") ? 10 : 1;
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReduceCandy(damageAmount);
            }

            if (spawner != null)
            {
                spawner.EnemyDestroyed();
            }
            
            Destroy(gameObject);
        }
        else
        {
            if (agent.isOnNavMesh && agent.isStopped)
            {
                agent.isStopped = false;
            }
        }
    }

    /// <summary>
    /// 적이 죽었을 때 호출되는 메서드입니다. (골드 획득)
    /// </summary>
    public void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(10);
        }

        if (spawner != null)
        {
            spawner.EnemyDestroyed();
        }

        Destroy(gameObject);
    }

    private void AttackTarget()
    {
        // 실제 공격 로직 (예: 데미지 처리)
        Debug.Log($"{gameObject.name}이(가) {candyTarget.name}을(를) 공격합니다!");
    }
}