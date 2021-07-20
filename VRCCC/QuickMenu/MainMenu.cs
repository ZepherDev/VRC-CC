using System;
using System.Collections.Generic;
using System.Linq;
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
        private GameObject _listContent;
        private GameObject _resultItem;
        
        private Button _searchButton;
        private Button _negativeSButton;
        private Button _negativeHalfSButton;
        private Button _positiveHalfSButton;
        private Button _positiveSButton;
        
        private Text _currentOffsetText;
        private InputField _inputField;
        private Subtitle _subtitle;
        
        public MainMenu(Transform parentMenuTransform, AssetBundle bundle) { 
            // Grab prefabs
            GameObject menuPrefab = bundle.LoadAsset<GameObject>("Assets/AssetBundles/VRCCC-UI.prefab");
            if (menuPrefab == null) { 
                MelonLogger.Error("Failure when trying to get menuPrefab or resultPrefab");
                return; 
            }
            
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
                _listContent = _menuObject.transform.Find(
                    "Pages/MainPage/ResultsArea/ScrollList/ListViewport/ListContent").gameObject;
                _resultItem = _menuObject.transform.Find(
                    "Pages/MainPage/ResultsArea/ScrollList/ListViewport/ListContent/ResultObject").gameObject;
            } catch (Exception e) { 
                MelonLogger.Error($"Error while trying to get individual components in the menu prefab. {e}.");
            }
            if (_searchButton == null) MelonLogger.Error("Unable to get Search Button");
            if (_listContent == null) MelonLogger.Error("Unable to get the list content GameObject");
            if (_resultItem == null) MelonLogger.Error("Unable to get the result item");
        }
        
        /**
         * Sets up the UI buttons for changing the offset, performing the search, and reverting to the default results.
         * When a search is successfully performed, that handler will set up the dynamically created Text and Button
         * objects that display the results and lets the user change to a new subtitle.
         */
        private void SetupStaticButtons() { 
            _searchButton.onClick.AddListener((Action)(async () => { 
                if (_inputField.text != "") { 
                    // TODO: fix clearing previous list 
                    //ClearResultList()
                    MelonLogger.Msg($"Searching for {_inputField.text}");
                    try {
                        _subtitle = await SubtitlesApi.QuerySubtitle(_inputField.text, true);
                        MelonLogger.Msg($"{_subtitle.Alternatives.Count} results in the list of alternatives.");
                        foreach (Subtitle subtitle in _subtitle.Alternatives.Take(3)) { 
                            if (subtitle != null) { 
                                MelonLogger.Msg($"Handling returned result for {subtitle.MovieName}.");
                                try { 
                                    GameObject result = CreateResultObject(subtitle);
                                    MelonLogger.Msg("Setting parent for result");
                                    if (result == null) MelonLogger.Error("Failed to get result.");
                                    if (result.transform == null) MelonLogger.Error("Failed to get result's transform");
                                    if (_listContent == null) MelonLogger.Error("Somehow list content is now null.");
                                    if (_listContent.transform == null) MelonLogger.Error("List content's transform is null");
                                    VRCCC.MainThreadExecutionQueue.Add( () => { 
                                        result.transform.SetParent(_listContent.transform, false);
                                        MelonLogger.Msg("parent set.");
                                    });
                                } catch (Exception e) { 
                                    MelonLogger.Error($"Exception when trying to create a result object {e}");
                                } 
                                
                            }
                        }
                    } catch (Exception e) { 
                        MelonLogger.Error($"Exception when trying to get new subtitles. {e}.");
                    }
                }
            }));
            
        }
        
        private void ClearResultList() { 
            if (_listContent == null || _listContent.transform == null) return;
            // Leave the first item, which is used as a template and duplicated 
            for (int x=_listContent.transform.childCount - 1; x>1; ++x) { 
                GameObject.Destroy(_listContent.transform.GetChild(x).gameObject);
            }
        }
       
        /**
         * Using the _resultItemPrefab retrieved earlier, instantiates a new result object and populates it with
         * the provided subtitle info. This also sets up a click handler that will change the currently playing
         * subtitles to the result.
         */
        private GameObject CreateResultObject(Subtitle subtitle) 
        {
            if (_resultItem == null) {
                MelonLogger.Error("Somehow the result item prefab is null.");
                return null;
            }
            GameObject newResult = Object.Instantiate(_resultItem);
            if (newResult == null) {
                MelonLogger.Error("Unable to duplicate the result object");
                return null;
            }
                
            foreach (Text text in newResult.GetComponentsInChildren<Text>()) { 
                if (text.name == "ResultText") { 
                    text.text = $"{subtitle.MovieName} " +
                                $"{(subtitle.SubHearingImpaired ? "(Hearing Impaired)" : "")} " +
                                $"({subtitle.Score}) ({subtitle.Score})";
                    MelonLogger.Msg($"Setting result text to {text.text}");
                    break;
                }
            }
            Button button = newResult.GetComponentInChildren<Button>();
            if (button == null) { 
                MelonLogger.Error("Unable to get Button from instantiated result object.");
                return null;
            }
            
            button.onClick.AddListener((Action)(() => { SelectButtonClick(subtitle); }));
            newResult.gameObject.SetActive(true);
            
            return newResult;
        }
        
        private async void SelectButtonClick(Subtitle subtitle) { 
            if (subtitle == null) return;
            MelonLogger.Msg($"Clicked select button for {subtitle.MovieName}!");
                
            string srtString = await SubtitlesApi.FetchSub(subtitle.SubDownloadLink);
            List<TimelineEvent> timelineEvents = SRTDecoder.DecodeSrtIntoTimelineEvents(srtString);
            
            foreach (TrackedPlayer player in VRCCC.TrackedPlayers) { 
                MelonLogger.Msg("Swapping to new timeline!");
                player.UnsafeSwapTimeline(subtitle.MovieName, timelineEvents);
                MelonLogger.Msg($"Timeline has {timelineEvents.Count} events.");
            }
            
        }
        
    }
}