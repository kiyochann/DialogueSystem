using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Branching;
using Runtime.Dialogue;

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
        [SerializeField, Tooltip("ダイアログUIのView。未指定の場合はシーン内から自動取得します")]
        private DialogueViewWindow defaultView;

        private IDialogueView currentView;
        public IDialogueView CurrentView
        {
            get
            {
                if (currentView == null)
                {
                    ResolveDefaultView();
                }
                return currentView;
            }
            private set => currentView = value;
        }

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

        private bool isSkipMode = false;
        public bool IsSkipMode => isSkipMode;

        // 外部入力待ち（方向選択・演出等）フラグ
        private bool isWaitingExternalInput = false;
        public bool IsWaitingExternalInput => isWaitingExternalInput;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            ResolveDefaultView();
        }

        private void Start()
        {
            if (currentView == null)
            {
                ResolveDefaultView();
            }
        }

        /// <summary>
        /// シーンまたはInspectorからIDialogueView（DialogueViewWindow）を自動解決します。
        /// </summary>
        public void ResolveDefaultView()
        {
            if (currentView != null) return;

            // 1. Inspectorで指定された defaultView があれば優先使用
            if (defaultView != null)
            {
                RegisterView(defaultView);
                return;
            }

            // 2. シーン内の DialogueViewWindow を検索（非アクティブも含む）
            var foundWindow = UnityEngine.Object.FindAnyObjectByType<DialogueViewWindow>(FindObjectsInactive.Include);
            if (foundWindow != null)
            {
                RegisterView(foundWindow);
                return;
            }

            // 3. DialogueViewWindow 以外の IDialogueView 実装コンポーネントを検索（非アクティブも含む）
            var allMonoBehaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var mb in allMonoBehaviours)
            {
                if (mb is IDialogueView customView)
                {
                    RegisterView(customView);
                    return;
                }
            }
        }

        // 外部入力待ちのロック/解除
        public void SetExternalInputLock(bool locked)
        {
            isWaitingExternalInput = locked;
            if (locked)
            {
                DisableAutoMode();
                DisableSkipMode();
            }
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
            if (CurrentView == null)
            {
                ResolveDefaultView();
            }

            if (CurrentView == null)
            {
                Debug.LogWarning("[DialogueManager] 有効な IDialogueView（DialogueViewWindow等）が見つかりません。会話UIが表示されない可能性があります。");
            }

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

                    // 外部入力待ち中でない場合のみオート/スキップを進行させる
                    if (!isWaitingExternalInput)
                    {
                        if (isSkipMode)
                        {
                            StartCoroutine(SkipAdvanceRoutine());
                        }
                        else if (isAutoMode)
                        {
                            StartAutoWait(cleanText.Length);
                        }
                    }
                }
            });

            if (isSkipMode && CurrentState == DialogueState.Playing)
            {
                CurrentView?.ForceCompleteTyping();
            }
        }

        public void HandleAdvanceInput()
        {
            // 外部入力待ち状態のときは進行入力を完全に無視する
            if (isWaitingExternalInput)
            {
                return;
            }

            if (autoWaitCoroutine != null)
            {
                StopCoroutine(autoWaitCoroutine);
                autoWaitCoroutine = null;
            }
            DisableAutoMode();
            DisableSkipMode();

            if (CurrentState == DialogueState.Playing)
            {
                CurrentView?.ForceCompleteTyping();
            }
            else if (CurrentState == DialogueState.WaitingForAdvance)
            {
                AdvanceToNextNode();
            }
        }

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

        public void EndDialogue()
        {
            CurrentState = DialogueState.Ended;
            CurrentView?.CloseView();

            Action callback = onDialogueEndedCallback;
            onDialogueEndedCallback = null;
            callback?.Invoke();

            CurrentState = DialogueState.Idle;

            // 会話が終わったらモードをオフにし、外部入力ロックも解除
            isAutoMode = false;
            isSkipMode = false;
            isWaitingExternalInput = false;
        }

        // --- Auto Mode ---
        public void ToggleAutoMode()
        {
            if (isWaitingExternalInput) return;

            isAutoMode = !isAutoMode;

            if (isAutoMode)
            {
                DisableSkipMode();

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
            if (isAutoMode) isAutoMode = false;
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
            if (!isWaitingExternalInput)
            {
                AdvanceToNextNode();
            }
        }

        // --- Skip Mode ---
        public void ToggleSkipMode()
        {
            if (isWaitingExternalInput) return;

            isSkipMode = !isSkipMode;

            if (isSkipMode)
            {
                DisableAutoMode();

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
            if (isSkipMode) isSkipMode = false;
        }

        private System.Collections.IEnumerator SkipAdvanceRoutine()
        {
            yield return null;

            if (isSkipMode && CurrentState == DialogueState.WaitingForAdvance && !isWaitingExternalInput)
            {
                AdvanceToNextNode();
            }
        }
    }
}
