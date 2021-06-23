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
            var baseUserInterface = GameObject.Find("UserInterface/UnscaledUI/HudContent/Hud").transform;
            var newText = Object.Instantiate(baseUserInterface.FindChild("AlertTextParent"), baseUserInterface); 
            
            newText.name = "VRCCC Text";
            newText.localPosition = new Vector3(0, -350, 0);
            TextComponent = newText.FindChild("Text").GetComponent<Text>();
            
            //TODO: Perhaps scale the font size depending on how much text is being rendered
            TextComponent.fontSize = 25;
        }
    }
}