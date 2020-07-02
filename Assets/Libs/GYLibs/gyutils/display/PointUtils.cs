using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace GYLib.Utils
{
    public class PointUtils
    {
        /// <summary>
        /// 点距
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double GetDistance(Vector2 p, Vector2 p2)
        {
            return Math.Sqrt(Math.Abs(p.x - p2.x) * Math.Abs(p.x - p2.x) + Math.Abs(p.y - p2.y) * Math.Abs(p.y - p2.y));
        }

        static Vector2 tmpPoint = new Vector2();
        /// <summary>
        /// 根据相对坐标计算偏移量
        /// </summary>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <returns></returns>
        public static float GetAngleByOffset(float offsetX, float offsetY)
        {
            tmpPoint.x = offsetX;
            tmpPoint.y = offsetY;
            //计算相对角度
            Vector3 cross = Vector3.Cross(Vector2.right, tmpPoint);
            float tmpAngle = Vector2.Angle(Vector2.right, tmpPoint);
            tmpAngle = cross.z > 0 ? tmpAngle : -tmpAngle;
            return tmpAngle;
        }

		public static Vector2 ParseStringToPoint(string str)
		{
			string[] strs = str.Split(',');
			if (strs.GetLength (0) > 1) {
				return new Vector2(Convert.ToInt32 (strs [0]), Convert.ToInt32 (strs [1]));
			} else {
				return new Vector2();
			}
		}

		public static Vector2 ParseStringToVector2(string str)
		{
			string[] strs = str.Split(',');
			if (strs.GetLength (0) > 1) {
				return new Vector2 (Convert.ToSingle (strs [0]), Convert.ToSingle (strs [1]));
			} else {
				return new Vector2(0 ,0);
			}
		}

        // similar to EventSystem.current.IsPointerOverGameObject
        public static bool IsPointerOverUIObject(Vector2 point)
        {
            PointerEventData pe = new PointerEventData(EventSystem.current);
            pe.position = point;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pe, results);

            return results.Count > 0;
        }
    }
}