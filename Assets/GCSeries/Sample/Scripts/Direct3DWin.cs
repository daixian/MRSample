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
        /// <summary>
        /// 投屏到一张纹理或两张纹理
        /// </summary>
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

        /// <summary>
        /// 投屏状态值
        /// </summary>
        public enum FAR_Status
        {
            /// <summary>
            /// 成功
            /// </summary>
            FAR_Ok = 1,
            /// <summary>
            /// 没被初始化或正在初始化中
            /// </summary>
            FAR_NotInitialized = 0,
            /// <summary>
            /// 非法硬件设备
            /// </summary>
            FAR_Illegal = -1,
            /// <summary>
            /// 窗口句柄丢失
            /// </summary>
            FAR_InvaliHwndHandle = -2,
            /// <summary>
            /// 渲染设备初始化失败
            /// </summary>
            FAR_D3DFailed = -3,
            /// <summary>
            /// 纹理句柄丢失
            /// </summary>
            FAR_InvaliTexturedHandle = -4,
            /// <summary>
            /// 渲染等待进程超时
            /// </summary>
            FAR_Timeout = -5,
            /// <summary>
            /// windows版本低于win10
            /// </summary>
            FAR_SysNnotSupported = -6
        }
        /// <summary>
        /// 投屏状态值
        /// </summary>
        FAR_Status status = FAR_Status.FAR_NotInitialized;
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
                FARStartRenderingView(WorkMode._DoubleTexture);
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
            if (Input.GetKeyDown(KeyCode.K))
            {
                FARDll.SwitchProjector(FARDll.ProjectorType._2D);
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                FARDll.SwitchProjector(FARDll.ProjectorType.LeftRight);
            }
        }
        ///<summary>
        ///通过f-ar接口读取屏幕信息后设置窗口位置
        ///</summary>
        ///<param name="farwin">投屏窗口句柄</param>
        public void UpdateWindowPos(IntPtr farwin)
        {
            //可先切换到扩展模式
            //SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, (SDC_APPLY | SDC_TOPOLOGY_EXTEND));
            //更新物理显示器列表
            int temp_MonitorCount = FARDll.fmARGetMonitorCount();
            if (temp_MonitorCount < 0)
                UnityEngine.Debug.LogError("Direct3DWin.UpdateWindowsPos() failed with error : fmARGetMonitorCount " + temp_MonitorCount);

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
                            FARDll.MoveWindow(farwin, gcinfo.RCleft, gcinfo.RCtop, gcinfo.RCright - gcinfo.RCleft, gcinfo.RCbottom - gcinfo.RCtop, true);
                            return;
                        }
                    }
                    else
                    {
                        //获取一个非GC显示器
                        if (!gcinfo.isGCmonitor)
                        {
                            UnityEngine.Debug.Log("Direct3DWin.UpdateWindows():投屏显示器：" + gcinfo.DeviceName);
                            FARDll.MoveWindow(farwin, gcinfo.RCleft, gcinfo.RCtop, gcinfo.RCright - gcinfo.RCleft, gcinfo.RCbottom - gcinfo.RCtop, true);
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
            int temp_MonitorCount = FARDll.fmARUpdatePhysicalMonitor();
            if(temp_MonitorCount < 2)
            {
                UnityEngine.Debug.LogError("Direct3DWin.CreateFARWindow() failed with error : Monitor count less than 2");
            }
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
                    UnityEngine.Debug.Log("FAR.CreateFARWindow():找到了窗口句柄！");
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
                                status = (FAR_Status)FARDll.StartView(_hViewClient, renderTexture.GetNativeTexturePtr(),IntPtr.Zero);
                                break;
                            case WorkMode._DoubleTexture:
                                status = (FAR_Status)FARDll.StartView(_hViewClient, renderTextureL.GetNativeTexturePtr(), renderTextureR.GetNativeTexturePtr());
                                break;
                            default:
                                status = (FAR_Status)FARDll.StartView(_hViewClient, renderTexture.GetNativeTexturePtr(), IntPtr.Zero);
                                break;
                        }
                    }
                    break;
                }
            }
            if (status < 0)
            {
                UnityEngine.Debug.LogError("FAR.CreateFARWindow():投屏启动失败" + status);
                FARDll.CloseDown();
            }
            else
                UnityEngine.Debug.Log("FAR.CreateFARWindow():开始绘图！");
        }

        private void OnApplicationQuit()
        {
            //安全关闭投屏窗口，可根据需求调用
            FARDll.CloseDown();
        }

        /// <summary>
        /// 启动FAR投屏窗口
        /// </summary>
        /// <param name="workmMode">投屏单张纹理或两张纹理</param>
        public void FARStartRenderingView(WorkMode workmMode)
        {
            StartCoroutine(CreateFARWindow(workmMode));
        }
    }
}