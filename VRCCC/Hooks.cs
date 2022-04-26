using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using VRC.SDK3.Internal.Video.Components.AVPro;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDKBase;
using VRCCC.QuickMenu;
using IntPtr = System.IntPtr;

namespace VRCCC
{
    public static class Hooks
    {
        private delegate void VideoPlayerInstanceDelegate(IntPtr instance, IntPtr methodInfo);
        private delegate void VideoPlayerInstanceSetURLDelegate(IntPtr instance, IntPtr newUrl, IntPtr methodInfo);
        private delegate void InputFieldInstanceDelegate(IntPtr instance, IntPtr methodInfo);
        private delegate void AVProInstanceDelegate(IntPtr instance, IntPtr methodInfo);
        private delegate void AVProInstanceLoadURLDelegate(IntPtr instance, IntPtr newUrl, IntPtr methodInfo);
        
        private static VideoPlayerInstanceDelegate _onPlay, _onPause, _onStop;
        private static VideoPlayerInstanceSetURLDelegate _onSetURL;
        private static InputFieldInstanceDelegate _onSelect;
        
        private static AVProInstanceDelegate _AVPOnPlay, _AVPOnPause, _AVPOnStop;
        private static AVProInstanceLoadURLDelegate _AVPOnLoadURL, _AVPOnPlayURL;
        

        public static unsafe void SetupHooks()
        {
            // Play
            var intPtr = (IntPtr) typeof(VideoPlayer).GetField("NativeMethodInfoPtr_Play_Public_Void_0",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            MelonUtils.NativeHookAttach(intPtr, new System.Action<IntPtr, IntPtr>(OnPlay).Method.MethodHandle.GetFunctionPointer());
            _onPlay = Marshal.GetDelegateForFunctionPointer<VideoPlayerInstanceDelegate>(*(IntPtr*) (void*) intPtr);
            
            // Pause
            intPtr = (IntPtr) typeof(VideoPlayer).GetField("NativeMethodInfoPtr_Pause_Public_Void_0",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            MelonUtils.NativeHookAttach(intPtr, new System.Action<IntPtr, IntPtr>(OnPause).Method.MethodHandle.GetFunctionPointer());
            _onPause = Marshal.GetDelegateForFunctionPointer<VideoPlayerInstanceDelegate>(*(IntPtr*) (void*) intPtr);
            
            // Stop
            intPtr = (IntPtr) typeof(VideoPlayer).GetField("NativeMethodInfoPtr_Stop_Public_Void_0",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            MelonUtils.NativeHookAttach(intPtr, new System.Action<IntPtr, IntPtr>(OnStop).Method.MethodHandle.GetFunctionPointer());
            _onStop = Marshal.GetDelegateForFunctionPointer<VideoPlayerInstanceDelegate>(*(IntPtr*) (void*) intPtr);
            
            // Set URL
            intPtr = (IntPtr) typeof(VideoPlayer).GetField("NativeMethodInfoPtr_set_url_Public_set_Void_String_0",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            MelonUtils.NativeHookAttach(intPtr, new System.Action<IntPtr, IntPtr, IntPtr>(OnSetURL).Method.MethodHandle.GetFunctionPointer());
            _onSetURL = Marshal.GetDelegateForFunctionPointer<VideoPlayerInstanceSetURLDelegate>(*(IntPtr*)(void*) intPtr);
            
            
            // AVPro ----
            // Play
            intPtr = *(IntPtr*)(IntPtr)UnhollowerUtils
                .GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                    typeof(VRCAVProVideoPlayer).GetMethod(nameof(VRCAVProVideoPlayer.Play))).GetValue(null);
            MelonUtils.NativeHookAttach((IntPtr)(&intPtr), new System.Action<IntPtr, IntPtr>(AVProPlay).Method.MethodHandle.GetFunctionPointer());
            _AVPOnPlay = (AVProInstanceDelegate) Marshal.GetDelegateForFunctionPointer(intPtr, typeof(AVProInstanceDelegate));
            
            // Pause
            intPtr = *(IntPtr*)(IntPtr)UnhollowerUtils
                .GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                    typeof(VRCAVProVideoPlayer).GetMethod(nameof(VRCAVProVideoPlayer.Pause))).GetValue(null);
            MelonUtils.NativeHookAttach((IntPtr)(&intPtr), new System.Action<IntPtr, IntPtr>(AVProPause).Method.MethodHandle.GetFunctionPointer());
            _AVPOnPause = (AVProInstanceDelegate) Marshal.GetDelegateForFunctionPointer(intPtr, typeof(AVProInstanceDelegate));
            
            // Stop
            intPtr = *(IntPtr*)(IntPtr)UnhollowerUtils
                .GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                    typeof(VRCAVProVideoPlayer).GetMethod(nameof(VRCAVProVideoPlayer.Stop))).GetValue(null);
            MelonUtils.NativeHookAttach((IntPtr)(&intPtr), new System.Action<IntPtr, IntPtr>(AVProStop).Method.MethodHandle.GetFunctionPointer());
            _AVPOnStop = (AVProInstanceDelegate) Marshal.GetDelegateForFunctionPointer(intPtr, typeof(AVProInstanceDelegate));
            
            // Play URL
            intPtr = *(IntPtr*)(IntPtr)UnhollowerUtils
                .GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                    typeof(VRCAVProVideoPlayer).GetMethod(nameof(VRCAVProVideoPlayer.PlayURL))).GetValue(null);
            MelonUtils.NativeHookAttach((IntPtr)(&intPtr), new System.Action<IntPtr, IntPtr, IntPtr>(AVProPlayURL).Method.MethodHandle.GetFunctionPointer());
            _AVPOnPlayURL = (AVProInstanceLoadURLDelegate)Marshal.GetDelegateForFunctionPointer(intPtr, typeof(AVProInstanceLoadURLDelegate));
            
            // Load URL
            intPtr = *(IntPtr*)(IntPtr)UnhollowerUtils
                .GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                    typeof(VRCAVProVideoPlayer).GetMethod(nameof(VRCAVProVideoPlayer.LoadURL))).GetValue(null);
            MelonUtils.NativeHookAttach((IntPtr)(&intPtr), new System.Action<IntPtr, IntPtr, IntPtr>(AVProLoadURL).Method.MethodHandle.GetFunctionPointer());
            _AVPOnLoadURL = (AVProInstanceLoadURLDelegate)Marshal.GetDelegateForFunctionPointer(intPtr, typeof(AVProInstanceLoadURLDelegate));
            
        }
        
        private static void OnPlay(IntPtr instance, IntPtr methodInfo)
        {
            _onPlay.Invoke(instance, methodInfo);
            var foundPlayer = VRCCC.TrackedPlayers.Find(player => player.Equals(instance));
            foundPlayer?.OnStateChange(TrackedPlayer.PlayerState.Play);
        }
        
        private static void OnPause(IntPtr instance, IntPtr methodInfo)
        {
            _onPause.Invoke(instance, methodInfo);
            var foundPlayer = VRCCC.TrackedPlayers.Find(player => player.Equals(instance));
            foundPlayer?.OnStateChange(TrackedPlayer.PlayerState.Pause);
        }
        
        private static void OnStop(IntPtr instance,IntPtr methodInfo)
        {
            _onStop.Invoke(instance, methodInfo);
            var foundPlayer = VRCCC.TrackedPlayers.Find(player => player.Equals(instance));
            foundPlayer?.OnStateChange(TrackedPlayer.PlayerState.Stop);
        }
        
        private static void OnSetURL(IntPtr instance, IntPtr newURL, IntPtr methodInfo)
        {
            _onSetURL.Invoke(instance, newURL, methodInfo);
            if (newURL == Il2CppSystem.IntPtr.Zero) return;
            var foundPlayer = VRCCC.TrackedPlayers.Find(player => player.Equals(instance));
            foundPlayer?.OnURLChange(new String(newURL));
        }
        
        private static void OnSelect(IntPtr instance, IntPtr methodInfo) 
        {
            MelonLogger.Msg("Selected an input field!");
            _onSelect.Invoke(instance, methodInfo);
        }
        
        
        private static void AVProPlay(IntPtr instance, IntPtr methodInfo) { 
            MelonLogger.Warning("Play");
            var foundPlayer = VRCCC.TrackedPlayers.Find(player => player.Equals(instance));
            foundPlayer?.OnStateChange(TrackedPlayer.PlayerState.Play);
            _AVPOnPlay.Invoke(instance, methodInfo);
        }
        
        private static void AVProPause(IntPtr instance, IntPtr methodInfo) { 
            MelonLogger.Warning("Pause");
            var foundPlayer = VRCCC.TrackedPlayers.Find(player => player.Equals(instance));
            foundPlayer?.OnStateChange(TrackedPlayer.PlayerState.Pause);
            _AVPOnPause.Invoke(instance, methodInfo);
        }
        
        private static void AVProStop(IntPtr instance, IntPtr methodInfo) { 
            MelonLogger.Warning("Stop");
            var foundPlayer = VRCCC.TrackedPlayers.Find(player => player.Equals(instance));
            foundPlayer?.OnStateChange(TrackedPlayer.PlayerState.Stop);
            _AVPOnStop.Invoke(instance, methodInfo);
        }
        
        private static void AVProLoadURL(IntPtr instance, IntPtr newUrl, IntPtr methodInfo) { 
            MelonLogger.Msg("Load URL");
            String url = new VRCUrl(newUrl).url;
            MelonLogger.Warning("New URL: " + url);
            var foundPlayer = VRCCC.TrackedPlayers.Find(player => player.Equals(instance));
            foundPlayer?.OnURLChange(url);
            if (foundPlayer != null) {
                MelonLogger.Msg("Found player"); 
            } else { 
                MelonLogger.Msg("Didn't find player"); 
            }
            _AVPOnLoadURL.Invoke(instance, newUrl, methodInfo);
        }
        
        private static void AVProPlayURL(IntPtr instance, IntPtr newUrl, IntPtr methodInfo) { 
            MelonLogger.Msg("Play URL");
            String url = new VRCUrl(newUrl).url;
            MelonLogger.Msg("New URL: " + url);
            var foundPlayer = VRCCC.TrackedPlayers.Find(player => player.Equals(instance));
            foundPlayer?.OnURLChange(url);
            _AVPOnPlayURL.Invoke(instance, newUrl, methodInfo);
        }
    }
}