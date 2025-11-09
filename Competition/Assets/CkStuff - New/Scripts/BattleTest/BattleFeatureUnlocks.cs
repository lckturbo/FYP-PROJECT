using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleFeatureUnlocks : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Toggle autoToggle;
    [SerializeField] private Toggle speedToggle;

    [Header("Labels")]
    [SerializeField] private Text autoLabel;
    [SerializeField] private Text speedLabel;

    [Header("Unlock Levels")]
    [SerializeField] private int autoUnlockLevel = 1;
    [SerializeField] private int speedUnlockLevel = 1;

    private Color lockedColor = new Color(1f, 1f, 1f, 0.4f);
    private Color unlockedColor = Color.white;

    private void Start()
    {
        UpdateUnlocks();
    }

    public void UpdateUnlocks()
    {
        int currentLevel = 1;

        if (PartyLevelSystem.Instance && PartyLevelSystem.Instance.levelSystem != null)
            currentLevel = PartyLevelSystem.Instance.levelSystem.level;

        if (autoToggle)
        {
            bool unlocked = currentLevel >= autoUnlockLevel;
            autoToggle.interactable = unlocked;

            var cg = autoToggle.GetComponent<CanvasGroup>();
            if (cg) cg.alpha = unlocked ? 1f : 0.4f;

            if (autoLabel)
                autoLabel.text = unlocked
                    ? "AUTO"
                    : $"AUTO (Unlocks at Lv {autoUnlockLevel})";
            if (autoLabel)
                autoLabel.color = unlocked ? unlockedColor : lockedColor;
        }

        if (speedToggle)
        {
            bool unlocked = currentLevel >= speedUnlockLevel;
            speedToggle.interactable = unlocked;

            var cg = speedToggle.GetComponent<CanvasGroup>();
            if (cg) cg.alpha = unlocked ? 1f : 0.4f;

            if (speedLabel)
                speedLabel.text = unlocked
                    ? "SPEED"
                    : $"SPEED (Unlocks at Lv {speedUnlockLevel})";
            if (speedLabel)
                speedLabel.color = unlocked ? unlockedColor : lockedColor;
        }
    }
}
