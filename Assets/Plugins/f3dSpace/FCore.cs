using System;
using System.Collections.Generic;
using UnityEngine;

namespace FSpace
{
    public static class FCore
    {
        /// <summary>
        /// 由于场景比例很小，所以加入一个这个缩放比例，用来调整笔在U3D的坐标位置的缩放。
        /// 当笔的移动距离太小无法适应场景的时候，调整这个缩放值更大一些。
        /// </summary>
        public static float ViewerScale = 2;


        /// <summary>
        /// 整个系统的锚点的RTS
        /// </summary>
        static internal Matrix4x4 anchorRTS = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.zero), Vector3.one);
        static internal Matrix4x4 anchorRMat = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Vector3.zero), Vector3.one);
        static internal Quaternion anchorRQuat = Quaternion.Euler(Vector3.zero);

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
                return anchorRQuat * BaseSystem.penRotation;
            }
        }

        /// <summary>
        /// 当前笔的坐标
        /// </summary>
        public static Vector3 penPosition
        {
            get
            {
                return (anchorRTS * BaseSystem.penPosition.toV4()).toV3();
            }
        }

        /// <summary>
        /// 笔的射线方向
        /// </summary>
        public static Vector3 penDirection
        {
            get
            {
                return (anchorRMat * BaseSystem.penDirection.toV4()).toV3();
            }
        }

        /// <summary> 
        /// 当前眼镜的坐标
        /// </summary>
        public static Vector3 glassPosition
        {
            get
            {
                return (anchorRTS * BaseSystem.glassPosition.toV4()).toV3();
            }
        }

        /// <summary>
        /// 当前眼镜的旋转,其中它自己z轴为正前方.
        /// </summary> 
        public static Quaternion glassRotation
        {
            get
            {
                return anchorRQuat * BaseSystem.glassRotation;
            }
        }

        /// <summary>
        /// 当前眼镜的旋转限制在水平.
        /// </summary> 
        public static Quaternion glassRotation_horizontal
        {
            get
            {
                return anchorRQuat * BaseSystem.glassRotation_horizontal;
            }
        }

        /// <summary>
        /// 左眼摄像机坐标
        /// </summary>
        public static Vector3 eyeLeftPosition
        {
            get
            {
                return (anchorRTS * BaseSystem.eyeLeftPosition.toV4()).toV3();
            }
        }

        /// <summary>
        /// 右眼摄像机坐标
        /// </summary>
        public static Vector3 eyeRightPosition
        {
            get
            {
                return (anchorRTS * BaseSystem.eyeRightPosition.toV4()).toV3();
            }
        }


        #endregion

        #region u3d场景物体(未去实现)

        /// <summary>
        /// 中间相机，需要用户手动设置它的值，切换场景前清空
        /// </summary>
        static Camera cam_c = null;

        /// <summary>
        /// 左边相机，需要用户手动设置它的值，切换场景前清空
        /// </summary>
        static Camera cam_l = null;

        /// <summary>
        /// 右边相机，需要用户手动设置它的值，切换场景前清空
        /// </summary>
        static Camera cam_r = null;

        /// <summary>
        /// 设置场景中的代由控制的物体
        /// </summary>
        /// <param name="cam_c"></param>
        /// <param name="cam_l"></param>
        /// <param name="cam_r"></param>
        static void setScene(Camera cam_c, Camera cam_l, Camera cam_r)
        {
            FCore.cam_c = cam_c;
            FCore.cam_l = cam_l;
            FCore.cam_r = cam_r;
        }

        /// <summary>
        /// 切换场景前调用，清除这些引用
        /// </summary>
        static void clearScene()
        {
            FCore.cam_c = null;
            FCore.cam_l = null;
            FCore.cam_r = null;
        }

        #endregion

        #region 笔按键事件

        /// <summary>
        /// 是否按键0（中键）按下
        /// </summary>
        public static bool isKey0Down
        {
            get { return (OCVData._data.PenKey & 0x01) == 0x01; }
        }

        /// <summary>
        /// 是否按键1（左键）按下
        /// </summary>
        public static bool isKey1Down
        {
            get { return (OCVData._data.PenKey & 0x02) == 0x02; }
        }

        /// <summary>
        /// 是否按键2（右键）按下
        /// </summary>
        public static bool isKey2Down
        {
            get { return (OCVData._data.PenKey & 0x04) == 0x04; }
        }

        /// <summary>
        ///  当前是否在正在拖拽状态
        /// </summary>
        public static bool isDraging
        {
            get
            {
                if (_dictRotate.Count > 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 最后一次拖拽物体的距离
        /// </summary>
        public static float lastDragDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// 添加一个拖拽物体
        /// </summary>
        /// <param name="go">要添加的物体</param>
        /// <param name="distance">拖拽的碰撞点坐标的距离</param>
        public static void addDragObj(GameObject go, float distance)
        {
            DragRecord dr = new DragRecord(go.transform, distance);
            if (!_dictRotate.ContainsKey(go))//如果还不包含这个物体
            {
                lastDragDistance = distance;//记录最后一个距离，画线用
                _dictRotate.Add(go, dr);
            }
        }

        /// <summary>
        /// 更新所有拖拽物体的位置状态
        /// </summary>
        internal static void updateDragObj()
        {
            foreach (var item in _dictRotate)
            {
                item.Value.Update();
            }
        }

        /// <summary>
        /// 删除拖拽物体
        /// </summary>
        public static void deleteDragObj(GameObject go)
        {
            if (go != null && _dictRotate.ContainsKey(go))//如果包含这个物体
            {
                _dictRotate.Remove(go);
            }
        }

        /// <summary>
        /// 清空所有的拖拽物体
        /// </summary>
        public static void clearDragObj()
        {
            _dictRotate.Clear();
        }


        private static Dictionary<GameObject, DragRecord> _dictRotate = new Dictionary<GameObject, DragRecord>();

        /// <summary>
        /// 按键0按下
        /// </summary>
        public static event Action EventKey0Down;

        /// <summary>
        /// 按键0抬起
        /// </summary>
        public static event Action EventKey0Up;

        /// <summary>
        /// 按键1按下
        /// </summary>
        public static event Action EventKey1Down;

        /// <summary>
        /// 按键1抬起
        /// </summary>
        public static event Action EventKey1Up;

        /// <summary>
        /// 按键2按下
        /// </summary>
        public static event Action EventKey2Down;

        /// <summary>
        /// 按键2抬起
        /// </summary>
        public static event Action EventKey2Up;


        static bool _isKey0Down_cur;
        static bool _isKey1Down_cur;
        static bool _isKey2Down_cur;

        /// <summary>
        /// 用于更新判断发出按键
        /// </summary>
        internal static void updateKeyEvent()
        {
            if (!_isKey0Down_cur && isKey0Down)
            {
                if (EventKey0Down != null) { EventKey0Down(); }
            }
            if (!_isKey1Down_cur && isKey1Down)
            {
                if (EventKey1Down != null) { EventKey1Down(); }
            }
            if (!_isKey2Down_cur && isKey2Down)
            {
                if (EventKey2Down != null) { EventKey2Down(); }
            }
            if (_isKey0Down_cur && !isKey0Down)
            {
                if (EventKey0Up != null) { EventKey0Up(); }
            }
            if (_isKey1Down_cur && !isKey1Down)
            {
                if (EventKey1Up != null) { EventKey1Up(); }
            }
            if (_isKey2Down_cur && !isKey2Down)
            {
                if (EventKey2Up != null) { EventKey2Up(); }
            }

            _isKey0Down_cur = isKey0Down;
            _isKey1Down_cur = isKey1Down;
            _isKey2Down_cur = isKey2Down;
        }

        #endregion

        #region 笔的功能

        /// <summary>
        /// 笔震动
        /// </summary>
        /// <param name="mode">震动模式</param>
        public static void PenShake(int mode = 0)
        {
            Debug.Log("FCore.PenShake():调用了一次PenShake()函数！");
            OCVData.setPenShake(1);
        }

        #endregion

        #region 设置

        /// <summary>
        /// 设置显示器屏幕为2D
        /// </summary>
        public static void SetScreen2D()
        {
            OCVData.setflagVRMode(0);
        }

        /// <summary>
        /// 设置显示器屏幕为3D
        /// </summary>
        public static void SetScreen3D()
        {
            OCVData.setflagVRMode(1);
        }

        /// <summary>
        /// 瞳距。默认6cm
        /// </summary>
        public static float PupilDistance = 0.062f;

        /// <summary>
        /// 是否自动设置倾斜
        /// </summary>
        public static bool isAutoSlant = true;

        #endregion

        #region 零平面

        /// <summary>
        /// 屏幕面左上角坐标
        /// </summary>
        public static Vector3 screenPointLeftTop
        {
            get
            {
                return (anchorRTS * BaseSystem.screenPoint.LeftTop.toV4()).toV3();
            }
        }

        /// <summary>
        /// 屏幕面右上角坐标
        /// </summary>
        public static Vector3 screenPointRightTop
        {
            get
            {
                return (anchorRTS * BaseSystem.screenPoint.RightTop.toV4()).toV3();
            }
        }

        /// <summary>
        /// 屏幕面左下角坐标
        /// </summary>
        public static Vector3 screenPointLeftBotton
        {
            get
            {
                return (anchorRTS * BaseSystem.screenPoint.LeftBotton.toV4()).toV3();
            }
        }

        /// <summary>
        /// 屏幕面右下角坐标
        /// </summary>
        public static Vector3 screenPointRightBotton
        {
            get
            {
                return (anchorRTS * BaseSystem.screenPoint.RightBotton.toV4()).toV3();
            }
        }

        /// <summary>
        /// 屏幕面中心坐标
        /// </summary>
        public static Vector3 screenCentre
        {
            get
            {
                return (anchorRTS * BaseSystem.screenPoint.Centre.toV4()).toV3();
            }
        }

        /// <summary>
        /// 屏幕面对应的世界旋转
        /// </summary>
        public static Quaternion screenRotation
        {
            get
            {
                return anchorRQuat * slantRotate;
            }
        }

        /// <summary>
        /// 倾斜角
        /// </summary>
        public static float slantAngle
        {
            get
            {
                if (!isAutoSlant)
                {
                    return 0;
                }
                else
                {
                    return OCVData._data.slantAngle;
                }

            }
        }

        /// <summary>
        /// 机身倾斜对应旋转
        /// </summary>
        public static Quaternion slantRotate
        {
            get
            {
                return Quaternion.AngleAxis(slantAngle, Vector3.right);
            }
        }

        #endregion

        #region 公共方法

        static Vector4 toV4(this Vector3 v3)
        {
            return new Vector4(v3.x, v3.y, v3.z, 1);
        }

        static Vector3 toV3(this Vector4 v4)
        {
            return new Vector3(v4.x, v4.y, v4.z);
        }


        #endregion

    }
}