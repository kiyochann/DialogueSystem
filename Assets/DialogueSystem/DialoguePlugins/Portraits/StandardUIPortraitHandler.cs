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

    // 💡 変更点1: IDialogueCommandHandler ではなく、本来の IDialoguePortraitHandler を実装
    public class StandardUIPortraitHandler : MonoBehaviour, IDialoguePortraitHandler
    {
        // 💡 変更点2: 優先度（Priority）を実装。他のプラグインと競合した際の処理順を決定します。
        public int Priority => 0;

        [Header("Character Profiles (キャラクターデータ)")]
        public List<CharacterProfile> profiles = new List<CharacterProfile>();

        [Header("UI Slots (表示位置ごとのImage設定)")]
        public List<PortraitSlot> portraitSlots = new List<PortraitSlot>();

        private void Start()
        {
            // 💡 変更点3: EventDispatcherではなく、専用の PortraitDispatcher に自身を登録
            if (DialoguePortraitDispatcher.Instance != null)
            {
                DialoguePortraitDispatcher.Instance.RegisterHandler(this);
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

        // 💡 変更点4: コマンドの解析処理は PortraitCommandHandler に任せ、純粋な立ち絵の反映処理のみを受け取る
        public bool TryHandlePortrait(string targetID, string expression, string position, Dictionary<string, string> args, Action onComplete)
        {
            bool success = ApplyPortrait(targetID, expression, position);

            if (!success)
            {
                Debug.LogWarning($"[StandardUIPortraitHandler] 立ち絵の適用に失敗しました (target:{targetID}, exp:{expression}, pos:{position})");
            }

            // 画像の切り替え自体は一瞬で終わるため、即座にコールバックを呼ぶ
            // （フェードインやスライドインを実装する場合は、アニメーション終了後に onComplete を呼ぶようにします）
            onComplete?.Invoke();

            return success; // 処理が行われたかどうかをディスパッチャーに返す
        }

        // 💡 変更点5: スキップ時の処理もインターフェースに合わせて修正
        public void ForceCompletePortrait(string targetID, string expression, string position, Dictionary<string, string> args)
        {
            // スキップ時も一瞬で同じ処理を適用する
            ApplyPortrait(targetID, expression, position);
        }

        // 実際の画像切り替え処理（既存のまま）
        // 実際の画像切り替え処理
        private bool ApplyPortrait(string targetID, string expression, string position)
        {
            // 💡 修正: "clear" コマンドの処理（全消去と個別消去の分岐）
            if (targetID.ToLower() == "clear")
            {
                // 位置が "all" または未指定の場合はすべて消す
                if (string.IsNullOrEmpty(position) || position.ToLower() == "all")
                {
                    foreach (var s in portraitSlots)
                    {
                        if (s.portraitImage != null)
                        {
                            s.portraitImage.color = new Color(1, 1, 1, 0);
                            s.portraitImage.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    // 位置が指定されている場合は、そのスロット（例: left）だけを消す
                    var slotToClear = portraitSlots.Find(s => s.positionID == position);
                    if (slotToClear != null && slotToClear.portraitImage != null)
                    {
                        slotToClear.portraitImage.color = new Color(1, 1, 1, 0);
                        slotToClear.portraitImage.gameObject.SetActive(false);
                    }
                }
                return true; // クリア処理を実行したためここで終了
            }

            // --- ここから下は既存の表示処理と同じ ---
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