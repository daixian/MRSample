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
    public class Direct3DWin : MonoBehaviour
    {
        public enum WorkMode
        {
            _2D,
            _3Dleftright
        }

        /// <summary>
        /// 渲染相机
        /// </summary>
        public RenderTexture renderTexture;
        public RenderTexture renderTextureL;
        public RenderTexture renderTextureR;

        /// <summary>
        /// 当前投屏摄像机
        /// </summary>
        public WorkMode WorkingMode = WorkMode._2D;
        private WorkMode _lastWorkingType;

        /// <summary>
        /// 投屏目标显示器名
        /// 如果为空则寻找第一个非GC显示器
        /// </summary>
        public string TargetDisplayName;

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
            _lastWorkingType = WorkingMode;
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
                FARDll.CloseDown();
            }
            if (_lastWorkingType != WorkingMode)
            {
                //切换到2D画面
                SetCameraAccordingTo23DState(WorkingMode);
                _lastWorkingType = WorkingMode;
            }

            ///切换投影方式
            ///调用StartView_LR(...)后，可切换到只显示左画面到投屏窗口或左右一起显示 
            /// -----------      ----------
            /// |  L |  R |  or  |    L   | 
            /// -----------      ---------- 
            /// 如果只调用StartView(...)，则此函数无效 
            if (Input.GetKeyDown(KeyCode.K))
            {
                FARDll.SwitchProjector(FARDll.ProjectorType._2D);
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                FARDll.SwitchProjector(FARDll.ProjectorType.LeftRight);
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
            int temp_MonitorCount = FARDll.fmARUpdatePhysicalMonitor();
            if (temp_MonitorCount < 0)
                UnityEngine.Debug.LogError("Direct3DWin.UpdateWindowsPos() failed with error : fmARUpdatePhysicalMonitor " + temp_MonitorCount);

            if (temp_MonitorCount != 2)
                UnityEngine.Debug.LogError("Direct3DWin.UpdateWindows() PosCurrent monitor count : " + temp_MonitorCount);

            for (int k = 0; k < temp_MonitorCount; k++)
            {
                FARDll.GCinfo gcinfo = new FARDll.GCinfo();
                int result = FARDll.fmARGetMonitorInfoByIndex(ref gcinfo, k);
                if (result >= 0)
                {
                    if (TargetDisplayName != "")
                    {
                        if (gcinfo.DeviceName.Contains(TargetDisplayName))
                        {
                            UnityEngine.Debug.Log("Direct3DWin.UpdateWindows():投屏显示器：" + gcinfo.DeviceName);
                            FARDll.MoveWindow(ProjectionWindow, gcinfo.RCleft, gcinfo.RCtop, gcinfo.RCright - gcinfo.RCleft, gcinfo.RCbottom - gcinfo.RCtop, true);
                            return;
                        }
                    }
                    else
                    {
                        //获取一个非GC显示器
                        if (!gcinfo.isGCmonitor)
                        {
                            UnityEngine.Debug.Log("Direct3DWin.UpdateWindows():投屏显示器：" + gcinfo.DeviceName);
                            FARDll.MoveWindow(ProjectionWindow, gcinfo.RCleft, gcinfo.RCtop, gcinfo.RCright - gcinfo.RCleft, gcinfo.RCbottom - gcinfo.RCtop, true);
                            return;
                        }
                    }
                }
                else
                    UnityEngine.Debug.LogError("Direct3DWin.UpdateWindows() failed with error : fmARGetMonitorInfoByIndex" + result);
            }
            UnityEngine.Debug.LogError("Direct3DWin.UpdateWindows() failed with error : 没有寻找到对应的显示器");
        }
        ///启动投屏窗口进程
        private IEnumerator OpenFARWindows()
        {
            //为了与主渲染进程不产生冲突，等待下一帧结束
            yield return new WaitForEndOfFrame();
            SetCameraAccordingTo23DState(WorkingMode);
            if (FARDll.FindWindow(null, "ClientWinCpp") == IntPtr.Zero)
            {
                string _path = Path.Combine(Application.streamingAssetsPath, "ClientWin.exe");
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = _path;
                viewProcess = new Process();
                viewProcess.StartInfo = startInfo;
                viewProcess.Start();

                _hViewClient = IntPtr.Zero;
            }

            while (true)
            {
                _hViewClient = FARDll.FindWindow(null, "ClientWinCpp");
                if (_hViewClient != IntPtr.Zero)
                {
                    UnityEngine.Debug.Log("FAR.OpenFARWindows():找到了窗口句柄！");
                    //全屏到非GC显示器
                    UpdateWindowPos(_hViewClient);
                    int pid = 0;
                    FARDll.GetWindowThreadProcessId(_hViewClient, out pid);
                    if (pid == viewProcess.Id)
                    {
                        //设置当前的色彩空间，u3d默认是Gama空间
                        FARDll.SetColorSpace(FARDll.U3DColorSpace.Gama);
                        //开始绘制同屏窗口，如目标纹理指针变更可随时调用
                        if (WorkingMode == WorkMode._2D)
                            FARDll.StartView(_hViewClient, renderTexture.GetNativeTexturePtr(), IntPtr.Zero);
                        else
                            FARDll.StartView(_hViewClient, renderTextureL.GetNativeTexturePtr(), renderTextureR.GetNativeTexturePtr());
                        break;
                    }
                }
            }
            UnityEngine.Debug.Log("FAR.OpenFARWindows():开始绘图！");
        }

        private void OnApplicationQuit()
        {
            //安全关闭投屏窗口，可根据需求调用
            FARDll.CloseDown();
        }

        public void SetCameraAccordingTo23DState(WorkMode type)
        {
            bool is3D = (type == WorkMode._2D) ? false : true;
          
            //当摄像机更新，只需要再调用一次投屏方法，不需要重新创建投屏窗口
            if (!is3D)
            {
                FARDll.StartView(_hViewClient, renderTexture.GetNativeTexturePtr(), IntPtr.Zero);
            }
            else
            {
                FARDll.StartView(_hViewClient, renderTextureL.GetNativeTexturePtr(), renderTextureR.GetNativeTexturePtr());
            }
        }
    }
}