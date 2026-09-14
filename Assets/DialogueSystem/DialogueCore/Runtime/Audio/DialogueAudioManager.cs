using System;
using UnityEngine;

namespace Runtime.Dialogue.Audio
{
    public class DialogueAudioManager : MonoBehaviour
    {
        public static DialogueAudioManager Instance { get; private set; }

        [Header("Audio Database")]
        [SerializeField] private DialogueAudioDatabase audioDatabase;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource seSource;
        [SerializeField] private AudioSource voiceSource;
        [SerializeField] private AudioSource typingSource;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void PlayBGM(string key, bool loop = true)
        {
            if (audioDatabase == null) return;
            var clip = audioDatabase.GetClip(key, AudioType.BGM);
            if (clip == null || bgmSource == null) return;

            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }

        public void StopBGM() => bgmSource?.Stop();

        public void PlaySE(string key)
        {
            if (audioDatabase == null) return;
            var clip = audioDatabase.GetClip(key, AudioType.SE);
            if (clip != null && seSource != null)
                seSource.PlayOneShot(clip);
        }

        public void PlayVoice(string key, Action onComplete = null)
        {
            if (audioDatabase == null) return;
            var clip = audioDatabase.GetClip(key, AudioType.Voice);
            if (clip != null && voiceSource != null)
            {
                voiceSource.clip = clip;
                voiceSource.Play();
            }
        }

        public bool IsVoicePlaying => voiceSource != null && voiceSource.isPlaying;

        public void PlayTypingSound(string key = null, float pitchMin = 0.95f, float pitchMax = 1.05f)
        {
            if (typingSource == null) return;
            if (IsVoicePlaying) return; // ボイス再生中はタイプ音を消音

            AudioClip clipToPlay = null;
            if (!string.IsNullOrEmpty(key) && audioDatabase != null)
            {
                clipToPlay = audioDatabase.GetClip(key, AudioType.Typing);
            }

            if (clipToPlay == null) clipToPlay = typingSource.clip;
            if (clipToPlay == null) return;

            typingSource.pitch = UnityEngine.Random.Range(pitchMin, pitchMax);
            typingSource.PlayOneShot(clipToPlay);
        }
    }
}