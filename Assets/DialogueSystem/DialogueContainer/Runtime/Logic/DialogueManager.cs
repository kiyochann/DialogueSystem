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

    /// <summary>
    /// 会話イベントの全体ステートと進行制御を行うマネージャークラス
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        private DialogueState currentState = DialogueState.Idle;
        private DialogueContainer currentContainer;
        private DialogueNode currentNode;
        private IDialogueView currentView;
        private Action onDialogueCompleteCallback;

        public DialogueState CurrentState => currentState;

        public IDialogueView CurrentView => currentView;

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
                currentState = DialogueState.ShowChoices;

                // 直接UIに投げるのをやめ、分岐ディスパッチャに「判定と実行」を丸投げする
                Runtime.Dialogue.Branching.DialogueBranchDispatcher.Instance.ProcessBranches(currentNode.choices, (nextID) =>
                {
                    TransitionToNode(nextID);
                });
            }
            else
            {
                currentState = DialogueState.WaitInput;
            }
        }

        public void HandleAdvanceInput()
        {
            if (currentState == DialogueState.Idle) return;

            // 1. 文字表示中なら文字送りを完了させる
            if (currentState == DialogueState.Typing)
            {
                currentView.ForceCompleteTyping();
                return;
            }

            // 2. WaitInput（一本道の入力待ち）の時だけ次のノードに進む
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