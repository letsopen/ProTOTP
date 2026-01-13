using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ProTOTP.ViewModels;

namespace ProTOTP
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private MainPageViewModel _viewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            this.InitializeComponent();
            _viewModel = new MainPageViewModel();
            this.DataContext = _viewModel;
            AccountsListView.ItemsSource = _viewModel.Accounts;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _viewModel.LoadAccounts();
        }

        private void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ProTOTP.Views.AddAccountPage));
        }

        private async void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var account = button.Tag as Models.TOTPAccount;
            
            bool result = await _viewModel.RemoveAccountAsync(account);
            if (result)
            {
                StatusTextBlock.Text = $"已删除账户: {account.AccountName}";
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _viewModel.StopTimers();
            base.OnNavigatedFrom(e);
        }
    }
}