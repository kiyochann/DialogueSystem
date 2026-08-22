using System;
using UnityEngine;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Logic;

namespace Runtime.Dialogue.Plugins.Commands
{
    [HandlerInfo(description: "ゲーム内のフラグ・変数を設定します。", usage: "[set_flag:key=変数名,val=数値]")]
    public class SetFlagCommandHandler : MonoBehaviour, IDialogueCommandHandler
    {
        public string TargetCommandName => "set_flag";

        private void Start()
        {
            if (DialogueEventDispatcher.Instance != null)
                DialogueEventDispatcher.Instance.RegisterHandler(this);
        }

        public void Execute(DialogueCommand command, Action onComplete)
        {
            string key = command.GetString("key", "");
            int val = command.GetInt("val", 1);

            if (!string.IsNullOrEmpty(key) && FlagManager.Instance != null)
            {
                FlagManager.Instance.SetFlag(key, val);
                Debug.Log($"[FlagManager] フラグ更新: {key} = {val}");
            }

            onComplete?.Invoke(); // 一瞬で終わるので即座に次へ
        }

        public void ForceComplete(DialogueCommand command)
        {
            // スキップ時も確実にフラグはセットする必要がある
            Execute(command, null);
        }
    }
}