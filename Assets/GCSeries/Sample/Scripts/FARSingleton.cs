using System;
using System.Runtime.InteropServices;
namespace FSpace
{
    public class FARSingleton//为了可切换场景，封装一个全局类
    {
        [DllImport("f-ar")]
        private static extern int fmARStartViewDX11(System.IntPtr hWnd, System.IntPtr textureHandle, int w, int h);
        [DllImport("f-ar")]
        private static extern int fmARStartView_LRDX11(System.IntPtr hWnd, System.IntPtr LeftTextureHandle, System.IntPtr ReftTextureHandle, int w, int h);
        [DllImport("f-ar")]
        private static extern void fmARSwitchProjector(int type);
        [DllImport("f-ar")]
        private static extern void fmARIsGamaSpace(int cSpace);
        [DllImport("f-ar")]
        private static extern void fmARStopView();

        [StructLayout(LayoutKind.Sequential)]
        public struct GCinfo
        {
            public bool isGCmonitor;
            public int RCleft;
            public int RCright;
            public int RCtop;
            public int RCbottom;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
            public string DeviceName;
        };
        [DllImport("f-ar")]
        public static extern int fmARUpdatePhysicalMonitor();
        [DllImport("f-ar")]
        public static extern int fmARGetMonitorCount();
        [DllImport("f-ar")]
        public static extern int fmARGetMonitorInfoByIndex(ref GCinfo out_struct, int index);


        [DllImport("f-ar")]
        public static extern int fmFViewReadJson();
        [DllImport("f-ar")]
        public static extern void fmFViewGetPosition(IntPtr value);
        [DllImport("f-ar")]
        public static extern void fmFViewGetRotation(IntPtr value);

        public static FARSingleton GetInstance()
        {
            if (m_fviewInstance == null) m_fviewInstance = new FARSingleton();
            return m_fviewInstance;
        }
        public enum U3DColorSpace
        {
            Gama = 0,
            Linear = 1
        }
        public enum ProjectorType
        {
            _2D = 0,//2D投影
            LeftRight = 1//左右投影
        }
        /// <summary>
        /// 输入 
        /// hWnd                 创建好的窗口句柄
        /// FullNativePTR    单个全屏的纹理指针
        /// 返回错误代码
        /// errorCode >0 成功
        /// errorCode ==-2 窗口句柄丢失
        /// errorCode ==-3 纹理句柄丢失
        /// </summary>
        public int StartView(IntPtr hWnd, IntPtr FullNativePTR)
        {
            errorCode = fmARStartViewDX11(hWnd, FullNativePTR, SwapchainWidth, SwapchanHeight);
            return errorCode;
        }

        /// <summary>
        /// 输入 
        /// hWnd                  创建好的窗口句柄
        /// leftNativePTR      半屏的左眼纹理指针
        ///  rightNativePTR    半屏的右眼纹理指针
        /// 返回错误代码
        /// errorCode >0 成功
        /// errorCode ==-2 窗口句柄丢失
        /// errorCode ==-3 纹理句柄丢失
        /// </summary>
        public int StartView_LR(IntPtr hWnd, IntPtr leftNativePTR, IntPtr rightNativePTR)
        {
            errorCode = fmARStartView_LRDX11(hWnd, leftNativePTR, rightNativePTR, SwapchainWidth, SwapchanHeight);
            return errorCode;
        }

        public void SetColorSpace(U3DColorSpace cSpace)
        {
#if UNITY_EDITOR
            if (UnityEditor.PlayerSettings.colorSpace == UnityEngine.ColorSpace.Linear)
            {
                fmARIsGamaSpace((int)U3DColorSpace.Linear);
            }
            else
                fmARIsGamaSpace((int)U3DColorSpace.Gama);
#else
            fmARIsGamaSpace((int)cSpace);
#endif
        }

        /// <summary>
        ///切换投影方式
        ///调用StartView_LR(...)后，可切换到只显示左画面到投屏窗口或左右一起显示 
        /// -----------         ----------
        /// |   L  |   R  |  or   |     L     | 
        /// -----------         ---------- 
        ///如果只调用StartView(...)，则此函数无效 
        /// </summary>
        public void SwitchProjector(ProjectorType type)
        {
            fmARSwitchProjector((int)type);
        }

        /// <summary>
        ///安全关闭窗口
        /// </summary>
        public void CloseDown()
        {
            fmARStopView();
        }

       

        public const int SwapchainWidth = 1920;
        public const int SwapchanHeight = 1080;
        private int errorCode = 0;
        private FARSingleton() { }
        ~FARSingleton() { fmARStopView(); }
        private static FARSingleton m_fviewInstance;
    }
}