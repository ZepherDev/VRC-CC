using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace VRCCC.QuickMenu
{
    public class MainMenu
    {
        private GameObject _menuObject;
        private GameObject _resultItemPrefab;
        private GameObject _listContent;
        
        private Button _searchButton;
        private Button _negativeSButton;
        private Button _negativeHalfSButton;
        private Button _positiveHalfSButton;
        private Button _positiveSButton;
        
        private Text _currentOffsetText;
        private InputField _inputField;
        private List<Subtitle> _subtitles;
        private Font _font;
        
        public MainMenu(Transform parentMenuTransform, AssetBundle bundle) { 
            // Grab prefabs
            _resultItemPrefab = bundle.LoadAsset<GameObject>("Assets/AssetBundles/ResultObject.prefab");
            if (_resultItemPrefab == null) MelonLogger.Error("Failure when trying to get Result Object prefab");
            GameObject menuPrefab = bundle.LoadAsset<GameObject>("Assets/AssetBundles/VRCCC-UI.prefab");
            if (menuPrefab == null) MelonLogger.Error("Failure when trying to get menuPrefab");
            
            // Build menu
            _menuObject = Object.Instantiate(menuPrefab, parentMenuTransform, true);
            _menuObject.transform.localPosition  = Vector3.zero;
            _menuObject.transform.localScale     = Vector3.oneVector;
            _menuObject.transform.localRotation  = new Quaternion(0, 0, 0, 1);
            
            InitReferences();
            MelonLogger.Msg("UI: Grabbed references");
            SetupStaticButtons(); 
            MelonLogger.Msg("UI: set up buttons and finished init");
        }
        
        private void InitReferences() { 
            try { 
                Button[] items = _menuObject.GetComponentsInChildren<Button>();
                foreach (Button button in items) { 
                    if (button.name == "SearchButton")  { _searchButton = button; continue; }
                    if (button.name == "NegativeS")     { _negativeSButton = button; continue; }
                    if (button.name == "NegativeHalfS") { _negativeHalfSButton = button; continue; }
                    if (button.name == "PositiveHalfS") { _positiveHalfSButton = button; continue; }
                    if (button.name == "PositiveS")     { _positiveSButton = button; }
                }
                Text[] texts = _menuObject.GetComponentsInChildren<Text>();
                foreach (Text text in texts) { 
                    if (text.name == "CurrentOffset")   { _currentOffsetText = text; }
                }
                _inputField = _menuObject.GetComponentInChildren<InputField>();
                /*
                foreach (Transform transform in _menuObject.GetComponents<Transform>()) { 
                    if (transform.gameObject.name == "ListContent") { 
                        _listContent = transform.gameObject;
                    }
                }
                */
                Transform child = _menuObject.transform.FindChild("ListContent");
                if (child != null) _listContent = child.gameObject;
            } catch (Exception e) { 
                MelonLogger.Error($"Error while trying to get individual components in the menu prefab. {e}.");
            }
            if (_searchButton == null) MelonLogger.Error("Unable to get Search Button");
            if (_listContent == null) MelonLogger.Error("Unable to get the list content GameObject");
        }
        
        /**
         * Sets up the UI buttons for changing the offset, performing the search, and reverting to the default results.
         * When a search is successfully performed, that handler will set up the dynamically created Text and Button
         * objects that display the results and lets the user change to a new subtitle.
         */
        private void SetupStaticButtons() { 
            _searchButton.onClick.AddListener((Action)(async () => { 
                if (_inputField.text != "") { 
                    ClearResultList();
                    MelonLogger.Msg($"Searching for {_inputField.text}");
                    try {
                        _subtitles = await SubtitlesApi.QuerySubtitles(_inputField.text);
                        foreach (Subtitle subtitle in _subtitles) { 
                            if (subtitle != null) { 
                                MelonLogger.Msg($"Handling returned result for {subtitle.MovieName}.");
                                GameObject result = CreateResultObject(subtitle);
                                MelonLogger.Msg("Setting parent for result");
                                result.transform.parent = _listContent.transform;
                                
                            }
                        }
                    } catch (Exception e) { 
                        MelonLogger.Error($"Exception when trying to get new subtitles. {e}.");
                    }
                }
            }));
            
        }
        
        private void ClearResultList() { 
            if (_listContent == null) return;
            foreach (Transform child in _listContent.transform) { 
                GameObject.Destroy(child.gameObject);
            }
        }
       
        /**
         * Using the _resultItemPrefab retrieved earlier, instantiate a new result object and populate it with
         * the provided subtitle info. This also sets up a click handler that will change the currently playing
         * subtitles to the result.
         */
        private GameObject CreateResultObject(Subtitle subtitle) 
        {
            GameObject result = Object.Instantiate(_resultItemPrefab, _menuObject.transform, true);
            foreach (Text text in result.GetComponentsInChildren<Text>()) { 
                if (text.name == "ResultText") { 
                    text.text = $"Name: {subtitle.MovieName}\n\tURL: {subtitle.SubDownloadLink}" +
                                $"\n\tLang: {subtitle.LanguageName}" +
                                $"\n\tScore: {subtitle.Score}";
                    break;
                }
            }
            Button button = result.GetComponentInChildren<Button>();
            button.onClick.AddListener((Action)(async () => { 
                MelonLogger.Msg($"Clicked select button for {subtitle.MovieName}!");
            }));
            
            
            
            /*
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
            */
            
            return result;
        }
        
    }
}