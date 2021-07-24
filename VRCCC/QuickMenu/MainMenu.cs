using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private Subtitle _subtitle;
        
        private bool _initSucc = false;
        
        public static InputField _inputField;
        public static VRCUiPopupManager _vrcUiPopupManager;
        
        public MainMenu(Transform parentMenuTransform, AssetBundle bundle) { 
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
                foreach (Text text in texts) 
                    if (text.name == "CurrentOffset")   { _currentOffsetText = text; }
                
                _inputField = _menuObject.GetComponentInChildren<InputField>();
                _listContent = _menuObject.transform.Find(
                    "Pages/MainPage/ResultsArea/ScrollList/ListViewport/ListContent").gameObject;
                _resultItem = _menuObject.transform.Find(
                    "Pages/MainPage/ResultsArea/ScrollList/ListViewport/ListContent/ResultObject").gameObject;
                _vrcUiPopupManager = VRCUiPopupManager.prop_VRCUiPopupManager_0;
            } catch (Exception e) { 
                MelonLogger.Error($"Error while trying to get individual components in the menu prefab. {e}.");
                return;
            }
            // Init complete
            //
            // Verify init
            if (_searchButton == null) { MelonLogger.Error("Unable to get Search Button"); return; }
            if (_listContent == null) { MelonLogger.Error("Unable to get the list content GameObject"); return; }
            if (_resultItem == null) { MelonLogger.Error("Unable to get the result item"); return; }
            
            _initSucc = true;
        }
        
        /**
         * Sets up the UI buttons for changing the offset, performing the search, and reverting to the default results.
         * When a search is successfully performed, that handler will set up the dynamically created Text and Button
         * objects that display the results and lets the user change to a new subtitle.
         */
        private void SetupStaticButtons() { 
            if (!_initSucc) return;
            _searchButton.onClick.AddListener((Action) ( () => { MelonCoroutines.Start(DoSearch()); }));
            _negativeSButton.onClick.AddListener( (Action)    ( () => { OffsetButtonClick(-1000); }));
            _negativeHalfSButton.onClick.AddListener((Action) ( () => { OffsetButtonClick(-500); }));
            _positiveHalfSButton.onClick.AddListener((Action) ( () => { OffsetButtonClick(500); }));
            _positiveSButton.onClick.AddListener((Action)     ( () => { OffsetButtonClick(1000); }));
        }
        
        private void OffsetButtonClick(int offset) { 
            if (VRCCC.TrackedPlayers.Count <= 0) return;
            VRCCC.TrackedPlayers[0].IncrementOrDecrementOffset(offset);
            _currentOffsetText.text = VRCCC.TrackedPlayers[0].GetCurrentOffsetMs().ToString();
        }
        
        private IEnumerator DoSearch() { 
            if (_inputField.text != "") {
                ClearResultList();
                
                MelonLogger.Msg($"Searching for {_inputField.text}");
                Task<Subtitle> task = SubtitlesApi.QuerySubtitle(_inputField.text, true);
                yield return new WaitUntil((Func<bool>)(() => task.IsCompleted));

                _subtitle = task.Result;
                MelonLogger.Msg($"{_subtitle.Alternatives.Count} results in the list of alternatives.");
                foreach (Subtitle subtitle in _subtitle.Alternatives.Take(6))
                    SetupResultAndSetParent(subtitle);
            }
        }
        
        private void SetupResultAndSetParent(Subtitle subtitle) { 
            if (subtitle != null) { 
                MelonLogger.Msg($"Handling returned result for {subtitle.MovieName}.");
                try { 
                    GameObject result = CreateResultObject(subtitle);
                    VRCCC.MainThreadExecutionQueue.Add( () => { 
                        result.transform.SetParent(_listContent.transform, false);
                        MelonLogger.Msg("Parent set.");
                    });
                } catch (Exception e) { 
                    MelonLogger.Error($"Exception when trying to create a result object {e}");
                } 
            }
        }
        
        private void ClearResultList() { 
            if (!_initSucc) return;
            // TODO: Delete child game objects.
            // foreach (var child in _listContent.transform) { 
                // GameObject.Destroy(((Transform)child).gameObject);
            // }
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
                                $"({subtitle.MovieYear})";
                    MelonLogger.Msg($"Setting result text to {text.text}");
                    break;
                }
            }
            Button button = newResult.GetComponentInChildren<Button>();
            if (button == null) { 
                MelonLogger.Error("Unable to get Button from instantiated result object.");
                return null;
            }
            
            button.onClick.AddListener((Action)(() => { MelonCoroutines.Start(SelectButtonClick(subtitle)); }));
            newResult.gameObject.SetActive(true);
            
            return newResult;
        }
        
        private IEnumerator SelectButtonClick(Subtitle subtitle) { 
            if (subtitle != null && VRCCC.TrackedPlayers != null && VRCCC.TrackedPlayers.Count >= 0) { 
                Task<string> task = SubtitlesApi.FetchSub(subtitle.SubDownloadLink);
                yield return new WaitUntil((Func<bool>)(() => task.IsCompleted));
                
                string srtString = task.Result;
                List<TimelineEvent> timelineEvents = SRTDecoder.DecodeSrtIntoTimelineEvents(srtString);
                // TODO: We need a way of knowing which TrackedPlayer is the right one
                VRCCC.TrackedPlayers[0].UnsafeSwapTimeline(subtitle.MovieName, timelineEvents);
            }
        }
        
        public static void GetMovieNameWithPopupKeyboard() { 
            if (_vrcUiPopupManager == null) { 
                MelonLogger.Error("Unable to get the UIPopupManager!");
                return;
            }
            
            EventSystem.current.SetSelectedGameObject(null);
             _vrcUiPopupManager.Method_Public_Void_String_String_InputType_Boolean_String_Action_3_String_List_1_KeyCode_Text_Action_String_Boolean_Action_1_VRCUiPopup_Boolean_Int32_0(
                "Search Subtitles", // title
                _inputField.text, // default value
                InputField.InputType.Standard,
                false, // numeric keypad
                "OK", // OK button text
                
                
                new Action<string, Il2CppSystem.Collections.Generic.List<KeyCode>, Text>((s, list, arg3) => {
                   _inputField.text = s;
                    EventSystem.current.SetSelectedGameObject(null);
                }), 
                new Action(() => { 
                    MelonLogger.Msg("Cancel pressed");
                }),
                "Enter movie name...", 
                true, // close after OK
                new Action<VRCUiPopup> ( (s) => { 
                    MelonLogger.Msg("Opened or maybe search button clicked"); 
                    EventSystem.current.SetSelectedGameObject(null);
                }),
                false,// multiline/enter deselects input
                1024 // char limit
                );
        }
    }
}