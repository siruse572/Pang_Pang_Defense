using UnityEngine;

/// <summary>
/// 'a' 오브젝트가 발사하는 Cube 발사체입니다.
/// 적(EnemyHealth)에 닿으면 데미지를 입히고 스스로 사라집니다.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CubeProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("발사 속도 (m/s)")]
    public float speed = 15f;

    [Tooltip("적에게 입히는 데미지")]
    public int damage = 1;

    [Tooltip("적을 맞추지 못했을 때 자동으로 사라지는 시간 (초)")]
    public float lifeTime = 5f;

    [Header("Homing (유도)")]
    [Tooltip("표적을 향해 회전하는 속도 (도/초). 값이 클수록 더 급격하게 유도됩니다.")]
    public float turnSpeed = 360f;

    private Rigidbody rb;
    private bool hasHit;
    private Transform target;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // 트리거 방식으로 충돌을 감지합니다.
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// 지정한 방향으로 발사체를 날립니다.
    /// </summary>
    public void Launch(Vector3 direction)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        transform.forward = direction.normalized;
        rb.linearVelocity = direction.normalized * speed;
    }

    /// <summary>
    /// 유도할 표적을 설정합니다.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void FixedUpdate()
    {
        if (hasHit || target == null || rb == null)
            return;

        // 표적의 중심부를 향하도록 진행 방향을 점진적으로 보정합니다.
        Vector3 targetPos = target.position + Vector3.up * 0.5f;
        Vector3 desiredDir = (targetPos - rb.position).normalized;
        if (desiredDir.sqrMagnitude < 0.0001f)
            return;

        Vector3 currentDir = rb.linearVelocity.sqrMagnitude > 0.0001f
            ? rb.linearVelocity.normalized
            : transform.forward;

        // turnSpeed(도/초) 만큼만 방향을 회전시켜 부드럽게 유도합니다.
        Vector3 newDir = Vector3.RotateTowards(
            currentDir,
            desiredDir,
            turnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime,
            0f).normalized;

        rb.linearVelocity = newDir * speed;
        transform.forward = newDir;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit)
            return;

        // 충돌한 콜라이더 또는 그 부모에서 EnemyHealth를 찾습니다.
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy == null)
            return;

        hasHit = true;
        enemy.TakeDamage(damage);
        Destroy(gameObject);
    }
}
