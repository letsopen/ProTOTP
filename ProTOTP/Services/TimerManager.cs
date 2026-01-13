using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using ProTOTP.Models;
using ProTOTP.Services;

namespace ProTOTP.Services
{
    public class TimerManager
    {
        private static TimerManager _instance;
        public static TimerManager Instance => _instance ?? (_instance = new TimerManager());

        private Dictionary<string, DispatcherTimer> _timers;
        private readonly object _lock = new object();

        private TimerManager()
        {
            _timers = new Dictionary<string, DispatcherTimer>();
        }

        public void StartTimer(TOTPAccount account)
        {
            lock (_lock)
            {
                // 如果已有该账户的计时器，则先停止
                if (_timers.ContainsKey(account.Id))
                {
                    StopTimer(account.Id);
                }

                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(500); // 每半秒更新一次进度条
                timer.Tick += (s, e) =>
                {
                    UpdateAccountCode(account);
                };
                timer.Start();

                _timers[account.Id] = timer;

                // 立即更新一次
                UpdateAccountCode(account);
            }
        }

        public void StopTimer(string accountId)
        {
            lock (_lock)
            {
                if (_timers.TryGetValue(accountId, out DispatcherTimer timer))
                {
                    timer.Stop();
                    timer.Tick -= Timer_Tick; // 移除事件处理器
                    _timers.Remove(accountId);
                }
            }
        }

        public void StopTimer(TOTPAccount account)
        {
            StopTimer(account.Id);
        }

        public void StopAllTimers()
        {
            lock (_lock)
            {
                var timersToStop = new List<DispatcherTimer>(_timers.Values);
                foreach (var timer in timersToStop)
                {
                    timer.Stop();
                    timer.Tick -= Timer_Tick;
                }
                _timers.Clear();
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            // 这个方法被用作占位符，在移除事件处理器时使用
        }

        private void UpdateAccountCode(TOTPAccount account)
        {
            try
            {
                // 计算TOTP码
                string code = GenerateTOTP(account.SecretKey, account.Algorithm, account.TimeStep);
                account.CurrentCode = code;

                // 计算剩余时间百分比
                long currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                long timeStep = currentTime / account.TimeStep;
                long timeStepStart = timeStep * account.TimeStep;
                long timeRemaining = timeStepStart + account.TimeStep - currentTime;
                double percentage = (double)timeRemaining / account.TimeStep * 100;
                account.TimeRemainingPercentage = Math.Max(0, percentage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新账户代码时出错: {ex.Message}");
                account.CurrentCode = "错误";
                account.TimeRemainingPercentage = 0;
            }
        }

        private string GenerateTOTP(string secret, string algorithm, int timeStep)
        {
            // 使用TOTPService中的方法
            return TOTPService.Instance.GenerateTOTP(secret, algorithm, timeStep);
        }
    }
}