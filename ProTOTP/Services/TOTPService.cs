using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
using ProTOTP.Models;
using Windows.UI.Xaml;
using ProTOTP.Services.Internal; // 引用TimerManager的正确命名空间

namespace ProTOTP.Services
{
    public class TOTPService
    {
        private static TOTPService _instance;
        public static TOTPService Instance => _instance ?? (_instance = new TOTPService());

        private List<TOTPAccount> _accounts;
        private const string FileName = "totp_accounts.json";
        private const int CodeLength = 6;

        public ObservableCollection<TOTPAccount> Accounts { get; private set; }

        private TOTPService()
        {
            _accounts = new List<TOTPAccount>();
            Accounts = new ObservableCollection<TOTPAccount>();
        }

        public async Task<bool> AddAccountAsync(TOTPAccount account)
        {
            try
            {
                // 检查账户是否已存在
                if (_accounts.Any(a => a.AccountName == account.AccountName))
                {
                    return false;
                }

                _accounts.Add(account);
                Accounts.Add(account);

                // 保存到本地存储
                await SaveAccountsAsync();

                // 启动计时器更新验证码
                StartAccountTimer(account);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RemoveAccountAsync(TOTPAccount account)
        {
            try
            {
                if (_accounts.Contains(account))
                {
                    _accounts.Remove(account);
                    Accounts.Remove(account);

                    // 停止相关的计时器
                    StopAccountTimer(account);

                    // 保存到本地存储
                    await SaveAccountsAsync();

                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<TOTPAccount>> LoadAccountsAsync()
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    FileName, CreationCollisionOption.OpenIfExists);

                string json = await FileIO.ReadTextAsync(file);
                if (!string.IsNullOrEmpty(json))
                {
                    _accounts = JsonConvert.DeserializeObject<List<TOTPAccount>>(json) ?? new List<TOTPAccount>();
                }
                else
                {
                    _accounts = new List<TOTPAccount>();
                }

                // 清空当前的ObservableCollection
                Accounts.Clear();

                // 添加账户到ObservableCollection
                foreach (var account in _accounts)
                {
                    Accounts.Add(account);
                    
                    // 为每个账户启动计时器
                    StartAccountTimer(account);
                }

                return _accounts;
            }
            catch (Exception)
            {
                _accounts = new List<TOTPAccount>();
                return _accounts;
            }
        }

        public async Task SaveAccountsAsync()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_accounts, Formatting.Indented);
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    FileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, json);
            }
            catch (Exception)
            {
                // 记录错误或处理异常
            }
        }

        public void StopAllTimers()
        {
            Internal.TimerManager.Instance.StopAllTimers();
        }

        private void StartAccountTimer(TOTPAccount account)
        {
            Internal.TimerManager.Instance.StartTimer(account);
        }

        private void StopAccountTimer(TOTPAccount account)
        {
            Internal.TimerManager.Instance.StopTimer(account);
        }

        public string GenerateTOTP(string secret, string algorithm, int timeStep)
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
                    for (int i = 0; i < CodeLength; i++)
                    {
                        mod *= 10;
                    }

                    int code = binary % mod;

                    // 确保代码有正确的位数，前面补0
                    return code.ToString().PadLeft(CodeLength, '0');
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"生成TOTP时出错: {ex.Message}");
                return "错误";
            }
        }

        private HMAC CreateHMAC(string algorithm, byte[] key)
        {
            switch (algorithm.ToUpper())
            {
                case "SHA256":
                    return new HMACSHA256(key);
                case "SHA512":
                    return new HMACSHA512(key);
                case "SHA1":
                default:
                    return new HMACSHA1(key);
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