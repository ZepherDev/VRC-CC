using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine.Video;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(VRCCC.VRCCC), "VRC Closed Captions", "1.0")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace VRCCC
{
    public class VRCCC : MelonMod
    {
        public static readonly List<TrackedPlayer> TrackedPlayers = new List<TrackedPlayer>();
        public static readonly List<Action> MainThreadExecutionQueue = new List<Action>();

        public override void OnApplicationStart()
        {
            Hooks.SetupHooks(); 
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            foreach (var lingeringPlayer in TrackedPlayers)
                lingeringPlayer.Dispose();
            TrackedPlayers.Clear();
            
            foreach (var discoveredPlayer in Object.FindObjectsOfType<VideoPlayer>())
                TrackedPlayers.Add(new TrackedPlayer(discoveredPlayer));
        }

        public override void OnUpdate()
        {
            if (MainThreadExecutionQueue.Count == 0) return;
            
            foreach (var execution in MainThreadExecutionQueue) execution.Invoke();
            MainThreadExecutionQueue.Clear();
        }
    }
}