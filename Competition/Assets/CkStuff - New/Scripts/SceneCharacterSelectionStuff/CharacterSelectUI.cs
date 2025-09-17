using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private NewCharacterDefinition[] roster;
    [SerializeField] private SelectedCharacter selectedStore;

    [Header("UI")]
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Image previewImage;
    [SerializeField] private TMP_Text previewName;
    [SerializeField] private TMP_Text previewStats;
    [SerializeField] private Button confirmButton;

    private int currentIndex = -1;

    void Start()
    {
        // Build buttons
        for (int i = 0; i < roster.Length; i++)
        {
            var def = roster[i];
            var btn = Instantiate(buttonPrefab, buttonContainer);

            int idx = i; // capture
            btn.onClick.AddListener(() => Preview(idx));
        }

        confirmButton.onClick.AddListener(ConfirmSelection);
        if (roster.Length > 0) Preview(0);
    }

    private void Preview(int idx)
    {
        currentIndex = idx;
        var def = roster[idx];

        if (previewStats && def.stats != null)
        {
            var s = def.stats;
            previewStats.text =
                $"HP {s.maxHealth}\n" +
                $"ATK {s.atkDmg}\n" +
                $"Speed {s.Speed}\n" +
                $"Crit {Mathf.RoundToInt(s.critRate * 100f)}% x{s.critDamage}\n" +
                $"Elem {s.attackElement}";
        }
    }

    private void ConfirmSelection()
    {
        if (currentIndex < 0 || currentIndex >= roster.Length) return;
        var def = roster[currentIndex];
        selectedStore.Set(def, currentIndex);

        // Load your gameplay scene
        //SceneManager.LoadScene("SceneCk");
        GameManager.instance.ChangeScene("SceneCk");
    }
}
