using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Dialogue.Core
{
    [Serializable]
    public class DialogueNode
    {
        public Vector2 graphPosition;
        public string nextNodeID;

        public string nodeID;
        public string speakerName; // 話者名
        public string dialogueText;
        public List<ChoiceData> choices = new List<ChoiceData>();
    }

    [Serializable]
    public class ChoiceData
    {
        public string choiceText;
        public string targetNodeID;
        public BranchType branchType;

        // 分岐設定用データ
        public string conditionKey = "";
        public int conditionValue = 0;
    }

    public enum BranchType
    {
        DefaultChoice,
        AutoBranch,
        SpecialUI
    }
}