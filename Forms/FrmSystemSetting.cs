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
    public partial class FrmSystemSetting : UserControl
    {
        // 是否已成功连接
        private bool isConnected = false;

        public FrmSystemSetting()
        {
            InitializeComponent();
            Button_Init(this);
        }

        //按键初始化
        private void Button_Init(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Button btn)
                    btn.Click += button_Click;
                else
                    Button_Init(c);
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            Button clicked = sender as Button;
            switch (clicked.Name)
            {
                case "Modbus_button_save":   SaveModbusConfig(); break;
                case "JXB_button_save":      SaveRobotConfig();  break;
                case "Vision_button_save":   SaveVisionConfig(); break;
                case "Mysql_button_connect": ConnectMysql();     break;
            }
        }

        /// <summary>保存机械臂配置：串口+波特率 → 全局配置</summary>
        private void SaveRobotConfig()
        {
            if (string.IsNullOrWhiteSpace(comboBox3.Text) ||!int.TryParse(comboBox2.Text, out int baud))
            {
                MessageBox.Show("请先选择正确的串口和波特率");
                return;
            }
            DobotController.Port = comboBox3.Text.Trim();   
            DobotController.BaudRate = baud;
            MessageBox.Show("机械臂配置已保存");
        }

        /// <summary>保存 Modbus 配置</summary>
        private void SaveModbusConfig() { /* TODO */ }

        /// <summary>保存视觉配置：相机连接参数写入 CameraManager</summary>
        private void SaveVisionConfig()
        {
            if (string.IsNullOrWhiteSpace(comboBox6.Text))
            {
                MessageBox.Show("请先选择视频格式");
                return;
            }
            if (!double.TryParse(textBox4.Text.Trim(), out double exposure))
            {
                MessageBox.Show("请输入正确的曝光度");
                return;
            }

            CameraManager.VideoFormat = comboBox6.Text.Trim();      // 视频格式
            CameraManager.Exposure = exposure;                      // 曝光度
            CameraManager.DelayLevel = (int)numericUpDown1.Value; //延迟级别
            CameraManager.Brightness = (double)numericUpDown2.Value; // 亮度
            CameraManager.Contrast = (double)numericUpDown3.Value;   // 对比度
            MessageBox.Show("视觉配置已保存");
        }


        /// <summary>连接 MySQL</summary>
        private async void ConnectMysql()
        {
            // 从输入框读取连接参数（按界面标签一一对应）
            string server = textBox1.Text.Trim();
            string port = textBox2.Text.Trim();
            string database = textBox3.Text.Trim();
            string uid = textBox6.Text.Trim();
            string pwd = textBox5.Text.Trim();
            string charset = comboBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(port) || string.IsNullOrWhiteSpace(database) || string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(pwd))
            {
                MessageBox.Show("请填写所有连接参数！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (await MySQLManager.TestConnectionAsync(server, port, database, uid, pwd, charset))
            {
                MySQLManager.SetConnection(server, port, database, uid, pwd, charset);
                isConnected = true;
                label14.Text = "已连接";
                label14.ForeColor = Color.Green;
            }
            else
            {
                isConnected = false;
                label14.Text = "未连接";
                label14.ForeColor = Color.Red;
                MessageBox.Show("测试连接失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
