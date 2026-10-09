using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Antigravity.Bridge
{
    [InitializeOnLoad]
    public static class AntigravityBridge
    {
        private const int DefaultPort = 8088;
        private static int currentPort = DefaultPort;
        private static HttpListener listener;
        private static Thread listenerThread;
        private static bool isRunning = false;

        private static readonly ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();

        static AntigravityBridge()
        {
            EditorApplication.update += ProcessMainThreadQueue;
            AssemblyReloadEvents.beforeAssemblyReload += StopServer;
            EditorApplication.quitting += StopServer;
            StartServer();
        }

        [MenuItem("Tools/Antigravity/Restart Bridge Server")]
        public static void RestartServer()
        {
            StopServer();
            StartServer();
        }

        [MenuItem("Tools/Antigravity/Bridge Status")]
        public static void ShowStatus()
        {
            EditorUtility.DisplayDialog(
                "Antigravity Bridge",
                $"Status: {(isRunning ? "Running ✓" : "Stopped")}\nPort: {currentPort}\nURL: http://127.0.0.1:{currentPort}/",
                "OK");
        }

        private static void StartServer()
        {
            if (isRunning) return;

            int[] ports = { 8088, 8089, 8090, 8091 };
            foreach (var port in ports)
            {
                try
                {
                    listener = new HttpListener();
                    listener.Prefixes.Add($"http://127.0.0.1:{port}/");
                    listener.Start();
                    currentPort = port;
                    isRunning = true;
                    break;
                }
                catch
                {
                    listener?.Close();
                    listener = null;
                }
            }

            if (!isRunning)
            {
                Debug.LogWarning("[Antigravity Bridge] Could not start on ports 8088-8091.");
                return;
            }

            listenerThread = new Thread(ListenLoop) { IsBackground = true, Name = "AntigravityBridge" };
            listenerThread.Start();
            Debug.Log($"<color=#4CAF50>[Antigravity Bridge]</color> Listening on http://127.0.0.1:{currentPort}/");
        }

        private static void StopServer()
        {
            isRunning = false;
            try { if (listener != null && listener.IsListening) { listener.Stop(); listener.Close(); } } catch { }
            listener = null;
            try { if (listenerThread != null && listenerThread.IsAlive) listenerThread.Abort(); } catch { }
            listenerThread = null;
        }

        private static void ProcessMainThreadQueue()
        {
            while (mainThreadQueue.TryDequeue(out var action))
            {
                try { action?.Invoke(); }
                catch (Exception ex) { Debug.LogError($"[Antigravity Bridge] {ex}"); }
            }
        }

        private static void ListenLoop()
        {
            while (isRunning && listener != null && listener.IsListening)
            {
                try
                {
                    var ctx = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(_ => HandleRequest(ctx));
                }
                catch { break; }
            }
        }

        private static void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var res = context.Response;
            res.Headers.Add("Access-Control-Allow-Origin", "*");
            res.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            res.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            if (req.HttpMethod == "OPTIONS") { res.StatusCode = 200; res.Close(); return; }

            string path = req.Url.AbsolutePath.TrimEnd('/').ToLowerInvariant();
            string body = "";
            if (req.HasEntityBody)
                using (var r = new StreamReader(req.InputStream, req.ContentEncoding))
                    body = r.ReadToEnd();

            string result = "";
            int status = 200;

            try
            {
                switch (path)
                {
                    case "": case "/status": case "/ping":
                        result = DispatchSync(GetStatusJson); break;
                    case "/hierarchy":
                        result = DispatchSync(GetHierarchyJson); break;
                    case "/screenshot":
                        string camMode = req.QueryString["camera"] ?? "game";
                        result = DispatchSync(() => TakeScreenshot(camMode)); break;
                    case "/action": case "/execute":
                        result = DispatchSync(() => ExecuteAction(body)); break;
                    default:
                        status = 404;
                        result = "{\"error\":\"Not found\"}";
                        break;
                }
            }
            catch (Exception ex)
            {
                status = 500;
                result = $"{{\"error\":\"{J(ex.Message)}\"}}";
            }

            try
            {
                byte[] buf = Encoding.UTF8.GetBytes(result);
                res.ContentType = "application/json; charset=utf-8";
                res.ContentLength64 = buf.Length;
                res.StatusCode = status;
                res.OutputStream.Write(buf, 0, buf.Length);
                res.OutputStream.Close();
            }
            catch { }
        }

        private static string DispatchSync(Func<string> fn, int timeoutMs = 8000)
        {
            string result = null;
            Exception caught = null;
            var done = new ManualResetEventSlim(false);
            mainThreadQueue.Enqueue(() =>
            {
                try { result = fn(); }
                catch (Exception ex) { caught = ex; }
                finally { done.Set(); }
            });
            if (!done.Wait(timeoutMs)) return "{\"error\":\"Timeout\"}";
            if (caught != null) throw caught;
            return result ?? "{}";
        }

        private static string GetStatusJson()
        {
            var scene = SceneManager.GetActiveScene();
            return $"{{\"status\":\"ok\",\"unityVersion\":\"{J(Application.unityVersion)}\",\"productName\":\"{J(Application.productName)}\",\"isPlaying\":{B(EditorApplication.isPlaying)},\"isPaused\":{B(EditorApplication.isPaused)},\"activeScene\":\"{J(scene.name)}\",\"rootCount\":{scene.rootCount},\"dataPath\":\"{J(Application.dataPath)}\"}}";
        }

        private static string GetHierarchyJson()
        {
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            var sb = new StringBuilder();
            sb.Append($"{{\"scene\":\"{J(scene.name)}\",\"objects\":[");
            for (int i = 0; i < roots.Length; i++)
            {
                if (i > 0) sb.Append(",");
                AppendGO(sb, roots[i]);
            }
            sb.Append("]}");
            return sb.ToString();
        }

        private static void AppendGO(StringBuilder sb, GameObject go)
        {
            var p = go.transform.position;
            var r = go.transform.eulerAngles;
            var s = go.transform.localScale;
            sb.Append($"{{\"name\":\"{J(go.name)}\",\"instanceID\":{go.GetInstanceID()},\"active\":{B(go.activeSelf)},\"tag\":\"{J(go.tag)}\",\"layer\":{go.layer}");
            sb.Append($",\"position\":[{p.x:F3},{p.y:F3},{p.z:F3}],\"rotation\":[{r.x:F3},{r.y:F3},{r.z:F3}],\"scale\":[{s.x:F3},{s.y:F3},{s.z:F3}]");
            var comps = go.GetComponents<Component>();
            sb.Append(",\"components\":[");
            for (int i = 0; i < comps.Length; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append($"\"{J(comps[i] != null ? comps[i].GetType().Name : "null")}\"");
            }
            sb.Append("],\"children\":[");
            for (int i = 0; i < go.transform.childCount; i++)
            {
                if (i > 0) sb.Append(",");
                AppendGO(sb, go.transform.GetChild(i).gameObject);
            }
            sb.Append("]}");
        }

        [Serializable]
        private class Payload
        {
            public string action, type, name, target, component;
            public bool play;
            public float[] position, rotation, scale, color;
        }

        private static string ExecuteAction(string json)
        {
            if (string.IsNullOrEmpty(json)) return "{\"success\":false,\"error\":\"Empty payload\"}";
            Payload p;
            try { p = JsonUtility.FromJson<Payload>(json); }
            catch (Exception ex) { return $"{{\"success\":false,\"error\":\"Invalid JSON: {J(ex.Message)}\"}}"; }

            switch ((p.action ?? "").ToLowerInvariant())
            {
                case "create_primitive":
                {
                    PrimitiveType pt = PrimitiveType.Cube;
                    if (!string.IsNullOrEmpty(p.type)) Enum.TryParse(p.type, true, out pt);
                    var go = GameObject.CreatePrimitive(pt);
                    if (!string.IsNullOrEmpty(p.name)) go.name = p.name;
                    if (p.position?.Length == 3) go.transform.position = new Vector3(p.position[0], p.position[1], p.position[2]);
                    if (p.scale?.Length == 3) go.transform.localScale = new Vector3(p.scale[0], p.scale[1], p.scale[2]);
                    if (p.color?.Length >= 3)
                    {
                        var rend = go.GetComponent<Renderer>();
                        if (rend != null)
                        {
                            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                            mat.color = new Color(p.color[0], p.color[1], p.color[2], p.color.Length > 3 ? p.color[3] : 1f);
                            rend.material = mat;
                        }
                    }
                    Undo.RegisterCreatedObjectUndo(go, "Antigravity: Create");
                    Selection.activeGameObject = go;
                    return $"{{\"success\":true,\"name\":\"{J(go.name)}\",\"instanceID\":{go.GetInstanceID()}}}";
                }
                case "create_empty":
                {
                    var go = new GameObject(string.IsNullOrEmpty(p.name) ? "GameObject" : p.name);
                    if (p.position?.Length == 3) go.transform.position = new Vector3(p.position[0], p.position[1], p.position[2]);
                    Undo.RegisterCreatedObjectUndo(go, "Antigravity: Create Empty");
                    Selection.activeGameObject = go;
                    return $"{{\"success\":true,\"name\":\"{J(go.name)}\",\"instanceID\":{go.GetInstanceID()}}}";
                }
                case "modify_transform":
                {
                    var go = FindGO(p.target);
                    if (go == null) return "{\"success\":false,\"error\":\"Target not found\"}";
                    Undo.RecordObject(go.transform, "Antigravity: Modify Transform");
                    if (p.position?.Length == 3) go.transform.position = new Vector3(p.position[0], p.position[1], p.position[2]);
                    if (p.rotation?.Length == 3) go.transform.eulerAngles = new Vector3(p.rotation[0], p.rotation[1], p.rotation[2]);
                    if (p.scale?.Length == 3) go.transform.localScale = new Vector3(p.scale[0], p.scale[1], p.scale[2]);
                    return $"{{\"success\":true,\"name\":\"{J(go.name)}\"}}";
                }
                case "add_component":
                {
                    var go = FindGO(p.target);
                    if (go == null) return "{\"success\":false,\"error\":\"Target not found\"}";
                    var t = FindType(p.component);
                    if (t == null) return $"{{\"success\":false,\"error\":\"Type '{J(p.component)}' not found\"}}";
                    Undo.AddComponent(go, t);
                    return $"{{\"success\":true,\"name\":\"{J(go.name)}\",\"component\":\"{t.Name}\"}}";
                }
                case "delete_object":
                {
                    var go = FindGO(p.target);
                    if (go == null) return "{\"success\":false,\"error\":\"Target not found\"}";
                    string n = go.name;
                    Undo.DestroyObjectImmediate(go);
                    return $"{{\"success\":true,\"deleted\":\"{J(n)}\"}}";
                }
                case "set_play_mode":
                    EditorApplication.isPlaying = p.play;
                    return $"{{\"success\":true,\"isPlaying\":{B(EditorApplication.isPlaying)}}}";
                case "select_object":
                {
                    var go = FindGO(p.target);
                    if (go == null) return "{\"success\":false,\"error\":\"Target not found\"}";
                    Selection.activeGameObject = go;
                    EditorGUIUtility.PingObject(go);
                    return $"{{\"success\":true,\"selected\":\"{J(go.name)}\"}}";
                }
                default:
                    return $"{{\"success\":false,\"error\":\"Unknown action: {J(p.action)}\"}}";
            }
        }

        private static string TakeScreenshot(string mode)
        {
            Camera cam = null;
            if (mode == "scene" && SceneView.lastActiveSceneView != null)
                cam = SceneView.lastActiveSceneView.camera;
            if (cam == null) cam = Camera.main;
            if (cam == null) cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (cam == null) return "{\"success\":false,\"error\":\"No camera found\"}";

            int w = 1280, h = 720;
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            var prevTarget = cam.targetTexture;
            var prevActive = RenderTexture.active;
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            cam.targetTexture = prevTarget;
            RenderTexture.active = prevActive;

            byte[] png = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(rt);

            string dir = @"C:\Users\G2546\.gemini\antigravity\scratch";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "unity_screenshot.png");
            File.WriteAllBytes(path, png);
            return $"{{\"success\":true,\"path\":\"{J(path)}\",\"camera\":\"{J(cam.name)}\",\"width\":{w},\"height\":{h}}}";
        }

        private static GameObject FindGO(string target)
        {
            if (string.IsNullOrEmpty(target)) return null;
            if (int.TryParse(target, out int id))
            {
                var obj = EditorUtility.InstanceIDToObject(id) as GameObject;
                if (obj != null) return obj;
            }
            return GameObject.Find(target);
        }

        private static Type FindType(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(name, false, true) ?? asm.GetType("UnityEngine." + name, false, true);
                if (t != null && typeof(Component).IsAssignableFrom(t)) return t;
            }
            return null;
        }

        private static string J(string s) => (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        private static string B(bool b) => b ? "true" : "false";
    }
}
