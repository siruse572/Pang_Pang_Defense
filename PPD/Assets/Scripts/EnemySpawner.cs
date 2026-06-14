using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;

public class EnemySpawner : MonoBehaviour
{

    [Header("생성할 적 프리팹 4종류")]
    public GameObject[] enemyPrefabs;

    public Transform spawnpoint;
    public float spawnInterval = 2f;
    public int maxEnemies = 20;

    [Header("Skybox")]
    public Material normalSkybox;
    public Material waveSkybox;

    [Header("UI")]
    public Button startButton;
    public TextMeshProUGUI waveText; // 웨이브 텍스트 참조 추가

    private int currentEnemyCount;
    public int currentWave;
    private bool isWaveRunning;

    private void Start()
    {
        UpdateWaveUI();
    }

    public void StartWave()
    {
        if (isWaveRunning)
        {
            Debug.Log("웨이브가 이미 진행 중입니다.");
            return;
        }

        currentWave++;
        UpdateWaveUI();

        int enemyCount = currentWave * 5;
        currentEnemyCount = enemyCount;
        isWaveRunning = true;

        if (startButton != null)
        {
            startButton.interactable = false;
        }
        
        RenderSettings.ambientLight = Color.gray;
        RenderSettings.skybox = waveSkybox;

        StartCoroutine(SpawnWave(enemyCount));
    }

    private void UpdateWaveUI()
    {
        if (waveText != null)
        {
            waveText.text = "Wave: " + currentWave;
        }
    }

    private IEnumerator SpawnWave(int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            bool isBoss = (currentWave % 10 == 0 && i == enemyCount - 1);
            SpawnEnemy(isBoss);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy(bool isBoss = false)
    {
        if (enemyPrefabs.Length == 0)
            return;

        int randomIndex = Random.Range(0, enemyPrefabs.Length);

        GameObject enemy = Instantiate(
            enemyPrefabs[randomIndex],
            spawnpoint.position,
            Quaternion.identity
        );

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();

        if (isBoss)
        {
            enemy.name = "Boss_" + enemy.name;

            // 스케일 2배
            enemy.transform.localScale *= 2f;

            // 체력 설정: 150 * (wave / 10)
            int bossHealth = 150 * (currentWave / 10);
            if (health != null)
            {
                health.SetMaxHealth(bossHealth);
            }

            // 이동 속도 0.3배로 설정
            UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.speed *= 0.3f;
            }

            Debug.Log($"[Spawner] Wave {currentWave} Boss Spawned! Name: {enemy.name}, Scale: {enemy.transform.localScale}, MaxHealth: {bossHealth}, Speed: {(agent != null ? agent.speed : 0f)}");
        }
        else
        {
            // 일반 에너미: 원래 체력(기본값 3)에 (웨이브 * 2) 만큼 더한 체력으로 설정
            if (health != null)
            {
                int baseHealth = health.maxHealth;
                int scaledHealth = baseHealth + (currentWave * 2);
                health.SetMaxHealth(scaledHealth);
                Debug.Log($"[Spawner] Spawned {enemy.name} (Wave {currentWave}) - Base Health: {baseHealth}, Scaled Health: {scaledHealth}");
            }
        }
    }

    public void EnemyDestroyed()
    {
        currentEnemyCount--;
        
        // 모든 적이 처치되어 웨이브가 종료되었는지 확인
        if (currentEnemyCount <= 0)
        {
            currentEnemyCount = 0;
            isWaveRunning = false;

            // 웨이브 종료 시 버튼 기능을 다시 활성화합니다.
            if (startButton != null)
            {
                startButton.interactable = true;
            }

            // 원래 Skybox로 복구
            RenderSettings.skybox = normalSkybox;

           
            RenderSettings.ambientLight = Color.white;
            
            // 웨이브가 끝나면 소지금의 10%만큼 이자를 추가로 더해줍니다.
            if (GameManager.Instance != null)
            {
                int interest = Mathf.RoundToInt(GameManager.Instance.gold * 0.1f);
                GameManager.Instance.AddGold(interest);
                Debug.Log($"[Spawner] 웨이브 {currentWave} 종료! 이자 지급(10%): {interest} Gold, 현재 소지금: {GameManager.Instance.gold} Gold");

                // wave가 끝나면 candy변수를 +3 해줍니다.
                GameManager.Instance.AddCandy(3);
            }

            Debug.Log($"웨이브 {currentWave} 종료!");
        }
    }

    /// <summary>
    /// 스포너의 상태를 초기 게임 상태로 리셋합니다.
    /// </summary>
    public void ResetSpawner()
    {
        StopAllCoroutines();
        isWaveRunning = false;
        currentWave = 0;
        currentEnemyCount = 0;

        if (startButton != null)
        {
            startButton.interactable = true;
        }

        RenderSettings.skybox = normalSkybox;
        RenderSettings.ambientLight = Color.white;
        UpdateWaveUI();
    }
}