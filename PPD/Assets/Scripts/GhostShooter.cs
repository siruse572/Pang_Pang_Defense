using UnityEngine;

/// <summary>
/// 'a' 오브젝트에 부착되어, 사거리 내 적을 자동으로 탐지해 Cube 발사체를 자동 발사합니다.
/// 발사된 Cube는 표적(적)을 향해 유도됩니다.
/// </summary>
public class GhostShooter : MonoBehaviour
{
    [Header("Projectile")]
    [Tooltip("발사할 Cube 프리팹")]
    public GameObject cubePrefab;

    [Tooltip("발사 위치 오프셋 (로컬 기준). 비워두면 오브젝트 정면/위쪽에서 발사됩니다.")]
    public Transform muzzlePoint;

    [Header("Targeting")]
    [Tooltip("자동 탐지/발사가 동작하는 최대 사거리 (0 이하이면 무제한)")]
    public float targetingRange = 0f;

    [Header("Fire Rate")]
    [Tooltip("연속 발사 사이의 최소 간격 (초)")]
    public float fireCooldown = 0.25f;

    [Tooltip("회전 속도")]
    public float rotationSpeed = 5f;

    [Tooltip("시각적 요소가 포함된 피벗 (소켓 장착 시 루트 회전이 고정될 수 있으므로 별도 회전용)")]
    public Transform visualsPivot;

    private float lastFireTime = -999f;

    void Update()
    {
        // 사거리 내 가장 가까운 적을 자동으로 탐지합니다.
        Vector3 spawnPos = GetSpawnPos();
        EnemyHealth target = FindNearestEnemy(spawnPos);

        if (target != null)
        {
            RotateTowardsTarget(target.transform.position);

            // 표적이 있고 쿨다운이 지났으면 자동으로 발사합니다.
            if (Time.time - lastFireTime >= fireCooldown)
            {
                Fire(target, spawnPos);
                lastFireTime = Time.time;
            }
        }
    }

    private void RotateTowardsTarget(Vector3 targetPos)
    {
        Transform rotateTransform = visualsPivot != null ? visualsPivot : transform;
        Vector3 direction = (targetPos - rotateTransform.position);
        direction.y = 0; // horizontal rotation only

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rotateTransform.rotation = Quaternion.Slerp(rotateTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private Vector3 GetSpawnPos()
    {
        if (muzzlePoint != null)
            return muzzlePoint.position;

        Transform forwardTransform = visualsPivot != null ? visualsPivot : transform;
        return transform.position + forwardTransform.forward * 0.6f + Vector3.up * 0.5f;
    }

    private void Fire(EnemyHealth target, Vector3 spawnPos)
    {
        if (cubePrefab == null)
        {
            Debug.LogWarning($"{name}: cubePrefab이 설정되지 않았습니다.");
            return;
        }

        // 적의 중심부를 향해 약간 위로 조준합니다.
        Vector3 targetPos = target.transform.position + Vector3.up * 0.5f;
        Vector3 direction = (targetPos - spawnPos).normalized;

        GameObject cube = Instantiate(cubePrefab, spawnPos, Quaternion.LookRotation(direction));

        // 발사체 스크립트가 있으면 발사, 없으면 직접 추가합니다.
        CubeProjectile projectile = cube.GetComponent<CubeProjectile>();
        if (projectile == null)
            projectile = cube.AddComponent<CubeProjectile>();

        projectile.Launch(direction);
        // 발사 후에도 표적을 추적하도록 유도 대상으로 설정합니다.
        projectile.SetTarget(target.transform);
        // 사거리 제한을 설정합니다.
        projectile.SetRange(targetingRange, spawnPos);
    }

    private EnemyHealth FindNearestEnemy(Vector3 fromPos)
    {
        EnemyHealth[] enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        EnemyHealth nearest = null;
        float bestSqr = float.MaxValue;
        float rangeSqr = targetingRange > 0f ? targetingRange * targetingRange : float.MaxValue;

        foreach (var e in enemies)
        {
            float sqr = (e.transform.position - fromPos).sqrMagnitude;
            if (sqr < bestSqr && sqr <= rangeSqr)
            {
                bestSqr = sqr;
                nearest = e;
            }
        }
        return nearest;
    }
}
