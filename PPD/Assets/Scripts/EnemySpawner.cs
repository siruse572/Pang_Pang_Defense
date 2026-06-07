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
    private int currentWave;
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
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0)
            return;

        int randomIndex = Random.Range(0, enemyPrefabs.Length);

        Instantiate(
            enemyPrefabs[randomIndex],
            spawnpoint.position,
            Quaternion.identity
        );
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
            

            Debug.Log($"웨이브 {currentWave} 종료!");
        }
    }
}