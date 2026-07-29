using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Runtime.Dialogue.Branching
{
    /// <summary>
    /// どの分岐プラグインに処理を任せるかを決定する配線盤
    /// </summary>
    public class DialogueBranchDispatcher : MonoBehaviour
    {
        public static DialogueBranchDispatcher Instance { get; private set; }

        private List<IDialogueBranchHandler> handlers = new List<IDialogueBranchHandler>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void RegisterHandler(IDialogueBranchHandler handler)
        {
            if (!handlers.Contains(handler))
            {
                handlers.Add(handler);
                // 優先度(Priority)が高い順に並び替える
                handlers = handlers.OrderByDescending(h => h.Priority).ToList();
            }
        }

        /// <summary>
        /// 上から順にハンドラーに「これ処理できる？」と聞き、処理されたら終了する
        /// </summary>
        public void ProcessBranches(List<ChoiceData> choices, Action<string> onBranchDecided)
        {
            foreach (var handler in handlers)
            {
                if (handler.TryHandleBranch(choices, onBranchDecided))
                {
                    return; // 誰かが引き受けたらそこで評価終了
                }
            }

            Debug.LogWarning("[BranchDispatcher] どの分岐ハンドラーも処理しませんでした。");
            onBranchDecided?.Invoke(string.Empty);
        }
    }
}