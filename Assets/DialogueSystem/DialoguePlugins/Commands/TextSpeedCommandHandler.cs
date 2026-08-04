/*
=========================================================
【AI生成用プロンプト・実装ルール】
AIアシスタントにこのスクリプトを渡して処理を実装させる場合は、以下のルールを厳守させてください。

1. [属性の付与] クラス直上に必ず `[HandlerInfo("コマンドの説明", "使い方: [コマンド名:引数=値]")]` を記述すること。
2. [必須実装] `IDialogueCommandHandler` インターフェースを実装すること。
3. [登録処理] `Start()` 内で `DialogueEventDispatcher.Instance.RegisterHandler(this);` を行うこと。
4. [引数取得] 引数は `command.GetString("key", default)` や `command.GetFloat()` などを用いて安全に取得すること。
5. [完了通知] 演出（コルーチンやTween等）が終了したら、最後に必ず `onComplete?.Invoke();` を呼び出すこと。
6. [スキップ対応] `ForceComplete()` 内では、即座に演出の最終状態を適用し、進行中のコルーチン等を停止すること。
=========================================================
*/
using System;
using UnityEngine;
using Runtime.Dialogue;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Logic;

namespace DialogueSystem.Plugins.Commands
{
    [HandlerInfo("文字の表示速度を変更します。", "使い方: [speed:val=0.05] (秒/文字。デフォルトは0.05など。0にすると一瞬で表示)")]
    public class TextSpeedCommandHandler : MonoBehaviour, IDialogueCommandHandler
    {
        public string TargetCommandName => "speed";

        private void Start()
        {
            if (DialogueEventDispatcher.Instance != null)
            {
                DialogueEventDispatcher.Instance.RegisterHandler(this);
            }
        }

        public void Execute(DialogueCommand command, Action onComplete)
        {
            float speedValue = command.GetFloat("val", 0.05f);

            // 現在アクティブな View (DialogueViewWindow) を探して、typingSpeed をリフレクション等で変更するか、
            // もし View に SetSpeed メソッドがあればそれを呼ぶのが理想です。
            // ここでは直接オブジェクトを探してフィールドを書き換える簡易的な例を示します。
            var view = FindObjectOfType<DialogueViewWindow>();
            if (view != null)
            {
                // 注意: typingSpeedはprivateなため、実際の運用では DialogueViewWindow に public な設定メソッド(SetTypingSpeed等)を追加してください。
                var fieldInfo = typeof(DialogueViewWindow).GetField("typingSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(view, speedValue);
                    Debug.Log($"[TextSpeedCommand] 文字表示速度を {speedValue} に変更しました。");
                }
            }

            // このコマンドは即座に完了する
            onComplete?.Invoke();
        }

        public void ForceComplete(DialogueCommand command)
        {
            // 即時完了処理なので特に何もしない
        }
    }
}