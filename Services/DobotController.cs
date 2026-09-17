using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionProSortPlatform.Services
{
    internal class DobotController
    {
        // 连接参数：FrmSystemSetting 保存时写入，连接时直接使用
        public static string Port = "COM1";
        public static int BaudRate = 115200;

        /// <summary>当前是否已连接</summary>
        public static bool IsConnected { get; private set; }

        /// <summary>连接机械臂</summary>
        public static bool Connect()
        {
            return Connect(Port, BaudRate);
        }

        /// <summary>连接机械臂</summary>
        public static bool Connect(string port, int baud)
        {
            StringBuilder fwType = new StringBuilder(128);
            StringBuilder version = new StringBuilder(128);

            int ret = DobotDll.ConnectDobot(port, baud, fwType, version);  

            IsConnected = (ret == 0);
            if (IsConnected)
            {
                DobotDll.SetQueuedCmdForceStopExec();   // ① 强制停止：打断可能还在跑的命令
                DobotDll.SetQueuedCmdClear();           // ② 清空队列：清掉积压的旧命令
                DobotDll.SetQueuedCmdStartExec();       // ③ 恢复运行：保证队列一定在跑
                SetJogSpeed();                          // ④ 设置点动速度                    
            }
            return IsConnected;
        }

        /// <summary>断开机械臂</summary>
        public static void Disconnect()
        {
            DobotDll.DisconnectDobot();
            IsConnected = false;
        }

        /// <summary>回零：机械臂回到原点位置</summary>
        public static bool Home()
        {
            if (!IsConnected) return false;
            HOMECmd cmd = default(HOMECmd);
            ulong queuedCmdIndex = 0;
            int ret = DobotDll.SetHOMECmd(ref cmd, true, ref queuedCmdIndex);

            return (ret == 0);
        }

        /// <summary>设置 JOG 点动速度（连接成功后调用一次）</summary>
        public static void SetJogSpeed()
        {
            if (!IsConnected) return;

            JOGCoordinateParams p = new JOGCoordinateParams();
            p.velocity = new float[4] { 50f, 50f, 50f, 10f };   // XYZ 速度 30，R 轴 10（mm/s）
            p.acceleration = new float[4] { 50f, 50f, 50f, 10f };   // 加速度同理

            ulong idx = 0;
            DobotDll.SetJOGCoordinateParams(ref p, true, ref idx);
        }

        /// <summary>JOG 点动：按住时沿指定方向移动</summary>
        public static void JogStart(string direction)
        {
            if (!IsConnected) return;

            byte cmd;
            switch (direction)     // 坐标模式下，XYZ 对应 A/B/C 轴
            {
                case "X+": cmd = (byte)JogCmdType.JogAPPressed; break;
                case "X-": cmd = (byte)JogCmdType.JogANPressed; break;
                case "Y+": cmd = (byte)JogCmdType.JogBPPressed; break;
                case "Y-": cmd = (byte)JogCmdType.JogBNPressed; break;
                case "Z+": cmd = (byte)JogCmdType.JogCPPressed; break;
                case "Z-": cmd = (byte)JogCmdType.JogCNPressed; break;
                default: cmd = (byte)JogCmdType.JogIdle; break;
            }

            JogCmd jog = new JogCmd();
            jog.isJoint = 0;          // 0 = 坐标模式（X/Y/Z），1 = 关节模式
            jog.cmd = cmd;

            ulong idx = 0;
            DobotDll.SetJOGCmd(ref jog, true, ref idx);
        }

        /// <summary>JOG 停止（松开按键时调用）</summary>
        public static void JogStop()
        {
            if (!IsConnected) return;

            JogCmd jog = new JogCmd();
            jog.isJoint = 0;
            jog.cmd = (byte)JogCmdType.JogIdle;    // 0 = 停止

            ulong idx = 0;
            DobotDll.SetJOGCmd(ref jog, true, ref idx);
        }

        /// <summary>读取机械臂当前坐标。返回 true 表示读取成功</summary>
        public static bool GetPosition(out float x, out float y, out float z)
        {
            x = y = z = 0;
            if (!IsConnected) return false;

            Pose pose = new Pose();
            int ret = DobotDll.GetPose(ref pose);     // 一条查询命令，立即返回当前坐标
            if (ret != 0) return false;

            x = pose.x;
            y = pose.y;
            z = pose.z;
            return true;
        }

        /// <summary>定点运动：机械臂移动到指定坐标（直线运动）</summary>
        public static bool MoveTo(float x, float y, float z)
        {
            if (!IsConnected) return false;

            PTPCmd cmd = new PTPCmd();
            cmd.ptpMode = (byte)PTPMode.PTPMOVLXYZMode;   // 直线运动到 XYZ
            cmd.x = x;
            cmd.y = y;
            cmd.z = z;
            cmd.rHead = 0;                                // 末端不旋转

            ulong idx = 0;
            int ret = DobotDll.SetPTPCmd(ref cmd, true, ref idx);
            return (ret == 0);
        }

        /// <summary>吸盘操作：true=吸，false=放</summary>
        public static bool SetSuction(bool on)
        {
            if (!IsConnected) return false;

            ulong idx = 0;
            int ret = DobotDll.SetEndEffectorSuctionCup(true, on, true, ref idx);
            return (ret == 0);
        }

        /// <summary>启动队列执行（批量入队后调用）</summary>
        public static void StartExec()
        {
            if (!IsConnected) return;
            DobotDll.SetQueuedCmdStartExec();
        }

        /// <summary>暂停队列执行</summary>
        public static void StopExec()
        {
            if (!IsConnected) return;
            DobotDll.SetQueuedCmdStopExec();
        }

        /// <summary>消除警报：清除机械臂全部警报状态</summary>
        public static bool ClearAlarm()
        {
            if (!IsConnected) return false;
            return (DobotDll.ClearAllAlarmsState() == 0);
        }

        /// <summary>清空机械臂内部队列：先强制停止，再清空，保证与表格一致</summary>
        public static void ClearQueue()
        {
            if (!IsConnected) return;
            DobotDll.SetQueuedCmdForceStopExec();   // ① 先停止正在执行的命令
            DobotDll.SetQueuedCmdClear();           // ② 再清空内部队列
        }

    }
}
