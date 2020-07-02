using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GYLib.UI
{
    /// <summary>
    /// 实用工具
    /// Author : 
    /// </summary>
    public class GYLibUITools
    {

        /// <summary>
        /// 在指定父级下创建一个新的GameObject
        /// </summary>
        /// <param name="parent">父级</param>
        /// <param name="name">GameObject名字</param>
        /// <returns></returns>
        public static GameObject CreateChild(string name, Transform parent)
        {
            GameObject go = name != null ? new GameObject(name) : new GameObject();

            if (parent != null)
            {
                SetChildParent(go, parent);
                go.layer = parent.gameObject.layer;
                Transform t = go.transform;
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
            }
            return go;
        }
        public static GameObject CreateUGUIChild(string name, Transform parent)
        {
            GameObject go = CreateChild(name, parent);
            go.AddComponent<RectTransform>();
            return go;
        }

        public static GameObject CreateChild(GameObject prefab, Transform parent)
        {
            GameObject go = GameObject.Instantiate(prefab) as GameObject;
            if (go != null)
            {
                if (parent != null)
                    SetChildParent(go, parent, false);

                Transform t = go.transform;

                // t.localPosition = Vector3.zero;

                // t.localRotation = Quaternion.identity;
                //  t.localScale = Vector3.one;
                go.name = prefab.name;
            }
            return go;
        }

        public static bool IsPrefabs(GameObject prefab)
        {
            if (prefab == null || prefab.scene == null || prefab.scene.name == null)
            {
                if (prefab.activeInHierarchy)
                    return false;
                else if (prefab.transform.parent!=null)
                    return false;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 添加一个预置到指定父级下
        /// </summary>
        /// <param name="prefab">已加载的预置</param>
        /// <param name="parent">父级</param>
        /// <returns></returns>
        public static GameObject AddChild(GameObject prefab, Transform parent)
        {
            GameObject go = prefab;
            if (IsPrefabs(prefab))
                go = GameObject.Instantiate(prefab) as GameObject;
            if (go != null)
            {
                if (parent != null)
                    SetChildParent(go, parent, false);

                Transform t = go.transform;

                // t.localPosition = Vector3.zero;

                // t.localRotation = Quaternion.identity;
                //  t.localScale = Vector3.one;
                go.name = prefab.name;
            }
            return go;
        }

        /// <summary>
        /// 添加一个预置到指定父级下
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="prefab">预置的路径</param>
        /// <returns></returns>
        public static GameObject AddChild(string prefab, Transform parent)
        {
            GameObject go = Resources.Load(prefab) as GameObject;
            return AddChild(go, parent);
        }

        /// <summary>
        /// 把一对象移动另一父级下
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        public static void SetChildParent(GameObject child, Transform parent, bool worldPositionStays = true)
        {
            Transform t = child.transform;
            t.SetParent(parent, worldPositionStays);
            //t.parent=parent;
            // SetLayer(child, parent.gameObject.layer);
        }
        /// <summary>
        /// 设置一个GameObject及其所有子对象的layer
        /// </summary>
        /// <param name="go"></param>
        /// <param name="layer"></param>
        public static void SetLayer(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0, imax = t.childCount; i < imax; ++i)
            {
                Transform child = t.GetChild(i);
                SetLayer(child.gameObject, layer);
            }
        }
        /// <summary>
        /// 把一个gameObject移至指定父级的路径下 ,路径使用"/"格式
        /// </summary>
        /// <param name="g"></param>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        public static void MoveChildIn(GameObject g, string path, Transform parent = null)
        {
            string[] array = path.Split(new Char[] { '/' });
            GameObject go = null;
            if (parent == null)
            {
                go = GameObject.Find(array[0]);
                if (go == null)
                    go = new GameObject(array[0]);
            }
            else
            {
                Transform tf = parent.Find(array[0]);
                if (tf == null)
                    go = new GameObject(array[0]);
                go.transform.parent = tf;
            }
            for (int i = 1; i < array.Length; i++)
            {
                string str = array[i];
                Transform tf = go.transform.Find(str);
                if (tf == null)
                {
                    GameObject child = new GameObject(str);
                    child.transform.parent = go.transform;
                    go = child;
                }
                else
                    go = tf.gameObject;
            }
            g.transform.parent = go.transform;
        }

        /// <summary>
        /// 获取一个GameObject下的组件，组件不存在则创建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <returns></returns>
        public static T GetComponent<T>(GameObject go) where T : Component
        {
            T t = null;
            if (go.activeInHierarchy)
                t = go.GetComponent<T>();
            else
            {
                foreach (T child in go.GetComponentsInChildren<T>(true))
                {
                    if (child.gameObject == go)
                    {
                        t = child;
                        break;
                    }

                }
            }
            if (t == null)
            {
                t = go.AddComponent<T>();
            }
            return t;
        }
        public static Component GetComponent(Type type, GameObject go)
        {
            Component t = go.GetComponent(type);
            if (t == null)
            {
                t = go.AddComponent(type);
            }
            return t;
        }


        public static Transform Find(Transform parent, string name)
        {
            Transform tf = null;
            if (parent != null)
            {
                if (parent.gameObject.activeInHierarchy)
                    tf = parent.Find(name);
                else
                {
                    foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
                    {
                        if (child.parent == parent && child.name == name)
                        {
                            tf = child;
                            break;
                        }

                    }
                }
            }
            return tf;
        }

        public static Vector2 MouseToLocalPoint(Transform tf)
        {
            Vector2 pos;
            RectTransform parent = tf.parent as RectTransform;
            if (parent != null)
            {
                Canvas canvas = tf.GetComponentInParent<Canvas>();
                canvas = canvas.rootCanvas;
                Camera cam = null;
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                    cam = canvas.worldCamera;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Input.mousePosition, cam, out pos))
                {
                    return pos;
                }
            }
            return new Vector2(0, 0);
        }
        public static void ClampInParent(RectTransform rt)
        {
            if (rt == null)
                return;
            RectTransform parent = rt.parent as RectTransform;
            Vector3 pos = rt.localPosition;
            if (parent != null)
            {
       
                Vector3 minPosition = parent.rect.min - rt.rect.min;
                Vector3 maxPosition = parent.rect.max - rt.rect.max;
                if (minPosition.x < maxPosition.x)
                    pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
                else
                    pos.x = Mathf.Clamp(pos.x, maxPosition.x, minPosition.x);
                if (minPosition.y < maxPosition.y)
                    pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);
                else
                    pos.y = Mathf.Clamp(pos.y, maxPosition.y, minPosition.y);
            }
            rt.localPosition = pos;
        }
    }
}
