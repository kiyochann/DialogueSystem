using Runtime.Dialogue.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Dialogue.Commands
{
    [HandlerInfo(description: "指定された名前のSE（効果音）を再生します。", usage: "[se:clip=SEの名前]")]
    [RequireComponent(typeof(AudioSource))]
    public class SECommandHandler : MonoBehaviour, IDialogueCommandHandler
    {
        public string TargetCommandName => "se";

        [Tooltip("ここにInspectorからSEを登録してください")]
        [SerializeField] private List<AudioClip> audioClips = new List<AudioClip>();

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (DialogueEventDispatcher.Instance != null)
                DialogueEventDispatcher.Instance.RegisterHandler(this);
        }

        public void Execute(DialogueCommand command, Action onComplete)
        {
            string clipName = command.GetString("clip", "");
            AudioClip clip = audioClips.Find(c => c.name == clipName);

            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
                Debug.Log($"🔊 [SE] 再生しました: {clipName}");
            }
            else
            {
                Debug.LogWarning($"[SE] 指定されたクリップ '{clipName}' がInspectorに登録されていません。");
            }

            // SEは鳴らしっぱなしで次へ進むので即コールバック
            onComplete?.Invoke();
        }

        public void ForceComplete(DialogueCommand command)
        {
            // スキップ時も一応鳴らす
            Execute(command, null);
        }
    }
}