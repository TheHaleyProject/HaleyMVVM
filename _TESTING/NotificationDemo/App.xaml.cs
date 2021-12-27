using Haley.Abstractions;
using Haley.Enums;
using Haley.Events;
using Haley.Models;
using Haley.MVVM;
using Haley.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Haley.WPF.Controls;
using System.Windows.Media;
using Haley.Services;

namespace NotificationDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ContainerRegistrations();

            //directDialogServiceTest();
            //directNotificationTest
            ClientTest();
        }

        private void ContainerRegistrations()
        {
             ContainerStore.Singleton.Controls.Register<DialogVM, InputTest01>(mode:RegisterMode.Transient);
        }

        private void ClientTest()
        {
            MainWindow _wndw = new MainWindow();
            _wndw.ShowDialog();
        }

        private void directNotificationTest()
        {
            Notification _nfc = new Notification();
            _nfc.Title = "Test Demo";
            _nfc.Content = "This is a mistake";
            _nfc.NotificationIcon = NotificationIcon.Error;
            _nfc.ContainerView = new InputTest01();
            _nfc.Type = DisplayType.GetInput;
            _nfc.ShowDialog();
        }

        private void directDialogServiceTest()
        {
            var _ds = new DialogService();
            _ds.SetGlow(Colors.White, 3.0);
            _ds.ShowDialog("Confirmation Error", "What are you trying to do bugga?",mode:DialogMode.Confirmation);
        }
    }
}
