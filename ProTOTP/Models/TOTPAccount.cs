using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace ProTOTP.Models
{
    public class TOTPAccount : INotifyPropertyChanged
    {
        private string _currentCode;
        private double _timeRemainingPercentage;
        private string _progressBarColor = "Green";
        private string _codeTextColor = "Green";

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string AccountName { get; set; }
        public string SecretKey { get; set; }
        public string Description { get; set; }
        public string Algorithm { get; set; } = "SHA1";
        public int TimeStep { get; set; } = 30;
        public DateTime AddedDate { get; set; }

        [JsonIgnore]
        public string CurrentCode
        {
            get => _currentCode;
            set
            {
                _currentCode = value;
                OnPropertyChanged(nameof(CurrentCode));
            }
        }

        [JsonIgnore]
        public double TimeRemainingPercentage
        {
            get => _timeRemainingPercentage;
            set
            {
                _timeRemainingPercentage = value;
                OnPropertyChanged(nameof(TimeRemainingPercentage));
                
                // 根据剩余时间更新进度条颜色
                if (value > 66)
                {
                    ProgressBarColor = "Green";
                    CodeTextColor = "Green";
                }
                else if (value > 33)
                {
                    ProgressBarColor = "Orange";
                    CodeTextColor = "Orange";
                }
                else
                {
                    ProgressBarColor = "Red";
                    CodeTextColor = "Red";
                }
            }
        }

        [JsonIgnore]
        public string ProgressBarColor
        {
            get => _progressBarColor;
            set
            {
                _progressBarColor = value;
                OnPropertyChanged(nameof(ProgressBarColor));
            }
        }

        [JsonIgnore]
        public string CodeTextColor
        {
            get => _codeTextColor;
            set
            {
                _codeTextColor = value;
                OnPropertyChanged(nameof(CodeTextColor));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}