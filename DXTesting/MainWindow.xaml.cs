﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows;

namespace DXTesting
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; } // тут будет форма
        
        public MainWindow()
        {
            
            

            InitializeComponent();

            //Connectionz cons = Connectionz.getInstance(1);

            Instance = this;

            GrabButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            ConnectButton.IsEnabled = true;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            Connectionz cons = Connectionz.getInstance();
            cons.ConnectAll();
        }

        private void GrabButton_Click(object sender, RoutedEventArgs e)
        {
            Connectionz cons = Connectionz.getInstance();
            cons.Grab();
            Instance.GrabButton.IsEnabled = false;
            Instance.StopButton.IsEnabled = true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Connectionz cons = Connectionz.getInstance();
            cons.Stop();
            Instance.GrabButton.IsEnabled = true;
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_1(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}
