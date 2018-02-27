using System;
using System.Collections.Generic;
using UnityEngine;

namespace FSpace
{
    internal class DragRecord
    {
        /// <summary>
        /// 构造，依赖FCore中的当前笔状态数据
        /// </summary>
        /// <param name="dragObj">拖拽物体</param>
        public DragRecord(Transform dragObj, float distance)
        {
            this.dragObj = dragObj;

            //Matrix4x4 rDragObj = new Matrix4x4();
            ////rDragObj.SetTRS(dragObj.position, dragObj.rotation, new Vector3(1, 1, 1));
            //rDragObj.SetTRS(hitPoint, dragObj.rotation, new Vector3(1, 1, 1));
            //offset = FCore.penMatrix4x4.inverse * rDragObj; 

            //float distance = (hitPoint - FCore.penPosition).magnitude;
            this.BeginGrab(dragObj.gameObject, distance, FCore.penPosition, FCore.penRotation);
        }

        /// <summary>
        /// 拖拽物体
        /// </summary>
        Transform dragObj;


        private Vector3 _initialGrabOffset = Vector3.zero;
        private Quaternion _initialGrabRotation = Quaternion.identity;
        private float _initialGrabDistance = 0.0f;

        /// <summary>
        /// 抓取点到笔尖的距离
        /// </summary>
        public float distance
        {
            get
            {
                return _initialGrabDistance;
            }
        }

        /// <summary>
        /// 使用当前的笔状态去更新这个拖拽物体的位置
        /// </summary>
        public void Update()
        {
            this.UpdateGrab(FCore.penPosition, FCore.penRotation);

        }


        private void BeginGrab(GameObject hitObject, float hitDistance, Vector3 inputPosition, Quaternion inputRotation)
        {
            Vector3 inputEndPosition = inputPosition + (inputRotation * (Vector3.forward * hitDistance));

            // Cache the initial grab state.
            dragObj = hitObject.transform;

            //ZSpace官方写法使用了本地旋转
            //_initialGrabOffset = Quaternion.Inverse(hitObject.transform.localRotation) * (hitObject.transform.localPosition - inputEndPosition);
            //_initialGrabRotation = Quaternion.Inverse(inputRotation) * hitObject.transform.localRotation;

            _initialGrabOffset = Quaternion.Inverse(hitObject.transform.rotation) * (hitObject.transform.position - inputEndPosition);
            _initialGrabRotation = Quaternion.Inverse(inputRotation) * hitObject.transform.rotation;

            _initialGrabDistance = hitDistance;
        }

        protected void UpdateGrab(Vector3 inputPosition, Quaternion inputRotation)
        {
            Vector3 inputEndPosition = inputPosition + (inputRotation * (Vector3.forward * _initialGrabDistance));

            // Update the grab object's rotation.
            Quaternion objectRotation = inputRotation * _initialGrabRotation;
            dragObj.rotation = objectRotation;

            // Update the grab object's position.
            Vector3 objectPosition = inputEndPosition + (objectRotation * _initialGrabOffset);
            dragObj.position = objectPosition;
        }

    }

}