using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandBurst : BaseMinigame
{
    [Header("UI References")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Slider energyBar;
    [SerializeField] private TMP_Text timerText;

    [Header("Settings")]
    [SerializeField] private float timePerPrompt = 1.5f;
    [SerializeField] private int totalPrompts = 5;

    private string[] commands = { "ATTACK", "BUFF", "SHIELD", "HEAL", "FOCUS" };
    private int successCount = 0;
    private float timer;

    private void Start()
    {
        StartCoroutine(PlaySequence());
    }
    public override IEnumerator Run()
    {
        instructionPanel.SetActive(true);
        minigamePanel.SetActive(false);

        float t = instructionTime;
        while (t > 0f)
        {
            instructionTimerText.text = Mathf.CeilToInt(t).ToString();
            t -= Time.deltaTime;
            yield return null;
        }

        instructionPanel.SetActive(false);
        minigamePanel.SetActive(true);

        yield return StartCoroutine(PlaySequence());

        if (successCount == totalPrompts)
            Result = MinigameManager.ResultType.Perfect;
        else if (successCount >= totalPrompts * 0.6f)
            Result = MinigameManager.ResultType.Success;
        else
            Result = MinigameManager.ResultType.Fail;
    }
    private IEnumerator PlaySequence()
    {
        successCount = 0;
        energyBar.value = 0f;

        for (int i = 0; i < totalPrompts; i++)
        {
            string cmd = commands[Random.Range(0, commands.Length)];
            promptText.text = cmd;

            float timer = timePerPrompt;
            bool pressed = false;

            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                timerText.text = timer.ToString("F1");

                if (AnyKeyPressed())
                {
                    if (CheckKey(cmd))
                    {
                        successCount++;
                        energyBar.value = (float)successCount / totalPrompts;
                        pressed = true;
                        break;
                    }
                    else
                    {
                        pressed = true;
                        break;
                    }
                }

                yield return null;
            }

            yield return new WaitForSeconds(0.2f);
        }

        promptText.text = "FINISH!";
    }
    private bool CheckKey(string cmd)
    {
        switch (cmd)
        {
            case "ATTACK":
                return Input.GetKeyDown(KeyCode.Z);
            case "BUFF":
                return Input.GetKeyDown(KeyCode.X);
            case "SHIELD":
                return Input.GetKeyDown(KeyCode.C);
            case "HEAL":
                return Input.GetKeyDown(KeyCode.V);
            case "FOCUS":
                return Input.GetKeyDown(KeyCode.B);
        }
        return false;
    }
    private bool AnyKeyPressed()
    {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                if (key == KeyCode.Mouse0 || key == KeyCode.Mouse1 || key == KeyCode.Mouse2)
                    continue;
                return true;
            }
        }
        return false;

    }
}