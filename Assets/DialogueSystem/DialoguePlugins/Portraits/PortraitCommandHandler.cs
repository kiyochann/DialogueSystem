using System;
using UnityEngine;
using Runtime.Dialogue;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Logic;

[HandlerInfo("キャラクターの立ち絵や表情を変更します", "使い方: [portrait:target=hero,exp=smile,pos=left]")]
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