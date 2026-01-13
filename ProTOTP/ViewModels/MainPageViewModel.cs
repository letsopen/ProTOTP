using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using ProTOTP.Models;
using ProTOTP.Services;

namespace ProTOTP.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TOTPAccount> Accounts { get; private set; }
        private TOTPService _totpService;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPageViewModel()
        {
            _totpService = TOTPService.Instance;
            Accounts = _totpService.Accounts;
        }

        public async Task LoadAccounts()
        {
            await _totpService.LoadAccountsAsync();
        }

        public async Task<bool> RemoveAccountAsync(TOTPAccount account)
        {
            return await _totpService.RemoveAccountAsync(account);
        }

        public void StopTimers()
        {
            // 停止所有相关的计时器
            TOTPService.Instance.StopAllTimers();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}