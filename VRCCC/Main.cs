using System;
using System.Linq;
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
            var titles = await SubtitlesApi.QuerySubtitles(uri.GetFileName());
            if (titles.Count == 0)
            {
                MelonLogger.Msg("Failed to find movie");
                return;
            }
            var bestMatch = titles.FirstOrDefault(title => title.LanguageName == "English" && title.SubHearingImpaired == "1") ??
                            titles.First(title => title.LanguageName == "English");
            MelonLogger.Msg($"Best Match Found!\n Score: {bestMatch.Score}\n DL Link: {bestMatch.SubDownloadLink}\n Hearing Impaired Designed: {bestMatch.SubHearingImpaired == "1"}");
            
        }
    }
}