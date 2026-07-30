using System;
using System.Collections.Generic;
using UnityEngine;

// 👇 Core名前空間に統一
namespace Runtime.Dialogue.Core
{
    [Serializable]
    public class DialogueNode
    {
        public Vector2 graphPosition; // 👈 追加
        public string nextNodeID;     // 👈 追加

        public string nodeID;
        public string dialogueText;
        public List<ChoiceData> choices = new List<ChoiceData>();
    }

    [Serializable]
    public class ChoiceData
    {
        public string choiceText;
        public string targetNodeID;
        public BranchType branchType;
    }

    public enum BranchType
    {
        DefaultChoice,
        AutoBranch,
        SpecialUI
    }
}