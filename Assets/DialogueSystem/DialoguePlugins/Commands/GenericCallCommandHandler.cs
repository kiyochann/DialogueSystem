using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Logic;

namespace Runtime.Dialogue.Plugins.Commands
{
    [Serializable]
    public class DirectTargetMapping
    {
        [Tooltip("タグで指定する識別ID（例: CameraManager, SoundManager）")]
        public string targetID;
        [Tooltip("対象のGameObject")]
        public GameObject targetObject;
    }

    [HandlerInfo("オブジェクトのメソッドを柔軟な指定方式で呼び出します",
                 "使い方1: [call:mode=nearest,script=EnemyController,method=Attack]\n" +
                 "使い方2: [call:mode=tag,tag=Boss,method=PlayAnim,arg=Roar]\n" +
                 "使い方3: [call:mode=direct,target=CameraRig,script=CameraShake,method=Shake]")]
    public class GenericCallCommandHandler : MonoBehaviour, IDialogueCommandHandler
    {
        public string TargetCommandName => "call";

        [Header("指定方式3用: Inspector登録リスト")]
        [SerializeField] private List<DirectTargetMapping> directTargets = new List<DirectTargetMapping>();

        [Header("指定方式1用: 距離判定の基準位置（空ならCamera.mainを使用）")]
        [SerializeField] private Transform referenceTransform;

        private void Start()
        {
            if (DialogueEventDispatcher.Instance != null)
            {
                DialogueEventDispatcher.Instance.RegisterHandler(this);
            }

            if (referenceTransform == null && Camera.main != null)
            {
                referenceTransform = Camera.main.transform;
            }
        }

        public void Execute(DialogueCommand command, Action onComplete)
        {
            string mode = command.GetString("mode", "direct").ToLower();
            string scriptName = command.GetString("script", "");
            string methodName = command.GetString("method", "");
            string arg = command.GetString("arg", null);

            GameObject targetObj = null;

            switch (mode)
            {
                case "nearest": // 1. 指定スクリプトを持つ中で「最も近い」オブジェクト
                    targetObj = FindNearestObjectWithScript(scriptName);
                    break;

                case "tag": // 2. 自前タグ（DialogueCustomTag）で指定
                    string customTag = command.GetString("tag", "");
                    targetObj = FindObjectByCustomTag(customTag);
                    break;

                case "direct": // 3. Inspector登録から指定
                default:
                    string targetID = command.GetString("target", "");
                    var mapping = directTargets.Find(t => t.targetID.Equals(targetID, StringComparison.OrdinalIgnoreCase));
                    if (mapping != null) targetObj = mapping.targetObject;
                    break;
            }

            if (targetObj != null && !string.IsNullOrEmpty(methodName))
            {
                InvokeMethod(targetObj, scriptName, methodName, arg);
            }
            else
            {
                Debug.LogWarning($"[GenericCall] 対象オブジェクトまたはメソッドが見つかりません (Mode: {mode})");
            }

            onComplete?.Invoke();
        }

        // 方式1: アセンブリ参照に左右されない安全なオブジェクト取得
        private GameObject FindNearestObjectWithScript(string scriptName)
        {
            if (referenceTransform == null || string.IsNullOrEmpty(scriptName)) return null;

#pragma warning disable CS0618
            var allMonoBehaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
#pragma warning restore CS0618

            GameObject nearestObj = null;
            float minDistance = float.MaxValue;

            foreach (var mb in allMonoBehaviours)
            {
                if (mb == null) continue;

                if (mb.GetType().Name.Equals(scriptName, StringComparison.OrdinalIgnoreCase))
                {
                    float dist = Vector3.Distance(referenceTransform.position, mb.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        nearestObj = mb.gameObject;
                    }
                }
            }

            return nearestObj;
        }

        // 方式2: アセンブリ参照に左右されない安全なタグ検索
        private GameObject FindObjectByCustomTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return null;

#pragma warning disable CS0618
            var tags = UnityEngine.Object.FindObjectsOfType<DialogueCustomTag>();
#pragma warning restore CS0618

            foreach (var t in tags)
            {
                if (t != null && t.TagName.Equals(tag, StringComparison.OrdinalIgnoreCase))
                {
                    return t.gameObject;
                }
            }
            return null;
        }

        private void InvokeMethod(GameObject targetObj, string scriptName, string methodName, string arg)
        {
            var components = targetObj.GetComponents<MonoBehaviour>();

            foreach (var comp in components)
            {
                if (comp == null) continue;

                Type type = comp.GetType();

                if (!string.IsNullOrEmpty(scriptName) &&
                    !type.Name.Equals(scriptName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                MethodInfo method = type.GetMethod(methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (method != null)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == 0)
                    {
                        method.Invoke(comp, null);
                        return;
                    }
                    else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                    {
                        method.Invoke(comp, new object[] { arg });
                        return;
                    }
                }
            }
        }

        public void ForceComplete(DialogueCommand command)
        {
            Execute(command, null);
        }
    }
}