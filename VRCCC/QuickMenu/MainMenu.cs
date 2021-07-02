using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace VRCCC.QuickMenu
{
    public class MainMenu
    {
        private GameObject _menuPrefab;
        private GameObject _menuObject;
        private Button _searchButton;
        private Button _selectButton;
        private Button _negativeSButton;
        private Button _negativeHalfSButton;
        private Button _positiveHalfSButton;
        private Button _positiveSButton;
        private Text _resultText;
        private Text _currentOffsetText;
            
        
        public MainMenu(Transform parentMenuTransform, AssetBundle bundle) { 
            _menuPrefab = bundle.LoadAsset<GameObject>("Assets/AssetBundles/VRCCC-UI.prefab");
            if (_menuPrefab == null) MelonLogger.Error("Failure when trying to get menuPrefab");
            _menuObject = Object.Instantiate(_menuPrefab, parentMenuTransform, true);
            _menuObject.transform.localPosition  = Vector3.zero;
            _menuObject.transform.localScale     = Vector3.oneVector;
            _menuObject.transform.localRotation  = new Quaternion(0, 0, 0, 1);
            
            try { 
                Button[] items = _menuObject.GetComponentsInChildren<Button>();
                foreach (Button button in items) { 
                    if (button.name == "SearchButton")  { _searchButton = button; continue; }
                    if (button.name == "SelectButton")  { _selectButton = button; continue; }
                    if (button.name == "NegativeS")     { _negativeSButton = button; continue; }
                    if (button.name == "NegativeHalfS") { _negativeHalfSButton = button; continue; }
                    if (button.name == "PositiveHalfS") { _positiveHalfSButton = button; continue; }
                    if (button.name == "PositiveS")     { _positiveSButton = button; }
                }
                Text[] texts = _menuObject.GetComponentsInChildren<Text>();
                foreach (Text text in texts) { 
                    if (text.name == "ResultsAreaText") { _resultText = text; }
                    if (text.name == "CurrentOffset")   { _currentOffsetText = text; }
                }
            } catch (Exception e) { 
                MelonLogger.Error($"Error while trying to get individual components in the menu prefab. {e}.");
            }
            if (_searchButton == null) MelonLogger.Error("Unable to get Search Button");
            
            _searchButton.onClick.RemoveAllListeners();
            _searchButton.onClick.AddListener((Action)(() => { 
                MelonLogger.Msg("Clicked search!");
            }));
        }
        
        private void OnClick() { 
        }
    }
}