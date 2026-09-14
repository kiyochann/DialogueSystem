using Runtime.Dialogue.Core;
using Runtime.Dialogue.Audio;
using System;
using UnityEngine;

namespace Runtime.Dialogue.Commands
{
    /// <summary>
    /// [bgm:name=曲名] または [bgm:clip=曲名] を処理するコマンドハンドラー
    /// </summary>
    [HandlerInfo(description: "BGMを再生または停止します。", usage: "[bgm:name=BGMの名前, stop=false]")]
    public class BGMCommandHandler : MonoBehaviour, IDialogueCommandHandler
    {
        public string TargetCommandName => "bgm";

        private void Start()
        {
            if (DialogueEventDispatcher.Instance != null)
            {
                DialogueEventDispatcher.Instance.RegisterHandler(this);
            }
        }

        public void Execute(DialogueCommand command, Action onComplete)
        {
            PlayOrStopBGM(command);

            // BGMの切り替えは非同期で進行するため即座にコールバックを消化
            onComplete?.Invoke();
        }

        public void ForceComplete(DialogueCommand command)
        {
            // スキップ時もそのままBGMを正しく適用
            PlayOrStopBGM(command);
        }

        private void PlayOrStopBGM(DialogueCommand command)
        {
            // string として取得してから bool にパース（解析）する
            string stopStr = command.GetString("stop", "false");
            bool isStop = bool.TryParse(stopStr, out bool stopParsed) && stopParsed;

            if (isStop)
            {
                DialogueAudioManager.Instance?.StopBGM();
                return;
            }

            // "name" または "clip" からキーを取得
            string bgmKey = command.GetString("name", command.GetString("clip", ""));

            string loopStr = command.GetString("loop", "true");
            bool isLoop = !bool.TryParse(loopStr, out bool loopParsed) || loopParsed;

            if (!string.IsNullOrEmpty(bgmKey))
            {
                DialogueAudioManager.Instance?.PlayBGM(bgmKey, isLoop);
            }
        }
    }
}