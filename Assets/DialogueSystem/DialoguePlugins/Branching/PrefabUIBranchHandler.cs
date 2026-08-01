using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Branching;

namespace Runtime.Dialogue.Branching
{
    public class PrefabUIBranchHandler : MonoBehaviour, IDialogueBranchHandler
    {
        [Header("UI Settings")]
        [SerializeField] private GameObject branchUIPrefab;
        [SerializeField] private Transform uiParent;

        public int Priority => 50;
        private GameObject currentUIInstance;

        private void Start()
        {
            if (DialogueBranchDispatcher.Instance != null)
            {
                DialogueBranchDispatcher.Instance.RegisterHandler(this);
            }
        }

        public bool TryHandleBranch(List<ChoiceData> choices, Action<string> onBranchDecided)
        {
            if (choices == null || choices.Count == 0) return false;

            // 👈 修正: 自身のクラス名とエディタで選ばれた名前が一致するか判定
            if (choices[0].branchType != this.GetType().Name) return false;

            if (branchUIPrefab == null)
            {
                Debug.LogError("[PrefabUIBranchHandler] Branch UI Prefab が設定されていません。");
                return false;
            }

            currentUIInstance = uiParent != null ? Instantiate(branchUIPrefab, uiParent) : Instantiate(branchUIPrefab);
            Button[] availableButtons = currentUIInstance.GetComponentsInChildren<Button>(true);
            int processCount = Mathf.Min(choices.Count, availableButtons.Length);

            if (processCount == 0)
            {
                Destroy(currentUIInstance);
                return false;
            }

            for (int i = 0; i < availableButtons.Length; i++)
            {
                Button btn = availableButtons[i];
                if (i < processCount)
                {
                    var choice = choices[i];
                    btn.gameObject.SetActive(true);

                    var textComponent = btn.GetComponentInChildren<Text>();
                    if (textComponent != null) textComponent.text = choice.choiceText;

                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        if (currentUIInstance != null) Destroy(currentUIInstance);
                        onBranchDecided?.Invoke(choice.targetNodeID);
                    });
                }
                else
                {
                    btn.gameObject.SetActive(false);
                }
            }
            return true;
        }
    }
}