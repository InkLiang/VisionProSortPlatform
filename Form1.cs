using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VisionProSortPlatform.Forms;

namespace VisionProSortPlatform
{
    public partial class Form1 : Form
    {
        // 缓存各页面实例，避免每次切换菜单都 new 出新对象，导致相机连接断开
        private readonly FrmMainRun _frmMainRun = new FrmMainRun();
        private readonly UserControl _deviceDebug = new FrmDeviceDebug();
        private readonly FrmVisionConfig _frmVisionConfig = new FrmVisionConfig();
        private readonly UserControl _dataLog = new FrmDataLog();
        private readonly UserControl _systemSetting = new FrmSystemSetting();
        public Form1()
        {
            InitializeComponent();

            // 启动默认显示主运行页
            _frmMainRun.Dock = DockStyle.Fill;
            panel1.Controls.Add(_frmMainRun);

            // 将主运行页的 cogRecordDisplay1 注入视觉配置页，用于跨页预览
            _frmVisionConfig.SetTargetDisplay(_frmMainRun.CogRecordDisplay);

            // 一次性给 menuStrip1 所有菜单项绑同一个事件
            foreach (ToolStripMenuItem item in menuStrip1.Items)
            {
                item.Click += menu_Click;
            }

            this.FormClosed += Form1_FormClosed;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        // 按菜单文字分发，把对应页面加载进 panel1
        private void menu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            panel1.Controls.Clear();                             // 清空内容区

            UserControl uc = null;
            switch (item.Text)
            {
                case "主运行页": uc = _frmMainRun; break;
                case "设备调试": uc = _deviceDebug; break;
                case "视觉配置": uc = _frmVisionConfig; break;
                case "数据日志": uc = _dataLog; break;
                case "系统设置": uc = _systemSetting; break;
            }

            if (uc != null)
            {
                uc.Dock = DockStyle.Fill;
                panel1.Controls.Add(uc);
            }
        }
    }
}
