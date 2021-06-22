using System;
using MelonLoader;
using UnityEngine.Video;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(VRCCC.VRCCC), "VRC Closed Captions", "1.0")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace VRCCC
{
    public class VRCCC : MelonMod
    {
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            foreach (var player in Object.FindObjectsOfType<VideoPlayer>())
                player.add_started(new Action<VideoPlayer>(VideoPlayerStarted));
        }
        
        async void VideoPlayerStarted(VideoPlayer source)
        {
            var uri = new VideoUri(source.url);
            MelonLogger.Msg(uri.GetFileName());
            var titles = await SubtitlesApi.QuerySubtitles(uri.GetFileName());
            foreach (var title in titles)
            {
                MelonLogger.Msg(title.SubDownloadLink);
            }
        }
    }
}