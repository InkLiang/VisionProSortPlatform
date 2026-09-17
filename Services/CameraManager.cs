using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;   


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

        /// <summary>加载好的方案（ToolBlock），页面可直接读输出端子</summary>
        public static CogToolBlock CTB { get; private set; }

        /// <summary>拍照触发计数：每次成功取到一帧+1（单次/连续共用）</summary>
        public static int TriggerCount { get; private set; }

        /// <summary>计数归零</summary>
        public static void ResetTriggerCount() { TriggerCount = 0; }


        /// <summary>建立相机连接（取所有相机列表中的第一台）</summary>
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

        /// <summary>停止采集并清空Fifo</summary>``````````````````````````````````````
        public static void StopCapture()
        {
            if (IsConnected) Acq.Flush();
        }

        /// <summary>加载VPP方案，成功后存入CTB</summary>
        public static bool LoadVpp(string path, out string error)
        {
            error = "";
            try
            {
                CTB = CogSerializer.LoadObjectFromFile(path) as CogToolBlock;   // 直接赋值，类型转换以后再说

                if (CTB == null)
                {
                    error = "VPP加载失败：根对象不是ToolBlock";
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        /// <summary>执行VPP检测：喂图→Run→返回输出端子Count的值</summary>
        public static object RunVpp(ICogImage img)
        {
            if (CTB == null) return null;
            CTB.Inputs["InputImage"].Value = img;
            CTB.Run();
            return CTB.Outputs["Count"].Value;
        }

        #region 单帧采集 

        private static Action<ICogImage> _onImage;   // 暂时存放"拍完图后要执行的代码"

        /// <summary>单帧采集：拍完一帧自动停，把图交给回调</summary>
        public static void CaptureOnce(Action<ICogImage> onImage)
        {
            if (!IsConnected) return;
        
            _onImage = onImage;                     // 1. 先记下"拍完图后要做什么"
            Acq.OwnedTriggerParams.TriggerModel = CogAcqTriggerModelConstants.Manual;
            Acq.Complete -= Acq_Continuous;
            Acq.Complete -= Acq_Complete;           // 2. 防重复绑定（就是你原来的写法）
            Acq.Complete += Acq_Complete;           // 3. 绑定采集完成事件
            Acq.StartAcquire();                     // 4. 开拍
        }

        /// <summary>拍完一帧（命名方法，写法和你原来的Acq_Complete一样）</summary>
        private static void Acq_Complete(object sender, CogCompleteEventArgs e)
        {
            ICogAcqFifo acq = sender as ICogAcqFifo;
            acq.GetFifoState(out int _, out int readyNum, out bool _);
            ICogImage img = null;
            if (readyNum > 0)
            {
                img = acq.CompleteAcquireEx(new CogAcqInfo());
                TriggerCount++;
            }
            Acq.Flush();                            // 拍完一帧自动停
            Acq.Complete -= Acq_Complete;           // 取完解绑（就是你原来的写法）

            var cb = _onImage;                      // 取出"拍完图后要做的代码"
            _onImage = null;
            cb?.Invoke(img);                        // 交给页面
        }
        #endregion

        #region 连续采集 


        /// <summary>连续拍照：每来一个触发信号拍一帧、回调一次（不自动停）</summary>
        public static void StartContinuous(Action<ICogImage> onImage)
        {
            if (!IsConnected) return;
           
            _onImage = onImage;                     // 记下每帧要做什么
            Acq.OwnedTriggerParams.TriggerEnabled = true;  // 开硬件触发
            Acq.OwnedTriggerParams.TriggerModel = CogAcqTriggerModelConstants.Auto;
            Acq.Complete -= Acq_Complete;           // 防止单次模式的回调还挂着
            Acq.Complete -= Acq_Continuous;         // 防重复绑定
            Acq.Complete += Acq_Continuous;
            //Acq.StartAcquire();                     // 开拍：之后每个触发信号出一帧
        }

        /// <summary>连续采集回调：每帧取图交给页面（不解绑、不停）</summary>
        private static void Acq_Continuous(object sender, CogCompleteEventArgs e)
        {
            ICogAcqFifo acq = sender as ICogAcqFifo;
            acq.GetFifoState(out int _, out int readyNum, out bool _);
            ICogImage img = null;
            if (readyNum > 0)
            {
                img = acq.CompleteAcquireEx(new CogAcqInfo());
                TriggerCount++;
            }
            _onImage?.Invoke(img);                  // 每帧都回调，让页面跑VPP
        }

        /// <summary>停止连续采集</summary>
        public static void StopContinuous()
        {
            if (!IsConnected) return;
            Acq.Complete -= Acq_Continuous;         // 解绑
            Acq.Flush();                            // 停采集
            Acq.OwnedTriggerParams.TriggerEnabled = false;   // 恢复软件自由采集
        }

        #endregion
    }
}
