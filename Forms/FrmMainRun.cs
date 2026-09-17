using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VisionProSortPlatform.Services;

namespace VisionProSortPlatform.Forms
{
    public partial class FrmMainRun : UserControl
    {
        /// <summary>静态单例，方便其他类直接访问主运行页实例</summary>
        public static FrmMainRun Instance { get; private set; }

        /// <summary>暴露 cogRecordDisplay1，供视觉配置页跨页预览时写入图像</summary>
        public CogRecordDisplay CogRecordDisplay => cogRecordDisplay1;

        //合格数量
        private int GoodCount = 0;

        //瑕疵数量
        private int BadCount = 0;
        public FrmMainRun()
        {
            InitializeComponent();
            Instance = this;
            Button_Init(this);
            SetRunningState(false);
        }

        //按键初始化
        private void Button_Init(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Button btn)
                {
                    btn.Click += button_Click;
                }
                else
                    Button_Init(c);
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            string name = ((Button)sender).Text;

            switch (name)
            {
                // ---- 任务：开始/停止 ----
                case "开始检测": StartInspect();         break;
                case "停止检测": StopInspect();          break;

                // ---- 任务模式：单次/连续 ----
                case "单次运行": RunSingleInspect();     break;
                case "连续运行": RunContinuousInspect(); break;

            }
        }
        /// <summary>切换检测状态：running=true检测中 / false停止</summary>
        private void SetRunningState(bool running)
        {
            button1.Enabled = !running;          // 开始检测
            button2.Enabled = running;           // 停止检测
            button3.Enabled = running;           // 单次运行
            button4.Enabled = running;           // 连续运行

            if (!running)                        // 停止时：恢复按钮默认颜色
            {
                button3.BackColor = SystemColors.Control;
                button4.BackColor = SystemColors.Control;
            }
        }

        /// <summary>开始检测</summary>
        private void StartInspect()
        {
            if (!CameraManager.IsConnected)
            {
                MessageBox.Show("请先连接相机");
                return;
            }
            SetRunningState(true);

            CameraManager.ResetTriggerCount();   // ← 加：新一轮计数归零
            label16.Text = "0";
        }

        /// <summary>停止检测</summary>
        private void StopInspect()
        {
            CameraManager.StopContinuous();   // 若连续采集中，先停
            SetRunningState(false);
        }

        /// <summary>单次运行检测</summary>
        private void RunSingleInspect()
        {

            if (!CameraManager.IsConnected)
            {
                MessageBox.Show("请先连接相机");
                return;
            }
            if (CameraManager.CTB == null)
            {
                MessageBox.Show("请先在视觉配置页加载VPP方案");
                return;
            }
  
            button3.BackColor = Color.SkyBlue;            // ← 加：高亮「单次运行」
            button4.BackColor = SystemColors.Control;     // ← 加：取消「连续运行」高亮


            cogRecordDisplay1.StopLiveDisplay(); // 若正在预览，先停止

            // 单帧采集：拍完一帧自动停，取到的图显示到预览区
            CameraManager.CaptureOnce(img =>
            {
                if (img != null)
                {
                    label16.Text = CameraManager.TriggerCount.ToString();
                    cogRecordDisplay1.Image = img;   // 单次拍照结果显示
                }
                   
                try
                {
                    object count = CameraManager.RunVpp(img);
                    if ((int)count == 0) BadCount += 1;

                    GoodCount += (int)count;
                    label17.Text = GoodCount.ToString();
                    label18.Text = BadCount.ToString();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("检测异常：" + ex.Message);
                }
            });
        }

        /// <summary>连续运行检测</summary>
        private void RunContinuousInspect()
        {
            if (!CameraManager.IsConnected)
            {
                MessageBox.Show("请先连接相机");
                return;
            }

            if (CameraManager.CTB == null)
            {
                MessageBox.Show("请先在视觉配置页加载VPP方案");
                return;
            }

            button4.BackColor = Color.SkyBlue;            // ← 加：高亮「连续运行」
            button3.BackColor = SystemColors.Control;     // ← 加：取消「单次运行」高亮


            CameraManager.StartContinuous(img =>
            {
                if (img != null)
                {
                    label16.Text = CameraManager.TriggerCount.ToString();
                    cogRecordDisplay1.Image = img;
                }
                try
                {
                    object count = CameraManager.RunVpp(img);
                    if ((int)count == 0) BadCount += 1;

                    GoodCount += (int)count;
                    label17.Text = GoodCount.ToString();
                    label18.Text = BadCount.ToString();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("检测异常：" + ex.Message);
                }
            });
        }

    }
}
