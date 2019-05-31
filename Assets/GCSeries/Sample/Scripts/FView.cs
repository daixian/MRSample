using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GCSeries
{

    public class FView : MonoBehaviour
    {
        #region extern fun
        [DllImport("FView")]
        private static extern void fmFViewStart(System.IntPtr hWnd, System.IntPtr textureHandle, int w, int h);

        [DllImport("FView")]
        private static extern void fmFViewStop();

        [DllImport("FView")]
        private static extern int fmFViewReadJson();


        [DllImport("FView")]
        private static extern void fmFViewGetPosition(IntPtr value);

        [DllImport("FView")]
        private static extern void fmFViewGetRotation(IntPtr value);


        //寻找当前目标窗口的进程
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        //根据窗口句柄获取pid
        [DllImport("User32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        #endregion

        /// <summary>
        /// 相机采图
        /// </summary>
        private WebCamTexture _camTex;

        /// <summary>
        /// 相机背景图
        /// </summary>
        public RawImage riC920;

        /// <summary>
        /// 渲染结果纹理
        /// </summary>
        public RenderTexture rt;

        /// <summary>
        /// 渲染纹理指针
        /// </summary>
        private IntPtr rtPtr = IntPtr.Zero;

        /// <summary>
        /// 显示窗口句柄
        /// </summary>
        IntPtr _hWndViewClient = IntPtr.Zero;

        /// <summary>
        /// 显示窗口进程
        /// </summary>
        Process viewProcess;

        /// <summary>
        /// 一个罗技摄像头姿态的记录
        /// </summary>
        Vector3 viewPosition;

        /// <summary>
        /// 一个罗技摄像头姿态的记录
        /// </summary>
        Quaternion viewRotation;

        /// <summary>
        /// 是否自动启动FView窗口,调试用
        /// </summary>
        public bool isAutoStartViewWin = true;

        void Awake()
        {
            //这里仅仅只是检查一下是否有标定过(标定工具软件路径  C:\Program Files\MRSystem\FViewTool.exe)
            string fViewJsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FViewTool", "FView.json");
            if (!File.Exists(fViewJsonPath))
            {
                UnityEngine.Debug.LogError("FView.OpenFViewWindows():先使用工具软件进行罗技摄像头的标定！");
                return;
            }

            if (fmFViewReadJson() == 0)//读取json成功
            {
                //读坐标viewPosition
                int size = Marshal.SizeOf(viewPosition);
                IntPtr structPtr = Marshal.AllocHGlobal(size);
                fmFViewGetPosition(structPtr);
                viewPosition = (Vector3)Marshal.PtrToStructure(structPtr, typeof(Vector3));
                Marshal.FreeHGlobal(structPtr);

                //读旋转viewRotation
                size = Marshal.SizeOf(viewRotation);
                structPtr = Marshal.AllocHGlobal(size);
                fmFViewGetRotation(structPtr);
                viewRotation = (Quaternion)Marshal.PtrToStructure(structPtr, typeof(Quaternion));
                Marshal.FreeHGlobal(structPtr);
            }
        }

        async void Start()
        {
            //打开相机
            StartCoroutine(InitCamera());

            //过3秒之后启动窗口(可以注释掉)
            if (isAutoStartViewWin)
            {
                await Task.Delay(3000);
                OpenFViewWindows();
            }
        }

        // Update is called once per frame
        void Update()
        {
            //如果按下了V键就打开窗口
            if (Input.GetKeyDown(KeyCode.V))
            {
                OpenFViewWindows();
            }

            //使用标定结果设置CamRoot的坐标(注意坐标需要缩放)
            transform.localPosition = viewPosition * FCore.ViewerScale;
            transform.localRotation = viewRotation;

            IntPtr curRtPtr = rt.GetNativeTexturePtr();
            if (curRtPtr != rtPtr && _hWndViewClient != IntPtr.Zero)
            {
                UnityEngine.Debug.Log("FView.Update():rtPtr值更新！");
                rtPtr = curRtPtr;
                fmFViewStart(_hWndViewClient, rtPtr, 1920, 1080);
            }
        }
        void OnApplicationQuit()
        {
            CloseFViewWindows();
        }

        /// <summary>
        /// 启动FView窗口
        /// </summary>
        public void OpenFViewWindows()
        {
            if (viewProcess != null)
            {
                UnityEngine.Debug.Log("FView.OpenFViewWindows():当前已经启动了一个fview窗口！");
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Path.Combine(Application.streamingAssetsPath, "ClientWin.exe"); ;
            viewProcess = new Process();
            viewProcess.StartInfo = startInfo;
            viewProcess.Start();

            _hWndViewClient = IntPtr.Zero;
            while (true)
            {
                _hWndViewClient = FindWindow(null, "ViewClient");
                if (_hWndViewClient != IntPtr.Zero)
                {
                    UnityEngine.Debug.Log("FView.OpenFViewWindows():找到了窗口句柄！");
                    int pid = 0;
                    GetWindowThreadProcessId(_hWndViewClient, out pid);
                    if (pid == viewProcess.Id)
                    {
                        break;
                    }
                }
            }

            UnityEngine.Debug.Log("FView.OpenFViewWindows():开始绘图！");

        }

        /// <summary>
        /// 关闭FView的窗口
        /// </summary>
        public void CloseFViewWindows()
        {
            if (viewProcess != null)
            {
                try
                {
                    viewProcess.Kill();

                }
                catch (Exception)
                {
                }
                finally
                {
                    viewProcess = null;
                }
            }
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
                        UnityEngine.Debug.LogError("FView.InitCamera():相机启动失败，没有外接相机");
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
                        UnityEngine.Debug.Log("FView.InitCamera():相机启动");
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