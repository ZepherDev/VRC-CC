using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace VRCCC.QuickMenu
{
    public class QuickModeMenu
    {
        private static bool _hasInitMenu; 
        private static MonoBehaviourPublicObCoGaCoObCoObCoUnique _qmTabManager = null;
        private static int _tabIndex;
        
        public static bool IsMenuShown => 
            (int)_qmTabManager.field_Private_EnumNPublicSealedvaHoNoPl4vUnique_0 == _tabIndex;
        public static MainMenu MainMenu;
        
        public static void InitIfNeeded() { 
            if (!_hasInitMenu)
                InitializeMenu();
        }
        
        private static void InitializeMenu() { 
            try { 
                CreateNotificationTab("VRCCC", "Text", Color.black); 
                _hasInitMenu = true;
            } catch (Exception e) { 
                MelonLogger.Error("Exception while trying to create the notification tab! Aborting. " + e);
            }
        }
        
        private static void CreateNotificationTab(string name, string text, Color color) { 
            var bundle = AssetBundle.LoadFromMemory(ExtractAb());
            _qmTabManager = Resources.FindObjectsOfTypeAll<MonoBehaviourPublicObCoGaCoObCoObCoUnique>()[0];
            List<GameObject> existingTabs = _qmTabManager.field_Public_ArrayOf_GameObject_0.ToList();
            global::QuickMenu quickMenu = Resources.FindObjectsOfTypeAll<global::QuickMenu>()[0];
            
            // Tab
            var quickModeTabs = quickMenu.transform.Find("QuickModeTabs").
                GetComponent<MonoBehaviourPublicObCoGaCoObCoObCoUnique>();
            Transform newTab = Object.Instantiate(quickModeTabs.transform.Find("NotificationsTab"), 
                quickModeTabs.transform);
            newTab.name = name; 
            Object.DestroyImmediate(newTab.GetComponent<MonoBehaviourPublicGaTeSiSiUnique>());
            SetTabIndex(newTab, 
                (MonoBehaviourPublicObCoGaCoObCoObCoUnique.EnumNPublicSealedvaHoNoPl4vUnique)existingTabs.Count);
            newTab.Find("Badge").GetComponent<RawImage>().color = color;
            newTab.Find("Badge/NotificationsText").GetComponent<Text>().text = text;
            
            _tabIndex = existingTabs.Count;
            existingTabs.Add(newTab.gameObject);
            
            _qmTabManager.field_Public_ArrayOf_GameObject_0 = existingTabs.ToArray();
            
            newTab.Find("Icon").GetComponent<Image>().sprite = LoadQmSprite(bundle);
            
            // Menu
            Transform quickModeMenus = quickMenu.transform.Find("QuickModeMenus"); 
            RectTransform newMenu = new GameObject(name + "Menu", new[] { 
                    Il2CppType.Of<RectTransform>() }).GetComponent<RectTransform>();
            newMenu.SetParent(quickModeMenus, false);
            newMenu.anchorMin = new Vector2(0, 1);
            newMenu.anchorMax = new Vector2(0, 1);
            newMenu.sizeDelta = new Vector2(1680f, 1200f);
            newMenu.pivot = new Vector2(0.5f, 0.5f);
            newMenu.anchoredPosition = new Vector2(0, 200f);
            newMenu.gameObject.SetActive(false);
            
            MainMenu = new MainMenu(newMenu, bundle);
            
            // Tab interaction
            Button tabButton = newTab.GetComponent<Button>();
            tabButton.onClick.RemoveAllListeners();
            tabButton.onClick.AddListener((Action)(() =>
                {
                    global::QuickMenu.prop_QuickMenu_0.field_Private_GameObject_6.SetActive(false);
                    global::QuickMenu.prop_QuickMenu_0.field_Private_GameObject_6 = newMenu.gameObject;
                    newMenu.gameObject.SetActive(true);
                }));
            
            newTab.transform.FindChild("Badge").gameObject.SetActive(false);
            
            // Allow invite menu to instantiate
            quickModeMenus.Find("QuickModeNotificationsMenu").gameObject.SetActive(true);
            quickModeMenus.Find("QuickModeNotificationsMenu").gameObject.SetActive(false);
        }
        
        private static void SetTabIndex(Transform tab, 
            MonoBehaviourPublicObCoGaCoObCoObCoUnique.EnumNPublicSealedvaHoNoPl4vUnique value) {
            MonoBehaviour tabDescriptor = tab.GetComponents<MonoBehaviour>()
                .First(c => c.GetIl2CppType().GetMethod("ShowTabContent") != null);
            tabDescriptor.GetIl2CppType().GetFields().First(f => f.FieldType.IsEnum).
                SetValue(tabDescriptor,
                    new Il2CppSystem.Int32 { 
                        m_value = (int)value 
                    }.BoxIl2CppObject()
                ); // wut
        }
        
        private static Sprite LoadQmSprite(AssetBundle bundle) { 
            var names = bundle.GetAllAssetNames();
            foreach (var name in names) { 
                MelonLogger.Msg($"name: {name}");
            }
            var t = bundle.LoadAsset<Texture2D>("assets/vrc-cc_logo");
            if (t == null) MelonLogger.Error("t is null");
            var rect = new Rect(0.0f, 0.0f, t.width, t.height);
            var pivot = new Vector2(0.5f, 0.5f);
            var border = Vector4.zero;
            
            return Sprite.CreateSprite_Injected(t, ref rect, ref pivot, 100.0f, 0,
                SpriteMeshType.Tight, ref border, false);
            
        }
        
        private static byte[] ExtractAb() { 
            var a = Assembly.GetExecutingAssembly();    
            
            MelonLogger.Msg("Resource names");
            foreach (var name in a.GetManifestResourceNames()) { 
                MelonLogger.Msg($"\tname: {name}");
            }
            using (Stream resFilestream = a.GetManifestResourceStream("VRCCC.VRCCC")) { 
                if (resFilestream == null) { 
                    MelonLogger.Error("Failed to find the resource filestream");
                    return null;
                }
                var ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                MelonLogger.Msg($"ba length: {ba.Length}");
                return ba;
            }
        }
    
    }
}