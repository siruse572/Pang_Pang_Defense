using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// XR 플레이어가 잡은 책(Fire book)에서 트리거(Activate)를 누르면 발사체를 발사합니다.
/// 발사체는 GhostShooter처럼 정면으로 올곧게 발사된 뒤 가장 가까운 적을 향해 유도됩니다.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class XRBookShooter : MonoBehaviour
{
    [Header("Projectile")]
    [Tooltip("발사할 발사체 프리팹 (Magic fire pro red)")]
    public GameObject projectilePrefab;

    [Tooltip("발사 시작 위치/방향. 비워두면 이 오브젝트의 위치/정면(+Z)을 사용합니다.")]
    public Transform muzzlePoint;

    [Header("Fire Rate")]
    [Tooltip("연속 발사 사이의 최소 간격 (초)")]
    public float fireCooldown = 0.25f;

    [Header("Targeting (유도)")]
    [Tooltip("유도 대상(적)을 탐지하는 최대 사거리 (0 이하이면 무제한)")]
    public float targetingRange = 0f;

    [Tooltip("발사 정면 기준 이 각도(도) 이내의 적만 유도 대상으로 삼습니다. 0 이하이면 모든 방향을 허용합니다.")]
    public float homingConeAngle = 90f;

    [Header("Grab Pose (잡기 자세)")]
    [Tooltip("책을 잡았을 때 X축으로 회전시킬 각도(도). 손 기준으로 이 각도만큼 기울여서 잡습니다.")]
    public float grabRotationX = 90f;

    [Tooltip("책을 잡았을 때 Y축으로 회전시킬 각도(도). 손 기준으로 이 각도만큼 돌려서 잡습니다.")]
    public float grabRotationY = 0f;
    [Tooltip("책을 잡았을 때 Z축으로 회전시킬 각도(도). 손 기준으로 이 각도만큼 돌려서 잡습니다.")]
    public float grabRotationZ = 180f;

    private XRGrabInteractable grabInteractable;
    private Transform grabAttach;
    private float lastFireTime = -999f;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        SetupGrabAttach();
    }

    /// <summary>
    /// 잡기용 Attach Transform을 구성합니다.
    /// 이 Transform을 X축으로 회전시켜 두면, 잡았을 때 책이 그만큼 기울어진 자세로 잡힙니다.
    /// 잡지 않은 상태의 책 본체 회전에는 영향을 주지 않습니다.
    /// </summary>
    private void SetupGrabAttach()
    {
        if (grabInteractable == null)
            return;

        grabAttach = grabInteractable.attachTransform;
        if (grabAttach == null)
        {
            var go = new GameObject("GrabAttach");
            go.transform.SetParent(transform, false);
            grabAttach = go.transform;
            grabInteractable.attachTransform = grabAttach;
        }

        grabAttach.localPosition = Vector3.zero;
        grabAttach.localRotation = Quaternion.Euler(grabRotationX, grabRotationY, grabRotationZ);
    }

    void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.AddListener(OnActivated);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(OnActivated);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    private void OnActivated(ActivateEventArgs args)
    {
        Fire();
    }

    /// <summary>
    /// 책을 놓으면 본체 회전을 0으로 되돌립니다.
    /// </summary>
    private void OnReleased(SelectExitEventArgs args)
    {
        transform.rotation = Quaternion.identity;

        // 물리로 인해 회전이 다시 흐트러지지 않도록 각속도를 제거합니다.
        if (TryGetComponent<Rigidbody>(out var rb))
            rb.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// 발사체를 발사합니다. (UnityEvent 등에서 직접 호출 가능)
    /// </summary>
    public void Fire()
    {
        if (Time.time - lastFireTime < fireCooldown)
            return;

        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{name}: projectilePrefab이 설정되지 않았습니다.");
            return;
        }

        Transform origin = muzzlePoint != null ? muzzlePoint : transform;
        Vector3 spawnPos = origin.position;
        Vector3 direction = origin.forward;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(direction));

        CubeProjectile cp = proj.GetComponent<CubeProjectile>();
        if (cp == null)
            cp = proj.AddComponent<CubeProjectile>();

        // 우선 정면으로 올곧게 발사합니다.
        cp.Launch(direction);

        // 가장 가까운 적을 유도 대상으로 설정합니다.
        EnemyHealth target = FindNearestEnemy(spawnPos, direction);
        if (target != null)
            cp.SetTarget(target.transform);

        // 사거리 제한(0 이하이면 무제한).
        cp.SetRange(targetingRange, spawnPos);

        lastFireTime = Time.time;
    }

    private EnemyHealth FindNearestEnemy(Vector3 fromPos, Vector3 forward)
    {
        EnemyHealth[] enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        EnemyHealth nearest = null;
        float bestSqr = float.MaxValue;
        float rangeSqr = targetingRange > 0f ? targetingRange * targetingRange : float.MaxValue;
        float cosThreshold = homingConeAngle > 0f ? Mathf.Cos(homingConeAngle * Mathf.Deg2Rad) : -1f;

        foreach (var e in enemies)
        {
            Vector3 toEnemy = e.transform.position - fromPos;
            float sqr = toEnemy.sqrMagnitude;
            if (sqr > rangeSqr)
                continue;

            // 발사 정면 기준 각도(콘) 안에 있는 적만 대상으로 삼습니다.
            if (cosThreshold > -1f && sqr > 0.0001f)
            {
                float dot = Vector3.Dot(forward.normalized, toEnemy.normalized);
                if (dot < cosThreshold)
                    continue;
            }

            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                nearest = e;
            }
        }
        return nearest;
    }
}
