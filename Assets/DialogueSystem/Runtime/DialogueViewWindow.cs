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
            // 開始時にDialogueManagerに自分自身(View)を登録する
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.RegisterView(this);
            }

            CloseView();
        }

        public void InitializeView()
        {

            bodyText.enabled = true;
            nameText.enabled = true;

            bodyText.text = string.Empty;
            nameText.text = string.Empty;

            var bgImage = windowRoot.GetComponent<UnityEngine.UI.Image>();
            if (bgImage != null) bgImage.enabled = true;
        }

        public void CloseView()
        {
            bodyText.enabled = false;
            nameText.enabled = false;

            var bgImage = windowRoot.GetComponent<UnityEngine.UI.Image>();
            if(bgImage != null) bgImage.enabled = false;

            HideChoices();
        }

        /// <summary>
        /// マネージャからセリフデータと、その行に含まれるコマンドのリストを受け取って再生する
        /// </summary>
        public void DisplaySentence(string speakerID, string cleanText, List<DialogueCommand> commands, Action onTypingComplete)
        {
            // 本来はspeakerIDを元にデータベースなどから「正式名」を引くが、今回は簡易的にIDをそのまま名前に表示
            nameText.text = speakerID;

            currentFullText = cleanText;
            currentCommands = commands;
            onCompleteCallback = onTypingComplete;

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeTextRoutine());
        }

        // 互換性維持のための古いインタフェース実装
        public void DisplaySentence(string speakerID, string cleanText, Action onTypingComplete)
        {
            DisplaySentence(speakerID, cleanText, new List<DialogueCommand>(), onTypingComplete);
        }

        /// <summary>
        /// 1文字ずつ出力しながら、指定文字数に到達したタグをリアルタイムに実行するコルーチン
        /// </summary>
        private IEnumerator TypeTextRoutine()
        {
            bodyText.text = string.Empty;
            int charCount = 0;

            while(charCount < currentFullText.Length)
            {
                // 現在の文字数インデックスに紐づく未実行のコマンドがあれば実行
                foreach(var cmd in currentCommands)
                {
                    if(!cmd.IsExecuted && cmd.CharacterIndex == charCount)
                    {
                        // 演出を実行(非同期なイベントだった場合も、Viewのタイピングは裏で進める設計)
                        DialogueEventDispatcher.Instance.ExecuteCommand(cmd, null);
                    }
                }

                bodyText.text += currentFullText[charCount];
                ++charCount;
                yield return new WaitForSeconds(typingSpeed);
            }

            // テキストの末尾(表示完了後)に配置されているタグを最後に一斉実行
            ExecuteRemainingCommands(false);

            typingCoroutine = null;
            onCompleteCallback?.Invoke(); // マネージャへ文字表示完了を通知
        }

        public void ForceCompleteTyping()
        {
            if (typingCoroutine == null) return;

            StopCoroutine(typingCoroutine);
            typingCoroutine = null;

            bodyText.text = currentFullText;

            // まだ実行されていないこの行のタグを【すべて強制完了(ワープ)】させる
            ExecuteRemainingCommands(true);

            onCompleteCallback?.Invoke();
        }

        /// <summary>
        /// 残っている未実行のコマンドを処理する
        /// </summary>
        /// <param name="forceComplete">trueなら一瞬で最終状態にワープ、falseなら通常実行</param>
        private void ExecuteRemainingCommands(bool forceComplete)
        {
            if (currentCommands == null) return;
            
            foreach(var cmd in currentCommands)
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

        public void ShowChoices(List<ChoiceData> choices, Action<int> onChoiceSelected)
        {
            HideChoices(); // 古いボタンをクリア

            for(int i = 0; i < choices.Count; ++i)
            {
                int index = i; // クローシャ対策(ラムダ氏起用にインデックスを固定)
                Button btn = Instantiate(choiceButtonPrefab, choiceButtonParent);

                // ボタンのテキストを書き換え(TextMeshProが子にある前提)
                var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();

                if (btnText != null) btnText.text = choices[i].choiceText;

                activeButtons.Add(btn);
            }
        }

        public void HideChoices()
        {
            foreach(var btn in activeButtons)
            {
                if (btn != null) Destroy(btn.gameObject);
            }
            activeButtons.Clear();
        }
    }                                                           
}