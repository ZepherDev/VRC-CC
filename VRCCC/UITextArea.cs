using System.Collections;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace VRCCC
{
    /*
     * Main interface for displaying text on the user's UI.
     * Note that this class is static and is constructed only when needed
     * So uh, please don't need this before the UI Manager has finished initializing (though this should be impossible)
     */
    public static class UITextArea
    {
        private static readonly Text TextComponent;
        private static readonly Transform TextParent;

        public static string Text
        {
            get => TextComponent?.text ?? "";
            set
            {
                if (TextComponent != null)
                    TextComponent.text = value;
            }
        }

        static UITextArea()
        {
            // Hippity Hoppity, your UI elements are now my property
            var baseUserInterface = GameObject.Find("UserInterface/UnscaledUI/HudContent_Old/Hud").transform;
            TextParent = Object.Instantiate(baseUserInterface.FindChild("AlertTextParent"), baseUserInterface); 
            
            TextParent.name = "VRCCC Text";
            TextParent.localPosition = new Vector3(0, -350, 0);
            TextComponent = TextParent.FindChild("Text").GetComponent<Text>();
            TextComponent.supportRichText = true;
            
            //TODO: Perhaps scale the font size depending on how much text is being rendered
            TextComponent.fontSize = 25;
        }

        public static IEnumerator DisplayAlert(string text, float timeInSeconds)
        {
            MelonLogger.Msg(text);
            Text = text;
            ToggleUI(true);
            yield return new WaitForSeconds(timeInSeconds);
            ToggleUI(false);
            Text = "";
        }
        
        public static void ToggleUI(bool newShownState) => TextParent.gameObject.SetActive(newShownState);
    }
}