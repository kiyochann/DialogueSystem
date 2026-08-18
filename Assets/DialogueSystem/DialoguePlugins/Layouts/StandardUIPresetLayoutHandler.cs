using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Logic;

namespace Runtime.Dialogue.Plugins.Layouts
{
    [HandlerInfo("レイアウトを変更します", "使い方: [layout:name=Narration]")]
    public class StandardUIPresetLayoutHandler : MonoBehaviour, IDialogueCommandHandler
    {
        public string TargetCommandName => "layout";

        [Header("Layout Presets (レイアウトパターンの設定)")]
        public List<DialogueLayoutPreset> presets = new List<DialogueLayoutPreset>();

        [Header("UI References (操作対象のUI)")]
        public GameObject backgroundBox;
        public GameObject nameBox;
        public RectTransform textRectTransform;
        public GameObject portraitRoot;

        private void Start()
        {
            // 開始時にコマンドシステムへ直接登録
            if (DialogueEventDispatcher.Instance != null)
            {
                DialogueEventDispatcher.Instance.RegisterHandler(this);
            }
        }

        public void Execute(DialogueCommand command, Action onComplete)
        {
            string layoutName = command.GetString("name", "Normal");
            ApplyLayout(layoutName);
            onComplete?.Invoke();
        }

        public void ForceComplete(DialogueCommand command)
        {
            Execute(command, null);
        }

        private void ApplyLayout(string layoutName)
        {
            var preset = presets.Find(p => p.layoutName == layoutName);
            if (preset == null) return;

            if (backgroundBox != null) backgroundBox.SetActive(preset.showBackgroundBox);
            if (nameBox != null) nameBox.SetActive(preset.showNameBox);
            if (portraitRoot != null) portraitRoot.SetActive(!preset.hidePortraits);

            if (preset.overrideTextRect && textRectTransform != null)
            {
                textRectTransform.offsetMin = preset.textOffsetMin;
                textRectTransform.offsetMax = preset.textOffsetMax;
            }
        }
    }
}