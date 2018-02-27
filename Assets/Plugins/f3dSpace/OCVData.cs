using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FSpace
{
    #region DetectPoint

    ///-------------------------------------------------------------------------------------------------
    /// <summary> 一个检测到的小跟踪点. </summary>
    ///
    /// <remarks> Xian Dai, 2017/5/6. </remarks>
    ///-------------------------------------------------------------------------------------------------
    public struct DetectPoint
    {
        /// <summary> 结果点的坐标. </summary>
        public Vector3 targetPoint;

        /// <summary> 这个点的可信度，规定如果小于等于0那么就不算检测到. </summary>
        public int credibility;

        /// <summary> 距离误差. </summary>
        public float d;

        /// <summary> 四个摄像机看这个点的射线方向. </summary>
        public Vector3 direction1;
        public Vector3 direction2;
        public Vector3 direction3;
        public Vector3 direction4;

    };

    public struct DetectPointx4
    {
        DetectPoint dp1;
        DetectPoint dp2;
        DetectPoint dp3;
        DetectPoint dp4;

        public DetectPoint this[int index]
        {
            get
            {
                if (index == 0)
                    return dp1;
                else if (index == 1)
                    return dp2;
                else if (index == 2)
                    return dp3;
                else if (index == 3)
                    return dp4;
                return new DetectPoint();
            }
            set
            {
                if (index == 0)
                    dp1 = value;
                else if (index == 1)
                    dp2 = value;
                else if (index == 2)
                    dp3 = value;
                else if (index == 3)
                    dp4 = value;
            }
        }
    }

    public struct DetectPointx16
    {
        DetectPointx4 dp4_1;
        DetectPointx4 dp4_2;
        DetectPointx4 dp4_3;
        DetectPointx4 dp4_4;

        public DetectPoint this[int index]
        {
            get
            {
                int i = index / 4;
                int j = index % 4;

                if (i == 0)
                    return dp4_1[j];
                else if (i == 1)
                    return dp4_2[j];
                else if (i == 2)
                    return dp4_3[j];
                else if (i == 3)
                    return dp4_4[j];
                return new DetectPoint();
            }
            set
            {
                int i = index / 4;
                int j = index % 4;

                if (i == 0)
                    dp4_1[j] = value;
                else if (i == 1)
                    dp4_2[j] = value;
                else if (i == 2)
                    dp4_3[j] = value;
                else if (i == 3)
                    dp4_4[j] = value;
            }
        }

    }

    public struct DetectPointx64
    {
        DetectPointx16 dp16_1;
        DetectPointx16 dp16_2;
        DetectPointx16 dp16_3;
        DetectPointx16 dp16_4;

        public DetectPoint this[int index]
        {
            get
            {
                int i = index / 16;
                int j = index % 16;

                if (i == 0)
                    return dp16_1[j];
                else if (i == 1)
                    return dp16_2[j];
                else if (i == 2)
                    return dp16_3[j];
                else if (i == 3)
                    return dp16_4[j];
                return new DetectPoint();
            }
            set
            {
                int i = index / 16;
                int j = index % 16;

                if (i == 0)
                    dp16_1[j] = value;
                else if (i == 1)
                    dp16_2[j] = value;
                else if (i == 2)
                    dp16_3[j] = value;
                else if (i == 3)
                    dp16_4[j] = value;
            }
        }

    }

    #endregion

    #region CamRay


    public struct CamRay
    {
        /// <summary> 这个点的可信度，如果为0则忽略 </summary>
        public int credibility;

        /// <summary> 相机的编号. </summary>
        public int camIndex;

        /// <summary> 射线的原点坐标. </summary>
        public Vector3 origin;

        /// <summary> 射线的方向. </summary>
        public Vector3 direction;

    };

    public struct CamRayx4
    {
        CamRay cr1;
        CamRay cr2;
        CamRay cr3;
        CamRay cr4;

        public CamRay this[int index]
        {
            get
            {
                if (index == 0)
                    return cr1;
                else if (index == 1)
                    return cr2;
                else if (index == 2)
                    return cr3;
                else if (index == 3)
                    return cr4;
                return new CamRay();
            }
            set
            {
                if (index == 0)
                    cr1 = value;
                else if (index == 1)
                    cr2 = value;
                else if (index == 2)
                    cr3 = value;
                else if (index == 3)
                    cr4 = value;
            }
        }
    }

    public struct CamRayx16
    {
        CamRayx4 cr4_1;
        CamRayx4 cr4_2;
        CamRayx4 cr4_3;
        CamRayx4 cr4_4;

        public CamRay this[int index]
        {
            get
            {
                int i = index / 4;
                int j = index % 4;

                if (i == 0)
                    return cr4_1[j];
                else if (i == 1)
                    return cr4_2[j];
                else if (i == 2)
                    return cr4_3[j];
                else if (i == 3)
                    return cr4_4[j];
                return new CamRay();
            }
            set
            {
                int i = index / 4;
                int j = index % 4;

                if (i == 0)
                    cr4_1[j] = value;
                else if (i == 1)
                    cr4_2[j] = value;
                else if (i == 2)
                    cr4_3[j] = value;
                else if (i == 3)
                    cr4_4[j] = value;
            }
        }

    }

    public struct CamRayx64
    {
        CamRayx16 cr16_1;
        CamRayx16 cr16_2;
        CamRayx16 cr16_3;
        CamRayx16 cr16_4;

        public CamRay this[int index]
        {
            get
            {
                int i = index / 16;
                int j = index % 16;

                if (i == 0)
                    return cr16_1[j];
                else if (i == 1)
                    return cr16_2[j];
                else if (i == 2)
                    return cr16_3[j];
                else if (i == 3)
                    return cr16_4[j];
                return new CamRay();
            }
            set
            {
                int i = index / 16;
                int j = index % 16;

                if (i == 0)
                    cr16_1[j] = value;
                else if (i == 1)
                    cr16_2[j] = value;
                else if (i == 2)
                    cr16_3[j] = value;
                else if (i == 3)
                    cr16_4[j] = value;
            }
        }

    }


    #endregion

    ///-------------------------------------------------------------------------------------------------
    /// <summary> 监测出来的摄像机状态. </summary>
    ///
    /// <remarks> Xian Dai, 2017/5/6. </remarks>
    ///-------------------------------------------------------------------------------------------------
    public struct CameraStatus
    {
        /// <summary>这个相机的序号,目前应该是0-3，一共4个. </summary>
        public int index;

        /// <summary> 检测状态，1表示成功定位自身，0表示丢失自身，2表示使用手动输入值. </summary>
        public int status;

        /// <summary> 相机的像素宽度. </summary>
        public int width;

        /// <summary> 相机的像素高度. </summary>
        public int height;

        /// <summary> 自身坐标. </summary>
        public Vector3 Position;

        /// <summary> 自身旋转. </summary>
        public Quaternion rotation;

        /// <summary> 自身旋转(使用u3d的zxy旋转的欧拉角表示). </summary>
        public Vector3 rotationZXY;

        /// <summary> 相机的视场方向向量,4个边角点作出. </summary>
        public Vector3 ViewFieldDirection1;
        public Vector3 ViewFieldDirection2;
        public Vector3 ViewFieldDirection3;
        public Vector3 ViewFieldDirection4;


        /// <summary>
        /// 根据这个结构体内容来设置摄像机的显示状态，坐标/旋转/是否显示
        /// </summary>
        /// <param name="cam">要被设置状态的摄像机</param>
        public void SetCamera(Transform cam)
        {
            if (status == 1 || status == 3)
            {
                cam.gameObject.SetActive(true);
                cam.position = Position;
                cam.rotation = rotation;
            }
            else if (status == 2)
            {
                cam.gameObject.SetActive(true);
                cam.position = Position;
                cam.rotation = Quaternion.Euler(rotationZXY);
            }
            else
            {
                cam.gameObject.SetActive(false);
            }
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary> A camera statusx 4. </summary>
    ///
    /// <remarks> Dx, 2017/7/18. </remarks>
    ///-------------------------------------------------------------------------------------------------
    public struct CameraStatusx4
    {
        public CameraStatus cam_1;
        public CameraStatus cam_2;
        public CameraStatus cam_3;
        public CameraStatus cam_4;

        public CameraStatus this[int index]
        {
            get
            {
                if (index == 0)
                    return cam_1;
                else if (index == 1)
                    return cam_2;
                else if (index == 2)
                    return cam_3;
                else if (index == 3)
                    return cam_4;

                throw new Exception("CameraStatusx4的下标index错误！");
                //return new CameraStatus();
            }
            set
            {
                if (index == 0)
                    cam_1 = value;
                else if (index == 1)
                    cam_2 = value;
                else if (index == 2)
                    cam_3 = value;
                else if (index == 3)
                    cam_4 = value;
            }
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary> U3D相机的数据设置. </summary>
    ///
    /// <remarks> Dx, 2017/11/1. </remarks>
    ///-------------------------------------------------------------------------------------------------
    public struct U3DCamera
    {
        /// <summary> 相机的世界坐标. </summary>
        public Vector3 Position;

        /// <summary> 相机的世界旋转. </summary>
        public Quaternion rotation;

        /// <summary> FOV. </summary>
        public float fov;
    };

    public struct ocvdata
    {
        /// <summary> ocv数据结构体的内存大小. </summary>
        public int ocvdataSize;

        /// <summary> 4个相机数据. </summary>
        public CameraStatusx4 cam_s;

        /// <summary> 眼镜的检测状态. </summary> 
        public int GlassStatus;

        /// <summary> 眼镜坐标. </summary>
        public Vector3 GlassPosition;

        /// <summary> 眼镜坐标(考虑机身倾角). </summary>
        public Vector3 GlassPosition_w;

        /// <summary> 眼镜旋转. </summary>
        public Quaternion GlassRotation;

        /// <summary> 眼镜旋转(考虑机身倾角). </summary>
        public Quaternion GlassRotation_w;

        /// <summary>笔的检测状态. </summary> 
        public int PenStatus;

        /// <summary> 笔坐标. </summary>
        public Vector3 PenPosition;

        /// <summary> 笔坐标(考虑机身倾角). </summary>
        public Vector3 PenPosition_w;

        /// <summary> 笔方向. </summary>
        public Vector3 PenDirection;

        /// <summary> 笔方向(考虑机身倾角). </summary>
        public Vector3 PenDirection_w;

        /// <summary> 笔的滚转角. </summary>
        public float PenRoll;

        /// <summary> 整个个设备的偏转角度. </summary>
        public float DevicePitch;

        /// <summary> 笔的按键值,0x01中键,0x02左键,0x04右键. </summary>
        public int PenKey;

        /// <summary> 陀螺仪旋转,只有x，y两轴. </summary>
        public Quaternion GyroscopeRotation;

        /// <summary> 整个机子倾斜角，默认为0. </summary>
        public float slantAngle;

        /// <summary>是否在睡眠状态,0为正常检查,>0为睡眠状态.</summary>
        public int isSleep;

        /// <summary> 当前处理程序的fps. </summary>
        public float fps;

        /// <summary> 相机硬件错误标志 </summary>
        public int cameraDevErrorCode;

        /// <summary> 笔硬件错误标志. </summary>
        public int penDevErrorCode;

        /// <summary> 主控硬件错误标志. </summary>
        public int mcDevErrorCode;

        /// <summary> 16:9摄像机的中央摄像机. </summary>
        public U3DCamera camera_c;

        /// <summary> 16:9摄像机的左边摄像机. </summary>
        public U3DCamera camera_l;

        /// <summary> 16:9摄像机的右边摄像机. </summary>
        public U3DCamera camera_r;

        /// <summary> 比较可靠的点 64个. </summary>
        public DetectPointx64 reliablePoint;

        /// <summary> 最多128个跟踪点. </summary>
        public DetectPointx64 points;

        /// <summary> 用来调试看效果的摄像机射线. </summary>
        public CamRayx64 camRays;

        /// <summary> 下一个. </summary>
        public IntPtr next;
    }


    public class OCVData
    {

        [DllImport("FSCore")]
        public static extern int init();

        [DllImport("FSCore")]
        public static extern int getGlassStatus();

        [DllImport("FSCore")]
        public static extern Vector3 getGlassPosition();

        [DllImport("FSCore")]
        public static extern Quaternion getGlassRotation();

        [DllImport("FSCore")]
        public static extern int getPenStatus();

        [DllImport("FSCore")]
        public static extern Vector3 getPenPosition();

        [DllImport("FSCore")]
        public static extern Vector3 getPenDirection();

        [DllImport("FSCore")]
        public static extern float getPenRoll();

        [DllImport("FSCore")]
        public static extern int getPenKey();

        [DllImport("FSCore")]
        public static extern float getSlantAngle();

        [DllImport("FSCore")]
        public static extern void setPenShake(int mode = 1);

        [DllImport("FSCore")]
        public static extern void setActiveUser();

        [DllImport("FSCore")]
        static extern IntPtr getOCVDataAddr();

        [DllImport("FSCore")]
        public static extern void setflagVRMode(int flag);

        /// <summary>是否在睡眠状态,0为正常检查,大于0为睡眠状态.</summary>
        [DllImport("FSCore")]
        public static extern int getIsSleep();

        /// <summary> 当前处理程序的fps. </summary>
        [DllImport("FSCore")]
        public static extern float getFps();

        /// <summary> 相机硬件错误标志 </summary>
        [DllImport("FSCore")]
        public static extern int getCameraDevErrorCode();

        /// <summary> 笔硬件错误标志. </summary>
        [DllImport("FSCore")]
        public static extern int getPenDevErrorCode();

        /// <summary> 主控硬件错误标志. </summary>
        [DllImport("FSCore")]
        public static extern int getMCDevErrorCode();


        /// <summary>
        /// 大家共用的检测数据
        /// </summary>
        public static ocvdata _data;


        /// <summary> 
        /// 数据起始的地址. 
        /// </summary>
        internal static IntPtr pOCVData;

        ///// <summary> 
        ///// u3d数据的地址. 
        ///// </summary>
        //internal static IntPtr pU3dData;

        /// <summary>
        /// 初始化这个数据，在Start()的时候运行一下就ok了
        /// </summary>
        internal static void InitData()
        {
            init();
            pOCVData = getOCVDataAddr();
            //pU3dData = getU3dDataAddr();
        }

        /// <summary>
        /// 每帧调用，应该在Update中调用
        /// </summary>
        internal static void UpdateOCVdata()
        {
            if (pOCVData != IntPtr.Zero)
            {
                _data = (ocvdata)Marshal.PtrToStructure(pOCVData, typeof(ocvdata));
            }
            else
            {
                Debug.LogWarning("OCVData.UpdateOCVdata():pOCVData为NULL，尝试再次InitData!");
                InitData();
            }

            updateCount++;
        }

        /// <summary>
        /// 写u3d数据到检测程序
        /// </summary>
        internal static void writeU3Ddata()
        {
            //将结构体拷到分配好的内存空间
            //Marshal.StructureToPtr(_u3ddata, pU3dData, false);

        }


        /// <summary>
        /// 数据更新的计数
        /// </summary>
        public static long updateCount;
    }
}