using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Dialogue.Branching.Handlers
{
    public class DefaultChoiceHandler : MonoBehaviour, IDialogueBranchHandler
    {
        public int Priority => 0;

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
                view.ShowChoices(choices, (selectedIndex) =>
                {
                    // 👇 ここを追加！ボタンが選ばれたらUIから消去する
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