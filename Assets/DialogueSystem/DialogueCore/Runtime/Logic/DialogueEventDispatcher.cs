using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Dialogue
{
    /// <summary>
    /// 各コマンドの実行を、登録されたハンドラーに丸投げするディスパッチャ
    /// </summary>
    public class DialogueEventDispatcher : MonoBehaviour
    {
        public static DialogueEventDispatcher Instance { get; private set; }

        // コマンド名と、それを処理するクラス(ハンドラー)の辞書
        private Dictionary<string, IDialogueCommandHandler> handlers = new Dictionary<string, IDialogueCommandHandler>();

        private void Awake()
        {
            if (Instance == null) { Instance = this; }
            else { Destroy(gameObject); }
        }

        /// <summary>
        /// 外部のスクリプトから新しい演出コマンドをシステムに登録する
        /// </summary>
        public void RegisterHandler(IDialogueCommandHandler handler)
        {
            string key = handler.TargetCommandName.ToLower();
            if (!handlers.ContainsKey(key))
            {
                handlers.Add(key, handler);
            }
        }

        public void ExecuteCommand(DialogueCommand command, Action onComplete)
        {
            string key = command.CommandName;

            if (handlers.TryGetValue(key, out var handler))
            {
                handler.Execute(command, () =>
                {
                    // ハンドラ側が完了を通知したタイミングでフラグを立てる
                    command.IsExecuted = true;
                    onComplete?.Invoke();
                });
            }
            else
            {
                Debug.LogWarning($"[Dialogue] 未登録のコマンド '{key}' が来ました。");
                command.IsExecuted = true;
                onComplete?.Invoke();
            }
        }

        public void ForceCompleteCommand(DialogueCommand command)
        {
            if (command.IsExecuted) return;
            command.IsExecuted = true;
            string key = command.CommandName;

            if (handlers.TryGetValue(key, out var handler))
            {
                handler.ForceComplete(command);
            }
        }
    }
}
