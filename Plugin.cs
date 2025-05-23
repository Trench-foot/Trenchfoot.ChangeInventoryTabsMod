using BepInEx;
using Comfort.Common;
using EFT;
using EFT.UI;
using EFT.UI.Screens;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static EFT.UI.MenuScreen;

namespace Trenchfoot.ChangeInventoryTabsMod
{
    [BepInPlugin("Trenchfoot.ChangeTabsMod", "TrenchfootChangeTabsMod", "1.0.2")]
    [BepInDependency("com.SPT.core", "3.11.0")]
    public class ChangeTabsPlugin : BaseUnityPlugin
    {
        private const string GCLASS_FIELD_NAME = "gclass3521_0";
        private const string MCLASS_FIELD_NAME = "gclass3587_0";
        private const string ALL_TABS_FIELD_NAME = "tab_0";
        private const string CURRENT_TAB_FIELD_NAME = "tab_2";

        private static FieldInfo _gclass = null;
        private static FieldInfo _mclass = null;
        private static FieldInfo _allTabs = null;
        private static FieldInfo _currentTab = null;

        private bool enableLogging = true;

        private bool enableLogging = false;
            
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

        private bool isInventoryScreenFocus()
        {
            //return ItemUiContext.Instance.ContextType == EItemUiContextType.InventoryScreen;
            return ItemUiContext.Instance.ContextType == EItemUiContextType.InventoryScreen;
        }

        private bool isTraderScreenFocus()
        {
            //return ItemUiContext.Instance.ContextType == EItemUiContextType.TraderScreen;
            return ItemUiContext.Instance.ContextType == EItemUiContextType.TraderScreen;
        }

        private TarkovApplication getTarkovApplication()
        {
            // OH MY GOD THIS WORKS!!!!
            // It took me two days to figure this out
            TarkovApplication _tarkovApplication;
            if(TarkovApplication.Exist(out _tarkovApplication))

            if (_tarkovApplication == null)
            {
                Logger.LogInfo("Tarkov application is null");
                return null;
            }
            return _tarkovApplication;
        }   

        private bool getCurrentGameWorld()
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            if(gameWorld is HideoutGameWorld)
            {
                Logger.LogInfo("Game world is hideout");
                return true;
            }
            if (gameWorld is ClientGameWorld)
            {
                Logger.LogInfo("Game world is main player");
                return false;
            }
            return true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(Settings.NexTabKey.Value.MainKey))
            {
                // do not trigger if inventory screen is not focused or input field is focused
                if (!isInventoryScreenFocus() || isInputFieldFocused())
                {
                    return;
                }
                else
                { 
                    ShiftTab(+1);
                }
            }

            if (Input.GetKeyDown(Settings.PrevTabKey.Value.MainKey) && isInventoryScreenFocus())
            {
                // do not trigger if inventory screen is not focused or input field is focused
                if (!isInventoryScreenFocus() || isInputFieldFocused())
                {
                    return;
                }
                else
                {
                    ShiftTab(-1);
                }
            }

            if (Input.GetKeyDown(KeyCode.T) && getCurrentGameWorld() && !isInputFieldFocused())
            {

                TarkovApplication _tarkovApplication = getTarkovApplication();
                    
                if (_tarkovApplication == null)
                {
                    Logger.LogInfo("Tarkov application is null");
                    return;
                }

                _tarkovApplication.method_53(EMenuType.Trade, true);
            }
            if(Input.GetKeyDown(KeyCode.P) && getCurrentGameWorld() && !isInputFieldFocused())
            {
                TarkovApplication _tarkovApplication = getTarkovApplication();

                if (_tarkovApplication == null)
                {
                    Logger.LogInfo("Tarkov application is null");
                    return;
                }

                _tarkovApplication.method_53(EMenuType.Player, true);
                { 
                    ShiftTab(-1);
                }
            }
        }

        // shift = +1/-1
        // shift can be either + 1 or -1
        private void ShiftTab(int shift)
        {
            GClass3521 gclass = GetInventroyScreenGclass();
            if (gclass == null)
            {
                if(enableLogging)
                {
                    Logger.LogInfo("GClass is null");
                }
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

            if(enableLogging)
            {
                Logger.LogInfo($"Current tab index: {currentTab}");
            }

            if (currentTabIndex == -1)
            {
                // do nothing since bad shit happened, probably mod is incompatible anymore
                if(enableLogging)
                {
                    Logger.LogInfo("Could not find current tab index");
                }
                return;
            }

            int shiftedIndex = currentTabIndex + shift;
            if(shiftedIndex >= allTabs.Length)
            {
                shiftedIndex = 0;
            } 
            else if(shiftedIndex < 0)
            {
                shiftedIndex = allTabs.Length - 1;
            }

            SelectTab(gclass, allTabs[shiftedIndex]);
        }

        private GClass3521 GetInventroyScreenGclass()
        {
            //var inventoryScreen = Singleton<CommonUI>.Instance.InventoryScreen;
            var inventoryScreen = Singleton<CommonUI>.Instance.InventoryScreen;

            if (inventoryScreen == null)
            {
                return null;
            }

            if (!inventoryScreen.isActiveAndEnabled)
            {
                if(enableLogging)
                {
                    Logger.LogInfo($"inventory screen is not active");
                }
                return null;
            }

            Type type = typeof(InventoryScreen);
            if (_gclass == null)
            {
                _gclass = AccessTools.Field(type, "gclass3521_0");
            }

            return (GClass3521)_gclass.GetValue(inventoryScreen);
        }
        private Tab GetCurrentTab(GClass3521 gclass)
        {
            Type type = typeof(GClass3521);

            if (_currentTab == null)
            {
                if(enableLogging)
                {
                    Logger.LogInfo("caching type of _currentTab");
                }
                _currentTab = AccessTools.Field(type, CURRENT_TAB_FIELD_NAME);
            }

            return (Tab)_currentTab.GetValue(gclass);
        }
        private Tab[] GetAllTabs(GClass3521 gclass)
        {
            Type type = typeof(GClass3521);

            if (_allTabs == null)
            {
                if(enableLogging)
                {
                    Logger.LogInfo("caching type of _allTabs");
                }
                _allTabs = AccessTools.Field(type, ALL_TABS_FIELD_NAME);
            }

            return (Tab[])_allTabs.GetValue(gclass);
        }

        private void SelectTab(GClass3521 gclass, Tab tab)
        {
            gclass.method_0(tab, true);
        }
    }
}
