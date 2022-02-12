using Haley.Abstractions;
using Haley.Enums;
using Haley.Events;
using Haley.Models;
using Haley.MVVM;
using Haley.Utils;
using Haley.IOC;
using Haley.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NotificationDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private IDialogServiceEx _ds;
        public MainWindow()
        {
            InitializeComponent();
            _ds = ContainerStore.Singleton.DI.Resolve<IDialogServiceEx>();
            //_ds = new DialogService();
            tbxTitle.Text = "Demo";
            tbxMessage.Text = "Hello World!";
            //SolidColorBrush _accnnew = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFD7A26C");
            //SolidColorBrush _accnnewFg = (SolidColorBrush)new BrushConverter().ConvertFromString("blue");
            //_ds.ChangeAccentColors(_accnnew, _accnnewFg);
            _ds.StartupLocation = WindowStartupLocation.CenterOwner;
            _ds.TopMost = true;
            _ds.ShowInTaskBar = true;
            //_ds.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#661C4476");
            //_ds.Foreground = Brushes.White;
            cbxGlowColor.ItemsSource = ColorUtils.GetSystemColors();
        }

       

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (rbtndefaultNotify.IsChecked.HasValue && rbtndefaultNotify.IsChecked.Value)
                {
                    defaultNotificaton();
                }

                if (cbxBlurBackground.IsChecked.HasValue)
                {
                    _ds.EnableBackgroundBlur = cbxBlurBackground.IsChecked.Value;
                }

                if (rbtnHaleyNotify.IsChecked.HasValue && rbtnHaleyNotify.IsChecked.Value)
                {
                    if (rbtnTypeContainerView.IsChecked.HasValue && rbtnTypeContainerView.IsChecked.Value)
                    {
                        var _blur = cbxBlur.IsChecked.Value;
                        var rest = _ds.ShowContainerView<InputTest01>(tbxTitle.Text, blurOtherWindows: _blur);
                        var _vm = (DialogVM)rest.ContainerViewModel;
                    }
                    else
                    {
                        haleyNotification();
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }

        private void haleyNotification()
        {
            NotificationIcon _image = NotificationIcon.Info;

            if (rbtnError.IsChecked.HasValue && rbtnError.IsChecked.Value)
            {
                _image = NotificationIcon.Error;
            }

            if (rbtnWarn.IsChecked.HasValue && rbtnWarn.IsChecked.Value)
            {
                _image = NotificationIcon.Warning;
            }

            if (rbtnSuccess.IsChecked.HasValue && rbtnSuccess.IsChecked.Value)
            {
                _image = NotificationIcon.Success;
            }

            DialogMode dialogMode = DialogMode.Notification;

            if (rbtnTypeConfirm.IsChecked.HasValue && rbtnTypeConfirm.IsChecked.Value)
            {
                dialogMode = DialogMode.Confirmation;
            }
            if (rbtnTypeGetInput.IsChecked.HasValue && rbtnTypeGetInput.IsChecked.Value)
            {
                dialogMode = DialogMode.GetInput;
            }

            if (cbxGlowColor.SelectedItem != null && cbxGlowColor.SelectedItem is KeyValuePair<string, Color> kvp)
            {
                double.TryParse(tbxGlowRadius.Text, out double _gr);
                _ds.GlowColor = kvp.Value;
                _ds.GlowRadius = _gr;
            }

            if (rbtnModeNotify.IsChecked.Value)
            {
                var _blur = cbxBlur.IsChecked.Value;
                var result = _ds.ShowDialog(tbxTitle.Text, tbxMessage.Text, _image, dialogMode, blurOtherWindows: _blur);
                var dresult = result.DialogResult;
                tblckDialogResult.Text = dresult.ToString();
                var uinput = result.UserInput;
                tblckUserInput.Text = uinput;
            }
            else
            {
                _ds.SendToast(tbxTitle.Text, tbxMessage.Text, _image);
            }
        }

        private void defaultNotificaton()
        {
            MessageBoxImage _image = MessageBoxImage.Information;

            if (rbtnError.IsChecked.HasValue && rbtnError.IsChecked.Value)
            {
                _image = MessageBoxImage.Error;
            }

            if (rbtnWarn.IsChecked.HasValue && rbtnWarn.IsChecked.Value)
            {
                _image = MessageBoxImage.Warning;
            }

            if (rbtnSuccess.IsChecked.HasValue && rbtnSuccess.IsChecked.Value)
            {
                _image = MessageBoxImage.Question;
            }

            var result = MessageBox.Show(tbxMessage.Text, tbxTitle.Text, MessageBoxButton.OKCancel, _image);
        }
    }
}
