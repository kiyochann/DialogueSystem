using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Logic;

namespace Runtime.Dialogue.Plugins.Portraits
{
    [Serializable]
    public class PortraitSlot
    {
        [Tooltip("コマンドで指定する位置（例: left, center, right）")]
        public string positionID;
        [Tooltip("対象となるUIのImageコンポーネント")]
        public Image portraitImage;
    }

    [HandlerInfo("立ち絵を変更します", "使い方: [portrait:target=hero,exp=smile,pos=left]")]
    public class StandardUIPortraitHandler : MonoBehaviour, IDialogueCommandHandler
    {
        public string TargetCommandName => "portrait";

        [Header("Character Profiles (キャラクターデータ)")]
        public List<CharacterProfile> profiles = new List<CharacterProfile>();

        [Header("UI Slots (表示位置ごとのImage設定)")]
        public List<PortraitSlot> portraitSlots = new List<PortraitSlot>();

        private void Start()
        {
            // 開始時にコマンドシステムへ直接登録
            if (DialogueEventDispatcher.Instance != null)
            {
                DialogueEventDispatcher.Instance.RegisterHandler(this);
            }

            // 初期状態では立ち絵を透明にしておく
            foreach (var slot in portraitSlots)
            {
                if (slot.portraitImage != null) slot.portraitImage.color = new Color(1, 1, 1, 0);
            }
        }

        public void Execute(DialogueCommand command, Action onComplete)
        {
            // targetが指定されていない場合は、現在話しているキャラを対象にする（★直感的な操作のための工夫）
            string targetID = command.GetString("target", "");
            if (string.IsNullOrEmpty(targetID) && DialogueManager.Instance != null)
            {
                targetID = DialogueManager.Instance.CurrentSpeaker; // ※後述のステップ3で追加します
            }

            string expression = command.GetString("exp", "normal");
            string position = command.GetString("pos", "center");

            ApplyPortrait(targetID, expression, position);
            onComplete?.Invoke();
        }

        public void ForceComplete(DialogueCommand command)
        {
            Execute(command, null);
        }

        private void ApplyPortrait(string targetID, string expression, string position)
        {
            // まず指定された表示位置(Slot)を探す
            var slot = portraitSlots.Find(s => s.positionID == position);
            if (slot == null || slot.portraitImage == null) return;

            // ★追加: 表情に "clear" または "none" が指定されたら、その位置の画像を非表示にして終了する
            if (expression.ToLower() == "clear" || expression.ToLower() == "none")
            {
                slot.portraitImage.gameObject.SetActive(false);
                return;
            }

            // 通常の表示処理
            var profile = profiles.Find(p => p.characterID == targetID);
            var sprite = profile?.GetExpression(expression);

            if (sprite != null)
            {
                slot.portraitImage.sprite = sprite;
                slot.portraitImage.color = new Color(1, 1, 1, 1);
                slot.portraitImage.gameObject.SetActive(true);
            }
        }
    }
}