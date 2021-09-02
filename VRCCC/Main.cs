﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnityEngine.Video;
using VRCCC.QuickMenu;
using UnityEngine;
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
            
            foreach (var discoveredPlayer in Object.FindObjectsOfType<VideoPlayer>())
                TrackedPlayers.Add(new TrackedPlayer(discoveredPlayer));
        }
        
        public override void OnSceneWasLoaded(int level, string levelName) { 
            if (level == -1) 
                QuickModeMenu.InitIfNeeded();
        }

        
        //public override void VRChat_OnUiManagerInit() => UiManagerInit();
        
        private static void UiManagerInit() { 
        }
        
        public override void OnUpdate() { 
            if (_shouldCheckUiManager) CheckUiManager();
            
            /*
            if (QuickModeMenu.MainMenu != null && QuickModeMenu.IsMenuShown) 
                QuickModeMenu.MainMenu.Update();
            */
            
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
            UiManagerInit();
        }
    }
}