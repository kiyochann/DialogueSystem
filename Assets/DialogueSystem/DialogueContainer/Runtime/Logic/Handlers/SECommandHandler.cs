using System;
using UnityEngine;

namespace Runtime.Dialogue.Commands
{
    /// <summary>
    /// [se] コマンドを処理するプラグイン
    /// </summary>
    public class SECommandHandler : MonoBehaviour, IDialogueCommandHandler
    {
        public string TargetCommandName => "se";

        private void Start()
        {
            if (DialogueEventDispatcher.Instance != null)
            {
                DialogueEventDispatcher.Instance.RegisterHandler(this);
            }
        }

        public void Execute(DialogueCommand command, Action onComplete)
        {
            string clipName = command.GetString("clip", "default_se");
            Debug.Log($"🔊 [SE] 再生: {clipName}");

            // SEの再生指示は一瞬で終わるため、待機せずにすぐ完了通知を返す
            onComplete?.Invoke();
        }

        public void ForceComplete(DialogueCommand command)
        {
            string clipName = command.GetString("clip", "default_se");
            Debug.Log($"🔊 [SE-Skip] スキップ時ですが音を鳴らします: {clipName}");
        }
    }
}