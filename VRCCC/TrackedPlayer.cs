using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using UnityEngine.Video;

namespace VRCCC
{
    public class TrackedPlayer
    {
        public enum PlayerState
        {
            Play,
            Pause,
            Stop
        }
        
        private readonly VideoPlayer _storedPlayer;
        private object _coroutineToken;

        private string _url;
        private PlayerState _currentState;
        private long _msOffset = 0;

        private long CurrentTimeInMs => (long) Math.Round(_storedPlayer.time, 2)*1000;
        private Timeline _tl;
        private bool _needsNewTitles;
        
        public TrackedPlayer(VideoPlayer original)
        {
            _storedPlayer = original;
            _currentState = GetPlayerState(_storedPlayer);
        }
        
        private static PlayerState GetPlayerState(VideoPlayer player)
        {
            var state = PlayerState.Stop;
            if (player.isPlaying) state = PlayerState.Play;
            if (player.isPaused) state = PlayerState.Pause;
            return state;
        }
        
        /**
         * <summary>Display of the subtitles can be offset by some number of ms, to account for differences between
         * the content the closed caption file is encoded for, and the actual content being viewed. This function
         * retrieves the current offset (initially 0ms).</summary>
         * <returns>The current offset in ms</returns>
         */
        public long getCurrentOffsetMs() 
        { 
            return _msOffset;
        }
        
        /**
         * <summary>Increments (or decrements) the current offset by `ms` milliseconds.</summary>
         * <param nmae="ms">A signed long indicating the number of ms to change the offset by. Set to negative to
         * decrement the offset.</param>
         */
        public void incrementOrDecrementOffset(long ms) 
        {
            _msOffset += ms;
        }

        public void OnStateChange(PlayerState newState)
        {
            _currentState = newState;
            if (_currentState == PlayerState.Play)
                _coroutineToken = MelonCoroutines.Start(UpdateSubtitles());
            else if (_coroutineToken != null) 
                MelonCoroutines.Stop(_coroutineToken);
        }

        public async void OnURLChange(string newURl)
        {
            var (movieName, timelineEvents) = await FetchSubtitlesForNewUrl(newURl);
            if (timelineEvents == null)
            {
                _tl = null;
                return;
            }
            
            _tl = new Timeline(timelineEvents);
            VRCCC.MainThreadExecutionQueue.Add(() => MelonCoroutines.Start(UITextArea.DisplayAlert($"Starting Subtitle Playback for: {movieName}", 3)));
        }

        private static async Task<(string, List<TimelineEvent>)> FetchSubtitlesForNewUrl(string newUrl)
        {
            var uri = new VideoUri(newUrl);
            var title = await SubtitlesApi.QuerySubtitles(uri.GetFileName());
            if (title == null)
            {
                MelonLogger.Msg("Failed to find movie");
                return ("", null);
            }

            var subFile = await SubtitlesApi.FetchSub(title.SubDownloadLink);
            return subFile.Length < 512 ? ("", null) : (title.MovieName, SRTDecoder.DecodeSrtIntoTimelineEvents(subFile));
        }

        private IEnumerator UpdateSubtitles()
        {
            while (true)
            {
                if (_tl != null && _currentState == PlayerState.Play)
                {
                    UITextArea.ToggleUI(true);
                    List<TimelineEvent> events = _tl.ScrubToTime(CurrentTimeInMs + _msOffset);
                    // Large gaps in time (resync, lag, a big seek) can result in a massive list of missed events.
                    // To prevent this, we only care about the two most recent events
                    if (events.Count > 2) 
                        events.RemoveRange(0, events.Count - 3);
                    
                    foreach (var eventObj in events.Where(eventObj => !eventObj.eventText.Contains("OpenSubtitles")))
                    {
                        MelonLogger.Msg(eventObj.eventText);
                        UITextArea.Text = eventObj.eventText;
                    }
                }
                else
                {
                    UITextArea.Text = "";
                    UITextArea.ToggleUI(false);
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        public bool Equals(IntPtr playerPtr) => playerPtr == _storedPlayer.Pointer;
    }
}