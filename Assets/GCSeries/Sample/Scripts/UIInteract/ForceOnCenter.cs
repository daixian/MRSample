using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GCSeries
{
    /// <summary>
    /// 让Canvas的UI适配到MRSystem的红框内，
    /// 把脚本挂载在Canvas物体。
    /// </summary>
    public class ForceOnCenter : MonoBehaviour
    {
        void Start()
        {
            //调整Canvas物体的缩放
            float scale = FCore.screenHeight / Screen.height;
            transform.localScale = Vector3.one * scale;
        }

        void Update()
        {
            transform.position = FCore.screenCentre;
            transform.rotation = FCore.screenRotation;
        }
    }
}