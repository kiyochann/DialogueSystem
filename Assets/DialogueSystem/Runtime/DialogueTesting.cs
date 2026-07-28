using Runtime.Dialogue;
using UnityEngine;

namespace Runtime.Dialogue
{
    /// <summary>
    /// キーボード入力を監視して会話を進めたり、テストを開始したりするコンポーネント
    /// </summary>
    public class DialogueTesting : MonoBehaviour
    {
        [SerializeField] private DialogueContainer testData; // インスペクターでテストデータをセット

        // ★【追加】インスペクターからUI（View）を直接セットできるようにする枠
        [SerializeField] private DialogueViewWindow dialogueView;

        private void Update()
        {
            // 新しいInput Systemの簡易入力（KeyboardとMouse）を使用
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                // 1. テスト開始（Tキーを押したら会話スタート）
                if (UnityEngine.InputSystem.Keyboard.current.tKey.wasPressedThisFrame &&
                    DialogueManager.Instance.CurrentState == DialogueState.Idle)
                {
                    // ★【重要】ダイアログを開始する直前に、手動でUIを強制的に叩き起こしてマネージャーに登録する！
                    if (dialogueView != null)
                    {
                        DialogueManager.Instance.RegisterView(dialogueView);
                    }

                    DialogueManager.Instance.StartDialogue(testData, () => {
                        Debug.Log("【テスト】会話イベントがすべて安全に終了しました。");
                    });
                }

                // 2. 会話の進行（スペースキーまたはマウス左クリックで文字送り・スキップ）
                bool isSpacePressed = UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame;
                bool isLeftClickPressed = UnityEngine.InputSystem.Mouse.current != null &&
                                         UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;

                if (isSpacePressed || isLeftClickPressed)
                {
                    DialogueManager.Instance.HandleAdvanceInput();
                }
            }
        }
    }
}
