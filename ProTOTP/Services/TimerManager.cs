using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using ProTOTP.Models;


namespace ProTOTP.Services.Internal
{ // 使用内部命名空间避免与TOTPService冲突
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
            bool wasInRedZone = account.TimeRemainingPercentage <= 33; // 检查之前是否在红色区域
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
            try
            {
                // 移除可能的空格和特殊字符
                string cleanSecret = secret.Replace(" ", "").Replace("-", "").Trim();

                // 将Base32编码的密钥转换为字节数组
                byte[] key = Base32Decode(cleanSecret);

                // 获取当前时间步数
                long counter = DateTimeOffset.Now.ToUnixTimeSeconds() / timeStep;

                // 将计数器转换为字节数组（8字节，大端序）
                byte[] counterBytes = new byte[8];
                for (int i = 0; i < 8; i++)
                {
                    counterBytes[i] = (byte)((counter >> (56 - 8 * i)) & 0xFF);
                }

                // 选择适当的哈希算法
                using (var hmac = CreateHMAC(algorithm, key))
                {
                    byte[] hash = hmac.ComputeHash(counterBytes);
                    
                    // 使用动态截断获取4字节的哈希值
                    int offset = hash[hash.Length - 1] & 0x0F;
                    int binary = ((hash[offset] & 0x7F) << 24) |
                                ((hash[offset + 1] & 0xFF) << 16) |
                                ((hash[offset + 2] & 0xFF) << 8) |
                                (hash[offset + 3] & 0xFF);

                    // 计算模数以获得指定长度的代码
                    int mod = 1;
                    for (int i = 0; i < 6; i++)
                    {
                        mod *= 10;
                    }

                    int code = binary % mod;

                    // 确保代码有正确的位数，前面补0
                    return code.ToString().PadLeft(6, '0');
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"生成TOTP时出错: {ex.Message}");
                return "错误";
            }
        }
        
        private System.Security.Cryptography.HMAC CreateHMAC(string algorithm, byte[] key)
        {
            switch (algorithm.ToUpper())
            {
                case "SHA256":
                    return new System.Security.Cryptography.HMACSHA256(key);
                case "SHA512":
                    return new System.Security.Cryptography.HMACSHA512(key);
                case "SHA1":
                default:
                    return new System.Security.Cryptography.HMACSHA1(key);
            }
        }
        
        private byte[] Base32Decode(string input)
        {
            // 移除填充字符
            string cleanInput = input.TrimEnd('=');

            // Base32字符到数值的映射
            var charMap = new Dictionary<char, int>();
            for (int i = 0; i < 26; i++)
            {
                charMap[(char)('A' + i)] = i;
            }
            for (int i = 0; i < 6; i++)
            {
                charMap[(char)('2' + i)] = 26 + i;
            }

            // 将输入转换为5位组
            var bits = new List<int>();
            foreach (char c in cleanInput.ToUpper())
            {
                if (charMap.ContainsKey(c))
                {
                    int value = charMap[c];
                    // 将5位值添加到比特流中
                    for (int i = 4; i >= 0; i--)
                    {
                        bits.Add((value >> i) & 1);
                    }
                }
            }

            // 将比特流转换为字节数组
            var result = new List<byte>();
            for (int i = 0; i < bits.Count / 8; i++)
            {
                byte b = 0;
                for (int j = 0; j < 8; j++)
                {
                    b = (byte)((b << 1) | bits[i * 8 + j]);
                }
                result.Add(b);
            }

            return result.ToArray();
        }
    }
}