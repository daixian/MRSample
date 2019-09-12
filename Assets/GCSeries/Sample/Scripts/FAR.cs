using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GCSeries
{
    public class FAR : MonoBehaviour
    {
        /// <summary>
        /// 相机采图
        /// </summary>
        private WebCamTexture _camTex;

        /// <summary>
        /// 相机背景图
        /// </summary>
        public RawImage riC920;

        /// <summary>
        /// 一个罗技摄像头姿态的记录
        /// </summary>
        Vector3 viewPosition;

        /// <summary>
        /// 一个罗技摄像头姿态的记录
        /// </summary>
        Quaternion viewRotation;


        private void Awake()
        {
            string fViewJsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FViewTool", "FView.json");
            if (!File.Exists(fViewJsonPath))
            {
                UnityEngine.Debug.LogError("FView.Awake():先使用工具软件进行罗技摄像头的标定！");
                return;
            }

            if (FARDll.fmFViewReadJson() == 0)//读取json成功
            {
                //读坐标viewPosition
                int size = Marshal.SizeOf(viewPosition);
                IntPtr structPtr = Marshal.AllocHGlobal(size);
                FARDll.fmFViewGetPosition(structPtr);
                viewPosition = (Vector3)Marshal.PtrToStructure(structPtr, typeof(Vector3));
                Marshal.FreeHGlobal(structPtr);

                //读旋转viewRotation
                size = Marshal.SizeOf(viewRotation);
                structPtr = Marshal.AllocHGlobal(size);
                FARDll.fmFViewGetRotation(structPtr);
                viewRotation = (Quaternion)Marshal.PtrToStructure(structPtr, typeof(Quaternion));
                Marshal.FreeHGlobal(structPtr);
            }
            //使用标定结果设置CamRoot的坐标(注意坐标需要缩放)
            transform.localPosition = viewPosition * FCore.ViewerScale;
            transform.localRotation = viewRotation;
        }

        // Use this for initialization
        async void Start()
        {
            StartCoroutine(InitCamera());
            await Task.Delay(3000);
        }

        /// <summary>
        /// 打开罗技HD Pro Webcam C920的相机
        /// </summary>
        /// <returns></returns>
        IEnumerator InitCamera()
        {
            //获取授权
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                var devices = WebCamTexture.devices;
                if (devices.Length > 4)
                {
                    string _deviceName = "";
                    foreach (var item in devices)
                    {
                        if (item.name.EndsWith("C920"))
                        {
                            _deviceName = item.name;
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(_deviceName))
                    {
                        UnityEngine.Debug.LogError("ScreenControlObj.InitCamera():相机启动失败，没有外接相机");
                    }
                    else
                    {
                        if (_camTex == null)
                        {
                            _camTex = new WebCamTexture(_deviceName, 1280, 720, 15);//设置为1280x720可以减少相机延迟
                            riC920.texture = _camTex;
                            _camTex.Play();
                        }
                        else
                        {
                            riC920.texture = _camTex;
                            _camTex.Play();
                        }
                        //_tex.anisoLevel = 2;
                        UnityEngine.Debug.Log("ScreenControlObj.InitCamera():相机启动");
                    }
                }
            }
        }

        /// <summary>
        /// 打开罗技相机采图
        /// </summary>
        public void OpenCamera()
        {
            if (_camTex != null)
            {
                if (!_camTex.isPlaying)
                {
                    _camTex.Play();
                }
            }
            else
            {
                UnityEngine.Debug.LogError("FView.OpenCamera():相机还没有初始化，需要等协程函数InitCamera()执行成功。");
            }
        }

        /// <summary>
        /// 关闭罗技相机采图
        /// </summary>
        public void CloseCamera()
        {
            if (_camTex != null)
            {
                if (_camTex.isPlaying)
                {
                    _camTex.Stop();
                }
            }
        }
    }
}