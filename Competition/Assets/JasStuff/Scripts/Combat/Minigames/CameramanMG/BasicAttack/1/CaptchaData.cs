using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CaptchaData", menuName = "Minigames/Captcha Data")]
public class CaptchaData : ScriptableObject
{
    [TextArea] public string prompt;

    [Header("Grid")]
    [Min(1)] public int rows = 4;
    [Min(1)] public int cols = 4;

    [Header("Tiles (length must be rows*cols)")]
    public Sprite[] tiles;

    [Header("Correct Tiles (0-based, row-major, top-left = 0)")]
    public List<int> correctIndices = new();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (rows < 1) rows = 1;
        if (cols < 1) cols = 1;
        if (tiles != null && tiles.Length != rows * cols)
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
