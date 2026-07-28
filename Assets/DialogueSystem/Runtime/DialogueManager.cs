using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Dialogue
{
    public enum DialogueState
    {
        Idle,
        Typing,
        WaitInput,
        ShowChoices,
        ExecuteEvent
    }

    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        private DialogueState currentState = DialogueState.Idle;
        private DialogueContainer currentContainer;
        private DialogueNode currentNode;
        private IDialogueView currentView;
        private Action onDialogueCompleteCallback;

        public DialogueState CurrentState => currentState;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterView(IDialogueView view)
        {
            currentView = view;
        }

        public void StartDialogue(DialogueContainer container, Action onComplete = null)
        {
            if (currentView == null)
            {
                Debug.LogError("[DialogueManager] UI(IDialogueView)が登録されていません。");
                return;
            }

            if (container == null || string.IsNullOrEmpty(container.startNodeID))
            {
                Debug.LogError("[DialogueManager] 開始データ、またはstartNodeIDが空です。");
                return;
            }

            currentContainer = container;
            onDialogueCompleteCallback = onComplete;

            currentView.InitializeView();
            TransitionToNode(currentContainer.startNodeID);
        }

        private void TransitionToNode(string nodeID)
        {
            currentNode = currentContainer.GetNodeByID(nodeID);

            if (currentNode == null)
            {
                EndDialogue();
                return;
            }

            DialogueTagParser.ParseText(currentNode.rawText, out string cleanText, out List<DialogueCommand> commands);

            currentState = DialogueState.Typing;

            if (currentView is DialogueViewWindow windowView)
            {
                windowView.DisplaySentence(currentNode.speakerID, cleanText, commands, OnTypingFinished);
            }
            else
            {
                currentView.DisplaySentence(currentNode.speakerID, cleanText, OnTypingFinished);
            }
        }

        private void OnTypingFinished()
        {
            if (currentNode.choices != null && currentNode.choices.Count > 0)
            {
                // 選択肢がある場合はステートをShowChoicesにする
                currentState = DialogueState.ShowChoices;
                currentView.ShowChoices(currentNode.choices, OnChoiceSelected);
            }
            else
            {
                currentState = DialogueState.WaitInput;
            }
        }

        private void OnChoiceSelected(int index)
        {
            currentView.HideChoices();

            // 選択肢に埋め込まれている分岐先IDを引っ張ってくる
            string nextID = currentNode.choices[index].targetNodeID;
            TransitionToNode(nextID);
        }

        public void HandleAdvanceInput()
        {
            if (currentState == DialogueState.Idle) return;

            // 1. 文字表示中ならスキップ
            if (currentState == DialogueState.Typing)
            {
                currentView.ForceCompleteTyping();
                return;
            }

            // 【重要・修正点】ステートが「WaitInput（一本道の入力待ち）」の時だけ次へ進むように制限！
            // これにより、ShowChoices（選択肢表示中）の時の誤動作（デッドロック）を完全に防ぎます。
            if (currentState == DialogueState.WaitInput)
            {
                TransitionToNode(currentNode.nextNodeID);
            }
        }

        private void EndDialogue()
        {
            currentState = DialogueState.Idle;
            currentView.CloseView();

            currentContainer = null;
            currentNode = null;

            onDialogueCompleteCallback?.Invoke();
            onDialogueCompleteCallback = null;
        }
    }
}
