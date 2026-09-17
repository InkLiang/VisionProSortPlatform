using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisionProSortPlatform.Services
{
    internal class TimerManager
    {
        // 所有定时器统一管理，便于批量停止与资源释放
        private static readonly List<Timer> _Timer = new List<Timer>();

        /// <summary>创建并启动一个定时器</summary>
        /// <param name="interval">定时间隔（毫秒）</param>
        /// <param name="onTick">定时触发的回调方法</param>
        /// <returns>创建的定时器实例</returns>
        public static Timer StartTimer(int interval, Action onTick)
        {
            var timer = new Timer();
            timer.Interval = interval;
            timer.Tick += (sender, e) => onTick?.Invoke();
            timer.Start();
            _Timer.Add(timer);
            return timer;
        }

        /// <summary>停止并释放所有定时器</summary>
        public static void StopAll()
        {
            foreach (var t in _Timer)
            {
                t.Stop();
                t.Dispose();
            }
            _Timer.Clear();
        }
    }
}
