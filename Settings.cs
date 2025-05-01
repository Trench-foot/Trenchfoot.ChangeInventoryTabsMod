using BepInEx.Configuration;
using UnityEngine;

namespace Trenchfoot.ChangeInventoryTabsMod
{
    internal class Settings
    {
        private const string KeybindsSectionTitle = "Keybinds";

        public static ConfigEntry<KeyboardShortcut> NexTabKey;
        public static ConfigEntry<KeyboardShortcut> PrevTabKey;


        public static void Init(ConfigFile Config)
        {
            NexTabKey = Config.Bind(
                KeybindsSectionTitle,
                "Next tab key",
                new KeyboardShortcut(KeyCode.E),
                "Move to next upper tab (e.g gear, health, overall e.t.c)"
            );

            PrevTabKey = Config.Bind(
                KeybindsSectionTitle,
                "Prev tab key",
                new KeyboardShortcut(KeyCode.Q),
                "Move to previous upper tab (e.g gear, health, overall e.t.c)"
            );

        }
    }
}
