using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FSpace
{

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    /// 这是一个临时的3d视觉实现，仅供内部使用！
    /// 在这个表示3d眼镜的物体下放三个相机的子节点， 分别是中央相机，左眼相机，右眼相机.
    /// </summary>
    ///
    /// <remarks> Dx, 2017/11/2. </remarks>
    ///-------------------------------------------------------------------------------------------------
    public class Camera3D : MonoBehaviour
    {
        /// <summary>
        /// 中央相机，场景中物体命名为"cam_c"
        /// </summary>
        public Camera cam_c;

        /// <summary>
        /// 中央相机，场景中物体命名为"cam_l"
        /// </summary>
        public Camera cam_l;

        /// <summary>
        /// 中央相机，场景中物体命名为"cam_r"
        /// </summary>
        public Camera cam_r;

        /// <summary>
        /// 原始的投影矩阵
        /// </summary>
        Matrix4x4 originalProjection;

        private void Start()
        {
            findCamera();
        }

        void findCamera()
        {
            //在这个物体的子节点下自动寻找相机节点
            Camera[] cams = GetComponentsInChildren<Camera>(true);
            for (int i = 0; i < cams.Length; i++)
            {
                if (cam_c == null && cams[i].name == "cam_c")
                {
                    cam_c = cams[i];
                }
                if (cam_l == null && cams[i].name == "cam_l")
                {
                    cam_l = cams[i];
                }
                if (cam_r == null && cams[i].name == "cam_r")
                {
                    cam_r = cams[i];
                }
                //设置它的父节点
                cams[i].transform.parent = transform;
                cams[i].transform.localPosition = Vector3.zero;
                cams[i].transform.localRotation = Quaternion.Euler(0, 0, 0);
            }

            if (cam_c == null)
            {
                GameObject go = new GameObject("cam_c");
                cam_c = go.AddComponent<Camera>();
                cam_c.transform.parent = transform;
                cam_c.transform.localPosition = Vector3.zero;
                cam_c.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            if (cam_l == null)
            {
                GameObject go = new GameObject("cam_l");
                cam_l = go.AddComponent<Camera>();
                cam_l.transform.parent = transform;
                cam_l.transform.localPosition = Vector3.zero;
                cam_l.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            if (cam_r == null)
            {
                GameObject go = new GameObject("cam_r");
                cam_r = go.AddComponent<Camera>();
                cam_r.transform.parent = transform;
                cam_r.transform.localPosition = Vector3.zero;
                cam_r.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }


            //设置瞳距为6.6cm
            cam_l.transform.localPosition = new Vector3(-FCore.PupilDistance / 2.0f, 0, 0) * FCore.ViewerScale;
            cam_r.transform.localPosition = new Vector3(FCore.PupilDistance / 2.0f, 0, 0) * FCore.ViewerScale;

            //调整视口为渲染半边
            cam_l.rect = new Rect(0, 0, 0.5f, 1);
            cam_r.rect = new Rect(0.5f, 0, 0.5f, 1);

            //目前中央相机实际上没有工作
            cam_l.gameObject.SetActive(true);
            cam_r.gameObject.SetActive(true);
            cam_c.gameObject.SetActive(false);

            transform.position = new Vector3(0, 0, -0.5f);

            originalProjection = cam_l.projectionMatrix;
        }


        void Update()
        {
            if (OCVData._data.GlassStatus == 1)
            {
                transform.position = FCore.glassPosition;
                transform.rotation = FCore.anchorRQuat * Quaternion.Euler(FCore.slantAngle, 0, 0);//让相机和屏幕平面垂直
            }

            //设置瞳距为6.6cm
            //cam_l.transform.localPosition = new Vector3(-FCore.PupilDistance / 2.0f, 0, 0) * FCore.ViewerScale;
            //cam_r.transform.localPosition = new Vector3(FCore.PupilDistance / 2.0f, 0, 0) * FCore.ViewerScale;
            cam_l.transform.position = FCore.eyeLeftPosition;
            cam_r.transform.position = FCore.eyeRightPosition;
            cam_l.transform.rotation = FCore.anchorRQuat * Quaternion.Euler(FCore.slantAngle, 0, 0);
            cam_r.transform.rotation = FCore.anchorRQuat * Quaternion.Euler(FCore.slantAngle, 0, 0);

            setCameraProjMat(cam_l, FCore.eyeLeftPosition);
            setCameraProjMat(cam_c, FCore.glassPosition);
            setCameraProjMat(cam_r, FCore.eyeRightPosition);

        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary> 设置相机投影矩阵. </summary>
        ///
        /// <remarks> Dx, 2017/11/7. </remarks>
        ///
        /// <param name="cam"> The camera. </param>
        ///-------------------------------------------------------------------------------------------------
        void setCameraProjMat(Camera cam, Vector3 camPosition)
        {
            Matrix4x4 w2cam = cam.worldToCameraMatrix;
            float[] s4p = new float[] { FCore.screenPointLeftTop.x, FCore.screenPointLeftTop.y, FCore.screenPointLeftTop.z,
                                        FCore.screenPointLeftBotton.x, FCore.screenPointLeftBotton.y, FCore.screenPointLeftBotton.z,
                                        FCore.screenPointRightTop.x, FCore.screenPointRightTop.y, FCore.screenPointRightTop.z,
                                        FCore.screenPointRightBotton.x, FCore.screenPointRightBotton.y, FCore.screenPointRightBotton.z};
            Matrix4x4 p = originalProjection;
            matFun6(ref w2cam, s4p, ref p);
            cam.projectionMatrix = p;
        }


        [DllImport("FSCore")]
        static extern void matFun6(ref Matrix4x4 A, float[] B, ref Matrix4x4 result);

    }

}