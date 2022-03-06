using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Il2CppSystem;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine.UI;
using UnityEngine.Video;
using VRC.SDK3.Internal.Video.Components.AVPro;
using VRC.SDK3.Video.Components.AVPro;
using VRCCC.QuickMenu;
using IntPtr = System.IntPtr;

namespace VRCCC
{
    public static class Hooks
    {
        private delegate void VideoPlayerInstanceDelegate(IntPtr instance, IntPtr methodInfo);
        private delegate void VideoPlayerInstanceSetURLDelegate(IntPtr instance, IntPtr newUrl, IntPtr methodInfo);
        private delegate void InputFieldInstanceDelegate(IntPtr instance, IntPtr methodInfo);
        
        private static VideoPlayerInstanceDelegate _onPlay, _onPause, _onStop;
        private static VideoPlayerInstanceSetURLDelegate _onSetURL;
        private static InputFieldInstanceDelegate _onSelect;

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
            _onSetURL = Marshal.GetDelegateForFunctionPointer<VideoPlayerInstanceSetURLDelegate>(*(IntPtr*) (void*) intPtr);
            
            
            intPtr = (IntPtr) typeof(VRCAVProVideoPlayer).GetField("NativeMethodInfoPtr_LoadURL_Public_Virtual_Void_VRCUrl_0",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            MelonUtils.NativeHookAttach(intPtr, new System.Action<IntPtr, IntPtr, IntPtr>(Debug).Method.MethodHandle.GetFunctionPointer());
            
            /*
            try { 
                intPtr = *(IntPtr*)(IntPtr) typeof(VRCAVProVideoPlayer).GetField("NativeMethodInfoPtr_LoadURL_Public_Virtual_Void_VRCUrl_0",
                    BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                MelonUtils.NativeHookAttach(intPtr, new System.Action<IntPtr, IntPtr>(Debug).Method.MethodHandle.GetFunctionPointer());
                
                if (intPtr == IntPtr.Zero) { 
                    MelonLogger.Warning("Failed to hook LoadURL again!");
                }
            } catch (System.Exception e) { 
                MelonLogger.Error(e);
            }
            */
            intPtr = *(IntPtr*)(IntPtr)UnhollowerUtils
                .GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                    typeof(VRCAVProVideoPlayer).GetMethod(nameof(VRCAVProVideoPlayer.Play))).GetValue(null);
            MelonUtils.NativeHookAttach((IntPtr)(&intPtr), new System.Action<IntPtr, IntPtr>(Debug2).Method.MethodHandle.GetFunctionPointer());
            
            intPtr = (IntPtr) typeof(VRCAVProVideoPlayer).GetField("NativeMethodInfoPtr_Play_Public_Virtual_Void_0",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            MelonUtils.NativeHookAttach(intPtr, new System.Action<IntPtr, IntPtr>(Debug3).Method.MethodHandle.GetFunctionPointer());
            
            intPtr = (IntPtr) typeof(VRCAVProVideoPlayer).GetField("NativeMethodInfoPtr_Pause_Public_Virtual_Void_0",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            MelonUtils.NativeHookAttach(intPtr, new System.Action<IntPtr, IntPtr>(Debug4).Method.MethodHandle.GetFunctionPointer());

            MelonLogger.Msg("Attempting to hook AVProVideoPlayer");
            try {
                FieldInfo fi =
                    UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                        typeof(VRCAVProVideoPlayer).GetMethods().First(it => 
                            it.Name == nameof(VRCAVProVideoPlayer.LoadURL) && 
                            it.GetParameters().Length == 1));
                if (fi != null) {
                    MelonLogger.Msg("fi: " + fi);
                    intPtr = (IntPtr) fi.GetValue(null);
                    MelonUtils.NativeHookAttach(intPtr,
                        new System.Action<IntPtr, IntPtr>(Debug5).Method.MethodHandle.GetFunctionPointer());
                    MelonUtils.NativeHookAttach(intPtr,
                        new System.Action<IntPtr, IntPtr, IntPtr>(Debug6).Method.MethodHandle.GetFunctionPointer());
                    MelonUtils.NativeHookAttach(intPtr,
                        new System.Action<IntPtr, IntPtr, IntPtr, IntPtr>(Debug7).Method.MethodHandle.GetFunctionPointer());
                }
                else {
                    MelonLogger.Error("Failed to get field info for LoadUrl!");
                }
            }
            catch (System.Exception e) {
                MelonLogger.Error(e);
            }
              
            intPtr = (IntPtr) typeof(VRCAVProVideoPlayer).GetField("NativeMethodInfoPtr_Stop_Public_Virtual_Void_0",
                BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            MelonUtils.NativeHookAttach(intPtr, new System.Action<IntPtr, IntPtr>(Debug5).Method.MethodHandle.GetFunctionPointer()); 
            // InputField onSelect
            /*
            intPtr = (IntPtr) typeof(InputField).GetField("NativeMethodInfoPtr_OnSelect_Public_Virtual_Void_BaseEventData_0",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Default).GetValue(null);
            MelonUtils.NativeHookAttach(intPtr, new System.Action<IntPtr, IntPtr>(OnSelect).Method.MethodHandle.GetFunctionPointer());
            _onSelect = Marshal.GetDelegateForFunctionPointer<InputFieldInstanceDelegate>(*(IntPtr*) (void*) intPtr);
            */
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
        
        
        private static void Debug(IntPtr instance, IntPtr newUrl, IntPtr methodInfo) { 
            MelonLogger.Warning("Debug called");
        }
        
        private static void Debug2(IntPtr instance, IntPtr methodInfo) { 
            MelonLogger.Warning("Debug 2 called");
        }
        
        private static void Debug3(IntPtr instance, IntPtr methodInfo) { 
            MelonLogger.Warning("Debug 3 called");
        }
        
        private static void Debug4(IntPtr instance, IntPtr methodInfo) { 
            MelonLogger.Warning("Debug 4 called");
        }
        
        private static void Debug5(IntPtr instance, IntPtr methodInfo) { 
            MelonLogger.Warning("Debug 5 called");
        }
        
        private static void Debug6(IntPtr instance, IntPtr newUrl, IntPtr methodInfo) { 
            MelonLogger.Warning("Debug 6 called");
        }
        private static void Debug7(IntPtr instance, IntPtr newUrl, IntPtr methodInfo, IntPtr a) { 
            MelonLogger.Warning("Debug 7 called");
        }
            
            
    }
}