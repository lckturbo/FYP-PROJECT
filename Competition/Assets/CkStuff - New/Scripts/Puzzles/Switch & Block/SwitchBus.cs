using System;
using System.Collections.Generic;

public static class SwitchBus
{
    private static readonly Dictionary<int, Action> _toggleByChannel = new();
    private static readonly Dictionary<int, Action<bool>> _setStateByChannel = new();

    public static void Subscribe(int channelIndex, Action onToggle, Action<bool> onSetState = null)
    {
        if (!_toggleByChannel.ContainsKey(channelIndex))
            _toggleByChannel[channelIndex] = null;
        _toggleByChannel[channelIndex] += onToggle;

        if (onSetState != null)
        {
            if (!_setStateByChannel.ContainsKey(channelIndex))
                _setStateByChannel[channelIndex] = null;
            _setStateByChannel[channelIndex] += onSetState;
        }
    }

    public static void Unsubscribe(int channelIndex, Action onToggle, Action<bool> onSetState = null)
    {
        if (_toggleByChannel.ContainsKey(channelIndex))
            _toggleByChannel[channelIndex] -= onToggle;

        if (onSetState != null && _setStateByChannel.ContainsKey(channelIndex))
            _setStateByChannel[channelIndex] -= onSetState;
    }

    public static void Toggle(int channelIndex)
    {
        if (_toggleByChannel.TryGetValue(channelIndex, out var evt))
            evt?.Invoke();
    }

    public static void SetState(int channelIndex, bool open)
    {
        if (_setStateByChannel.TryGetValue(channelIndex, out var evt))
            evt?.Invoke(open);
    }
}
