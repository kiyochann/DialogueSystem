using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Branching;

namespace Runtime.Dialogue.Logic
{
    public enum DialogueState
    {
        Idle,
        Playing,
        WaitingForAdvance,
        WaitingForChoice,
        Ended
    }

    public class DialogueManager : MonoBehaviour
    {
        public string CurrentSpeaker => currentNode != null ? currentNode.speakerName : string.Empty;

        public static DialogueManager Instance { get; private set; }

        [SerializeField] private DialogueContainer currentContainer;
        public IDialogueView CurrentView { get; private set; }
        public DialogueState CurrentState { get; private set; } = DialogueState.Idle;

        private DialogueNode currentNode;
        private Action onDialogueEndedCallback;

        [Header("Auto Mode Settings")]
        [SerializeField, Tooltip("オート再生時の基本待機時間（秒）")]
        private float baseAutoWaitTime = 1.0f;
        [SerializeField, Tooltip("1文字あたりの追加待機時間（秒）")]
        private float autoWaitTimePerChar = 0.05f;

        private bool isAutoMode = false;
        public bool IsAutoMode => isAutoMode;
        private Coroutine autoWaitCoroutine;

        // 👇追加: スキップ機能用の変数
        private bool isSkipMode = false;
        public bool IsSkipMode => isSkipMode;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void Initialize(IDialogueView view)
        {
            CurrentView = view;
            CurrentView?.InitializeView();
        }

        public void RegisterView(IDialogueView view)
        {
            CurrentView = view;
            CurrentView?.InitializeView();
        }

        public void StartDialogue(DialogueContainer container, Action onEnded = null)
        {
            if (container != null)
            {
                currentContainer = container;
            }
            onDialogueEndedCallback = onEnded;
            StartDialogue(string.Empty);
        }

        public void StartDialogue(string startNodeID = "")
        {
            if (currentContainer == null)
            {
                Debug.LogError("DialogueContainer が設定されていません！");
                CurrentState = DialogueState.Idle;
                return;
            }

            CurrentState = DialogueState.Playing;
            string targetID = string.IsNullOrEmpty(startNodeID) ? currentContainer.startNodeID : startNodeID;
            currentNode = currentContainer.GetNodeByID(targetID);

            if (currentNode == null)
            {
                Debug.LogError($"指定されたIDのノードが見つかりませんでした: {targetID}");
                EndDialogue();
                return;
            }

            ShowCurrentNode();
        }

        private void ShowCurrentNode()
        {
            if (currentNode == null)
            {
                EndDialogue();
                return;
            }

            string speakerID = currentNode.speakerName;
            string cleanText;
            List<DialogueCommand> commands;
            DialogueTagParser.ParseText(currentNode.dialogueText, out cleanText, out commands);

            CurrentView?.DisplaySentence(speakerID, cleanText, commands, () =>
            {
                // 1. 選択肢が存在する場合の処理
                if (currentNode.choices != null && currentNode.choices.Count > 0)
                {
                    CurrentState = DialogueState.WaitingForChoice;

                    // 👇 選択肢が出た時はオート・スキップを解除して止める
                    DisableAutoMode();
                    DisableSkipMode();

                    if (DialogueBranchDispatcher.Instance != null)
                    {
                        bool handled = DialogueBranchDispatcher.Instance.TryHandleBranch(currentNode.choices, (nextID) =>
                        {
                            CurrentState = DialogueState.Playing;
                            StartDialogue(nextID);
                        });

                        if (!handled)
                        {
                            CurrentState = DialogueState.WaitingForAdvance;
                        }
                    }
                    else
                    {
                        CurrentState = DialogueState.WaitingForAdvance;
                    }
                }
                // 2. 選択肢がない場合（通常の文章）の処理
                else
                {
                    CurrentState = DialogueState.WaitingForAdvance;

                    // 👇 スキップ中なら即座に次へ、オート中なら待機
                    if (isSkipMode)
                    {
                        StartCoroutine(SkipAdvanceRoutine());
                    }
                    else if (isAutoMode)
                    {
                        StartAutoWait(cleanText.Length);
                    }
                }
            });

            // 👇 追加: スキップ中なら、文字表示アニメーションを即座に強制完了させる
            if (isSkipMode && CurrentState == DialogueState.Playing)
            {
                CurrentView?.ForceCompleteTyping();
            }
        }

        public void HandleAdvanceInput()
        {
            // 1. 手動入力が来たらオートとスキップを解除する
            if (autoWaitCoroutine != null)
            {
                StopCoroutine(autoWaitCoroutine);
                autoWaitCoroutine = null;
            }
            DisableAutoMode();
            DisableSkipMode(); // 👈 追加

            // 2. 状態に応じた進行処理
            if (CurrentState == DialogueState.Playing)
            {
                CurrentView?.ForceCompleteTyping();
            }
            else if (CurrentState == DialogueState.WaitingForAdvance)
            {
                AdvanceToNextNode(); // 👈 スッキリさせるためにメソッド化
            }
        }

        // 👇 追加: 次のノードへ進む処理を独立させたメソッド
        private void AdvanceToNextNode()
        {
            if (!string.IsNullOrEmpty(currentNode.nextNodeID))
            {
                StartDialogue(currentNode.nextNodeID);
            }
            else
            {
                EndDialogue();
            }
        }

        private void EndDialogue()
        {
            CurrentState = DialogueState.Ended;
            CurrentView?.CloseView();

            Action callback = onDialogueEndedCallback;
            onDialogueEndedCallback = null;
            callback?.Invoke();

            CurrentState = DialogueState.Idle;

            // 会話が終わったら念のためモードをオフに
            isAutoMode = false;
            isSkipMode = false;
        }

        // --- Auto Mode ---
        public void ToggleAutoMode()
        {
            isAutoMode = !isAutoMode;
            Debug.Log($"Auto Mode toggled: {isAutoMode}");

            if (isAutoMode)
            {
                DisableSkipMode(); // オートとスキップは排他

                if (CurrentState == DialogueState.WaitingForAdvance)
                {
                    StartAutoWait(0);
                }
            }
            else if (autoWaitCoroutine != null)
            {
                StopCoroutine(autoWaitCoroutine);
                autoWaitCoroutine = null;
            }
        }

        public void DisableAutoMode()
        {
            if (isAutoMode)
            {
                isAutoMode = false;
                Debug.Log("Auto Mode disabled.");
            }
        }

        private void StartAutoWait(int textLength)
        {
            if (autoWaitCoroutine != null) StopCoroutine(autoWaitCoroutine);
            float waitTime = baseAutoWaitTime + (textLength * autoWaitTimePerChar);
            autoWaitCoroutine = StartCoroutine(AutoWaitRoutine(waitTime));
        }

        private System.Collections.IEnumerator AutoWaitRoutine(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            autoWaitCoroutine = null;
            AdvanceToNextNode(); // 👈 自動進行用
        }

        // --- Skip Mode (👇 今回追加) ---
        public void ToggleSkipMode()
        {
            isSkipMode = !isSkipMode;
            Debug.Log($"Skip Mode toggled: {isSkipMode}");

            if (isSkipMode)
            {
                DisableAutoMode(); // オートとスキップは排他

                if (CurrentState == DialogueState.Playing)
                {
                    CurrentView?.ForceCompleteTyping();
                }
                else if (CurrentState == DialogueState.WaitingForAdvance)
                {
                    StartCoroutine(SkipAdvanceRoutine());
                }
            }
        }

        public void DisableSkipMode()
        {
            if (isSkipMode)
            {
                isSkipMode = false;
                Debug.Log("Skip Mode disabled.");
            }
        }

        private System.Collections.IEnumerator SkipAdvanceRoutine()
        {
            // プログラムが一瞬でループしすぎてフリーズ（スタックオーバーフロー）するのを防ぐため、必ず1フレーム待つ
            yield return null;

            if (isSkipMode && CurrentState == DialogueState.WaitingForAdvance)
            {
                AdvanceToNextNode();
            }
        }
    }
}