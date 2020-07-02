/****************************************************************
*   Author:			peckhuang
*   CreateTime:		2016/4/26 18:40:15
*   Description:
*
*   Modify:
*
*
*****************************************************************/

using UnityEngine;

namespace GYLib.Utils
{
    public class GameObjectUtil
    {
        /// <summary>
        /// 深度查找第一个名字相同的子节点
        /// </summary>
        /// <param name="name"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static GameObject Find(Transform transform, string name)
        {
            if (transform == null)
            {
                return null;
            }

            Transform t = transform.Find(name);
            if (t != null)
            {
                return t.gameObject;
            }

            for (int i = 0, count = transform.childCount; i < count; i++)
            {
                Transform child = transform.GetChild(i);
                GameObject go = Find(child, name);
                if (go != null)
                {
                    return go;
                }
            }

            return null;
        }

        public static GameObject Find(GameObject go, string name)
        {
            return Find(go.transform, name);
        }

        public static GameObject Find(Transform transform, string name, string child)
        {
            GameObject go = Find(transform, name);
            if (go != null)
            {
                Transform childTransform = go.transform.Find(child);
                if (childTransform != null)
                {
                    return childTransform.gameObject;
                }
            }

            return null;
        }

        public static GameObject Find(GameObject go, string name, string child)
        {
            return Find(go.transform, name, child);
        }

        public static T RequireComponent<T>(GameObject go) where T : Component
        {
            if (go != null)
            {
                T com = go.GetComponent<T>();
                if (com == null)
                {
                    com = go.AddComponent<T>();
                }

                return com;
            }

            return null;
        }

        public static T AddComponentWithChildGameObject<T>(GameObject gameObject, string childGameObjectName = null) where T : Component
        {
            GameObject childGameObject = new GameObject(childGameObjectName ?? typeof(T).ToString());
            childGameObject.transform.SetParent(gameObject.transform);

            return childGameObject.AddComponent<T>();
        }
    }
}
