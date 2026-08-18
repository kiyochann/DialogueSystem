using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Dialogue.Core
{
    [Serializable]
    public class ExpressionData
    {
        public string expressionID;
        public Sprite faceSprite;
    }

    [CreateAssetMenu(fileName = "NewCharacterProfile", menuName = "Dialogue/Character Profile")]
    public class CharacterProfile : ScriptableObject
    {
        public string characterID;
        public string displayName;
        public List<ExpressionData> expressions = new List<ExpressionData>();

        public Sprite GetExpression(string id)
        {
            var data = expressions.Find(e => e.expressionID == id);
            return data?.faceSprite;
        }
    }
}