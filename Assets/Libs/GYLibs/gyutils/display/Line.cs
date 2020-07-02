using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GYLib.Utils
{
    /// <summary>
    /// 直线几何类
    /// </summary>
    public class Line
    {
        private Vector2 NaN = new Vector2(Single.NaN, Single.NaN);

        #region define_variable
        public double k = 0;       //斜率
        public double b = 0;       //截距
        public double bigX = 0;    //最大值
        public double bigY = 0;
        public double minX = 0;
        public double minY = 0;
        public Vector2 p1;           //两点
        public Vector2 p2;
        public int quadrant;        //象限
        #endregion

        #region function
        //constructor,point A and point B
        public Line(Vector2 p1, Vector2 p2, int precision = -1)
        {
            this.p1 = p1;
            this.p2 = p2;
            decimal p1x = (decimal)p1.x;
            decimal p1y = (decimal)p1.y;
            decimal p2x = (decimal)p2.x;
            decimal p2y = (decimal)p2.y;
            decimal dx = Convert.ToDecimal(p2x - p1x);
            decimal dy = Convert.ToDecimal(p2y - p1y);
            if (dx == 0)
            {
                k = double.PositiveInfinity;
            }
            else
            {
                k = Convert.ToDouble(dy / dx);
                if (precision != -1)
                {
                    k = Math.Round(k, precision);
                }
            }

            if (!double.IsInfinity(this.k))
            {
                b = Convert.ToDouble(((decimal)p1y - ((decimal)k * p1x)));
            }
            else
            {
                b = p1.y - k * p1.x;
            }
            if (precision != -1)
            {
                b = Math.Round(b, precision);
            }
            bigX = p1.x > p2.x ? p1.x : p2.x;
            bigY = p1.y > p2.y ? p1.y : p2.y;
            minX = p1.x < p2.x ? p1.x : p2.x;
            minY = p1.y < p2.y ? p1.y : p2.y;

            if (p1.x - p2.x > 0)
            {
                this.quadrant = p1.y - p2.y > 0 ? 3 : 2;
            }
            else
            {
                this.quadrant = p1.y - p2.y > 0 ? 4 : 1;
            }
        }

        /// <summary>
        /// 检测交点是否存在(非线段范围内)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool ExistPoint(Line target)
        {
            /*
                * D = A1 * B2 - A2 * B1
                *  y = kx + b  =>  kx - y + b = 0 | A:k | B:-1 | C:b *
                * */
            if (this.k == target.k || (double.IsInfinity(k) && double.IsInfinity(target.k)))
            {
                return this.b == target.b;
            }
                //垂直于X轴时
            if (double.IsInfinity(this.k) && double.IsInfinity(target.k))
            {
                return target.p1.y == p1.y;
            }
            else if (double.IsInfinity(this.k) || double.IsInfinity(target.k))
            {
                return true;
            }
            double D = this.k * (-1) + target.k;
            if (D <= 0.00001f)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 获得交点
        /// </summary>
        /// <param name="target"></param>
        /// <param name="needCut">needCut 是否限定在线段范围</param>
        /// <returns></returns>
        public Vector2 GetItPoint(Line target, bool needCut = true)
        {
            double curMaxX = Math.Min(this.bigX, target.bigX);
            double curMaxY = Math.Min(this.bigY, target.bigY);
            double curMinX = Math.Max(Math.Min(this.p1.x, this.p2.x), Math.Min(target.p1.x, target.p2.x));
            double curMinY = Math.Max(Math.Min(this.p1.y, this.p2.y), Math.Min(target.p1.y, target.p2.y));

            //处理有其一为点
            var thisPoint = this.IsPoint();
            var targetPoint = target.IsPoint();
            if (thisPoint || targetPoint)
            {
                if (thisPoint && targetPoint)
                {
                    return this.p1 == target.p1 ? this.p1 : NaN;
                }
                else
                {
                    Vector2 linePoint = thisPoint ? this.p1 : target.p1;
                    Line noPointLine = thisPoint ? target : this;
                    if (double.IsInfinity(noPointLine.k) || (noPointLine.k == 0))
                    {
                        //垂直x轴
                        return noPointLine.IsInRange(linePoint.x, linePoint.y) ? linePoint : NaN;
                    }
                    else
                    {
                        return noPointLine.IsInRange(linePoint.x, linePoint.y) && noPointLine.GetFy(linePoint.x) == linePoint.y ? linePoint : NaN;
                    }
                }
            }

            //处理斜率相同导致的重叠
            if (this.k == target.k || (double.IsInfinity(k) && double.IsInfinity(target.k)))
            {
                //处理平行x轴
                if (k == 0)
                {
                    if (!needCut)
                    {
                        return this.p1.y == target.p1.y ? new Vector2((float)p1.x, (float)p1.y) : NaN;
                    }
                    else
                    {
                        if (this.p1.y == target.p1.y)
                        {
                            if (!(target.minX > this.bigX || this.minX > target.bigX))
                            {
                                var minXLine = this.minX >= target.minX ? this : target;
                                var bigXLine = this.minX >= target.minX ? target : this;
                                var tmpX = (bigXLine.bigX + minXLine.minX) * 0.5;
                                return new Vector2((float)tmpX, (float)minXLine.GetFy(tmpX));
                            }
                            else
                            {
                                return NaN;
                            }
                        }
                        else
                        {
                            return NaN;
                        }
                    }
                }
                //处理垂直x轴
                if (double.IsInfinity(k))
                {
                    if (!needCut)
                    {
                        return this.p1.x == target.p1.x ? new Vector2((float)p1.x, (float)p1.y) : NaN;
                    }
                    else
                    {
                        if (this.p1.x == target.p1.x)
                        {
                            if (!(target.minY > this.bigY || this.minY > target.bigY))
                            {
                                var minYLine = this.minY >= target.minY ? this : target;
                                var bigYLine = this.minY >= target.minY ? target : this;
                                var tmpY = (bigYLine.bigY + minYLine.minY) * 0.5;
                                return new Vector2((float)minYLine.GetFx(tmpY), (float)tmpY);
                            }
                            else
                            {
                                return NaN;
                            }
                        }
                        else
                        {
                            return NaN;
                        }
                    }
                }

                //普通斜率
                if (this.b != target.b)
                {
                    return NaN;
                }
                else
                {
                    if (!needCut)
                    {
                        //不是线段,随意返回一个点
                        return new Vector2((float)p1.x, (float)p1.y);
                    }
                    else
                    {

                        var minYLine = this.minY >= target.minY ? this : target;
                        var bigYLine = this.minY >= target.minY ? target : this;
                        var tmpY = (bigYLine.bigY + minYLine.minY) * 0.5;
                        

                        double[] portsX = { minYLine.GetFx(tmpY), p1.x, p2.x, target.p1.x, target.p2.x };
                        double[] portsY = { tmpY, p1.y, p2.y, target.p1.y, target.p2.y };
                        for (int i = 0; i < portsX.Length; i++)
                        {
                            if (portsX[i] >= curMinX && portsX[i] <= curMaxX &&
                                portsY[i] >= curMinY && portsY[i] <= curMaxY)
                            {
                                return new Vector2((float)portsX[i], (float)portsY[i]);
                            }
                        }
                    }
                }
            }

            double pointX = (target.b - this.b) / (this.k - target.k);
            
            double newX = pointX;
            double newY = (k * pointX + b);
            //处理垂直
            if (double.IsInfinity(this.k))
            {
                newX = this.p1.x;
                newY = (target.k * newX + target.b);
            }
            else if (double.IsInfinity(target.k))
            {
                newX = target.p1.x;
                newY = (this.k * newX + this.b);
            }
            if (needCut)
            {
                if (double.IsNaN(newX) || double.IsNaN(newY) || double.IsInfinity(newX) || double.IsInfinity(newY))
                {
                    return NaN;
                }
                var decNewX = (decimal)newX;
                var decNewY = (decimal)newY;
                var decCurMinX = (decimal)curMinX;
                var decCurMaxX = (decimal)curMaxX;
                var decCurMinY = (decimal)curMinY;
                var decCurMaxY = (decimal)curMaxY;

                if (decNewX >= decCurMinX && decNewX <= decCurMaxX &&
                    decNewY >= decCurMinY && decNewY <= decCurMaxY)
                {
                    return new Vector2((float)newX, (float)newY);
                }
            }
            else
            {
                return new Vector2((float)newX, (float)newY);
            }
            //没有交点
            return NaN;
        }

        public double GetFy(double x)
        {
            if (double.IsInfinity(k))
            {
                return double.PositiveInfinity;
            }
            if (k == 0)
            {
                return p1.y;
            }
            return k * x + b;
        }

        public double GetFx(double y)
        {
            if (double.IsInfinity(k))
            {
                return p1.x;
            }
            if (k == 0)
            {
                return double.PositiveInfinity;
            }
            return (y - b) / k;
        }

        /// <summary>
        /// 获取点到直线距离
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public float GetDistance(Vector2 point)
        {
            if (double.IsInfinity(k))
            {
                return Math.Abs(p1.x - point.x);
            }
            else if (k == 0)
            {
                return Math.Abs(p1.y - point.y);
            }
            else
            {
                double A = 1;
                double B = -k;
                double C = -b;
                return (float)(Math.Abs(A * point.y + B * point.x + C) / Math.Sqrt(A * A + B * B));
            }
        }

        /// <summary>
        /// 是否在矩形范围内
        /// </summary>
        /// <param name="newX"></param>
        /// <param name="newY"></param>
        /// <returns></returns>
        public bool IsInRange(double newX, double newY)
        {
            if (newX >= minX && newX <= bigX &&
                    newY >= minY && newY <= bigY)
            {
                return true;
            }
            return false;
        }

        public bool IsPoint()
        {
            return p1 == p2;
        }

        /// <summary>
        /// 检查有效性
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static bool CheckInvalid(Vector2 pos)
        {
            return (Single.IsNaN(pos.x) || Single.IsNaN(pos.y));
        }



        #endregion
    }
}