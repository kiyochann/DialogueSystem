using UnityEngine;

namespace Utils
{
    public static class UnityExtensions
    {
        /// <summary>
        /// Gets component or adds it if not present.
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            return comp != null ? comp : go.AddComponent<T>();
        }

        /// <summary>
        /// Destroys all children under this transform.
        /// </summary>
        public static void DestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Sets the X coordinate of transform position.
        /// </summary>
        public static void SetPositionX(this Transform transform, float x)
        {
            var p = transform.position;
            transform.position = new Vector3(x, p.y, p.z);
        }

        /// <summary>
        /// Sets the Y coordinate of transform position.
        /// </summary>
        public static void SetPositionY(this Transform transform, float y)
        {
            var p = transform.position;
            transform.position = new Vector3(p.x, y, p.z);
        }
    }
}
