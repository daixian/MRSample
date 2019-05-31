using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GCSeries
{
    /// <summary>
    /// 简单拖拽的示例，首先在Start()中绑定事件监听操控笔的按下抬起动作。
    /// 当操控笔按钮1按下的时候，可以开始一个物体的拖拽，
    /// </summary>
    public class SimpleDrag : MonoBehaviour
    {
        /// <summary>
        /// 创建的笔的射线物体
        /// </summary>
        GameObject _penObj;

        void Start()
        {
            //设置屏幕为3D显示模式
            FCore.SetScreen3D();

            FCore.EventKey1Down += OnKey1Down;
            FCore.EventKey1Up += OnKey1Up;

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
        /// 按键1按下的事件响应.
        /// </summary>
        /// <remarks> Dx, 2017/9/22. </remarks>
        ///-------------------------------------------------------------------------------------------------
        private void OnKey1Down()
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
        /// 按键1抬起的事件响应.
        /// </summary>
        /// <remarks> Dx, 2017/9/22. </remarks>
        ///-------------------------------------------------------------------------------------------------
        private void OnKey1Up()
        {
            FCore.deleteDragObj(_curDragObj);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        /// 简单的检测，如果有物体则返回物体，否则返回null.
        /// </summary>
        /// <remarks> Dx, 2017/9/22. </remarks>
        /// <param name="raycastHit"> [out] The raycast hit. </param>
        /// <returns> A GameObject. </returns>
        ///-------------------------------------------------------------------------------------------------
        private GameObject Raycast(out RaycastHit raycastHit)
        {
            int layer = LayerMask.NameToLayer("Default");
            float rayLength = FCore.ViewerScale * PenRay.MAX_RAY_LENGTH;//规定射线拿取的最大长度为1m，这里可以自由规定
            if (Physics.Raycast(FCore.penRay, out raycastHit, rayLength, 1 << layer))
            {
                return raycastHit.collider.gameObject;
            }
            return null;
        }
    }
}