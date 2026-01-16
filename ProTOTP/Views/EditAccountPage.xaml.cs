using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ProTOTP.Models;
using ProTOTP.Services;

namespace ProTOTP.Views
{
    public sealed partial class EditAccountPage : Page
    {
        private TOTPAccount _editingAccount;

        public EditAccountPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _editingAccount = e.Parameter as TOTPAccount;
            
            if (_editingAccount != null)
            {
                AccountNameTextBox.Text = _editingAccount.AccountName;
                SecretKeyTextBox.Text = _editingAccount.SecretKey;
                DescriptionTextBox.Text = _editingAccount.Description;
                
                // 设置算法选择
                for (int i = 0; i < AlgorithmComboBox.Items.Count; i++)
                {
                    var item = (ComboBoxItem)AlgorithmComboBox.Items[i];
                    if (item.Content.ToString() == _editingAccount.Algorithm)
                    {
                        AlgorithmComboBox.SelectedIndex = i;
                        break;
                    }
                }
                
                TimeStepTextBox.Text = _editingAccount.TimeStep.ToString();
            }
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

            // 更新账户信息
            _editingAccount.AccountName = AccountNameTextBox.Text.Trim();
            _editingAccount.SecretKey = SecretKeyTextBox.Text.Trim();
            _editingAccount.Description = DescriptionTextBox.Text.Trim();
            _editingAccount.Algorithm = algorithm;
            _editingAccount.TimeStep = timeStep;

            // 保存更改
            await TOTPService.Instance.SaveAccountsAsync();

            await new Windows.UI.Popups.MessageDialog("账户更新成功！").ShowAsync();
            Frame.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}