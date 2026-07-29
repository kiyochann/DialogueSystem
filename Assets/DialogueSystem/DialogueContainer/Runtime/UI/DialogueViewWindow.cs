using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Runtime.Dialogue
{
    /// <summary>
    /// 画面下の固定ウィンドウ型UI(パターンA)を制御するクラス
    /// </summary>
    public class DialogueViewWindow : MonoBehaviour, IDialogueView
    {
        [Header("UI References")]
        [SerializeField] private GameObject windowRoot;         // ウィンドウ全体の親オブジェクト
        [SerializeField] private TextMeshProUGUI nameText;      // 名前テキスト
        [SerializeField] private TextMeshProUGUI bodyText;      // 本文テキスト
        [SerializeField] private Transform choiceButtonParent;  // 選択ボタンを配置する親コンテナ
        [SerializeField] private Button choiceButtonPrefab;     // 選択ボタンのプレハブ

        [Header("Settings")]
        [SerializeField] private float typingSpeed = 0.05f;     // 文字の表示速度(秒)

        private Coroutine typingCoroutine;
        private string currentFullText;
        private List<DialogueCommand> currentCommands;
        private Action onCompleteCallback;
        private List<Button> activeButtons = new List<Button>();

        private void Awake()
        {
            // 開始時にDialogueManagerへ自分自身(View)を登録
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.RegisterView(this);
            }
            CloseView();
        }

        public void InitializeView()
        {
            // UI表示状態の初期化
            bodyText.enabled = true;
            nameText.enabled = true;
            bodyText.text = string.Empty;
            nameText.text = string.Empty;

            var bgImage = windowRoot.GetComponent<UnityEngine.UI.Image>();
            if (bgImage != null) bgImage.enabled = true;
        }

        public void CloseView()
        {
            // オブジェクトはActiveのまま、表示コンポーネントのみを非表示化
            bodyText.enabled = false;
            nameText.enabled = false;

            var bgImage = windowRoot.GetComponent<UnityEngine.UI.Image>();
            if (bgImage != null) bgImage.enabled = false;

            HideChoices();
        }

        /// <summary>
        /// マネージャーからセリフデータとコマンドリストを受け取ってタイピング再生を開始
        /// </summary>
        public void DisplaySentence(string speakerID, string cleanText, List<DialogueCommand> commands, Action onTypingComplete)
        {
            nameText.text = speakerID;
            currentFullText = cleanText;
            currentCommands = commands;
            onCompleteCallback = onTypingComplete;

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeTextRoutine());
        }

        public void DisplaySentence(string speakerID, string cleanText, Action onTypingComplete)
        {
            DisplaySentence(speakerID, cleanText, new List<DialogueCommand>(), onTypingComplete);
        }

        /// <summary>
        /// 1文字ずつ出力しながら、指定文字数に到達したタグ演出をリアルタイム実行するコルーチン
        /// </summary>
        private IEnumerator TypeTextRoutine()
        {
            bodyText.text = string.Empty;
            int charCount = 0;

            while (charCount < currentFullText.Length)
            {
                if (currentCommands != null)
                {
                    foreach (var cmd in currentCommands)
                    {
                        if (!cmd.IsExecuted && cmd.CharacterIndex == charCount)
                        {
                            DialogueEventDispatcher.Instance.ExecuteCommand(cmd, null);
                        }
                    }
                }

                bodyText.text += currentFullText[charCount];
                ++charCount;
                yield return new WaitForSeconds(typingSpeed);
            }

            ExecuteRemainingCommands(false);
            typingCoroutine = null;
            onCompleteCallback?.Invoke();
        }

        public void ForceCompleteTyping()
        {
            if (typingCoroutine == null) return;

            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            bodyText.text = currentFullText;

            ExecuteRemainingCommands(true);
            onCompleteCallback?.Invoke();
        }

        private void ExecuteRemainingCommands(bool forceComplete)
        {
            if (currentCommands == null) return;

            foreach (var cmd in currentCommands)
            {
                if (!cmd.IsExecuted)
                {
                    if (forceComplete)
                        DialogueEventDispatcher.Instance.ForceCompleteCommand(cmd);
                    else
                        DialogueEventDispatcher.Instance.ExecuteCommand(cmd, null);
                }
            }
        }

        /// <summary>
        /// 選択肢ボタンを動的に生成して画面に提示
        /// </summary>
        public void ShowChoices(List<ChoiceData> choices, Action<int> onChoiceSelected)
        {
            HideChoices(); // 古いボタンの削除

            for (int i = 0; i < choices.Count; ++i)
            {
                int index = i; // クロージャ対策（ラムダ式用にインデックスを固定）
                Button btn = Instantiate(choiceButtonPrefab, choiceButtonParent);

                var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = choices[i].choiceText;

                btn.onClick.AddListener(() => onChoiceSelected?.Invoke(index));
                activeButtons.Add(btn);
            }
        }

        public void HideChoices()
        {
            foreach (var btn in activeButtons)
            {
                if (btn != null) Destroy(btn.gameObject);
            }
            activeButtons.Clear();
        }
    }
}