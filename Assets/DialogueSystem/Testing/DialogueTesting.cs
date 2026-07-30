// DialogueTesting.cs の修正

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
            // 1. 会話の開始判定（Tキー）
            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            {
                if (DialogueManager.Instance.CurrentState == DialogueState.Idle)
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

            // 2. 会話の進行・スキップ判定（マウス左クリック）
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                // アイドル状態以外（文字表示中、またはクリック待ち状態）の時に入力を受け付ける
                if (DialogueManager.Instance.CurrentState != DialogueState.Idle)
                {
                    DialogueManager.Instance.HandleAdvanceInput();
                }
            }
        }
    }
}