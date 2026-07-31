using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Branching;

public class GoldBranchHandler : MonoBehaviour, IDialogueBranchHandler
{
    public int Priority => 80;

    [SerializeField] private int playerGold = 1000; // 現在の所持金

    private void Start()
    {
        if (DialogueBranchDispatcher.Instance != null)
            DialogueBranchDispatcher.Instance.RegisterHandler(this);
    }

    public bool TryHandleBranch(List<ChoiceData> choices, Action<string> onBranchDecided)
    {
        foreach (var choice in choices)
        {
            if (choice.conditionKey == "Gold")
            {
                bool isConditionMet = false;

                // 👇 追加: 演算子による判定
                switch (choice.conditionOperator)
                {
                    case ConditionOperator.Equal:
                        isConditionMet = playerGold == choice.conditionValue;
                        break;
                    case ConditionOperator.Greater:
                        isConditionMet = playerGold > choice.conditionValue;
                        break;
                    case ConditionOperator.Less:
                        isConditionMet = playerGold < choice.conditionValue;
                        break;
                    case ConditionOperator.GreaterOrEqual:
                        isConditionMet = playerGold >= choice.conditionValue;
                        break;
                    case ConditionOperator.LessOrEqual:
                        isConditionMet = playerGold <= choice.conditionValue;
                        break;
                }

                if (isConditionMet)
                {
                    Debug.Log($"[GoldBranch] 所持金クリア");
                    onBranchDecided?.Invoke(choice.targetNodeID);
                    return true;
                }
            }
        }
        return false;
    }
}