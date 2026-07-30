using System;
using System.Collections;
using UnityEngine;

namespace Runtime.Dialogue.Commands
{
    /// <summary>
    /// [wait:time=秒数] 指定した秒数だけ文字送りを一時停止させるプラグイン
    /// </summary>
    public class WaitCommandHandler : MonoBehaviour, IDialogueCommandHandler
    {
        public string TargetCommandName => "wait";

        private void Start()
        {
            if (DialogueEventDispatcher.Instance != null)
                DialogueEventDispatcher.Instance.RegisterHandler(this);
        }

        public void Execute(DialogueCommand command, Action onComplete)
        {
            float duration = command.GetFloat("time", 1.0f);
            StartCoroutine(WaitRoutine(duration, onComplete));
        }

        public void ForceComplete(DialogueCommand command)
        {
            // スキップされたら待機を即座にやめる
            StopAllCoroutines();
        }

        private IEnumerator WaitRoutine(float duration, Action onComplete)
        {
            yield return new WaitForSeconds(duration);
            onComplete?.Invoke(); // ここでコールバックを呼ぶと、UI側の文字送りが再開される
        }
    }
}
