using System;
using System.Collections.Generic;
using UnityEngine;

namespace FSpace
{
    /// <summary>
    /// 简单拖拽的示例，首先在Start()中绑定事件监听操控笔的按下抬起动作。
    /// 当操控笔按钮0按下的时候，可以开始一个物体的拖拽，在拖拽的同时时候按动按钮1，按钮2，
    /// 可以进行拖拽物体的拉近拉远操作。
    /// </summary>
    public class SimpleDrag : MonoBehaviour
    {

        /// <summary>
        /// 创建的笔的射线物体
        /// </summary>
        GameObject _penObj;

        private void Awake()
        {
            FCore.ViewerScale = 2;
        }

        void Start()
        {

            //设置屏幕为3D显示模式
            FCore.SetScreen3D();

            FCore.EventKey0Down += OnKey0Down;
            FCore.EventKey0Up += OnKey0Up;
            FCore.EventKey1Down += OnKey1Down;
            FCore.EventKey2Down += OnKey2Down;

            _penObj = new GameObject("penRay");
            _penObj.AddComponent<PenRay>();
        }

        private void OnApplicationQuit()
        {
            //在程序退出的时候设置屏幕为2D显示
            FCore.SetScreen2D();
        }

        /// <summary>
        /// 是否在点击的时候震动一下
        /// </summary>
        public bool enableShake = true;

        /// <summary>
        /// 记录当前拖拽的物体
        /// </summary>
        GameObject _curDragObj;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        /// 按键0按下的事件响应.
        /// </summary>
        ///
        /// <remarks> Dx, 2017/9/22. </remarks>
        ///-------------------------------------------------------------------------------------------------
        private void OnKey0Down()
        {
            RaycastHit raycastHit;
            GameObject dragObj = Raycast(out raycastHit);
            if (dragObj != null)
            {
                _curDragObj = dragObj;//记录当前拖拽物体
                FCore.addDragObj(raycastHit.collider.gameObject, raycastHit.distance);
                if (enableShake)
                {
                    FCore.PenShake();//震动一下
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        /// 按键0抬起的事件响应.
        /// </summary>
        ///
        /// <remarks> Dx, 2017/9/22. </remarks>
        ///-------------------------------------------------------------------------------------------------
        private void OnKey0Up()
        {
            FCore.deleteDragObj(_curDragObj);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        /// 按键1按下的事件响应.
        /// </summary>
        ///
        /// <remarks> Dx, 2018/3/5. </remarks>
        ///-------------------------------------------------------------------------------------------------
        private void OnKey1Down()
        {
            if (FCore.isDraging)
            {
                FCore.pushDragObj(_curDragObj, 0.10f);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        /// 按键2按下的事件响应.
        /// </summary>
        ///
        /// <remarks> Dx, 2018/3/5. </remarks>
        ///-------------------------------------------------------------------------------------------------
        private void OnKey2Down()
        {
            if (FCore.isDraging)
            {
                FCore.pullDragObj(_curDragObj, 0.10f);
            }
        }

        private void OnDrawGizmos()
        {
            //调整整个系统坐标尺度缩放，放大一倍。编辑器中的红框可以看到放大一倍。
            //这里的设置只是为了编辑器中的显示
            FCore.ViewerScale = 2;
        }

        private void Update()
        {
            if (FCore.isDraging)
            {
                //响应两个按键长按的功能
                if (FCore.isKey1Down)
                    FCore.pushDragObj(_curDragObj, 1.50f * Time.deltaTime);
                if (FCore.isKey2Down)
                    FCore.pullDragObj(_curDragObj, 1.50f * Time.deltaTime);
            }

        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        /// 简单的检测，如果有物体则返回物体，否则返回null.
        /// </summary>
        ///
        /// <remarks> Dx, 2017/9/22. </remarks>
        ///
        /// <param name="raycastHit"> [out] The raycast hit. </param>
        ///
        /// <returns> A GameObject. </returns>
        ///-------------------------------------------------------------------------------------------------
        private GameObject Raycast(out RaycastHit raycastHit)
        {
            int layer = LayerMask.NameToLayer("Default");
            float rayLength = FCore.ViewerScale * 1;//规定射线拿取的最大长度为1m，这里可以自由规定
            if (Physics.Raycast(FCore.penRay, out raycastHit, rayLength, 1 << layer))
            {
                return raycastHit.collider.gameObject;
            }
            return null;
        }
    }
}