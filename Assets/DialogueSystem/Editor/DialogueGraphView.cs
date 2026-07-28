using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Dialogue
{
    using Runtime.Dialogue; // 本編用のデータ構造を参照

    /// <summary>
    /// ノードを配置し、線を引くための無限キャンバスエリア（分岐・セーブロード完全修正版）
    /// </summary>
    public class DialogueGraphView : GraphView
    {
        public DialogueGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            ConstructContextMenu();
        }

        private void ConstructContextMenu()
        {
            this.AddManipulator(new ContextualMenuManipulator(menuEvent =>
            {
                menuEvent.menu.AppendAction("Create Dialogue Node", actionEvent =>
                {
                    Vector2 mousePos = actionEvent.eventInfo.localMousePosition;
                    Vector2 graphPos = viewTransform.matrix.inverse.MultiplyPoint(mousePos);

                    CreateNodeView(new DialogueNode()
                    {
                        nodeID = Guid.NewGuid().ToString(),
                        speakerID = "New Speaker",
                        rawText = "セリフをここに入力してください",
                        graphPosition = graphPos
                    });
                });
            }));
        }

        public Node CreateNodeView(DialogueNode nodeData)
        {
            Node nodeView = new Node();
            nodeView.title = "Dialogue Node";
            nodeView.userData = nodeData;

            Button addChoiceButton = new Button(() =>
            {
                var newChoice = new ChoiceData { choiceText = "新しい選択肢", targetNodeID = string.Empty };
                nodeData.choices.Add(newChoice);
                AddChoicePort(nodeView, newChoice, nodeView.outputContainer.Query<Port>().ToList().Count);
            })
            { text = "Add Choice" };
            nodeView.titleContainer.Add(addChoiceButton);

            TextField speakerField = new TextField("Speaker ID") { value = nodeData.speakerID };
            speakerField.RegisterValueChangedCallback(evt => {
                var d = nodeView.userData as DialogueNode;
                if (d != null) d.speakerID = evt.newValue;
            });
            nodeView.extensionContainer.Add(speakerField);

            TextField textField = new TextField("Raw Text") { value = nodeData.rawText, multiline = true };
            textField.style.maxWidth = 250;
            textField.RegisterValueChangedCallback(evt => {
                var d = nodeView.userData as DialogueNode;
                if (d != null) d.rawText = evt.newValue;
            });
            nodeView.extensionContainer.Add(textField);

            Port inputPort = nodeView.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "In";
            nodeView.inputContainer.Add(inputPort);

            Port outputPort = nodeView.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort.portName = "Out";
            nodeView.outputContainer.Add(outputPort);

            nodeView.SetPosition(new Rect(nodeData.graphPosition, new Vector2(200, 150)));
            nodeView.RefreshPorts();
            nodeView.RefreshExpandedState();

            AddElement(nodeView);
            return nodeView;
        }

        public void AddChoicePort(Node nodeView, ChoiceData choiceData, int portIndex)
        {
            Port choicePort = nodeView.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            choicePort.userData = portIndex;

            TextField choiceTextField = new TextField() { value = choiceData.choiceText };
            choiceTextField.style.width = 120;
            choiceTextField.RegisterValueChangedCallback(evt =>
            {
                var nodeData = nodeView.userData as DialogueNode;
                // 現在のポートの並び順から、対応する選択肢のインデックスを正確に逆引きする親切設計
                int actualIndex = nodeView.outputContainer.IndexOf(choicePort) - 1; // 通常のOutポートの分をマイナス
                if (nodeData != null && actualIndex >= 0 && actualIndex < nodeData.choices.Count)
                {
                    var updatedChoice = nodeData.choices[actualIndex];
                    updatedChoice.choiceText = evt.newValue;
                    nodeData.choices[actualIndex] = updatedChoice;
                }
            });

            choicePort.Add(choiceTextField);
            choicePort.portName = $"[{portIndex}]";

            nodeView.outputContainer.Add(choicePort);
            nodeView.RefreshPorts();
            nodeView.RefreshExpandedState();

            Port defaultOutPort = nodeView.outputContainer.Children().OfType<Port>().FirstOrDefault(p => p.userData == null);
            if (defaultOutPort != null) defaultOutPort.style.display = DisplayStyle.None;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        public void ClearGraph()
        {
            var elements = graphElements.ToList();
            foreach (var element in elements)
            {
                RemoveElement(element);
            }
        }

        public void SaveGraph(DialogueContainer container) => DialogueGraphViewIO.Save(this, container);
        public void LoadGraph(DialogueContainer container) => DialogueGraphViewIO.Load(this, container);
    }
}
