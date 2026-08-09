using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Logic;
using System.Linq;
using System.Reflection;

namespace Runtime.Dialogue
{
    /// <summary>
    /// 画面下の固定ウィンドウ型UI(パターンA)を制御するクラス
    /// </summary>
    public class DialogueViewWindow : MonoBehaviour, IDialogueView
    {
        [Header("UI References")]
        [SerializeField] private GameObject windowRoot;         // ウィンドウ全体の親オブジェクト
        [SerializeField] private TextMeshProUGUI nameText;       // 名前テキスト
        [SerializeField] private TextMeshProUGUI bodyText;       // 本文テキスト
        [SerializeField] private Transform choiceButtonParent;   // 選択ボタンを配置する親コンテナ
        [SerializeField] private Button choiceButtonPrefab;      // 選択ボタンのプレハブ

        [Header("Settings")]
        [SerializeField] private float typingSpeed = 0.05f;      // 文字の表示速度(秒)

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
            // ウィンドウ全体のオブジェクトを確実にアクティブにする
            if (windowRoot != null)
            {
                windowRoot.SetActive(true);
            }

            // UIコンポーネントを有効化して初期化
            if (bodyText != null) bodyText.enabled = true;
            if (nameText != null) nameText.enabled = true;

            if (bodyText != null) bodyText.text = string.Empty;
            if (nameText != null) nameText.text = string.Empty;

            var bgImage = windowRoot != null ? windowRoot.GetComponent<UnityEngine.UI.Image>() : null;
            if (bgImage != null) bgImage.enabled = true;
        }

        public void CloseView()
        {
            // ウィンドウ全体、またはコンポーネントを非表示化
            if (windowRoot != null)
            {
                windowRoot.SetActive(false);
            }

            if (bodyText != null) bodyText.enabled = false;
            if (nameText != null) nameText.enabled = false;

            HideChoices();
        }

        /// <summary>
        /// マネージャーからセリフデータとコマンドリストを受け取ってタイピング再生を開始
        /// </summary>
        public void DisplaySentence(string speakerID, string cleanText, List<DialogueCommand> commands, Action onTypingComplete)
        {
            // 👇 必ず最初にビューを初期化（ウィンドウとテキストを有効化）する
            InitializeView();

            if (nameText != null) nameText.text = speakerID;
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
            bodyText.text = currentFullText;
            bodyText.maxVisibleCharacters = 0;
            bodyText.ForceMeshUpdate(); // テキスト解析を強制し、正確な文字数を取得

            int totalVisibleChars = bodyText.textInfo.characterCount;
            int currentVisibleIndex = 0;

            while (currentVisibleIndex < totalVisibleChars)
            {
                int pendingCommands = 0;
                if (currentCommands != null)
                {
                    foreach (var cmd in currentCommands)
                    {
                        if (cmd != null && !cmd.IsExecuted && cmd.CharacterIndex == currentVisibleIndex)
                        {
                            pendingCommands++;
                            if (DialogueEventDispatcher.Instance != null)
                                DialogueEventDispatcher.Instance.ExecuteCommand(cmd, () => pendingCommands--);
                            else
                                pendingCommands--;
                        }
                    }
                }

                if (pendingCommands > 0) yield return new WaitUntil(() => pendingCommands <= 0);

                currentVisibleIndex++;
                bodyText.maxVisibleCharacters = currentVisibleIndex;

                yield return new WaitForSeconds(typingSpeed);
            }

            ExecuteRemainingCommands(false);
            typingCoroutine = null;
            onCompleteCallback?.Invoke();
        }

        public void ForceCompleteTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            if (bodyText != null)
            {
                bodyText.maxVisibleCharacters = 99999; // 全て表示
            }

            ExecuteRemainingCommands(true);
            onCompleteCallback?.Invoke();
        }

        private void ExecuteRemainingCommands(bool forceComplete)
        {
            if (currentCommands == null) return;

            foreach (var cmd in currentCommands)
            {
                if (cmd != null && !cmd.IsExecuted)
                {
                    if (DialogueEventDispatcher.Instance != null)
                    {
                        if (forceComplete)
                            DialogueEventDispatcher.Instance.ForceCompleteCommand(cmd);
                        else
                            DialogueEventDispatcher.Instance.ExecuteCommand(cmd, null);
                    }
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
                int index = i; // クロージャ対策
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

        [Header("Fonts")]
        [SerializeField] private TMPro.TMP_FontAsset defaultFont; // 通常のフォント

        // DialogueViewWindow.cs に追加

        // タイピング速度を動的に変更する
       

        // セリフ開始時に標準状態へリセット（ResetStyleを拡張）
        public void ResetStyle()
        {
            if (bodyText != null)
            {
                if (defaultFont != null) bodyText.font = defaultFont;
                bodyText.color = Color.white;
                bodyText.fontSize = 36; // デフォルトサイズ例
            }
            typingSpeed = 0.05f; // デフォルト速度に戻す
        }

        public void SetTypingSpeed(float newSpeed)
        {
            typingSpeed = Mathf.Max(0.001f, newSpeed);
        }



    }
}