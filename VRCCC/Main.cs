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

        public override void OnApplicationStart()
        {
            Hooks.SetupHooks();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            TrackedPlayers.Clear();
            foreach (var discoveredPlayer in Object.FindObjectsOfType<VideoPlayer>())
                TrackedPlayers.Add(new TrackedPlayer(discoveredPlayer));
        }
    }
}