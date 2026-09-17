namespace VisionProSortPlatform
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tsmi_MainRun = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_DeviceDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_VisionConfig = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_DataLog = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_SystemSetting = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmi_MainRun,
            this.tsmi_DeviceDebug,
            this.tsmi_VisionConfig,
            this.tsmi_DataLog,
            this.tsmi_SystemSetting});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1378, 36);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tsmi_MainRun
            // 
            this.tsmi_MainRun.Name = "tsmi_MainRun";
            this.tsmi_MainRun.Size = new System.Drawing.Size(98, 32);
            this.tsmi_MainRun.Text = "主运行页";
            // 
            // tsmi_DeviceDebug
            // 
            this.tsmi_DeviceDebug.Name = "tsmi_DeviceDebug";
            this.tsmi_DeviceDebug.Size = new System.Drawing.Size(98, 32);
            this.tsmi_DeviceDebug.Text = "设备调试";
            // 
            // tsmi_VisionConfig
            // 
            this.tsmi_VisionConfig.Name = "tsmi_VisionConfig";
            this.tsmi_VisionConfig.Size = new System.Drawing.Size(98, 32);
            this.tsmi_VisionConfig.Text = "视觉配置";
            // 
            // tsmi_DataLog
            // 
            this.tsmi_DataLog.Name = "tsmi_DataLog";
            this.tsmi_DataLog.Size = new System.Drawing.Size(98, 32);
            this.tsmi_DataLog.Text = "数据日志";
            // 
            // tsmi_SystemSetting
            // 
            this.tsmi_SystemSetting.Name = "tsmi_SystemSetting";
            this.tsmi_SystemSetting.Size = new System.Drawing.Size(98, 32);
            this.tsmi_SystemSetting.Text = "系统设置";
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 36);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1378, 808);
            this.panel1.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1378, 844);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmi_MainRun;
        private System.Windows.Forms.ToolStripMenuItem tsmi_DeviceDebug;
        private System.Windows.Forms.ToolStripMenuItem tsmi_VisionConfig;
        private System.Windows.Forms.ToolStripMenuItem tsmi_DataLog;
        private System.Windows.Forms.ToolStripMenuItem tsmi_SystemSetting;
        private System.Windows.Forms.Panel panel1;
    }
}

