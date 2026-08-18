using System;
using UnityEngine;
using Runtime.Dialogue;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Logic;

[HandlerInfo("ウィンドウのレイアウトや枠の見た目を変更します", "使い方: [layout:name=Narration]")]
public class LayoutCommandHandler : MonoBehaviour, IDialogueCommandHandler
{
    public string TargetCommandName => "layout";

    private void Start()
    {
        if (DialogueEventDispatcher.Instance != null)
        {
            DialogueEventDispatcher.Instance.RegisterHandler(this);
        }
    }

    public void Execute(DialogueCommand command, Action onComplete)
    {
        string layoutName = command.GetString("name", "Normal");

        if (DialogueLayoutDispatcher.Instance != null)
        {
            DialogueLayoutDispatcher.Instance.TryHandleLayout(layoutName, command.Arguments);
        }

        onComplete?.Invoke();
    }

    public void ForceComplete(DialogueCommand command)
    {
        StopAllCoroutines();

        string layoutName = command.GetString("name", "Normal");

        if (DialogueLayoutDispatcher.Instance != null)
        {
            DialogueLayoutDispatcher.Instance.TryHandleLayout(layoutName, command.Arguments);
        }
    }
}