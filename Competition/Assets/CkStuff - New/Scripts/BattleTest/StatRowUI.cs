using TMPro;
using UnityEngine;

public class StatRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Text oldValue;
    [SerializeField] private TMP_Text arrow;
    [SerializeField] private TMP_Text newValue;

    public void Bind(string labelText, string oldText, string newText, Color valueColor)
    {
        if (label) label.text = labelText;
        if (oldValue) { oldValue.text = oldText; oldValue.color = valueColor; }
        if (arrow) arrow.text = "->";
        if (newValue) { newValue.text = newText; newValue.color = valueColor; }
    }
}
