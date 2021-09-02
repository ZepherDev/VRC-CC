using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Il2CppSystem;
using MelonLoader;
using UnityEngine.UI;
using UnityEngine.Video;
using VRCCC.QuickMenu;
using IntPtr = System.IntPtr;

namespace VRCCC
{
    public static class Hooks
    {
        private delegate void VideoPlayerInstanceDelegate(IntPtr instance);
        private delegate void VideoPlayerInstanceSetURLDelegate(IntPtr instance, IntPtr newUrl);
        private delegate void InputFieldInstanceDelegate(IntPtr instance);
        
        private static VideoPlayerInstanceDelegate _onPlay, _onPause, _onStop;
        private static VideoPlayerInstanceSetURLDelegate _onSetURL;
        private static InputFieldInstanceDelegate _onSelect;

        public static unsafe void SetupHooks()
        {
            // Play
            var intPtr = (IntPtr) typeof(VideoPlayer).GetField("NativeMethodInfoPtr_Play_Public_Void_0",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            MelonUtils.NativeHookAttach(intPtr, new System.Action<IntPtr>(OnPlay).Method.MethodHandle.GetFunctionPointer());
            _onPlay = Marshal.GetDelegateForFunctionPointer<VideoPlayerInstanceDelegate>(*(IntPtr*) (void*) intPtr);
            
            // Pause
            intPtr = (IntPtr) typeof(VideoPlayer).GetField("NativeMethodInfoPtr_Pause_Public_Void_0",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            MelonUtils.NativeHookAttach(intPtr, new System.Action<IntPtr>(OnPause).Method.MethodHandle.GetFunctionPointer());
            _onPause = Marshal.GetDelegateForFunctionPointer<VideoPlayerInstanceDelegate>(*(IntPtr*) (void*) intPtr);
            
            // Stop
            intPtr = (IntPtr) typeof(VideoPlayer).GetField("NativeMethodInfoPtr_Stop_Public_Void_0",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            MelonUtils.NativeHookAttach(intPtr, new System.Action<IntPtr>(OnStop).Method.MethodHandle.GetFunctionPointer());
            _onStop = Marshal.GetDelegateForFunctionPointer<VideoPlayerInstanceDelegate>(*(IntPtr*) (void*) intPtr);
            
            // Set URL
            intPtr = (IntPtr) typeof(VideoPlayer).GetField("NativeMethodInfoPtr_set_url_Public_set_Void_String_0",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            MelonUtils.NativeHookAttach(intPtr, new System.Action<IntPtr, IntPtr>(OnSetURL).Method.MethodHandle.GetFunctionPointer());
            _onSetURL = Marshal.GetDelegateForFunctionPointer<VideoPlayerInstanceSetURLDelegate>(*(IntPtr*) (void*) intPtr);
            
            // InputField onSelect
            intPtr = (IntPtr) typeof(InputField).GetField("NativeMethodInfoPtr_OnSelect_Public_Virtual_Void_BaseEventData_0",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Default).GetValue(null);
            MelonUtils.NativeHookAttach(intPtr, new System.Action<IntPtr>(OnSelect).Method.MethodHandle.GetFunctionPointer());
            _onSelect = Marshal.GetDelegateForFunctionPointer<InputFieldInstanceDelegate>(*(IntPtr*) (void*) intPtr);
        }

        private static void OnPlay(IntPtr instance)
        {
            _onPlay.Invoke(instance);
            var foundPlayer = VRCCC.TrackedPlayers.Find(player => player.Equals(instance));
            foundPlayer?.OnStateChange(TrackedPlayer.PlayerState.Play);
        }
        
        private static void OnPause(IntPtr instance)
        {
            _onPause.Invoke(instance);
            var foundPlayer = VRCCC.TrackedPlayers.Find(player => player.Equals(instance));
            foundPlayer?.OnStateChange(TrackedPlayer.PlayerState.Pause);
        }
        
        private static void OnStop(IntPtr instance)
        {
            _onStop.Invoke(instance);
            var foundPlayer = VRCCC.TrackedPlayers.Find(player => player.Equals(instance));
            foundPlayer?.OnStateChange(TrackedPlayer.PlayerState.Stop);
        }
        
        private static void OnSetURL(IntPtr instance, IntPtr newURL)
        {
            _onSetURL.Invoke(instance, newURL);
            if (newURL == Il2CppSystem.IntPtr.Zero) return;
            var foundPlayer = VRCCC.TrackedPlayers.Find(player => player.Equals(instance));
            foundPlayer?.OnURLChange(new String(newURL));
        }
        
        private static void OnSelect(IntPtr instance) 
        {
            MelonLogger.Msg("Selected an input field!");
            _onSelect.Invoke(instance);
            if (instance.Equals(MainMenu._inputField.Pointer)) { 
                if (MainMenu._readyToOpenKeyboard &&
                    MainMenu._inputField != null) {
                    MainMenu.GetMovieNameWithPopupKeyboard();
                    // MainMenu._readyToOpenKeyboard = false;
                }
            }
        }
    }
}