using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public string name;
        public GameObject prefab;
        public int price;
        public Sprite icon; // Added icon field
    }

    [Header("Items")]
    public List<ShopItem> ghostItems;
    public List<ShopItem> bookItems;

    [Header("UI References")]
    public TextMeshProUGUI categoryTitle;
    public TextMeshProUGUI[] itemNameTexts;
    public UnityEngine.UI.Image[] itemIcons; // Added icons array (for the red areas)
    public Button[] purchaseButtons;
    public TextMeshProUGUI[] priceTexts;
    public Button leftArrow;
    public Button rightArrow;

    private bool showingGhosts = true;
    private Transform playerTransform;

    void Start()
    {
        FindPlayer();

        if (leftArrow != null) leftArrow.onClick.AddListener(ToggleCategory);
        if (rightArrow != null) rightArrow.onClick.AddListener(ToggleCategory);

        for (int i = 0; i < purchaseButtons.Length; i++)
        {
            int index = i;
            if (purchaseButtons[i] != null)
                purchaseButtons[i].onClick.AddListener(() => PurchaseItem(index));
        }

        UpdateShopUI();
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("XR Origin (XR Rig)");
        if (player == null) player = GameObject.Find("XR Origin");
        if (player == null) player = GameObject.Find("Player");
        if (player == null && Camera.main != null) player = Camera.main.gameObject;
        
        if (player != null) playerTransform = player.transform;
    }

    void ToggleCategory()
    {
        showingGhosts = !showingGhosts;
        UpdateShopUI();
    }

    void UpdateShopUI()
    {
        List<ShopItem> currentItems = showingGhosts ? ghostItems : bookItems;
        if (categoryTitle != null) categoryTitle.text = showingGhosts ? "Ghost Shop" : "Magic Book Shop";

        for (int i = 0; i < itemNameTexts.Length; i++)
        {
            if (i < currentItems.Count)
            {
                itemNameTexts[i].text = currentItems[i].name;
                priceTexts[i].text = currentItems[i].price + "Gold";
                
                // Set Icon
                if (itemIcons[i] != null)
                {
                    itemIcons[i].sprite = currentItems[i].icon;
                    // Remove the red color if icon is set
                    itemIcons[i].color = currentItems[i].icon != null ? Color.white : new Color(0.8f, 0.2f, 0.2f, 1f);
                }

                itemNameTexts[i].gameObject.SetActive(true);
                purchaseButtons[i].gameObject.SetActive(true);
            }
            else
            {
                itemNameTexts[i].gameObject.SetActive(false);
                if (itemIcons[i] != null) itemIcons[i].gameObject.SetActive(false);
                purchaseButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void PurchaseItem(int index)
    {
        List<ShopItem> currentItems = showingGhosts ? ghostItems : bookItems;
        if (index >= currentItems.Count) return;

        ShopItem item = currentItems[index];

        if (GameManager.Instance != null && GameManager.Instance.gold >= item.price)
        {
            GameManager.Instance.gold -= item.price;
            GameManager.Instance.AddGold(0);
            
            // Use Camera.main as the primary reference for "Player's Front"
            Transform refTransform = Camera.main != null ? Camera.main.transform : (playerTransform != null ? playerTransform : transform);
            
            if (item.prefab == null)
            {
#if UNITY_EDITOR
                AutoAssignMissingPrefabs(currentItems);
#endif
            }

            if (item.prefab != null)
            {
                // Calculate direction (flattened to horizontal plane)
                Vector3 forward = refTransform.forward;
                forward.y = 0;
                if (forward.sqrMagnitude < 0.1f) forward = refTransform.up; // Fallback if looking straight down/up
                forward.Normalize();

                // Position 2 meters in front of the camera's horizontal position
                Vector3 spawnPos = refTransform.position + forward * 2f;
                
                // Set a consistent height relative to the ground/player
                spawnPos.y = 0.5f; 
                
                // Rotation matches the flattened forward direction
                Quaternion spawnRotation = Quaternion.LookRotation(forward);
                
                GameObject spawned = Instantiate(item.prefab, spawnPos, spawnRotation);
                Debug.Log("Purchased and Summoned: " + item.name + " at " + spawnPos + " (Relative to Camera)");
            }
            else
            {
                Debug.LogError("[ShopManager] Cannot purchase " + item.name + ": Prefab is missing and auto-assignment failed! Please check your project for a prefab named '" + item.name + "'.");
            }
        }
        else
        {
            Debug.Log("Not enough gold! Need " + item.price + " Gold.");
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        AutoAssignMissingPrefabs(ghostItems);
        AutoAssignMissingPrefabs(bookItems);
    }

    private void AutoAssignMissingPrefabs(List<ShopItem> items)
    {
        if (items == null) return;
        foreach (var item in items)
        {
            if (item.prefab == null && !string.IsNullOrEmpty(item.name))
            {
                string searchName = item.name;
                string[] guids = UnityEditor.AssetDatabase.FindAssets(searchName + " t:Prefab");
                if (guids.Length == 0)
                {
                    searchName = item.name.Replace(" ", "");
                    guids = UnityEditor.AssetDatabase.FindAssets(searchName + " t:Prefab");
                }

                if (guids.Length > 0)
                {
                    string bestPath = null;
                    foreach (var guid in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                        if (fileName.Equals(item.name, System.StringComparison.OrdinalIgnoreCase) ||
                            fileName.Equals(item.name.Replace(" ", ""), System.StringComparison.OrdinalIgnoreCase))
                        {
                            bestPath = path;
                            break;
                        }
                    }

                    if (bestPath == null)
                    {
                        bestPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    }

                    item.prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(bestPath);
                    if (item.prefab != null)
                    {
                        Debug.Log("[ShopManager] Automatically assigned missing prefab '" + item.prefab.name + "' for item '" + item.name + "' to prevent bugs.");
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                }
            }
        }
    }
#endif
}
