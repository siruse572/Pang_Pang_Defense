using UnityEngine;

/// <summary>
/// 적의 체력을 관리하는 스크립트입니다.
/// 최대 체력은 3이며, 체력이 0이 되면 EnemyMovement.Die()를 호출합니다.
/// </summary>
[RequireComponent(typeof(EnemyMovement))]
public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("적의 최대 체력입니다.")]
    public int maxHealth = 3;

    [SerializeField, Tooltip("현재 체력 (런타임 표시용)")]
    private int currentHealth;

    private EnemyMovement enemyMovement;
    private bool isDead;

    public int CurrentHealth => currentHealth;

    void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        currentHealth = maxHealth;
    }

    /// <summary>
    /// 최대 체력과 현재 체력을 재설정합니다 (예: 보스 몬스터 설정 시 사용).
    /// </summary>
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = newMaxHealth;
    }

    /// <summary>
    /// 적에게 데미지를 입힙니다.
    /// </summary>
    /// <param name="amount">감소시킬 체력 양</param>
    public void TakeDamage(int amount)
    {
        if (isDead)
            return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} 체력: {currentHealth}/{maxHealth}");

        // 데미지 플래시 연출 실행
        DamageFlash flash = GetComponent<DamageFlash>();
        if (flash == null)
        {
            flash = gameObject.AddComponent<DamageFlash>();
        }
        flash.Flash();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        if (enemyMovement != null)
        {
            // 기존 사망 처리(골드 획득, 스포너 알림, 오브젝트 파괴)를 재사용합니다.
            enemyMovement.Die();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
