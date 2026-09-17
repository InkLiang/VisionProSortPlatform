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
    public partial class FrmVisionConfig : UserControl
    {
        // 是否单次拍照（拍完自动停止）
        private bool isSingleShot = false;

        // 由 Form1 注入，指向主运行页的 cogRecordDisplay1，实现跨页预览
        public CogRecordDisplay TargetDisplay { get; private set; }
        public void SetTargetDisplay(CogRecordDisplay display)
        {
            TargetDisplay = display;
        }
        public FrmVisionConfig()
        {
            InitializeComponent();
            Button_Init(this);

            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
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
           
            Button clicked = sender as Button;
            switch (clicked.Name)
            {
                case "button1":  ConnectCamera();       break; // 连接相机
                case "button2":  DisconnectCamera();    break; // 断开相机
                case "button3":  StartPreview();        break; // 实时预览
                case "button4":  StopPreview();         break; // 停止预览
                case "button5":  SingleShot();          break; // 单次拍照
                case "button9":  LoadVpp();             break; // 加载VPP方案
                case "button10": SaveVpp();             break; // 保存方案
                case "button6":  CaptureTrainImage();   break; // 采集训练图
                case "button7":  TrainTemplate();       break; // 训练模板
                case "button8":  SaveTemplate();        break; // 保存模板
                case "button11": SelectSavePath();      break; // 选择路径
            }
        }

        // ============ 相机 ============
        private void ConnectCamera()
        {
            if (CameraManager.Connect())
            {
                button1.Enabled = false;
                button2.Enabled = true;
                button3.Enabled = true;
                button5.Enabled = true;
                MessageBox.Show(CameraManager.Connect().ToString());
                MessageBox.Show("相机连接成功：" + CameraManager.Grabber.Name);
            }
            else
            {
                MessageBox.Show("相机连接失败：" + CameraManager.LastError, "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            
        }

        private void DisconnectCamera()
        {
            CameraManager.Disconnect();
            button1.Enabled = true;
            button2.Enabled = false;
            MessageBox.Show("相机已断开");
        }

        private void StartPreview()
        {
            if (!CameraManager.IsConnected)
            {
                MessageBox.Show("请先连接相机");
                return;
            }
            // 优先送到主运行页的 cogRecordDisplay1；未注入时回退到本页 cogRecordDisplay2
            var display = TargetDisplay ?? cogRecordDisplay2;
            display.StartLiveDisplay(CameraManager.Acq, false);
            button3.Enabled = false; 
            button4.Enabled = true;  


        }

        private void StopPreview()
        {
            (TargetDisplay ?? cogRecordDisplay2).StopLiveDisplay();
            button3.Enabled = true;  
            button4.Enabled = false; 
        }

        private void SingleShot()
        {
            if (!CameraManager.IsConnected)
            {
                MessageBox.Show("请先连接相机");
                return;
            }
            StopPreview(); // 若正在预览，先停止

            isSingleShot = true;                            // 拍一帧后自动停止
            CameraManager.Acq.Complete -= Acq_Complete;     // 防重复绑定
            CameraManager.Acq.Complete += Acq_Complete;     // 绑定采集完成事件
            CameraManager.StartCapture();
        }
        /// <summary>采集完成回调（参照PDF例1）：取图并显示</summary>
        private void Acq_Complete(object sender, CogCompleteEventArgs e)
        {
            ICogAcqFifo acq = sender as ICogAcqFifo;
            acq.GetFifoState(out int _, out int ReadyNum, out bool _);
            if (ReadyNum > 0)
            {
                // 获取图像
                ICogImage Img = acq.CompleteAcquireEx(new CogAcqInfo());
             
                cogRecordDisplay2.Image = Img;
            }

            if (isSingleShot) // 单次拍照：取完一帧自动停止
            {
                isSingleShot = false;
                acq.Complete -= Acq_Complete;
                CameraManager.StopCapture(); // Acq.Flush
            }
        }


        // ============ VPP方案 ============

        private void LoadVpp()
        {
            using (OpenFileDialog OFD = new OpenFileDialog())
            {
                OFD.RestoreDirectory = false;
                OFD.Filter = "VPP方案文件|*.vpp|所有文件|*.*";
                OFD.InitialDirectory = Application.StartupPath;
                if (OFD.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = OFD.FileName;
                }
                else
                {
                    MessageBox.Show("未读取到文件");
                }
            }
        }

        private void SaveVpp()
        {
            // TODO: 保存方案
        }

        // ============ PMA模板训练 ============

        private void CaptureTrainImage()
        {
            // TODO: 采集训练图
        }

        private void TrainTemplate()
        {
            // TODO: 训练模板
        }

        private void SaveTemplate()
        {
            // TODO: 保存模板
        }

        // ============ 图像保存配置 ============

        private void SelectSavePath()
        {
            // TODO: 选择路径
        }
    }
}
