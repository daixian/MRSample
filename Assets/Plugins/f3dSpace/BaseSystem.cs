using System;
using System.Collections.Generic;
using UnityEngine;

namespace FSpace
{
    /// <summary>
    /// 整个系统的基本坐标，中心在原点的基本系统。这个系统的底边为倾斜旋转轴，底边坐标为0.
    /// </summary>
    internal static class BaseSystem
    {


        #region opt检测数据

        /// <summary>
        /// 笔的射线
        /// </summary>
        public static Ray penRay
        {
            get
            {
                Ray ray = new Ray(penPosition, penDirection);
                return ray;
            }

        }

        /// <summary>
        /// 当前笔的旋转,其中它自己z轴为正前方.
        /// </summary>  
        public static Quaternion penRotation
        {
            get
            {
                if (FCore.isAutoSlant)
                {
                    Quaternion qRoll = Quaternion.AngleAxis(OCVData._data.PenRoll, -OCVData._data.PenDirection);
                    return FCore.slantRotate * qRoll * OCVData._data.GyroscopeRotation;

                }
                else
                {
                    Quaternion qRoll = Quaternion.AngleAxis(OCVData._data.PenRoll, -OCVData._data.PenDirection);
                    return qRoll * OCVData._data.GyroscopeRotation;
                }
            }
        }

        /// <summary>
        /// 当前笔的坐标
        /// </summary>
        public static Vector3 penPosition
        {
            get
            {
                //如果眼镜坐标为0
                if (OCVData._data.PenPosition.z == 0)
                {
                    OCVData._data.PenPosition = new Vector3(0, 0, -0.2f);
                }
                if (FCore.isAutoSlant)
                {
                    return FCore.slantRotate * ((OCVData._data.PenPosition * FCore.ViewerScale + penDirection.normalized * 0.01f * FCore.ViewerScale));
                }
                else
                {
                    return (OCVData._data.PenPosition + OCVData._data.PenDirection.normalized * 0.01f) * FCore.ViewerScale;
                }

            }
        }

        /// <summary>
        /// 笔的射线方向
        /// </summary>
        public static Vector3 penDirection
        {
            get
            {
                //如果笔方向为0
                if (OCVData._data.PenDirection == Vector3.zero)
                {
                    OCVData._data.PenDirection = new Vector3(0, 0, 0.18f);
                }
                if (FCore.isAutoSlant)
                {
                    return FCore.slantRotate * (OCVData._data.PenDirection * FCore.ViewerScale);
                }
                else
                {
                    return OCVData._data.PenDirection * FCore.ViewerScale;
                }
            }
        }

        static private long _penMatrixUpdateCount = 0;
        static private Matrix4x4 _penMatrix;

        /// <summary>
        /// 当前笔的矩阵
        /// </summary>
        public static Matrix4x4 penMatrix4x4
        {
            get
            {
                if (_penMatrixUpdateCount != OCVData.updateCount)
                {
                    _penMatrix = new Matrix4x4();//当前的笔矩阵
                    _penMatrix.SetTRS(FCore.penPosition, FCore.penRotation, new Vector3(1, 1, 1));
                    _penMatrixUpdateCount = OCVData.updateCount;
                }
                return _penMatrix;
            }
        }



        /// <summary> 
        /// 当前眼镜的坐标
        /// </summary>
        public static Vector3 glassPosition
        {
            get
            {
                //如果眼镜坐标为0
                if (OCVData._data.GlassPosition.z == 0)
                {
                    OCVData._data.GlassPosition = new Vector3(0, 0, -0.4f);
                }
                if (FCore.isAutoSlant)
                {
                    return FCore.slantRotate * (OCVData._data.GlassPosition * FCore.ViewerScale);
                }
                else
                {
                    return OCVData._data.GlassPosition * FCore.ViewerScale;
                }
            }
        }

        /// <summary>
        /// 当前眼镜的旋转,其中它自己z轴为正前方.
        /// </summary> 
        public static Quaternion glassRotation
        {
            get
            {
                if (FCore.isAutoSlant)
                {
                    return FCore.slantRotate * (OCVData._data.GlassRotation);
                }
                else
                {
                    return OCVData._data.GlassRotation;
                }


            }
        }

        /// <summary>
        /// 当前眼镜的旋转限制在水平.
        /// </summary> 
        public static Quaternion glassRotation_horizontal
        {
            get
            {
                if (FCore.isAutoSlant)
                {
                    Vector3 eulerAngles = OCVData._data.GlassRotation.eulerAngles;
                    eulerAngles.z = 0;
                    return FCore.slantRotate * Quaternion.Euler(eulerAngles);
                }
                else
                {
                    Vector3 eulerAngles = OCVData._data.GlassRotation.eulerAngles;
                    eulerAngles.z = 0;
                    return Quaternion.Euler(eulerAngles);
                }
            }
        }

        /// <summary>
        /// 检测系统的左眼坐标
        /// </summary>
        public static Vector3 eyeLeftPosition
        {
            get
            {
                return glassPosition + new Vector3(-FCore.PupilDistance / 2, 0, 0) * FCore.ViewerScale;
            }
        }

        /// <summary>
        /// 检测系统的右眼坐标
        /// </summary>
        public static Vector3 eyeRightPosition
        {
            get
            {
                return glassPosition + new Vector3(FCore.PupilDistance / 2, 0, 0) * FCore.ViewerScale;
            }
        }

        #endregion



        /// <summary>
        /// 屏幕的四个角点的位置坐标
        /// </summary>
        internal static Screen4Point screenPoint = new Screen4Point();

        /// <summary>
        /// 更新屏幕的四个角
        /// </summary>
        internal static void updateScreenPoint()
        {
            //倾斜角默认状态应该是0
            screenPoint.update(FCore.slantAngle);
        }

    }


    /// <summary>
    /// 屏幕的四个角点的位置坐标数据
    /// </summary>
    internal class Screen4Point
    {
        public Screen4Point()
        {
            width = 0.543f;
            height = 0.302f;
            slant = 0;

            LeftTop = new Vector3(-width / 2, height, 0) * FCore.ViewerScale;
            RightTop = new Vector3(width / 2, height, 0) * FCore.ViewerScale;
            LeftBotton = new Vector3(-width / 2, 0, 0) * FCore.ViewerScale;
            RightBotton = new Vector3(width / 2, 0, 0) * FCore.ViewerScale;

        }
        /// <summary>
        /// 屏幕宽
        /// </summary>
        public float width;

        /// <summary>
        /// 屏幕高
        /// </summary>
        public float height;

        /// <summary>
        /// 屏幕的倾斜角
        /// </summary>
        public float slant;

        public Vector3 LeftTop;
        public Vector3 RightTop;
        public Vector3 LeftBotton;
        public Vector3 RightBotton;

        public Vector3 Centre;

        public void update(float slant)
        {
            float h = (float)Math.Cos(slant * Math.PI / 180) * height;
            float off = (float)Math.Sin(slant * Math.PI / 180) * height;

            LeftTop = new Vector3(-width / 2, h, off) * FCore.ViewerScale;
            RightTop = new Vector3(width / 2, h, off) * FCore.ViewerScale;
            LeftBotton = new Vector3(-width / 2, 0, 0) * FCore.ViewerScale;
            RightBotton = new Vector3(width / 2, 0, 0) * FCore.ViewerScale;

            Centre = (LeftTop + RightTop + LeftBotton + RightBotton) / 4;
        }

        public void DebugDraw()
        {
            Debug.DrawLine(LeftTop, RightTop, Color.red);
            Debug.DrawLine(RightTop, RightBotton, Color.red);
            Debug.DrawLine(RightBotton, LeftBotton, Color.red);
            Debug.DrawLine(LeftTop, LeftBotton, Color.red);

        }


    }
}