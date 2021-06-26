using UnityEngine;

namespace VRCCC.QuickMenu
{
    public class MainMenu
    {
        public MainMenu(Transform parentMenuTransform, AssetBundle bundle) { 
            var menuPrefab = bundle.LoadAsset<GameObject>("VRC-CC");
            var menuObject = Object.Instantiate(menuPrefab, parentMenuTransform, true);
            menuObject.transform.localPosition  = Vector3.zero;
            menuObject.transform.localScale     = Vector3.oneVector;
            menuObject.transform.localRotation  = new Quaternion(0, 0, 0, 1);
            
            foreach (var sprite in Resources.FindObjectsOfTypeAll<Sprite>()) {
                switch (sprite.name) { 
                    case "UI_ButtonToggleBottom_Bifrost":
                        ToggleButton.ToggleDown = sprite;
                        break;
                    case "UI_ButtonToggleTop_Bifrost":
                        ToggleButton.ToggleUp = sprite;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}