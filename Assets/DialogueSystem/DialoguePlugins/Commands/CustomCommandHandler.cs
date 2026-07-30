using System;
using System.Collections;
using UnityEngine;
using Runtime.Dialogue;

namespace Runtime.Dialogue.Commands
{
    /// <summary>
    /// カスタム演出コマンドのテンプレート ([my_command:target=value])
    /// </summary>
    public class CustomCommandHandler : MonoBehaviour, IDialogueCommandHandler
    {
        public string TargetCommandName => "my_command";

        private void Start()
        {
            if (DialogueEventDispatcher.Instance != null)
            {
                DialogueEventDispatcher.Instance.RegisterHandler(this);
            }
        }

        public void Execute(DialogueCommand command, Action onComplete)
        {
            string targetValue = command.GetString("target", "default_value");
            float timeValue = command.GetFloat("time", 1.0f);

            StartCoroutine(CommandRoutine(timeValue, targetValue, onComplete));
        }

        public void ForceComplete(DialogueCommand command)
        {
            StopAllCoroutines();
        }

        private IEnumerator CommandRoutine(float time, string target, Action onComplete)
        {
            Debug.Log($"[CustomCommand] 開始: target={target}, time={time}");
            yield return new WaitForSeconds(time);
            Debug.Log("[CustomCommand] 完了");
            onComplete?.Invoke();
        }
    }
}