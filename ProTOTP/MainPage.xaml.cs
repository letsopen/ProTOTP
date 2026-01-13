using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ProTOTP.ViewModels;

namespace ProTOTP
{
    public sealed partial class MainPage : Page
    {
        private MainPageViewModel _viewModel;

        // 注意：不需要实现INotifyPropertyChanged，因为MainPage不需要通知属性更改

        public MainPage()
        {
            this.InitializeComponent();
            _viewModel = new MainPageViewModel();
            this.DataContext = _viewModel;
            AccountsListView.ItemsSource = _viewModel.Accounts;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await _viewModel.LoadAccounts();
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