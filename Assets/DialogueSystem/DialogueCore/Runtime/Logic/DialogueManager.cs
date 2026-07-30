using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Branching;

namespace Runtime.Dialogue.Logic
{
    // テストコード側が参照している状態定義
    public enum DialogueState
    {
        Idle,
        Playing,            // 文字がパラパラ表示されている最中
        WaitingForAdvance,  // 👈 追加: 文字表示が終わり、クリックを待っている状態
        WaitingForChoice,
        Ended
    }

    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [SerializeField] private DialogueContainer currentContainer;
        public IDialogueView CurrentView { get; private set; }

        // テストコード側が参照している現在の状態
        public DialogueState CurrentState { get; private set; } = DialogueState.Idle;

        private DialogueNode currentNode;
        private Action onDialogueEndedCallback;

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

        // テストコード側から呼ばれる View 登録用メソッド
        public void RegisterView(IDialogueView view)
        {
            CurrentView = view;
            CurrentView?.InitializeView();
        }

        // テストコード側からコンテナとコールバック付きで呼ばれるオーバーロード
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
                if (currentNode.choices == null || currentNode.choices.Count == 0)
                {
                    // 👈 変更: 勝手に進めず、クリック待ち状態にする
                    CurrentState = DialogueState.WaitingForAdvance;
                }
                else
                {
                    CurrentState = DialogueState.WaitingForChoice;
                    bool handled = DialogueBranchDispatcher.Instance.TryHandleBranch(currentNode.choices, (nextID) =>
                    {
                        CurrentState = DialogueState.Playing;
                        StartDialogue(nextID);
                    });

                    if (!handled)
                    {
                        Debug.LogWarning("どの分岐ハンドラーも選択肢を処理できませんでした。");
                    }
                }
            });
        }

        // テストコード側からスペースキー等で呼ばれる文字送り・スキップ入力の処理
        public void HandleAdvanceInput()
        {
            if (CurrentState == DialogueState.Playing)
            {
                // タイピング中なら一瞬で全文表示＆演出コマンド一括完了
                CurrentView?.ForceCompleteTyping();
                // ※ ForceCompleteTyping の中でコールバックが呼ばれるため、自動的に WaitingForAdvance に移行します。
            }
            else if (CurrentState == DialogueState.WaitingForAdvance)
            {
                // クリック待ち状態なら、次のノードへ進む
                if (!string.IsNullOrEmpty(currentNode.nextNodeID))
                {
                    StartDialogue(currentNode.nextNodeID);
                }
                else
                {
                    EndDialogue();
                }
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
        }
    }
}