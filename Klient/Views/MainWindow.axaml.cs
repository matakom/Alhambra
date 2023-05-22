using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using System;
using Avalonia.Markup.Xaml;
using System.Windows;

namespace Klient.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //Topmost = true;
            Height = 1080;
            Width = 1920;
            //this.WindowState = WindowState.Maximized;
            InitializeComponent();
        }
    }
}