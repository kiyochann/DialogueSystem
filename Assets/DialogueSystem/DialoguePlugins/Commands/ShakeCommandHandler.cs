using System;
using System.Collections;
using UnityEngine;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Logic;

namespace Runtime.Dialogue.Commands
{
    /// <summary>
    /// [shake:target=window|canvas|both,magnitude=5,time=0.5,mode=pos|rot|both] を処理するコマンドハンドラー
    /// </summary>
    [HandlerInfo(
        description: "ダイアログウィンドウまたはCanvasGroupを指定した大きさ・時間で揺らします。",
        usage: "[shake:target=window,magnitude=10,time=0.3,mode=pos]"
    )]
    public class ShakeCommandHandler : MonoBehaviour, IDialogueCommandHandler
    {
        public string TargetCommandName => "shake";

        private Coroutine shakeCoroutine;

        private void Start()
        {
            if (DialogueEventDispatcher.Instance != null)
                DialogueEventDispatcher.Instance.RegisterHandler(this);
        }

        public void Execute(DialogueCommand command, Action onComplete)
        {
            // 引数取得
            string targetStr = command.GetString("target", "window").ToLower();
            float magnitude = command.GetFloat("magnitude", 5f);
            float time = command.GetFloat("time", 0.5f);
            string modeStr = command.GetString("mode", "pos").ToLower();

            // 対象取得
            Transform targetTrans = null;
            if (targetStr == "window" || targetStr == "both")
            {
                var view = UnityEngine.Object.FindObjectOfType<DialogueViewWindow>();
                if (view != null && view.windowRoot != null)
                    targetTrans = view.windowRoot.transform;
            }
            if (targetStr == "canvas" || targetStr == "both")
            {
                // CanvasGroup が付いているオブジェクトを探す（windowRoot にある想定）
                var view = UnityEngine.Object.FindObjectOfType<DialogueViewWindow>();
                if (view != null && view.windowRoot != null)
                {
                    var cg = view.windowRoot.GetComponent<CanvasGroup>();
                    if (cg != null) targetTrans = cg.transform;
                }
            }

            if (targetTrans == null)
            {
                Debug.LogWarning("[ShakeCommandHandler] 対象が見つかりませんでした。");
                onComplete?.Invoke();
                return;
            }

            // 既にシェイク中なら停止
            if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
            shakeCoroutine = StartCoroutine(ShakeRoutine(targetTrans, magnitude, time, modeStr, onComplete));
        }

        public void ForceComplete(DialogueCommand command)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }
            // 変形をリセット
            var view = UnityEngine.Object.FindObjectOfType<DialogueViewWindow>();
            if (view != null && view.windowRoot != null)
            {
                view.windowRoot.transform.localPosition = Vector3.zero;
                view.windowRoot.transform.localRotation = Quaternion.identity;
            }
        }

        private IEnumerator ShakeRoutine(Transform trans, float magnitude, float duration, string mode, Action onComplete)
        {
            Vector3 startPos = trans.localPosition;
            Quaternion startRot = trans.localRotation;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percent = elapsed / duration;
                float damper = 1f - Mathf.Clamp01(percent * 4f); // 減衰

                float x = 0f, y = 0f, z = 0f;
                if (mode.Contains("pos") || mode == "both")
                {
                    x = UnityEngine.Random.Range(-1f, 1f) * magnitude * damper;
                    y = UnityEngine.Random.Range(-1f, 1f) * magnitude * damper;
                }
                if (mode.Contains("rot") || mode == "both")
                {
                    z = UnityEngine.Random.Range(-1f, 1f) * magnitude * damper; // Z軸回転として使用
                }

                trans.localPosition = startPos + new Vector3(x, y, 0f);
                trans.localRotation = startRot * Quaternion.Euler(0f, 0f, z);

                yield return null;
            }

            // 元に戻す
            trans.localPosition = startPos;
            trans.localRotation = startRot;
            shakeCoroutine = null;
            onComplete?.Invoke();
        }
    }
}
