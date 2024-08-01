using BepInEx;
using Comfort.Common;
using EFT.UI;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KzTarkov.ChangeInventoryTabsMod
{
    [BepInPlugin("KzTarkov.ChangeTabsMod", "KzTarkovChangeTabsMod", "1.0.2")]
    [BepInDependency("com.SPT.core", "3.9.4")]
    public class ChangeTabsPlugin : BaseUnityPlugin
    {
        private const string GCLASS_FIELD_NAME = "gclass3087_0";
        private const string ALL_TABS_FIELD_NAME = "tab_0";
        private const string CURRENT_TAB_FIELD_NAME = "tab_1";

        private static FieldInfo _gclass = null;
        private static FieldInfo _allTabs = null;
        private static FieldInfo _currentTab = null;

        private void Awake()
        {
            Settings.Init(Config);
        }

        private bool isInputFieldFocused()
        {
            if (
                EventSystem.current.currentSelectedGameObject != null
                && EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null)
            {
                return true;
            }
            return false;
        }
        private void Update()
        {
            if (!isInputFieldFocused())
            {
                if (Input.GetKeyDown(Settings.NexTabKey.Value.MainKey))
                {
                    ShiftTab(+1);
                }

                if (Input.GetKeyDown(Settings.PrevTabKey.Value.MainKey))
                {
                    ShiftTab(-1);
                }
            }

        }

        // shift = +1/-1
        // shift can be either + 1 or -1
        private void ShiftTab(int shift)
        {
            GClass3087 gclass = GetInventroyScreenGclass();
            if (gclass == null)
            {
                Logger.LogInfo("GClass is null");
                return;
            }
            Tab currentTab = GetCurrentTab(gclass);
            Tab[] allTabs = GetAllTabs(gclass);

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
                // do nothing since bad shit happened, probably mod is incompatible anymore
                Logger.LogInfo("Could not find current tab index");
                return;
            }

            int shiftedIndex = currentTabIndex + shift;
            if (shiftedIndex >= allTabs.Length || shiftedIndex < 0)
            {
                Logger.LogInfo("Shifted index is out of bounds");
                return;
            }

            SelectTab(gclass, allTabs[shiftedIndex]);
        }

        private GClass3087 GetInventroyScreenGclass()
        {
            var inventoryScreen = Singleton<CommonUI>.Instance.InventoryScreen;

            if (inventoryScreen == null)
            {
                return null;
            }

            Type type = typeof(InventoryScreen);
            if (_gclass == null)
            {
                _gclass = AccessTools.Field(type, "gclass3087_0");
            }

            return (GClass3087)_gclass.GetValue(inventoryScreen);
        }
        private Tab GetCurrentTab(GClass3087 gclass)
        {
            Type type = typeof(GClass3087);

            if (_currentTab == null)
            {
                Logger.LogInfo("caching type of _currentTab");
                _currentTab = AccessTools.Field(type, CURRENT_TAB_FIELD_NAME);
            }

            return (Tab)_currentTab.GetValue(gclass);
        }
        private Tab[] GetAllTabs(GClass3087 gclass)
        {
            Type type = typeof(GClass3087);

            if (_allTabs == null)
            {
                Logger.LogInfo("caching type of _allTabs");
                _allTabs = AccessTools.Field(type, ALL_TABS_FIELD_NAME);
            }

            return (Tab[])_allTabs.GetValue(gclass);
        }

        private void SelectTab(GClass3087 gclass, Tab tab)
        {
            gclass.method_0(tab, true);
        }
    }
}
