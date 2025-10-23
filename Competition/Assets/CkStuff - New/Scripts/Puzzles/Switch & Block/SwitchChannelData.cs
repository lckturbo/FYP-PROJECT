using UnityEngine;

[CreateAssetMenu(fileName = "SwitchChannel", menuName = "Switch Puzzle/Switch Channel")]
public class SwitchChannelData : ScriptableObject
{
    public string channelName = "Default";

    public int channelIndex = 0;
}
