using System;
using UnityEngine;
using Runtime.Dialogue;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Logic;

[HandlerInfo(
    description: @"キャラクターの立ち絵表示・表情変更・消去を行うコマンドハンドラーです。
スライド移動、フェードイン/アウト、Animator連動モーションに対応しています。",

    usage: @"【基本パラメータ】
[portrait:target=キャラクターID, exp=表情ID, pos=配置位置]

【拡張パラメータ】
・fade=0.5 (秒数指定でフェード表示/非表示)
・moveTime=0.3 (秒数指定で位置移動補間)
・motion=トリガー名 (AnimatorのTriggerを発火)

【使用例】
・標準表示: [portrait:target=hero,exp=smile,pos=left]
・演出付き: [portrait:target=hero,exp=smile,pos=left,fade=0.5,motion=nod]
・立ち絵消去: [portrait:target=clear,pos=left,fade=0.3]"
)]
public class PortraitCommandHandler : MonoBehaviour, IDialogueCommandHandler
{
    public string TargetCommandName => "portrait";

    private void Start()
    {
        if (DialogueEventDispatcher.Instance != null)
        {
            DialogueEventDispatcher.Instance.RegisterHandler(this);
        }
    }

    public void Execute(DialogueCommand command, Action onComplete)
    {
        string target = command.GetString("target", "");
        string exp = command.GetString("exp", "default");
        string pos = command.GetString("pos", "center");

        if (DialoguePortraitDispatcher.Instance != null)
        {
            bool handled = DialoguePortraitDispatcher.Instance.TryHandlePortrait(target, exp, pos, command.Arguments, onComplete);
            if (!handled) onComplete?.Invoke();
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    public void ForceComplete(DialogueCommand command)
    {
        StopAllCoroutines();

        string target = command.GetString("target", "");
        string exp = command.GetString("exp", "default");
        string pos = command.GetString("pos", "center");

        if (DialoguePortraitDispatcher.Instance != null)
        {
            DialoguePortraitDispatcher.Instance.ForceCompletePortrait(target, exp, pos, command.Arguments);
        }
    }
}