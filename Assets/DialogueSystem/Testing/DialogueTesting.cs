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

            // 2. 会話の進行判定（マウス左クリック）
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (DialogueManager.Instance.CurrentState != DialogueState.Idle)
                {
                    DialogueManager.Instance.HandleAdvanceInput();
                }
            }

            // 3. オート再生の切り替え判定（Aキー）
            if (Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame)
            {
                if (DialogueManager.Instance.CurrentState != DialogueState.Idle)
                {
                    DialogueManager.Instance.ToggleAutoMode();
                }
            }

            // 4. スキップ再生の切り替え判定（Sキー）
            if (Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame)
            {
                if (DialogueManager.Instance.CurrentState != DialogueState.Idle)
                {
                    DialogueManager.Instance.ToggleSkipMode();
                }
            }
        }
    }
}