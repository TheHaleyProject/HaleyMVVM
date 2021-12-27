using Haley.Abstractions;
using Haley.Enums;
using Haley.Events;
using Haley.Models;
using Haley.MVVM;
using Haley.Utils;
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
using System.Windows.Shapes;


namespace NotificationDemo
{
    /// <summary>
    /// Interaction logic for GlobalizationWindow.xaml
    /// </summary>
    public partial class GlobalizationWindow : Window
    {
        public GlobalizationWindow()
        {
            InitializeComponent();
            cmbbxLanguages.Items.Add("en");
            cmbbxLanguages.Items.Add("de");
            cmbbxLanguages.Items.Add("zu");
        }

        private void cmbbxLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LangUtils.ChangeCulture(cmbbxLanguages.SelectedValue as string);
        }
    }
}
