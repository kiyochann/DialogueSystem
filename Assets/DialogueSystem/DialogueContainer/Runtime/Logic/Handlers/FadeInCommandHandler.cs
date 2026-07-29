using System;
using System.Collections;
using UnityEngine;

namespace Runtime.Dialogue.Commands
{
    /// <summary>
    /// [fade_in] コマンドを処理するプラグイン
    /// </summary>
    public class FadeInCommandHandler : MonoBehaviour, IDialogueCommandHandler
    {
        public string TargetCommandName => "fade_in";

        private void Start()
        {
            if (DialogueEventDispatcher.Instance != null)
            {
                DialogueEventDispatcher.Instance.RegisterHandler(this);
            }
        }

        public void Execute(DialogueCommand command, Action onComplete)
        {
            float duration = command.GetFloat("time", 1.0f);
            StartCoroutine(FadeRoutine(duration, onComplete));
        }

        public void ForceComplete(DialogueCommand command)
        {
            StopAllCoroutines();
            Debug.Log("[FadeIn] 画面を【一瞬で通常表示】に戻します（演出ワープ）");
        }

        private IEnumerator FadeRoutine(float duration, Action onComplete)
        {
            Debug.Log($"[FadeIn] 明転開始。{duration}秒 かけて実行中...");
            yield return new WaitForSeconds(duration);
            Debug.Log($"[FadeIn] 明転完了。");
            onComplete?.Invoke();
        }
    }
}