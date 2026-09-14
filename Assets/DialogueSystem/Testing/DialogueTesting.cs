using Runtime.Dialogue.Logic;
using Runtime.Dialogue.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Dialogue
{
    public class DialogueTesting : MonoBehaviour
    {
        [SerializeField] private DialogueContainer testData;
        [SerializeField] private DialogueViewWindow dialogueView;

        private void Update()
        {
            // 会話の開始判定（Tキー）
            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            {
                if (DialogueManager.Instance != null && DialogueManager.Instance.CurrentState == DialogueState.Idle)
                {
                    if (dialogueView != null)
                    {
                        DialogueManager.Instance.RegisterView(dialogueView);
                    }

                    DialogueManager.Instance.StartDialogue(testData, () => {
                        Debug.Log("【テスト】会話イベントが終了しました。");
                    });
                }
            }
        }
    }
}