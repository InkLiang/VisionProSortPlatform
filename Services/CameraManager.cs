using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VisionPro;

namespace VisionProSortPlatform.Services
{
    internal class CameraManager
    {
        // ===== 连接参数（FrmSystemSetting 保存配置时写入） =====
        public static string VideoFormat { get; set; } = "Generic GigEVision (Mono)";
        public static double Exposure { get; set; } = 5;      // 曝光度（毫秒，浮点）
        public static double Brightness { get; set; } = 0.5;  // 亮度
        public static double Contrast { get; set; } = 0;      // 对比度
        public static int DelayLevel { get; set; } = 3;  // 延迟级别


        // ===== 运行状态 =====
        public static ICogFrameGrabber Grabber { get; private set; }  // 当前相机
        public static ICogAcqFifo Acq { get; private set; }           // 采集Fifo
        public static bool IsConnected => Acq != null;
        public static string LastError { get; private set; } = "";

        /// <summary>建立相机连接（取所有相机列表中的第一台，参照示例 Grabbers[0]）</summary>
        public static bool Connect()
        {
            try
            {
                if (IsConnected) return true;


                CogFrameGrabbers grabbers = new CogFrameGrabbers();
                if (grabbers.Count == 0)
                {
                    LastError = "未找到相机，请检查相机连接、驱动和网络配置";
                    return false;
                }
               
                Grabber = grabbers[0]; // 取第一台相机

                // 创建采集Fifo：参数1: 视频格式，参数2: 像素格式，参数3: 相机端口给0，参数4: 是否自动准备好给true
                Acq = Grabber.CreateAcqFifo(VideoFormat, CogAcqFifoPixelFormatConstants.Format8Grey, 0, true);

                // 给相机设置属性
                Acq.OwnedExposureParams.Exposure = Exposure;      // 曝光度
                Acq.OwnedBrightnessParams.Brightness = Brightness; // 亮度
                Acq.OwnedContrastParams.Contrast = Contrast;       // 对比度
                Acq.OwnedGigEVisionTransportParams.LatencyLevel = DelayLevel; //延迟级别

                LastError = "";
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Disconnect();
                return false;
            }
        }

        /// <summary>断开相机连接</summary>
        public static void Disconnect()
        {
            try
            {
                if (Acq != null)
                {
                    Acq.Flush();  // 停止采集并清空Fifo
                    Acq = null;
                }
            }
            catch { }
        }

        /// <summary>开始采集</summary>
        public static void StartCapture()
        {
            if (IsConnected) Acq.StartAcquire();
        }

        /// <summary>停止采集并清空Fifo</summary>
        public static void StopCapture()
        {
            if (IsConnected) Acq.Flush();
        }


    }
}
