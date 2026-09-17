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
    public partial class FrmDeviceDebug : UserControl
    {
        public FrmDeviceDebug()
        {
            InitializeComponent();
            Button_Init(this);

            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("No", "序号");
            dataGridView1.Columns.Add("Action", "动作");
            dataGridView1.Columns.Add("Param", "参数");
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        //按键初始化
        private void Button_Init(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Button btn)
                {
                    btn.Click += button_Click;
                    if (IsJogButton(btn.Text))          // JOG 六键：按住动、松开停
                    {
                        btn.MouseDown += btnJog_MouseDown;
                        btn.MouseUp += btnJog_MouseUp;
                        btn.MouseLeave += (s, e) => DobotController.JogStop();   // 拖出也停
                    }
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
                // ---- Modbus：连接/断开 ----
                case "连接Modbus":; break;
                case "断开Modbus":; break;

                // ---- 机械臂：连接/断开 ----
                case "连接机械臂": ConnectRobot();    break;
                case "断开机械臂": DisconnectRobot(); break;

                // ---- Modbus 传送带 ----
                case "正转":
                case "反转":
                case "启动":
                case "停止":
                    MessageBox.Show($"「{name}」功能待实现");
                    break;

                // ---- 机械臂功能 ----
                case "回零":     RobotHome();  break;
                case "消除警报": ClearAlarm(); break;
                case "定点运动": PointMove();  break;
                case "吸盘吸":   SuctionOn();  break;
                case "吸盘放":   SuctionOff(); break;

                // ---- 机械臂队列 ----
                case "添加队列":   AddToQueue();          break;
                case "添加移动点": AddMoveStep();         break;
                case "添加吸盘吸": AddSuctionStep(true);  break;
                case "添加吸盘放": AddSuctionStep(false); break;
                case "删除选中":   DeleteSelected();      break;
                case "清空队列":   ClearQueue();          break;
                case "启动队列":   StartQueue();          break;
                case "暂停队列":   PauseQueue();          break;

            }
        }

        #region 机械臂连接
        private void ConnectRobot()
        {
            bool ok = DobotController.Connect();            
            MessageBox.Show(ok ? "机械臂连接成功" : "连接失败，请先在系统设置页保存正确参数");
            if (ok)
            {
                TimerManager.StopAll();                  
                TimerManager.StartTimer(100, Task_UpdatePose);   // 100ms 刷一次坐标
            }

        }
        #endregion

        #region 机械臂断开
        private void DisconnectRobot()
        {
            DobotController.Disconnect();
            MessageBox.Show("机械臂已断开");
            TimerManager.StopAll();
        }
        #endregion

        #region 机械臂实时坐标任务
        private void Task_UpdatePose()
        {
            float x, y, z;
            if (DobotController.GetPosition(out x, out y, out z))
            {
                textBox6.Text = x.ToString("F1");
                textBox1.Text = y.ToString("F1");
                textBox2.Text = z.ToString("F1");
            }
        }
        #endregion

        #region 机械臂回零
        private void RobotHome() 
        {
            if (DobotController.Home())
                MessageBox.Show("回零命令已发出，机械臂正在回原点");
            else
                MessageBox.Show("回零失败：请先连接机械臂");
        }
        #endregion

        #region 机械臂自由移动
        // 判断是不是 JOG 六键
        private bool IsJogButton(string text)
        {
            return text == "X+" || text == "X-" || text == "Y+" ||
                   text == "Y-" || text == "Z+" || text == "Z-";
        }

        // 按住 → 开始移动
        private void btnJog_MouseDown(object sender, MouseEventArgs e)
        {
            DobotController.JogStart(((Button)sender).Text);
        }

        // 松开 → 停止
        private void btnJog_MouseUp(object sender, MouseEventArgs e)
        {
            DobotController.JogStop();
        }
        #endregion

        #region 机械臂定点运动
        private void PointMove()
        { // 1. 校验输入：三个框都要能转成数字，转不了就提示
            if (!float.TryParse(textBox3.Text, out float x) ||   // 目标X
                !float.TryParse(textBox4.Text, out float y) ||   // 目标Y
                !float.TryParse(textBox5.Text, out float z))     // 目标Z
            {
                MessageBox.Show("请输入有效的目标坐标");
                return;
            }

            // 2. 发送运动命令
            if (DobotController.MoveTo(x, y, z))
                MessageBox.Show("定点运动命令已发出");
            else
                MessageBox.Show("定点运动失败：请先连接机械臂");
        }
        #endregion

        #region 机械臂吸盘
        private void SuctionOn() {
            if (DobotController.SetSuction(true))
                MessageBox.Show("吸盘已吸");
            else
                MessageBox.Show("吸盘操作失败：请先连接机械臂");
        }
        private void SuctionOff() {
            if (DobotController.SetSuction(false))
                MessageBox.Show("吸盘已放");
            else
                MessageBox.Show("吸盘操作失败：请先连接机械臂");
        }
        #endregion

        #region 机械臂消除警报
        private void ClearAlarm() 
        {
            if (!DobotController.IsConnected) { MessageBox.Show("请先连接机械臂"); return; }
            if (DobotController.ClearAlarm())
                MessageBox.Show("警报已清除");
            else
                MessageBox.Show("消除警报失败");
        }
        #endregion

        #region 机械臂队列操作
        // 确认当前队列：弹窗确认后开放「启动队列」，防误操作
        private void AddToQueue() 
        {
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("队列为空，请先添加移动点/吸盘动作");
                return;
            }
            if (MessageBox.Show($"确认将当前 {dataGridView1.Rows.Count} 条动作加入执行队列？",
                                "确认队列", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                button22.Enabled = true;   // 确认后才能点击「启动队列」
            }
        }
        // 实时坐标入队
        private void AddMoveStep() 
        {
            float x, y, z;
            if (!DobotController.GetPosition(out x, out y, out z))
            {
                MessageBox.Show("请先连接机械臂");
                return;
            }
            dataGridView1.Rows.Add(dataGridView1.Rows.Count + 1, "移动", $"{x:F1}, {y:F1}, {z:F1}");
        }
        // 吸盘动作入队
        private void AddSuctionStep(bool on)
        {
            dataGridView1.Rows.Add(dataGridView1.Rows.Count + 1, "吸盘", on ? "吸" : "放");
        }
        // 删除选中行（修改 = 双击单元格）
        private void DeleteSelected()
        {
            if (dataGridView1.CurrentRow != null)
                dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);
        }
        // 启动队列：逐行解析入队 → 统一执行
        private void StartQueue() 
        {
            if (!DobotController.IsConnected)  { MessageBox.Show("请先连接机械臂"); return; }
            if (dataGridView1.Rows.Count == 0) { MessageBox.Show("队列为空"); return; }

            DobotController.ClearQueue();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                string action = Convert.ToString(row.Cells[1].Value);
                string param = Convert.ToString(row.Cells[2].Value);

                if (action == "移动")
                {
                    string[] p = param.Split(',');
                    float x, y, z;
                    if (p.Length < 3 ||
                        !float.TryParse(p[0].Trim(), out x) ||
                        !float.TryParse(p[1].Trim(), out y) ||
                        !float.TryParse(p[2].Trim(), out z))
                        continue;                            // 改坏的行跳过
                    DobotController.MoveTo(x, y, z);         // 入队
                }
                else if (action == "吸盘")
                {
                    DobotController.SetSuction(param == "吸");   // 入队
                }
            }
                DobotController.StartExec();   // 执行全部
                MessageBox.Show("队列已启动");
        }
        // 暂停队列执行
        private void PauseQueue() 
        {
            if (!DobotController.IsConnected) { MessageBox.Show("请先连接机械臂"); return; }
            DobotController.StopExec();
            MessageBox.Show("队列已暂停");
        }
        // 清空队列
        private void ClearQueue() 
        {
            dataGridView1.Rows.Clear();
            DobotController.ClearQueue();           // ← 同步清空机械臂内部队列
            button22.Enabled = false;
        }

        #endregion
    }
}
