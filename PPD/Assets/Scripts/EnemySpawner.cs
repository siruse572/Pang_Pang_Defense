using System.Collections;
using UnityEngine;
using UnityEngine.UI; // UI 패키지 추가
using UnityEngine.Rendering;

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
    public Button startButton; // Start 버튼 참조 추가

    private int currentEnemyCount;
    private int currentWave;
    private bool isWaveRunning;

    public void StartWave()
    {
        // 웨이브가 진행 중이면 Start 버튼을 눌러도 아무 동작도 하지 않습니다.
        // SetActive를 끄지 않고 기능만 작동하지 않게 하기 위한 체크입니다.
        if (isWaveRunning)
        {
            Debug.Log("웨이브가 이미 진행 중입니다.");
            return;
        }

        currentWave++;

        // 이번 웨이브의 총 적 수를 웨이브 시작 시 미리 확정합니다 (예: 1웨이브는 5마리).
        // 적이 소환될 때마다 ++ 하는 방식이 아닌, 시작 시점에 총 수를 정해두는 방식입니다.
        int enemyCount = currentWave * 5;
        currentEnemyCount = enemyCount;

        // 다음 StartWave 호출이 즉시 차단되도록 진입 직후에 플래그를 설정합니다.
        isWaveRunning = true;

        // 버튼의 interactable을 꺼서 기능이 작동하지 않게 합니다. (SetActive는 유지)
        if (startButton != null)
        {
            startButton.interactable = false;
        }

        
        RenderSettings.ambientLight = Color.gray;
        
        // 웨이브 시작 시 Skybox 변경
        RenderSettings.skybox = waveSkybox;

        StartCoroutine(SpawnWave(enemyCount));
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