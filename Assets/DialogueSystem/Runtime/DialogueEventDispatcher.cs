using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Dialogue
{
    /// <summary>
    /// すべてのダイアと具演出イベントを一手に引き受けて実行する配線盤(ディスパッチャ)
    /// </summary>
    public class DialogueEventDispatcher : MonoBehaviour
    {
        public static DialogueEventDispatcher Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) { Instance = this; }
            else { Destroy(gameObject); }
        }


        /// <summary>
        /// コマンドを通常実行する(非同期対応)
        /// </summary>
        /// <param name="command">実行するコマンド</param>
        /// <param name="onComplete">演出が完全に終了した時に呼ぶコールバック</param>
        public void ExecuteCommand(DialogueCommand command, Action onComplete)
        {
            command.IsExecuted = true;

            // コマンド名に応じて実際の処理へ分配
            switch (command.CommandName)
            {
                case "fade_out":
                    float fadeOutTime = GetFloatArg(command.Arguments, "time", 1.0f);
                    StartCoroutine(DummyFadeRoutine(fadeOutTime, true, onComplete));
                    break;

                case "fade_in":
                    float fadeInTime = GetFloatArg(command.Arguments, "time", 1.0f);
                    StartCoroutine(DummyFadeRoutine(fadeInTime, false, onComplete));
                    break;

                case "se":
                    string clipName = GetStringArg(command.Arguments, "clip", "default_se");
                    PlayDummySE(clipName);
                    onComplete?.Invoke(); // SE再生は一瞬で終わる（待機しない）ため即時完了通知
                    break;

                default:
                    // 登録のない未知のコマンドはクラッシュを防ぐため警告を吐いて即時完了
                    Debug.LogWarning($"[DialogueEventDispatcher] 未定義のコマンドが呼ばれました: {command.CommandName}");
                    onComplete?.Invoke();
                    break;
            }
        }

        /// <summary>
        /// プレイヤが会話をスキップした際、演出を一瞬で採取状態にワープさせる(強制終了)
        /// </summary>
        public void ForceCompleteCommand(DialogueCommand command)
        {
            if (command.IsExecuted) return; // すでに通常実行が終わっているならスルー
            command.IsExecuted = true;

            switch (command.CommandName)
            {
                case "fade_out":
                    Debug.Log("[Dispatcher-Skip] 画面を【一瞬で真っ暗】にします（演出ワープ）");
                    // ここに実際の画面管理クラスの「一瞬でアルファ値を1にする」コードを呼ぶ
                    break;

                case "fade_in":
                    Debug.Log("[Dispatcher-Skip] 画面を【一瞬で通常表示】にします（演出ワープ）");
                    // ここに実際の画面管理クラスの「一瞬でアルファ値を0にする」コードを呼ぶ
                    break;

                case "se":
                    // スキップ時はSEを鳴らさない、あるいは一瞬だけ鳴らす等の処理
                    break;
            }
        }


        #region ダミーの演出用ロジック(ここに実際のゲームの処理をつなぐ)
        private IEnumerator DummyFadeRoutine(float duration, bool isFadeOut, Action onComplete)
        {
            string type = isFadeOut ? "暗転(FadeOut)" : "明転(FadeIn)";
            Debug.Log($"[Dispatcher]{type}開始。{duration}秒 かけて実行中");
            yield return new WaitForSeconds(duration); // 指定秒待機

            Debug.Log($"[Dispatcher] {type}完了。");
            onComplete?.Invoke(); // 終わったことをマネージャに通知
        }

        private void PlayDummySE(string clipName)
        {
            Debug.Log($"[Dispatcher] SE再生: {clipName}");
        }
        #endregion

        #region パラメータ取得用の便利ヘルパー
        private float GetFloatArg(Dictionary<string, string> args, string key, float defaultValue)
        {
            if (args.TryGetValue(key, out string val) && float.TryParse(val, out float result)) return result;
            return defaultValue;
        }

        private string GetStringArg(Dictionary<string, string> args, string key, string defaultValue)
        {
            if (args.TryGetValue(key, out string val)) return val;
            return defaultValue;
        }
        #endregion
    }
}