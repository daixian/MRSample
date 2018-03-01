using UnityEngine;

namespace FSpace
{
    /// <summary>
    /// 绘制一条从笔尖发出的射线
    /// </summary>
    internal class PenRay : MonoBehaviour
    {
        private LineRenderer _lineRenderer;

        /// <summary>
        /// 射线的最长长度
        /// </summary>
        private float rayLength;

        private void Awake()
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.material = new Material(Shader.Find("Unlit/Texture"));
        }

        private void Update()
        {
            rayLength = 1 * FCore.ViewerScale;

            _lineRenderer.SetPosition(0, FCore.penPosition);
            if (FCore.isDraging)//如果当前正在拖拽状态
            {
                //使用记录的拖拽距离来设置射线长
                _lineRenderer.SetPosition(1, FCore.penPosition + FCore.lastDragDistance * FCore.penDirection.normalized);
            }
            else//如果不在拖拽状态
            {
                RaycastHit raycastHit;
                int layer = LayerMask.NameToLayer("Default");
                if (Physics.Raycast(FCore.penRay, out raycastHit, rayLength, 1 << layer))//使用设定的射线长度来做射线检测
                {
                    _lineRenderer.SetPosition(1, raycastHit.point);
                }
                else
                {
                    _lineRenderer.SetPosition(1, FCore.penPosition + (rayLength * FCore.penDirection.normalized));
                }
            }
            _lineRenderer.startWidth = 0.0015f * FCore.ViewerScale;
            _lineRenderer.endWidth = 0.0015f * FCore.ViewerScale;
        }

        private void OnDestroy()
        {
            Destroy(_lineRenderer.material);
        }
    }
}