using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Runtime.Dialogue.Core;

namespace DialogueSystem.Editor
{
    public class DialogueNodeView : Node
    {
        public DialogueNode NodeData => nodeData;

        public DialogueNode nodeData;
        public Port inputPort;
        public Port nextPort;

        public DialogueNodeView(DialogueNode nodeData)
        {
            this.nodeData = nodeData;
            this.title = nodeData.nodeID;
            this.viewDataKey = nodeData.nodeID;

            CreateInputPort();
            CreateOutputPort();
            CreateTextFields();

            for (int i = 0; i < nodeData.choices.Count; i++)
            {
                CreateChoiceView(nodeData.choices[i]);
                AddChoicePort(nodeData.choices[i], i);
            }

            RefreshExpandedState();
            RefreshPorts();

            // 👈 追加: 初期化時にNextポートの表示状態を更新
            UpdateNextPortVisibility();
        }

        private void CreateInputPort()
        {
            inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);
        }

        private void CreateOutputPort()
        {
            // 👈 変更: nextPort変数に保持する
            nextPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            nextPort.portName = "Next";
            nextPort.userData = null;
            outputContainer.Add(nextPort);
        }

        private void CreateTextFields()
        {
            // 🎯 目標1: ノードIDを自由に変更できる入力欄
            var idField = new TextField("Node ID") { value = nodeData.nodeID };
            idField.RegisterValueChangedCallback(evt =>
            {
                nodeData.nodeID = evt.newValue;
                this.title = evt.newValue; // タイトル表示も連動させる
            });
            extensionContainer.Add(idField);

            // 🎯 目標4: 分岐タイプを「ノード単位」にまとめる
            BranchType currentBranchType = nodeData.choices.Count > 0 ? nodeData.choices[0].branchType : BranchType.DefaultChoice;
            var branchTypeField = new EnumField("Branch Type", currentBranchType);
            branchTypeField.RegisterValueChangedCallback(evt =>
            {
                // 見た目は1つにし、裏側で全選択肢のタイプを一括変更する（ランタイム変更不要）
                var newType = (BranchType)evt.newValue;
                foreach (var choice in nodeData.choices)
                {
                    choice.branchType = newType;
                }
            });
            extensionContainer.Add(branchTypeField);

            // 🎯 目標5: セリフが右端で自動的に改行（折り返し）されるようにする
            var textTextField = new TextField("Text")
            {
                value = nodeData.dialogueText,
                multiline = true // 改行を許可
            };
            textTextField.style.whiteSpace = WhiteSpace.Normal; // 👈 自動折り返し設定
            textTextField.style.minHeight = 60;                 // 👈 高さを少し確保

            textTextField.RegisterValueChangedCallback(evt =>
            {
                nodeData.dialogueText = evt.newValue;
            });
            extensionContainer.Add(textTextField);

            var addChoiceButton = new Button(() =>
            {
                var currentType = (BranchType)branchTypeField.value;
                var newChoice = new ChoiceData { choiceText = "新しい選択肢", targetNodeID = "", branchType = currentType };
                nodeData.choices.Add(newChoice);
                CreateChoiceView(newChoice);
                AddChoicePort(newChoice, nodeData.choices.Count - 1);
            })
            { text = "Add Choice" };
            extensionContainer.Add(addChoiceButton);
        }

        public void CreateChoiceView(ChoiceData choice)
        {
            VisualElement choiceContainer = new VisualElement();
            choiceContainer.style.flexDirection = FlexDirection.Row;

            
            var choiceTextField = new TextField
            {
                value = choice.choiceText
            };
            choiceTextField.RegisterValueChangedCallback(evt =>
            {
                choice.choiceText = evt.newValue;
                var port = outputContainer.Children().OfType<Port>().FirstOrDefault(p => p.userData is int idx && idx == nodeData.choices.IndexOf(choice));
                if (port != null) port.portName = evt.newValue;
            });
            choiceContainer.Add(choiceTextField);

            var deleteButton = new Button(() =>
            {
                int idx = nodeData.choices.IndexOf(choice);
                if (idx >= 0)
                {
                    nodeData.choices.RemoveAt(idx);

                    var portToRemove = outputContainer.Children().OfType<Port>().FirstOrDefault(p => p.userData is int i && i == idx);
                    if (portToRemove != null)
                    {
                        if (portToRemove.connected)
                        {
                            var edges = portToRemove.connections.ToList();
                            foreach (var e in edges)
                            {
                                if (e == null) continue;
                                try { if (e.input != null) e.input.Disconnect(e); if (e.output != null) e.output.Disconnect(e); } catch { }
                                e.RemoveFromHierarchy();
                            }
                        }
                        outputContainer.Remove(portToRemove);
                    }

                    var ports = outputContainer.Children().OfType<Port>().Where(p => p.userData is int).ToList();
                    for (int i = 0; i < ports.Count; i++)
                    {
                        ports[i].userData = i;
                        if (i < nodeData.choices.Count) ports[i].portName = nodeData.choices[i].choiceText;
                    }
                }
                extensionContainer.Remove(choiceContainer);
                RefreshPorts();
                RefreshExpandedState();

                UpdateNextPortVisibility();
            })
            {
                text = "X"
            };
            choiceContainer.Add(deleteButton);

            extensionContainer.Add(choiceContainer);
        }

        public void AddChoicePort(ChoiceData choice, int choiceIndex)
        {
            var generatedPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            generatedPort.portName = choice.choiceText;
            generatedPort.userData = choiceIndex;
            outputContainer.Add(generatedPort);
            RefreshPorts();
            RefreshExpandedState();

            UpdateNextPortVisibility();
        }

        private void UpdateNextPortVisibility()
        {
            if (nextPort != null)
            {
                nextPort.style.display = nodeData.choices.Count > 0 ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }
    }
}
