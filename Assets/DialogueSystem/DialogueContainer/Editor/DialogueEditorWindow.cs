using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Dialogue
{
    using Runtime.Dialogue;

    /// <summary>
    /// エディタ上の単一ノードUIおよびデータのバインディングを担当するクラス
    /// 話者名・本文の入力フィールドや、動的な選択肢ポートの追加を制御します。
    /// </summary>
    public class DialogueNodeView : Node
    {
        public DialogueNode NodeData { get; private set; }

        public DialogueNodeView(DialogueNode nodeData)
        {
            NodeData = nodeData;
            userData = nodeData;
            title = "Dialogue Node";

            BuildUI();
            SetPosition(new Rect(nodeData.graphPosition, new Vector2(200, 150)));
        }

        /// <summary>
        /// ノード内部の基本UI（ボタン、テキストフィールド、In/Outポート）を構築します。
        /// </summary>
        private void BuildUI()
        {
            // 1. 選択肢追加ボタン
            Button addChoiceButton = new Button(() =>
            {
                var newChoice = new ChoiceData { choiceText = "新しい選択肢", targetNodeID = string.Empty };
                NodeData.choices.Add(newChoice);
                AddChoicePort(newChoice, NodeData.choices.Count - 1);
            })
            { text = "Add Choice" };
            titleContainer.Add(addChoiceButton);

            // 2. 話者ID入力フィールド
            TextField speakerField = new TextField("Speaker ID") { value = NodeData.speakerID };
            speakerField.RegisterValueChangedCallback(evt =>
            {
                NodeData.speakerID = evt.newValue;
            });
            extensionContainer.Add(speakerField);

            // 3. セリフ本文入力フィールド
            TextField textField = new TextField("Raw Text") { value = NodeData.rawText, multiline = true };
            textField.style.maxWidth = 250;
            textField.RegisterValueChangedCallback(evt =>
            {
                NodeData.rawText = evt.newValue;
            });
            extensionContainer.Add(textField);

            // 4. デフォルトの In / Out ポート作成
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            RefreshPorts();
            RefreshExpandedState();
        }

        /// <summary>
        /// 選択肢用の動的ポートおよびテキストフィールドを追加します。
        /// </summary>
        public void AddChoicePort(ChoiceData choiceData, int portIndex)
        {
            Port choicePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            choicePort.userData = portIndex; // ポートの固定インデックスをuserDataに保持

            TextField choiceTextField = new TextField() { value = choiceData.choiceText };
            choiceTextField.style.width = 120;
            choiceTextField.RegisterValueChangedCallback(evt =>
            {
                if (choicePort.userData is int idx && idx >= 0 && idx < NodeData.choices.Count)
                {
                    var updatedChoice = NodeData.choices[idx];
                    updatedChoice.choiceText = evt.newValue;
                    NodeData.choices[idx] = updatedChoice;
                }
            });

            choicePort.Add(choiceTextField);
            choicePort.portName = $"[{portIndex}]";

            outputContainer.Add(choicePort);
            RefreshPorts();
            RefreshExpandedState();

            // 選択肢が追加されたらデフォルトの Out ポートを非表示にする
            Port defaultOutPort = outputContainer.Children().OfType<Port>().FirstOrDefault(p => p.userData == null);
            if (defaultOutPort != null) defaultOutPort.style.display = DisplayStyle.None;
        }
    }
}