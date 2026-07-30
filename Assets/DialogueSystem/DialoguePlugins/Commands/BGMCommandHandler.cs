using System;
using UnityEngine;

namespace Runtime.Dialogue.Commands
{
    /// <summary>
    /// [bgm:clip=曲名] を処理する後付けスクリプト
    /// </summary>
    public class BGMCommandHandler : MonoBehaviour, IDialogueCommandHandler
    {
        // 1. 担当するコマンド名を宣言
        public string TargetCommandName => "bgm";

        private void Start()
        {
            // 2. ゲーム開始時に、自分自身をディスパッチャへ登録する
            if (DialogueEventDispatcher.Instance != null)
            {
                DialogueEventDispatcher.Instance.RegisterHandler(this);
            }
        }

        // 3. 通常時の処理
        public void Execute(DialogueCommand command, Action onComplete)
        {
            string clipName = command.GetString("clip", "default_bgm");
            Debug.Log($"🎵 BGMを再生します: {clipName}");

            // BGM再生は一瞬で完了扱いにするので、すぐコールバックを呼ぶ
            onComplete?.Invoke();
        }

        // 4. スキップ時の処理（BGMの場合はそのまま再生させ続ける等）
        public void ForceComplete(DialogueCommand command)
        {
            string clipName = command.GetString("clip", "default_bgm");
            Debug.Log($"🎵 (スキップ) BGMを瞬時に切り替えます: {clipName}");
        }
    }
}