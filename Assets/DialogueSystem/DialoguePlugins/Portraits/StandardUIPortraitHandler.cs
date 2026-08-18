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
        [Tooltip("コマンドで指定する位置 例: left, center, right")]
        public string positionID;
        [Tooltip("対象となるUIのImageコンポーネント")]
        public Image portraitImage;
    }

    [HandlerInfo(description: "指定したキャラクターの表情と表示位置を切り替えます。", usage: "[portrait:target=hero,exp=smile,pos=center]")]
    public class StandardUIPortraitHandler : MonoBehaviour, IDialogueCommandHandler
    {
        // IDialogueCommandHandler の実装（TargetCommandName を "portrait" にする）
        public string TargetCommandName => "portrait";

        [Header("Character Profiles (キャラクターデータ)")]
        public List<CharacterProfile> profiles = new List<CharacterProfile>();

        [Header("UI Slots (表示位置ごとのImage設定)")]
        public List<PortraitSlot> portraitSlots = new List<PortraitSlot>();

        private void Start()
        {
            // DialogueEventDispatcher に自分自身を「portrait」コマンドの担当として登録する
            if (DialogueEventDispatcher.Instance != null)
            {
                DialogueEventDispatcher.Instance.RegisterHandler(this);
            }

            // 初期状態ではすべての立ち絵を透明にしておく
            foreach (var slot in portraitSlots)
            {
                if (slot.portraitImage != null)
                {
                    slot.portraitImage.color = new Color(1, 1, 1, 0);
                }
            }
        }

        // --- 通常時のコマンド実行 ---
        public void Execute(DialogueCommand command, Action onComplete)
        {
            // DialogueCommand の便利メソッドを使って引数を安全に取得
            string targetID = command.GetString("target", "");
            string expression = command.GetString("exp", "default");
            string position = command.GetString("pos", "center");

            bool success = ApplyPortrait(targetID, expression, position);
            if (!success)
            {
                Debug.LogWarning($"[StandardUIPortraitHandler] 立ち絵の適用に失敗しました (target:{targetID}, exp:{expression}, pos:{position})");
            }

            // 即時完了扱いにする場合はすぐに呼ぶ（フェード等を挟む場合はアニメーション終了後に呼ぶ）
            onComplete?.Invoke();
        }

        // --- スキップ/早送り時の強制完了 ---
        public void ForceComplete(DialogueCommand command)
        {
            // スキップ時も一瞬で同じ処理を適用する
            string targetID = command.GetString("target", "");
            string expression = command.GetString("exp", "default");
            string position = command.GetString("pos", "center");

            ApplyPortrait(targetID, expression, position);
        }

        // 実際の画像切り替え処理
        private bool ApplyPortrait(string targetID, string expression, string position)
        {
            var profile = profiles.Find(p => p.characterID == targetID);
            if (profile == null) return false;

            var sprite = profile.GetExpression(expression);
            if (sprite == null) return false;

            var slot = portraitSlots.Find(s => s.positionID == position);
            if (slot == null || slot.portraitImage == null) return false;

            slot.portraitImage.sprite = sprite;
            slot.portraitImage.color = new Color(1, 1, 1, 1);
            slot.portraitImage.gameObject.SetActive(true);

            return true;
        }
    }
}