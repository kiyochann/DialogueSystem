using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Runtime.Dialogue.Audio;
using Runtime.Dialogue.Logic;

namespace Runtime.Dialogue
{
    /// <summary>
    /// 会話進行・オート・スキップ等のユーザー入力と音演出（SE）を統括するハンドラー
    /// </summary>
    public class DialogueInputHandler : MonoBehaviour
    {
        [Header("SE Keys")]
        [SerializeField] private string nextPageSEKey = "next_page";

        [Header("References")]
        [SerializeField] private DialogueViewWindow dialogueView;

        private void Awake()
        {
            if (dialogueView == null)
            {
                dialogueView = UnityEngine.Object.FindAnyObjectByType<DialogueViewWindow>();
            }
        }

        private void Update()
        {
            if (DialogueManager.Instance == null || DialogueManager.Instance.CurrentState == DialogueState.Idle)
            {
                return;
            }

            // 1. 会話送り（マウスクリック / Spaceキー / Enterキー）
            bool isMousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool isKeyboardPressed = Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame);

            if (isMousePressed || isKeyboardPressed)
            {
                // 選択肢が表示されている間だけ、UI上のクリック（ボタン選択等）を誤発動防止のためガードする
                if (isMousePressed && dialogueView != null && dialogueView.IsShowingChoices)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(PointerInputModule.kMouseLeftId))
                    {
                        return;
                    }
                }

                HandleAdvance();
            }

            // 2. オートモード切り替え（Aキー）
            if (Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame)
            {
                DialogueManager.Instance.ToggleAutoMode();
            }

            // 3. スキップモード切り替え（Sキー）
            if (Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame)
            {
                DialogueManager.Instance.ToggleSkipMode();
            }
        }

        /// <summary>
        /// 会話を次に進める（決定音を流してManagerを呼び出す）
        /// </summary>
        public void HandleAdvance()
        {
            if (dialogueView != null && dialogueView.IsShowingChoices) return;

            // 会話送り音を再生
            DialogueAudioManager.Instance?.PlaySE(nextPageSEKey);

            // DialogueManagerへ進行通知
            DialogueManager.Instance?.HandleAdvanceInput();
        }
    }
}