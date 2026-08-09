using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Branching;

[HandlerInfo("Gold判定分岐", "ConditionKeyを「Gold」に設定")]
public class GoldBranchHandler : MonoBehaviour, IDialogueBranchHandler
{
    public int Priority => 80;
    [SerializeField] private int playerGold = 1000;

    private void Start() => DialogueBranchDispatcher.Instance?.RegisterHandler(this);

    public bool TryHandleBranch(List<ChoiceData> choices, Action<string> onBranchDecided)
    {
        foreach (var c in choices)
        {
            if (c.conditionKey != "Gold") continue;

            // switch式でスッキリ判定
            bool met = c.conditionOperator switch
            {
                ConditionOperator.Equal => playerGold == c.conditionValue,
                ConditionOperator.Greater => playerGold > c.conditionValue,
                ConditionOperator.Less => playerGold < c.conditionValue,
                ConditionOperator.GreaterOrEqual => playerGold >= c.conditionValue,
                ConditionOperator.LessOrEqual => playerGold <= c.conditionValue,
                _ => false
            };

            if (met)
            {
                onBranchDecided?.Invoke(c.targetNodeID);
                return true;
            }
        }
        return false;
    }
}