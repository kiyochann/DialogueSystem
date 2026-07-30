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
        public Port outputPort;

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
        }

        private void CreateInputPort()
        {
            inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "In";
            inputContainer.Add(inputPort);
        }

        private void CreateOutputPort()
        {
            var defaultOutput = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            defaultOutput.portName = "Next";
            defaultOutput.userData = null;
            outputContainer.Add(defaultOutput);
        }

        private void CreateTextFields()
        {
            var textTextField = new TextField
            {
                value = nodeData.dialogueText
            };
            textTextField.RegisterValueChangedCallback(evt =>
            {
                nodeData.dialogueText = evt.newValue;
            });
            extensionContainer.Add(textTextField);

            var addChoiceButton = new Button(() =>
            {
                var newChoice = new ChoiceData { choiceText = "新しい選択肢", targetNodeID = "" };
                nodeData.choices.Add(newChoice);
                CreateChoiceView(newChoice);
                AddChoicePort(newChoice, nodeData.choices.Count - 1);
            })
            {
                text = "Add Choice"
            };
            extensionContainer.Add(addChoiceButton);
        }

        public void CreateChoiceView(ChoiceData choice)
        {
            VisualElement choiceContainer = new VisualElement();
            choiceContainer.style.flexDirection = FlexDirection.Row;

            var branchTypeField = new EnumField(choice.branchType);
            branchTypeField.RegisterValueChangedCallback(evt =>
            {
                choice.branchType = (BranchType)evt.newValue;
            });
            choiceContainer.Add(branchTypeField);

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
                                try
                                {
                                    if (e.input != null) e.input.Disconnect(e);
                                    if (e.output != null) e.output.Disconnect(e);
                                }
                                catch { }
                                e.RemoveFromHierarchy();
                            }
                        }
                        outputContainer.Remove(portToRemove);
                    }

                    var ports = outputContainer.Children().OfType<Port>().Where(p => p.userData is int).ToList();
                    for (int i = 0; i < ports.Count; i++)
                    {
                        ports[i].userData = i;
                        if (i < nodeData.choices.Count)
                        {
                            ports[i].portName = nodeData.choices[i].choiceText;
                        }
                    }
                }

                extensionContainer.Remove(choiceContainer);
                RefreshPorts();
                RefreshExpandedState();
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
        }
    }
}
