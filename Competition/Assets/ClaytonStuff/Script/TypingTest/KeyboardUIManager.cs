using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyboardUI : MonoBehaviour
{
    [Header("Keyboard Settings")]
    public GameObject keyPrefab; // Prefab with an Image + TMP_Text
    public RectTransform keyboardParent; // Parent layout container
    public Color normalColor = Color.white;
    public Color pressedColor = Color.cyan;

    [Header("Layout Settings")]
    public float keySpacing = 5f;     // Space between keys
    public float rowSpacing = 10f;    // Space between rows
    public Vector2 keySize = new Vector2(60f, 60f); // Default key size

    private Dictionary<KeyCode, Image> keyImages = new Dictionary<KeyCode, Image>();

    // QWERTY layout rows
    private string[] rows = new string[]
    {
        "Q W E R T Y U I O P",
        "A S D F G H J K L",
        "Z X C V B N M"
    };

    void Start()
    {
        GenerateKeyboard();
    }

    void Update()
    {
        foreach (var pair in keyImages)
        {
            if (Input.GetKeyDown(pair.Key))
                pair.Value.color = pressedColor;

            if (Input.GetKeyUp(pair.Key))
                pair.Value.color = normalColor;
        }
    }

    private void GenerateKeyboard()
    {
        //float startY = 0f;
        float totalHeight = (rows.Length * keySize.y) + ((rows.Length - 1) * rowSpacing);
        float topOffset = totalHeight / 2f - keySize.y / 2f;

        for (int i = 0; i < rows.Length; i++)
        {
            string[] keys = rows[i].Split(' ');
            float totalRowWidth = (keys.Length * keySize.x) + ((keys.Length - 1) * keySpacing);
            float startX = -totalRowWidth / 2f + keySize.x / 2f;

            for (int j = 0; j < keys.Length; j++)
            {
                string k = keys[j];

                GameObject keyObj = Instantiate(keyPrefab, keyboardParent);
                keyObj.name = $"Key_{k}";
                RectTransform keyRect = keyObj.GetComponent<RectTransform>();

                // Set position manually
                float x = startX + j * (keySize.x + keySpacing);
                float y = topOffset - i * (keySize.y + rowSpacing);
                keyRect.anchoredPosition = new Vector2(x, y);
                keyRect.sizeDelta = keySize;

                // Text and visuals
                TMP_Text text = keyObj.GetComponentInChildren<TMP_Text>();
                text.text = k.ToUpper();

                Image img = keyObj.GetComponent<Image>();
                img.color = normalColor;

                KeyCode keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), k.ToUpper());
                keyImages[keyCode] = img;
            }
        }
    }
}
