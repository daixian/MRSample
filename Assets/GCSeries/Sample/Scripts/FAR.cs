using FSpace;
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

        //寻找当前目标窗口的进程
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        //根据窗口句柄获取pid
        [DllImport("User32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern long SetWindowPos(IntPtr hwnd, long hWndInsertAfter, long x, long y, long cx, long cy, long wFlags);
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLongPtrA(IntPtr hwnd, int _nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool BRePaint);

        /// <summary>
        /// 相机采图
        /// </summary>
        private WebCamTexture _camTex;

        /// <summary>
        /// 相机背景图
        /// </summary>
        public RawImage riC920;

        /// <summary>
        /// 渲染结果相机
        /// </summary>
        public Camera ARcam;

        /// <summary>
        /// 显示窗口句柄
        /// </summary>
        IntPtr _hViewClient = IntPtr.Zero;

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


        private void Awake()
        {
            string fViewJsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FViewTool", "FView.json");
            if (!File.Exists(fViewJsonPath))
            {
                UnityEngine.Debug.LogError("FView.Awake():先使用工具软件进行罗技摄像头的标定！");
                return;
            }

            if (FARSingleton.fmFViewReadJson() == 0)//读取json成功
            {
                //读坐标viewPosition
                int size = Marshal.SizeOf(viewPosition);
                IntPtr structPtr = Marshal.AllocHGlobal(size);
                FARSingleton.fmFViewGetPosition(structPtr);
                viewPosition = (Vector3)Marshal.PtrToStructure(structPtr, typeof(Vector3));
                Marshal.FreeHGlobal(structPtr);

                //读旋转viewRotation
                size = Marshal.SizeOf(viewRotation);
                structPtr = Marshal.AllocHGlobal(size);
                FARSingleton.fmFViewGetRotation(structPtr);
                viewRotation = (Quaternion)Marshal.PtrToStructure(structPtr, typeof(Quaternion));
                Marshal.FreeHGlobal(structPtr);
            }
            //使用标定结果设置CamRoot的坐标(注意坐标需要缩放)
            transform.localPosition = viewPosition * FCore.ViewerScale;
            transform.localRotation = viewRotation;
            //创建一个新的渲染目标纹理并绑定到ARcam
            RenderTexture temp_RT = new RenderTexture((int)FARSingleton.SwapchainWidth, (int)FARSingleton.SwapchanHeight, 0);
            ARcam.targetTexture = temp_RT;
        }

        // Use this for initialization
        async void Start()
        {
            StartCoroutine(InitCamera());
            await Task.Delay(3000);

            StartCoroutine(OpenFARWindows());
        }

        const uint SWP_SHOWWINDOW = 0x0001;//全屏
        const int GWL_STYLE = -16;//无边框
        const int WS_POPUP = 0x800000;
        public void UpdateWindowPos(IntPtr ProjectionWindow)
        {
            //可先切换到扩展模式
            //SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, (SDC_APPLY | SDC_TOPOLOGY_EXTEND));
            //更新物理显示器列表
            int temp_MonitorCount = FARSingleton.fmARUpdatePhysicalMonitor();
            if (temp_MonitorCount < 0)
                throw new Exception("fmARUpdatePhysicalMonitor failed with error :" + temp_MonitorCount);

            if (temp_MonitorCount != 2)
                throw new Exception("Current monitor count : " + temp_MonitorCount);

            for (int k = 0; k < temp_MonitorCount; k++)
            {
                FARSingleton.GCinfo gcinfo = new FARSingleton.GCinfo();
                int result = FARSingleton.fmARGetMonitorInfoByIndex(ref gcinfo, k);
                if (result >= 0)
                {
                    if (!gcinfo.isGCmonitor)
                    {
                        UnityEngine.Debug.Log("FARSingleton.fmARGetMonitorInfoByIndex()：投屏显示器：" + gcinfo.DeviceName);
                        if (IntPtr.Size.Equals(8))
                            SetWindowLongPtrA(ProjectionWindow, GWL_STYLE, WS_POPUP);
                        else
                            SetWindowLong(ProjectionWindow, GWL_STYLE, WS_POPUP);
                        MoveWindow(ProjectionWindow, gcinfo.RCleft, gcinfo.RCtop, gcinfo.RCright - gcinfo.RCleft, gcinfo.RCbottom - gcinfo.RCtop, false);
                    }
                }
                else
                    throw new Exception("fmARGetMonitorInfoByIndex failed with error :" + result);
            }
        }

        private IEnumerator OpenFARWindows()
        {
            //等待下一帧开始
            yield return new WaitForEndOfFrame();
            if (FindWindow(null, "ViewClient") != IntPtr.Zero)
            {
                yield break;
            }
            string _path = Path.Combine(Application.streamingAssetsPath, "ClientWin.exe");
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = _path;
            viewProcess = new Process();
            viewProcess.StartInfo = startInfo;
            viewProcess.Start();

            _hViewClient = IntPtr.Zero;
            while (true)
            {
                _hViewClient = FindWindow(null, "ViewClient");
                if (_hViewClient != IntPtr.Zero)
                {
                    UnityEngine.Debug.Log("FAR.OpenFARWindows():找到了窗口句柄！");
                    //全屏到非GC显示器
                    UpdateWindowPos(_hViewClient);
                    int pid = 0;
                    GetWindowThreadProcessId(_hViewClient, out pid);
                    if (pid == viewProcess.Id)
                    {
                        //设置当前的色彩空间，u3d默认是Gama空间
                        FARSingleton.GetInstance().SetColorSpace(FARSingleton.U3DColorSpace.Gama);
                        //开始绘制同屏窗口，如目标纹理指针变更可随时调用
                        FARSingleton.GetInstance().StartView(_hViewClient, ARcam.targetTexture.GetNativeTexturePtr());
                        break;
                    }
                }
            }
            UnityEngine.Debug.Log("FAR.OpenFARWindows():开始绘图！");
        }

        private void OnApplicationQuit()
        {
            FARSingleton.GetInstance().CloseDown();
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