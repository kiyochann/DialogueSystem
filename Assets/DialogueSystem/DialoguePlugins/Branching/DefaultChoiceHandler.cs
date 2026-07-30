using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Logic;

namespace Runtime.Dialogue.Branching
{
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
                // BranchType.DefaultChoice のもの、あるいは汎用的な選択肢をUIに渡す
                view.ShowChoices(choices, (selectedIndex) =>
                {
                    view.HideChoices();
                    string nextID = choices[selectedIndex].targetNodeID;
                    onBranchDecided?.Invoke(nextID);
                });
                return true;
            }
            return false;
        }
    }
}