using System;
using System.Collections.Generic;
using UnityEngine;

namespace FSpace
{
    /// <summary>
    /// 简单拖拽的示例
    /// </summary>
    public class SimpleDrag : MonoBehaviour
    {
        GameObject _penObj;

        void Start()
        {
            //设置屏幕为3D显示模式
            FCore.SetScreen3D();

            FCore.EventKey0Down += OnKey0Down;
            FCore.EventKey0Up += OnKey0Up;

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
            if (Physics.Raycast(FCore.penRay, out raycastHit, 10, 1 << layer))
            {
                return raycastHit.collider.gameObject;
            }
            return null;
        }
    }
}