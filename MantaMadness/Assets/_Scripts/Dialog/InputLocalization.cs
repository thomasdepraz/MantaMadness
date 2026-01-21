using System.Collections.Generic;
using static DialogLoader;

public static class InputLocalization
{
    private static readonly Dictionary<InputDeviceType, Dictionary<string, string>> inputMap =
        new()
        {
            {
                InputDeviceType.KeyboardMouse, new Dictionary<string, string>
                {
                    { "INTERACT", "LMB" },
                    { "JUMP", "Space" },
                    { "STRAF_LEFT", "A" },
                    { "STRAF_RIGHT", "E" },
                }
            },
            {
                InputDeviceType.Xbox, new Dictionary<string, string>
                {
                    { "INTERACT", "X" },
                    { "JUMP", "A" },
                    { "STRAF_LEFT", "LB" },
                    { "STRAF_RIGHT", "RB" },
                }
            },
            {
                InputDeviceType.PlayStation, new Dictionary<string, string>
                {
                    { "INTERACT", "Square" },
                    { "JUMP", "✕" },
                    { "STRAF_LEFT", "L1" },
                    { "STRAF_RIGHT", "R1" },
                }
            },
        };

    public static string GetInput(string key)
    {
        var device = InputManager.CurrentDevice;

        if (inputMap.TryGetValue(device, out var map) &&
            map.TryGetValue(key, out var value))
            return value;

        return $"[{key}]";
    }
}
