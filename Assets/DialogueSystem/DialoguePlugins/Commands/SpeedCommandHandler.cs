using System;
using System.Collections;
using UnityEngine;
using Runtime.Dialogue;
using Runtime.Dialogue.Core; // HandlerInfo と DialogueCommand のために必要

[HandlerInfo("文字出力速度を変更する演出コマンド", "使い方: [speed:value=0.05]")]
public class SpeedCommandHandler : MonoBehaviour, IDialogueCommandHandler
{
    // インターフェースの仕様通りにプロパティを実装
    public string TargetCommandName => "speed";

    private Coroutine currentCoroutine;

    private void Start()
    {
        // 登録のみ。Unregister はシステムに存在しないため書かない
        if (DialogueEventDispatcher.Instance != null)
        {
            DialogueEventDispatcher.Instance.RegisterHandler(this);
        }
    }

    // エラー原因1: 'HandleCommand' を 'Execute' に修正
    public void Execute(DialogueCommand command, Action onComplete)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(ExecuteCommandCoroutine(command, onComplete));
    }

    private IEnumerator ExecuteCommandCoroutine(DialogueCommand command, Action onComplete)
    {
        float speed = command.GetFloat("value", 0.05f);

        // TODO: テキスト表示システム等へ速度設定を適用する処理
        // (例: DialogueManager.Instance.CurrentView などを経由して速度を渡す等)

        yield return null;

        currentCoroutine = null;

        // 完了時に必ずコールバックを呼ぶ
        onComplete?.Invoke();
    }

    // エラー原因2: 引数に 'DialogueCommand command' を追加して仕様と一致させる
    public void ForceComplete(DialogueCommand command)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        // スキップ時の最終状態適用処理（必要に応じてデフォルト速度や設定値を反映）
    }

    // エラー原因3: 存在しない UnregisterHandler を呼ぶ OnDestroy は完全に削除
}