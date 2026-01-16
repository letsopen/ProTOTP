using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using ProTOTP.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Core;

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

        private async void CodeTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            var account = textBlock.DataContext as Models.TOTPAccount;
            
            if (!string.IsNullOrEmpty(account?.CurrentCode))
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(account.CurrentCode);
                Clipboard.SetContent(dataPackage);
                
                StatusTextBlock.Text = "验证码已复制到剪贴板";
                
                // 临时改变状态文本颜色以提供视觉反馈
                StatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                
                // 重置状态文本颜色
                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2);
                timer.Tick += (s, args) =>
                {
                    StatusTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                    timer.Stop();
                };
                timer.Start();
            }
        }

        private void CodeTextBlock_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
        }

        private void CodeTextBlock_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
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