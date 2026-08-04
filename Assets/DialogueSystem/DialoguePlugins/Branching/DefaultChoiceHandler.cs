using System;
using System.Collections.Generic;
using System.Linq; // 👈 追加: リストの抽出(Where)を使うため
using UnityEngine;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Logic;

namespace Runtime.Dialogue.Branching
{
    [HandlerInfo(description: "プレイヤーが画面上のボタンをクリックして選択する、標準的な選択肢を表示します。", usage: "ノードエディタ上で、選択肢のBranchTypeに「DefaultChoice」を指定してください。")]
    public class DefaultChoiceHandler : MonoBehaviour, IDialogueBranchHandler
    {
        public int Priority => 0; // 最低優先度（フォールバック用）

        private void Start()
        {
            if (DialogueBranchDispatcher.Instance != null)
                DialogueBranchDispatcher.Instance.RegisterHandler(this);
        }

        public bool TryHandleBranch(List<ChoiceData> choices, Action<string> onBranchDecided)
        {
            var view = DialogueManager.Instance.CurrentView;
            if (view != null)
            {
                // 👈 修正: AutoBranch等の見えない分岐を除外し、通常のボタン(DefaultChoice)だけを抽出する
                var displayChoices = choices.Where(c => c.branchType == "DefaultChoice").ToList();

                // もし画面に出せる選択肢が1つもない場合は、このハンドラーでは処理できないとして false を返す
                if (displayChoices.Count == 0) return false;

                // 抽出したリストだけをUIに渡してボタンを作る
                view.ShowChoices(displayChoices, (selectedIndex) =>
                {
                    view.HideChoices();
                    // 選ばれたボタンの遷移先IDを取得して進行
                    string nextID = displayChoices[selectedIndex].targetNodeID;
                    onBranchDecided?.Invoke(nextID);
                });
                return true;
            }
            return false;
        }
    }
}