using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Runtime.Dialogue.Audio;
using Runtime.Dialogue.Logic;

namespace Runtime.Dialogue
{
    /// <summary>
    /// 会話進行・オート・スキップのユーザー入力と効果音（SE）を統括するハンドラー
    /// </summary>
    public class DialogueInputHandler : MonoBehaviour
    {
        [Header("SE Keys")]
        [SerializeField] private string nextPageSEKey = "next_page";

        [Header("References")]
        [SerializeField] private DialogueViewWindow dialogueView;

        private float startInputCooldown = 0f;
        private DialogueState prevState = DialogueState.Idle;

        private void Awake()
        {
            if (dialogueView == null)
            {
                dialogueView = UnityEngine.Object.FindAnyObjectByType<DialogueViewWindow>();
            }

            // 会話時に日本語(IME)変換キー暴発を防止
            Input.imeCompositionMode = IMECompositionMode.Off;
        }

        private void Update()
        {
            if (DialogueManager.Instance == null) return;

            DialogueState currentState = DialogueManager.Instance.CurrentState;

            // 会話開始直後（Idle -> 会話中への遷移時）にクールダウンを設けて連打・押しっぱなしによる誤送りを防止
            if (prevState == DialogueState.Idle && currentState != DialogueState.Idle)
            {
                startInputCooldown = 0.2f;
            }
            prevState = currentState;

            if (currentState == DialogueState.Idle) return;

            if (startInputCooldown > 0f)
            {
                startInputCooldown -= Time.deltaTime;
                return;
            }

            // --- 入力判定 ---
            // 1. マウスクリック
            bool isMousePressed = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                               || Input.GetMouseButtonDown(0);

            // 2. キーボード（Space, Enter）
            bool isStandardKeyboard = (Keyboard.current != null && (
                Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame
            ))
            || Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.Return);

            // 3. Fキー（インタラクトキーでの送り）
            bool isFKeyPressed = (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
                              || Input.GetKeyDown(KeyCode.F);

            if (isMousePressed || isStandardKeyboard || isFKeyPressed)
            {
                // 選択肢が表示されている間は、UIへのクリック誤爆を防ぐためガード
                if (isMousePressed && dialogueView != null && dialogueView.IsShowingChoices)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(PointerInputModule.kMouseLeftId))
                    {
                        return;
                    }
                }

                HandleAdvance();
            }

            // 4. オートモード切り替え（Aキー）
            if ((Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame) || Input.GetKeyDown(KeyCode.A))
            {
                DialogueManager.Instance.ToggleAutoMode();
            }

            // 5. スキップモード切り替え（Sキー）
            if ((Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame) || Input.GetKeyDown(KeyCode.S))
            {
                DialogueManager.Instance.ToggleSkipMode();
            }
        }

        /// <summary>
        /// 会話を次に進める（効果音を鳴らしManagerを呼び出し）
        /// </summary>
        public void HandleAdvance()
        {
            if (dialogueView != null && dialogueView.IsShowingChoices) return;

            // 会話送りSE再生
            DialogueAudioManager.Instance?.PlaySE(nextPageSEKey);

            // DialogueManagerへ進行通知
            DialogueManager.Instance?.HandleAdvanceInput();
        }
    }
}