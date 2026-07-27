using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Dialogue
{
    /// <summary>
    /// ダイアログシステムの進行状態(ステート)
    /// </summary>
    public enum DialogueState
    {
        Idle,           // 会話していない状態
        Typing,         // 文字を表示している状態(演出中)
        WaitInput,      // 文字表示が終わり、プレイヤの「次へ」の入力を待っている状態
        ShowChoices,    // 選択を表示し、プレイヤのボタン選択を待っている状態
        ExecuteEvent    // テキスト以外の演出(暗転など)の完了を待っている状態
    }

    /// <summary>
    /// ダイアログシステム全体の進行と状態を管理するマネージャ(脳)
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        private DialogueState currentState = DialogueState.Idle;
        private DialogueContainer currentContainer;
        private DialogueNode currentNode;
        private IDialogueView currentView;
        private Action onDialogueCompleteCallback;

        /// <summay>現在のシステムの進行状態を取得する</summay>
        public DialogueState CurrentState => currentState;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // シーンを跨いでも破棄しないように保持
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 使用するUI(View)をマネージャに登録する(ウィンドウ型可吹き出しが鷹をここで差し替える)
        /// </summary>
        public void RegisterView(IDialogueView view)
        {
            currentView = view;
        }

        /// <summary>
        /// 外部スクリプトから会話イベントを開始する
        /// </summary>
        /// <param name="container">再生したい会話データアセット</param>
        /// <param name="onComplete">会話がすべて終了した後に実行したい処理(コールバック)</param>
        public void StartDialogue(DialogueContainer container, Action onComplete = null)
        {
            if (currentView == null)
            {
                Debug.LogError("[DialogueManager] UI(IDialogueView)が登録されていません。RegisterViewを事前に読んでください。");
                return;
            }

            if(container == null || string.IsNullOrEmpty(container.startNodeID))
            {
                Debug.LogError("[DialogueManager] 開始データ、またはstartNodeIDが空です。");
                return;
            }

            currentContainer = container;
            onDialogueCompleteCallback = onComplete;

            currentView.InitializeView();

            // 開始ノードを取得して進行開始
            TransitionToNode(currentContainer.startNodeID);
        }

        /// <summary>
        /// 指定したIDのノードへ会話を進行・遷移させる
        /// </summary>
        private void TransitionToNode(string nodeID)
        {
            currentNode = currentContainer.GetNodeByID(nodeID);

            // 次のノードが見つからない、またはID画からの場合は会話終了
            if(currentNode == null)
            {
                EndDialogue();
                return;
            }

            // [駐]ステップ3でここに「テキスト内コマンド(タグ)の事前解析」が入ります。
            // 現段階では、生テキストをそのままUIへ渡します。
            string cleanText = currentNode.rawText;

            currentState = DialogueState.Typing;
            currentView.DisplaySentence(currentNode.speakerID, cleanText, OnTypingFinished);
        }

        /// <summary>
        /// UI(View)側で文字の表示(タイピング演出)が完了したときに自動で呼び出される 
        /// </summary>
        private void OnTypingFinished()
        {
            // ノードが選択肢データを持っているかどうかで次のステートを分岐
            if(currentNode.choices != null && currentNode.choices.Count > 0)
            {
                currentState = DialogueState.ShowChoices;
                currentView.ShowChoices(currentNode.choices, OnChoiceSelected);
            }
            else
            {
                currentState = DialogueState.WaitInput;
            }
        }

        /// <summary>
        /// プレイヤが選択しのボタンを押したときにUI(View)側から呼び出される
        /// </summary>
        /// <param name="index">選ばれた選択肢の要素番号</param>
        private void OnChoiceSelected(int index)
        {
            currentView.HideChoices();

            // 選択肢に埋め込まれている遷移先idを引っ張て着て次へ進む
            string nextID = currentNode.choices[index].targetNodeID;
            TransitionToNode(nextID);
        }

        /// <summary>
        /// プレイヤからの「決定キー/クリック」等の進行にゅりょくを処理する窓口
        /// </summary>
        public void HandleAdvanceInput()
        {
            if (currentState == DialogueState.Idle) return;

            // 1.文字の表示中にボタンが押されたら→タイピングを強制スキップして全表示に
            if(currentState == DialogueState.Typing)
            {
                currentView.ForceCompleteTyping();
                return;
            }

            // 2.全文表示完了後の入力町中にボタンが押されたら→一本道の次のノードへ進む
            if(currentState == DialogueState.WaitInput)
            {
                TransitionToNode(currentNode.nextNodeID);
            }
        }

        /// <summary>
        /// 会話イベント全体の終了処理
        /// </summary>
        private void EndDialogue()
        {
            currentState = DialogueState.Idle;
            currentView.CloseView();
            
            currentView = null;
            currentNode = null;

            // 登録されていた終了後アクションを実行(プレイヤの操作ロック解除など)
            onDialogueCompleteCallback?.Invoke();
            onDialogueCompleteCallback = null;
        }
    }
}