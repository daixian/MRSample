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
            _SingleTexture,
            _DoubleTexture
        }

        /// <summary>
        /// 纹理目标
        /// </summary>
        public RenderTexture renderTexture;
        public RenderTexture renderTextureL;
        public RenderTexture renderTextureR;

        /// <summary>
        /// 当前工作模式
        /// </summary>
        public WorkMode workingMode = WorkMode._SingleTexture;
        private WorkMode _lastWorkingType;

        /// <summary>
        /// 投屏目标显示器名
        /// 如果为空字符串则寻找第一个非GC显示器
        /// </summary>
        public string targetMonitorName;

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
            _lastWorkingType = workingMode;
        }

        void Start()
        {
            //启动投屏窗口
            FARStartRenderingView(workingMode);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                //启动投屏窗口
                FARStartRenderingView(WorkMode._SingleTexture);
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                //关闭投屏窗口
                FARDll.CloseDown();
            }
            if (_lastWorkingType != workingMode)
            {
                //切换到2D画面
                FARStartRenderingView(workingMode);
                _lastWorkingType = workingMode;
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
                    if (targetMonitorName != string.Empty)
                    {
                        if (gcinfo.DeviceName.Contains(targetMonitorName))
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
        private IEnumerator CreateFARWindow(WorkMode _workmode)
        {
            //为了与主渲染进程不产生冲突，等待下一帧结束
            yield return new WaitForEndOfFrame();
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
                        switch (_workmode)
                        {
                            case WorkMode._SingleTexture:
                                FARDll.StartView(_hViewClient, renderTexture.GetNativeTexturePtr(), IntPtr.Zero);
                                break;
                            case WorkMode._DoubleTexture:
                                FARDll.StartView(_hViewClient, renderTextureL.GetNativeTexturePtr(), renderTextureR.GetNativeTexturePtr());
                                break;
                            default:
                                FARDll.StartView(_hViewClient, renderTexture.GetNativeTexturePtr(), IntPtr.Zero);
                                break;
                        }
                    }
                    break;
                }
            }
            UnityEngine.Debug.Log("FAR.OpenFARWindows():开始绘图！");
        }

        private void OnApplicationQuit()
        {
            //安全关闭投屏窗口，可根据需求调用
            FARDll.CloseDown();
        }

        /// <summary>
        /// 启动FAR投屏窗口
        /// input WorkMode:投屏单张纹理或两张纹理
        /// </summary>
        public void FARStartRenderingView(WorkMode workmMode)
        {
            StartCoroutine(CreateFARWindow(workmMode));
        }
    }
}