using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Economy")]
    public int gold = 0;
    public int candy = 10;

    [Header("UI References")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI candyText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateUI();
    }

    public void AddCandy(int amount)
    {
        candy += amount;
        UpdateUI();
    }

    public void ReduceCandy(int amount)
    {
        candy -= amount;
        if (candy <= 0)
        {
            candy = 0;
            UpdateUI();
            // 검은색으로 페이드 아웃 → 초기화 → 페이드 인
            ScreenFader.Instance.FadeOutAndIn(ResetEverything);
            return;
        }
        UpdateUI();

        // candy가 줄어들 때 마다 Candy 오브젝트의 데미지 플래시를 실행합니다.
        GameObject candyObj = GameObject.Find("Candy");
        if (candyObj != null)
        {
            DamageFlash flash = candyObj.GetComponent<DamageFlash>();
            if (flash == null)
            {
                flash = candyObj.AddComponent<DamageFlash>();
            }
            flash.Flash();
        }
    }

    public void ResetEverything()
    {
        // 1. Reset stats
        gold = 2100;
        candy = 100;
        UpdateUI();

        // 2. Reset EnemySpawner
        EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.ResetSpawner();
        }

        // 3. Destroy all enemies in the scene
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude);
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        // 4. Destroy all projectiles
        CubeProjectile[] projectiles = FindObjectsByType<CubeProjectile>(FindObjectsInactive.Exclude);
        foreach (var proj in projectiles)
        {
            if (proj != null)
            {
                Destroy(proj.gameObject);
            }
        }

        // 5. Destroy all purchased towers/allies from shop
        GhostShooter[] ghosts = FindObjectsByType<GhostShooter>(FindObjectsInactive.Exclude);
        foreach (var ghost in ghosts)
        {
            if (ghost != null)
            {
                Destroy(ghost.gameObject);
            }
        }

        XRBookShooter[] books = FindObjectsByType<XRBookShooter>(FindObjectsInactive.Exclude);
        foreach (var book in books)
        {
            if (book != null)
            {
                Destroy(book.gameObject);
            }
        }

        Debug.Log("게임이 리셋되었습니다! 모든 상태와 스포너, 적, 타워가 초기화되었습니다.");
    }

    private void UpdateUI()
    {
        if (goldText != null) goldText.text = "Gold:" + gold.ToString();
        if (candyText != null) candyText.text = "Candy:" + candy.ToString();
    }
}
