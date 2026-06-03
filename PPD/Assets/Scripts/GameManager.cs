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

    public void ReduceCandy(int amount)
    {
        candy -= amount;
        if (candy < 0) candy = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (goldText != null) goldText.text = "Gold:" + gold.ToString();
        if (candyText != null) candyText.text = "Candy:" + candy.ToString();
    }
}
