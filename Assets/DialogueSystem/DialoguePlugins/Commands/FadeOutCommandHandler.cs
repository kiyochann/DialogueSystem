using System;
using System.Collections;
using UnityEngine;

namespace Runtime.Dialogue.Commands
{
    /// <summary>
    /// [fade_out] コマンドを処理するプラグイン
    /// </summary>
    public class FadeOutCommandHandler : MonoBehaviour, IDialogueCommandHandler
    {
        public string TargetCommandName => "fade_out";

        private void Start()
        {
            // ゲーム開始時にディスパッチャへ自身を登録
            if (DialogueEventDispatcher.Instance != null)
            {
                DialogueEventDispatcher.Instance.RegisterHandler(this);
            }
        }

        public void Execute(DialogueCommand command, Action onComplete)
        {
            // DialogueCommandの新しい便利メソッドを使って、パラメータ(time)を取得
            float duration = command.GetFloat("time", 1.0f);
            StartCoroutine(FadeRoutine(duration, onComplete));
        }

        public void ForceComplete(DialogueCommand command)
        {
            // スキップされたら進行中のコルーチンを止め、一瞬で最終状態にする
            StopAllCoroutines();
            Debug.Log("[FadeOut] 画面を【一瞬で真っ暗】にします（演出ワープ）");

            // ※ここに実際の「画面を真っ黒にする処理」を書く
        }

        private IEnumerator FadeRoutine(float duration, Action onComplete)
        {
            Debug.Log($"[FadeOut] 暗転開始。{duration}秒 かけて実行中...");

            // ※ここに実際の「画面を徐々に黒くする処理」を書く
            yield return new WaitForSeconds(duration);

            Debug.Log($"[FadeOut] 暗転完了。");
            onComplete?.Invoke();
        }
    }
}