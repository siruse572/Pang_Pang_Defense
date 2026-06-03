using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Lighting")]
    public Light directionalLight;

    [Header("생성할 적 프리팹 4종류")]
    public GameObject[] enemyPrefabs;

    public Transform spawnpoint;
    public float spawnInterval = 2f;
    public int maxEnemies = 20;

    [Header("Skybox")]
    public Material normalSkybox;
    public Material waveSkybox;

    private int currentEnemyCount;
    private int currentWave;
    private bool isWaveRunning;

    public void StartWave()
    {
        if (isWaveRunning)
            return;

        currentWave++;

        if (directionalLight != null)
        {
            directionalLight.intensity = 0.3f;
        }
        // 웨이브 시작 시 Skybox 변경
        RenderSettings.skybox = waveSkybox;

        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        isWaveRunning = true;

        int enemyCount = currentWave * 5;

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

        currentEnemyCount++;
    }

    public void EnemyDestroyed()
    {
        currentEnemyCount--;
        
        // 웨이브 종료
        if (currentEnemyCount <= 0)
        {
            currentEnemyCount = 0;
            isWaveRunning = false;

            // 원래 Skybox로 복구
            RenderSettings.skybox = normalSkybox;

            if (directionalLight != null)
            {
                directionalLight.intensity = 1f;
            }

            Debug.Log($"웨이브 {currentWave} 종료!");
        }
    }
}