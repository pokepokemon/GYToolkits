using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace GYLib.Utils
{
	public class DisplayUtils
	{
		public DisplayUtils ()
		{
		}

		public static bool AddClickEvent(GameObject parent, string btnName, UnityAction clickFunc)
		{
			if (parent) {
				GameObject go = GetChildByName(parent, btnName);
				if (go)
				{
					Button btn = go.GetComponent<Button>();
					if (btn)
					{
						btn.onClick.AddListener(clickFunc);
						return true;
					}
				}
			}
			return false;
		}

        public static GameObject GetChildByName(GameObject parent, string name)
		{
            Transform rootTf = parent.transform;
            Transform transform = rootTf.Find (name);
			if (transform == null) {
				GameObject rs = null;
				for (int i = 0; i < rootTf.childCount; i++) {
					Transform childTrans = rootTf.GetChild (i);
					rs = GetChildByName (childTrans.gameObject, name);
					if (rs != null) {
						return rs;
					}
				}
				return null;
			} else {
				return transform.gameObject;
			}
		}
        
        public static void SetButtonText(Button button, string content)
        {
            Text text = DisplayUtils.GetChildByName(button.gameObject, "Text").GetComponent<Text>();
            if (text)
            {
                text.text = content;
            }
        }

        /// <summary>
        /// Selects a suitable prefix for the value to keep 1-3 significant figures
        /// on the left of the decimal point.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unit">Optional unit to be put after the prefix</param>
        /// <returns></returns>
        public static string ToSIPrefix(double value, string unit = "")
        {
            string[] superSuffix = new string[] { "K", "M", "G", "T", "P", "E", "Z", "Y", "B", "aa", "bb", "cc", "dd", "ee"};
            string[] subSuffix = new string[] { "m", "u", "n", "p", "f", "a" };
            double v = value;
            int exp = 0;
            /*
            while (v - Math.Floor(v) > 0)
            {
                if (exp >= subSuffix.Length * 3)
                    break;
                exp += 3;
                v *= 1000;
                v = Math.Round(v, 12);
            }*/
            //Replace those comment
            //v = Math.Round(v, 3);

            while (Math.Floor(v).ToString().Length > 3)
            {
                if (exp <= -superSuffix.Length * 3)
                    break;
                exp -= 3;
                v /= 1000;
                //v = Math.Round(v, 12); 
            }
            if (exp != 0)
            {
                v = Math.Round(v, 2);
            }
            if (exp > 0)
                return v.ToString() + subSuffix[exp / 3 - 1] + unit;
            else if (exp < 0)
                return v.ToString() + superSuffix[-exp / 3 - 1] + unit;
            return v.ToString() + unit;
        }

        /// <summary>
        /// 根据T值，计算贝塞尔曲线上面相对应的点
        /// </summary>
        /// <param name="t"></param>T值
        /// <param name="p0"></param>起始点
        /// <param name="p1"></param>控制点
        /// <param name="p2"></param>目标点
        /// <returns></returns>根据T值计算出来的贝赛尔曲线点
        private static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 p = uu * p0;
            p += 2 * u * t * p1;
            p += tt * p2;

            return p;
        }

        /// <summary>
        /// 获取存储贝塞尔曲线点的数组
        /// </summary>
        /// <param name="startPoint"></param>起始点
        /// <param name="controlPoint"></param>控制点
        /// <param name="endPoint"></param>目标点
        /// <param name="segmentNum"></param>采样点的数量
        /// <returns></returns>存储贝塞尔曲线点的数组
        public static Vector3[] GetBezierList(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, int segmentNum)
        {
            Vector3[] path = new Vector3[segmentNum];
            for (int i = 0; i < segmentNum; i++)
            {
                float t = i / (float)(segmentNum - 1);
                Vector3 pixel = CalculateCubicBezierPoint(t, startPoint,
                    controlPoint, endPoint);
                path[i] = pixel;
            }
            return path;

        }

        /// <summary>
        /// 设置图片锚点为Sprite锚点
        /// </summary>
        /// <param name="image"></param>
        /// <param name="sp"></param>
        public static void ApplyImagePivotToSprite(Image image, Sprite sp)
        {
            Vector2 size = image.rectTransform.sizeDelta;
            Vector2 pixelPivot = image.sprite.pivot;
            Vector2 percentPivot = new Vector2(pixelPivot.x / size.x, pixelPivot.y / size.y);
            image.rectTransform.pivot = percentPivot;
        }

        /// <summary>
        /// 根据模型转换UI坐标
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="modelTransform"></param>
        /// <param name="cam"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <returns></returns>
        public static Vector2 GetUIPosFromModelPos(Canvas canvas, Transform modelTransform, Camera cam, float offsetX = 0, float offsetY = 0)
        {
            float scaleFactor = canvas.scaleFactor;
            Vector2 screentPosition = cam.WorldToScreenPoint(modelTransform.position);
            screentPosition.x = screentPosition.x - Screen.width / 2 + offsetX * scaleFactor;
            screentPosition.y = screentPosition.y - Screen.height / 2 + offsetY * scaleFactor;

            Vector2 convertPos = new Vector2(screentPosition.x / scaleFactor, screentPosition.y / scaleFactor); 
            return convertPos;
        }

        /// <summary>
        /// 根据世界转换UI坐标
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="worldPos"></param>
        /// <param name="cam"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <returns></returns>
        public static Vector2 GetUIPosFromWorldPos(Canvas canvas, Vector3 worldPos, Camera cam, float offsetX = 0, float offsetY = 0)
        {
            float scaleFactor = canvas.scaleFactor;
            Vector2 screentPosition = cam.WorldToScreenPoint(worldPos);
            screentPosition.x = screentPosition.x - Screen.width / 2 + offsetX * scaleFactor;
            screentPosition.y = screentPosition.y - Screen.height / 2 + offsetY * scaleFactor;

            Vector2 convertPos = new Vector2(screentPosition.x / scaleFactor, screentPosition.y / scaleFactor);
            return convertPos;
        }
    }
}

