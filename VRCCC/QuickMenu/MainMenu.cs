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
        private GameObject _menuObject;
        private Button _searchButton;
        private Button _selectButton;
        private Button _negativeSButton;
        private Button _negativeHalfSButton;
        private Button _positiveHalfSButton;
        private Button _positiveSButton;
        private Text _resultText;
        private Text _currentOffsetText;
        private InputField _inputField;
        private Subtitle _subtitle;
        
        public MainMenu(Transform parentMenuTransform, AssetBundle bundle) { 
            GameObject menuPrefab = bundle.LoadAsset<GameObject>("Assets/AssetBundles/VRCCC-UI.prefab");
            if (menuPrefab == null) MelonLogger.Error("Failure when trying to get menuPrefab");
            _menuObject = Object.Instantiate(menuPrefab, parentMenuTransform, true);
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
                    if (text.name == "MovieResult") { _resultText = text; continue; }
                    if (text.name == "CurrentOffset")   { _currentOffsetText = text; }
                }
                _inputField = _menuObject.GetComponentInChildren<InputField>();
            } catch (Exception e) { 
                MelonLogger.Error($"Error while trying to get individual components in the menu prefab. {e}.");
            }
            if (_searchButton == null) MelonLogger.Error("Unable to get Search Button");
            if (_resultText == null) MelonLogger.Error("Unable to get Results Text");
            
            _searchButton.onClick.AddListener((Action)(async () => { 
                if (_inputField.text != "") { 
                    MelonLogger.Msg($"Searching for {_inputField.text}");
                    try {
                        _subtitle = await SubtitlesApi.QuerySubtitles(_inputField.text);
                        if (_subtitle != null) { 
                            _resultText.text = $"Name: {_subtitle.MovieName}\n\tURL: {_subtitle.SubDownloadLink}" +
                                               $"\n\tLang: {_subtitle.LanguageName}" +
                                               $"\n\tScore: {_subtitle.Score}";
                        }
                    } catch (Exception e) { 
                        MelonLogger.Error($"Exception when trying to get new subtitles. {e}.");
                    }
                }
            }));
            
            _selectButton.onClick.AddListener((Action)(async () => { 
                if (_subtitle != null) { 
                    // string subFile = await SubtitlesApi.FetchSub(_subtitle.SubDownloadLink);
                    // List<TimelineEvent> subData = SRTDecoder.DecodeSrtIntoTimelineEvents(subFile);
                    MelonLogger.Msg($"Trying to switch to url {_subtitle.SubDownloadLink}");
                    string srtString = await SubtitlesApi.FetchSub(_subtitle.SubDownloadLink);
                    List<TimelineEvent> timelineEvents = SRTDecoder.DecodeSrtIntoTimelineEvents(srtString);
                    Timeline timeline = new Timeline(timelineEvents);
                    foreach (TrackedPlayer player in VRCCC.TrackedPlayers) { 
                        MelonLogger.Msg("Swapping to new timeline!");
                        player.UnsafeSwapTimeline(_subtitle.MovieName, timelineEvents);
                        MelonLogger.Msg($"Timeline has {timelineEvents.Count} events.");
                    }
                }
                // var foundPlayer = VRCCC.TrackedPlayers.Find(player => player.Equals(instance));
                // VRCCC.TrackedPlayers.Find
            }));
        }
        
        private void OnClick() { 
        }
    }
}