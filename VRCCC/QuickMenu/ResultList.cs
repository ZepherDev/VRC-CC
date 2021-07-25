using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace VRCCC.QuickMenu
{
    public class ResultList : List<Subtitle>
    {
        private readonly Transform _targetTransform, _resultTemplate;
        
        public ResultList(Transform targetContent, Transform itemTemplate)
        {
            _targetTransform = targetContent;
            _resultTemplate = itemTemplate;
        }

        public new void Add(Subtitle item)
        {
            base.Add(item);

            var newSubtitleObject = Object.Instantiate(_resultTemplate, _targetTransform);
            foreach (Text text in newSubtitleObject.GetComponentsInChildren<Text>())
            {
                if (text.name != "ResultText") continue;
                
                text.text = $"{item.MovieName} " +
                            $"{(item.SubHearingImpaired ? "(Hearing Impaired)" : "")} " +
                            $"({item.MovieYear})";
                MelonLogger.Msg($"Setting result text to {text.text}");
                break;
            }
            Button button = newSubtitleObject.GetComponentInChildren<Button>();
            if (button == null) { 
                MelonLogger.Error("Unable to get Button from instantiated result object.");
                return;
            }
            
            button.onClick.AddListener((Action)(() => { MelonCoroutines.Start(MainMenu.SelectButtonClick(item)); }));
            newSubtitleObject.gameObject.SetActive(true);
        }

        public new void Clear()
        {
            base.Clear();
            
            foreach (Transform child in _targetTransform)
                if (child.name != "ResultObject")
                    Object.DestroyImmediate(child);
        }
    }
}