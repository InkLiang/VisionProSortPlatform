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
        public FrmMainRun()
        {
            InitializeComponent();
            Instance = this;
            Button_Init(this);
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

        /// <summary>开始检测</summary>
        private void StartInspect()
        {
            if (!CameraManager.IsConnected)
            {
                MessageBox.Show("请先连接相机");
                return;
            }
        }

        /// <summary>停止检测</summary>
        private void StopInspect()
        {

        }

        /// <summary>单次运行检测</summary>
        private void RunSingleInspect()
        {

        }

        /// <summary>连续运行检测</summary>
        private void RunContinuousInspect()
        {

        }

    }
}
