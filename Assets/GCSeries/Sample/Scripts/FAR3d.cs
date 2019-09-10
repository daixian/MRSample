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
    public class FAR3d : MonoBehaviour
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
        /// 渲染相机
        /// </summary>
        public Camera Camera2D;
        public Camera Camera3Dleft;
        public Camera Camera3Dright;
        private bool is3d = true;

        /// <summary>
        /// 显示窗口句柄
        /// </summary>
        IntPtr _hViewClient = IntPtr.Zero;

        /// <summary>
        /// 显示窗口进程
        /// </summary>
        Process viewProcess;


        private void Awake()
        {
            //创建一个新的渲染目标纹理并绑定到Camera
            RenderTexture temp_2D = new RenderTexture((int)1920, (int)1080, 0);
            Camera2D.targetTexture = temp_2D;
            //3D下由于屏幕是水平分割像素点，所以纹理高度只需要一半。不过这个尺寸可以按实际情况修改
            RenderTexture temp_left = new RenderTexture((int)1920, (int)1080 / 2, 0);
            Camera3Dleft.targetTexture = temp_left;

            RenderTexture temp_right = new RenderTexture((int)1920, (int)1080 / 2, 0);
            Camera3Dright.targetTexture = temp_right;
        }

        void Start()
        {
            //启动投屏窗口
            StartCoroutine(OpenFARWindows());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                //启动投屏窗口
                StartCoroutine(OpenFARWindows());
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                //关闭投屏窗口
                FARSingleton.GetInstance().CloseDown();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                //切换到2D画面
                is3d = false;
                SetCameraAccordingTo23DState(is3d);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                //切换到3D左右画面
                is3d = true;
                SetCameraAccordingTo23DState(is3d);
            }

            ///切换投影方式
            ///调用StartView_LR(...)后，可切换到只显示左画面到投屏窗口或左右一起显示 
            /// -----------      ----------
            /// |  L |  R |  or  |    L   | 
            /// -----------      ---------- 
            /// 如果只调用StartView(...)，则此函数无效 
            if (Input.GetKeyDown(KeyCode.K))
            {
                FARSingleton.GetInstance().SwitchProjector(FARSingleton.ProjectorType._2D);
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                FARSingleton.GetInstance().SwitchProjector(FARSingleton.ProjectorType.LeftRight);
            }
        }

        private Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        private void OnGUI()
        {
            // 将targetTexture绘制到主窗口
            if (is3d)
            {
                if (Camera3Dleft.targetTexture != null)
                {
                    GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Camera3Dleft.targetTexture);
                }
                if (Camera3Dright.targetTexture != null)
                {
                    GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Camera3Dright.targetTexture);
                }
            }
            else
            {
                if (Camera2D.targetTexture != null)
                {
                    GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Camera2D.targetTexture);
                }
            }
        }

        const uint SWP_SHOWWINDOW = 0x0040;//全屏
        const int GWL_STYLE = -16;//无边框
        const int WS_POPUP = 0x800000;
        ///通过f-ar接口读取屏幕信息后设置窗口位置
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
                    //获取一个非GC显示器
                    if (!gcinfo.isGCmonitor)
                    {
                        UnityEngine.Debug.Log("投屏显示器：" + gcinfo.DeviceName);  
                        if (IntPtr.Size.Equals(8))                  
                            SetWindowLongPtrA(ProjectionWindow, GWL_STYLE, WS_POPUP);             
                        else
                            SetWindowLong(ProjectionWindow, GWL_STYLE, WS_POPUP);
                        MoveWindow(ProjectionWindow, gcinfo.RCleft, gcinfo.RCtop, gcinfo.RCright - gcinfo.RCleft, gcinfo.RCbottom - gcinfo.RCtop, true);
                        break;
                    }
                }
                else
                    throw new Exception("fmARGetMonitorInfoByIndex failed with error :" + result);
            }
        }
        ///启动投屏窗口进程
        private IEnumerator OpenFARWindows()
        {
            //为了与主渲染进程不产生冲突，等待下一帧结束
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
                        FARSingleton.GetInstance().StartView_LR(_hViewClient, Camera3Dleft.targetTexture.GetNativeTexturePtr(), Camera3Dright.targetTexture.GetNativeTexturePtr());
                        break;
                    }
                }
            }
            UnityEngine.Debug.Log("FAR.OpenFARWindows():开始绘图！");
        }

        private void OnApplicationQuit()
        {
            //安全关闭投屏窗口，可根据需求调用
            FARSingleton.GetInstance().CloseDown();
        }

        public void SetCameraAccordingTo23DState(bool is3D)
        {
            Camera2D.gameObject.SetActive(!is3D);
            Camera3Dleft.gameObject.SetActive(is3D);
            Camera3Dright.gameObject.SetActive(is3D);

            //当摄像机更新，只需要再调用一次投屏方法，不需要重新创建投屏窗口
            if (Camera2D.gameObject.activeSelf)
            {
                FARSingleton.GetInstance().StartView(_hViewClient, Camera2D.targetTexture.GetNativeTexturePtr());
            }
            else
            {
                FARSingleton.GetInstance().StartView_LR(_hViewClient, Camera3Dleft.targetTexture.GetNativeTexturePtr(), Camera3Dright.targetTexture.GetNativeTexturePtr());
            }
        }
    }
}