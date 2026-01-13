using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ProTOTP.Models;
using ProTOTP.Services;

namespace ProTOTP.Views
{
    public sealed partial class AddAccountPage : Page
    {
        public AddAccountPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 验证输入
            if (string.IsNullOrWhiteSpace(AccountNameTextBox.Text))
            {
                await new Windows.UI.Popups.MessageDialog("请输入账户名称").ShowAsync();
                return;
            }

            if (string.IsNullOrWhiteSpace(SecretKeyTextBox.Text))
            {
                await new Windows.UI.Popups.MessageDialog("请输入TOTP密钥").ShowAsync();
                return;
            }

            int timeStep = 30;
            if (!string.IsNullOrEmpty(TimeStepTextBox.Text) && !int.TryParse(TimeStepTextBox.Text, out timeStep))
            {
                await new Windows.UI.Popups.MessageDialog("时间步长必须是一个有效的数字").ShowAsync();
                return;
            }

            // 获取选择的算法
            string algorithm = "SHA1"; // 默认值
            if (AlgorithmComboBox.SelectedItem != null)
            {
                algorithm = ((ComboBoxItem)AlgorithmComboBox.SelectedItem).Content.ToString();
            }

            // 创建新的TOTP账户
            var account = new TOTPAccount
            {
                AccountName = AccountNameTextBox.Text.Trim(),
                SecretKey = SecretKeyTextBox.Text.Trim(),
                Description = DescriptionTextBox.Text.Trim(),
                Algorithm = algorithm,
                TimeStep = timeStep,
                AddedDate = DateTime.Now
            };

            // 保存账户
            bool success = await TOTPService.Instance.AddAccountAsync(account);

            if (success)
            {
                await new Windows.UI.Popups.MessageDialog("账户添加成功！").ShowAsync();
                Frame.GoBack();
            }
            else
            {
                await new Windows.UI.Popups.MessageDialog("账户添加失败，请重试。").ShowAsync();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}