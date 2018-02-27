using System;
using System.Collections;
using UnityEngine;

namespace FSpace
{
    /// <summary>
    /// 这个类能够更新OCV数据，这个脚本的优先级可能要放到最前比较好。同时让它作为整个系统的锚点。当开启了自动倾斜的时候，这个系统以底边
    /// </summary>
    public class MRSystem : MonoBehaviour
    {
        void Start()
        {
            OCVData.InitData();
            OCVData.UpdateOCVdata();
        }

        private void FixedUpdate()
        {
            this.Update();
        }

        private void Update()
        {

            //写入u3d到ocv的数据
            //OCVData.writeU3Ddata();

            //设置当前的TRS
            FCore.anchorRTS.SetTRS(transform.position, transform.rotation, transform.lossyScale);
            FCore.anchorRMat.SetTRS(Vector3.zero, transform.rotation, transform.lossyScale);
            FCore.anchorRQuat = transform.rotation;

            //读取新的OCV数据
            OCVData.UpdateOCVdata();

            //读取新的数据之后将底边设置为0
            setBottonZero();

            //更新倾斜的屏幕点
            BaseSystem.updateScreenPoint();

            //更新按键状态以便发出事件
            FCore.updateKeyEvent();

            //更新一下记录的拖拽物体的状态好了
            FCore.updateDragObj();

            //设置自己是活动用户
            OCVData.setActiveUser();
        }

        private void OnDrawGizmos()
        {

            //设置当前的TRS
            FCore.anchorRTS.SetTRS(transform.position, transform.rotation, transform.lossyScale);
            FCore.anchorRMat.SetTRS(Vector3.zero, transform.rotation, transform.lossyScale);
            FCore.anchorRQuat = transform.rotation;

            //画位置要改
            Debug.DrawLine(FCore.screenPointLeftTop, FCore.screenPointRightTop, Color.red);
            Debug.DrawLine(FCore.screenPointRightTop, FCore.screenPointRightBotton, Color.red);
            Debug.DrawLine(FCore.screenPointRightBotton, FCore.screenPointLeftBotton, Color.red);
            Debug.DrawLine(FCore.screenPointLeftTop, FCore.screenPointLeftBotton, Color.red);

        }

        private void setBottonZero()
        {
            //眼镜和笔的坐标都上抬0.15m,让底边在0点。
            OCVData._data.PenPosition.y += 0.15f;
            OCVData._data.GlassPosition.y += 0.15f;

            //这里还应该尝试上抬更多东西（调试数据）

        }


    }
}