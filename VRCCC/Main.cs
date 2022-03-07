using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnityEngine.Video;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Video.Components.AVPro;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(VRCCC.VRCCC), "VRC Closed Captions", "1.0",
    "benaclejames and foxipso", "https://github.com/benaclejames/VRC-CC")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace VRCCC
{
    public class VRCCC : MelonMod
    {
        public static readonly List<TrackedPlayer> TrackedPlayers = new List<TrackedPlayer>();
        public static readonly List<Action> MainThreadExecutionQueue = new List<Action>();
        private bool _shouldCheckUiManager;
        public static Type _uiManager;
        private MethodInfo _uiManagerInstance;
        private Assembly _assemblyCSharp;

        public override void OnApplicationStart() {
            Hooks.SetupHooks();
            
            _assemblyCSharp = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => 
                assembly.GetName().Name == "Assembly-CSharp");
            _shouldCheckUiManager = typeof(MelonMod).GetMethod("VRChat_OnUiManagerInit") == null;
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
            foreach (var lingeringPlayer in TrackedPlayers)
                lingeringPlayer.Dispose();
            TrackedPlayers.Clear();
            
            foreach (var discoveredPlayer in Object.FindObjectsOfType<VideoPlayer>()) {
                MelonLogger.Msg("Discovered VideoPlayer");
                TrackedPlayers.Add(new TrackedPlayer(discoveredPlayer));
            }
                
            foreach (var discoveredPlayer in Object.FindObjectsOfType<VRCAVProVideoPlayer>()) { 
                MelonLogger.Msg("Found VRC AVPro Video Player");
                TrackedPlayers.Add(new TrackedPlayer(discoveredPlayer));
            }
        }
        
        public override void OnSceneWasLoaded(int level, string levelName) { 
            // if (level == -1) 
                // QuickModeMenu.InitIfNeeded();
        }

        
        //public override void VRChat_OnUiManagerInit() => UiManagerInit();
        
        private static void UiManagerInit() {
            var a = Assembly.GetExecutingAssembly();
            using (Stream resFilestream = a.GetManifestResourceStream("VRCCC.vrcccmainmenu")) {
                var ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                var bundle = AssetBundle.LoadFromMemory(ba);
                var mainPrefab = bundle.LoadAsset<GameObject>("assets/vrc-cc.prefab");
                var menuContent = GameObject.Find("UserInterface/MenuContent");
                var parent = menuContent.transform.FindChild("Screens");
                var newScreen = GameObject.Instantiate(mainPrefab, parent);
                newScreen.transform.localPosition = new Vector3(-0.0586f, -0.0086f, 0.045f);
                // newScreen.AddComponent<MainMenuTab>();
                newScreen.SetActive(false);
                newScreen.name = "VRC-CC";
                var menuTabs = menuContent.transform.FindChild("Backdrop/Header/Tabs/ViewPort/Content");
                var safetyTab = menuTabs.Find("SafetyPageTab");
                var newTab = GameObject.Instantiate(safetyTab, menuTabs); // Yoink
                newTab.name = "VRC-CC";
                newTab.SetSiblingIndex(newTab.GetSiblingIndex()-1);
                newTab.FindChild("Button/Text").GetComponent<Text>().text = "VRC-CC";
                newTab.GetComponent<VRCUiPageTab>().field_Public_String_1 = "UserInterface/MenuContent/Screens/VRC-CC";
            }
        }
        
        public override void OnUpdate() { 
            if (_shouldCheckUiManager) CheckUiManager();
            
            //if (QuickModeMenu.MainMenu != null && QuickModeMenu.IsMenuShown) 
            //    QuickModeMenu.MainMenu.Update();

            if (MainThreadExecutionQueue.Count <= 0) return;
            
            MainThreadExecutionQueue[0].Invoke();
            MainThreadExecutionQueue.RemoveAt(0);
        }
        
        private void CheckUiManager() { 
            if (_assemblyCSharp == null) return;
            
            if (_uiManager == null) _uiManager = _assemblyCSharp.GetType("VRCUiManager");
            if (_uiManager == null) { 
                _shouldCheckUiManager = false;
                return;
            }
            
            if (_uiManagerInstance == null) 
                _uiManagerInstance = _uiManager.GetMethods().First(x => x.ReturnType == _uiManager);
            if (_uiManagerInstance == null) { 
                _shouldCheckUiManager = false;
                return;
            }
            
            if (_uiManagerInstance.Invoke(null, Array.Empty<object>()) == null) 
                return;
            
            _shouldCheckUiManager = false;
            // UiManagerInit();
        }
    }
}