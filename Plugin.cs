using BepInEx;
using EFT.UI;
using System;
using System.Reflection;
using UnityEngine;

namespace KzTarkov.ChangeInventoryTabsMod
{
    [BepInPlugin("KzTarkov.ChangeTabsMod", "KzTarkovChangeTabsMod", "1.0.0")]
    [BepInDependency("com.SPT.core", "3.9.4")]
    public class ChangeTabsPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Settings.Init(Config);
        }

        void Start()
        {
            Logger.LogInfo("SetSpeedPlugin loaded");

        }

        void Update()
        {
            if (Input.GetKeyDown(Settings.NexTabKey.Value.MainKey))
            {
                GClass3087 gclass = getInventroyScreenGclass();
                if (gclass == null)
                {
                    return;
                }
                shiftTab(gclass, +1);
            }

            if (Input.GetKeyDown(Settings.PrevTabKey.Value.MainKey))
            {
                GClass3087 gclass = getInventroyScreenGclass();
                if (gclass == null)
                {
                    return;
                }
                shiftTab(gclass, -1);
            }
        }

        private GClass3087 getInventroyScreenGclass()
        {
            GameObject obj = GameObject.Find("InventoryScreen");
            if (obj == null)
            {
                Logger.LogInfo("Inventory screen not found");
                return null;
            }

            InventoryScreen inventoryScreen = obj.GetComponent<InventoryScreen>();
            if (inventoryScreen == null)
            {
                Logger.LogInfo("Could not get component InventoryScreen");
                return null;
            }
            Logger.LogInfo("Trying to get gclass");
            Type type = typeof(InventoryScreen);
            FieldInfo prviateField = type.GetField("gclass3087_0", BindingFlags.NonPublic | BindingFlags.Instance);
            return (GClass3087)prviateField.GetValue(inventoryScreen);
        }

        private Tab[] getAllTabs(GClass3087 gclass)
        {
            Type type = typeof(GClass3087);
            FieldInfo tabsPrivateField = type.GetField("tab_0", BindingFlags.NonPublic | BindingFlags.Instance);
            return (Tab[])tabsPrivateField.GetValue(gclass);
        }

        private Tab getCurrentTab(GClass3087 gclass)
        {
            Type type = typeof(GClass3087);
            FieldInfo tabsPrivateField = type.GetField("tab_1", BindingFlags.NonPublic | BindingFlags.Instance);
            return (Tab)tabsPrivateField.GetValue(gclass);
        }

        // shift = +1/-1
        // shift can be either + 1 or -1
        private void shiftTab(GClass3087 gclass, int shift)
        {
            Tab currentTab = getCurrentTab(gclass);
            Tab[] allTabs = getAllTabs(gclass);

            int currentTabIndex = -1;
            for (int i = 0; i < allTabs.Length; i++)
            {
                if (allTabs[i].gameObject.name == currentTab.gameObject.name)
                {
                    currentTabIndex = i;
                }
            }
            Logger.LogInfo($"Current tab index: {currentTab}");

            if (currentTabIndex == -1)
            {
                // do nothing since bad shit happened
                Logger.LogInfo("Could not find current tab index");
                return;
            }

            int shiftedIndex = currentTabIndex + shift;
            if (shiftedIndex >= allTabs.Length || shiftedIndex < 0)
            {
                Logger.LogInfo("Shifted index is out of bounds");
                return;
            }

            selectTab(gclass, allTabs[shiftedIndex]);
        }

        private void selectTab(GClass3087 gclass, Tab tab)
        {
            gclass.method_0(tab, true);
            //gclass.SelectTab(tabs[0], true);
        }
    }
}
