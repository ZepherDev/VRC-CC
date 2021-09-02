using System;
using UnityEngine;
using UnityEngine.UI;

namespace VRCCC.QuickMenu
{
    public static class PopupManager
    {
        private static VRCUiPopupManager _popupManager => VRCUiPopupManager.prop_VRCUiPopupManager_0;
        
        public static void CreateKeyboardPopup(string title, string defaultValue, InputField.InputType inputType,
            bool useNumeric, string submitButtonText, Action<string> onSubmitted, Action onCancelled,
            string initialText,
            bool closeAfterSubmit) => _popupManager
            .Method_Public_Void_String_String_InputType_Boolean_String_Action_3_String_List_1_KeyCode_Text_Action_String_Boolean_Action_1_VRCUiPopup_Boolean_Int32_0(
                title, defaultValue, inputType, useNumeric, submitButtonText,
                new Action<string, Il2CppSystem.Collections.Generic.List<KeyCode>, Text>((s, list, arg3) =>
                    onSubmitted.Invoke(s)),
                onCancelled, initialText, closeAfterSubmit, new Action<VRCUiPopup>((s) => { }), false, 1024);
    }
}