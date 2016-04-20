using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;

namespace Product_Browser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += OnDispatcherUnhandledException;
        }

        void OnDispatcherUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }

        void OnDispatcherUnhandledException(object sender,  FirstChanceExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception);
        }
    }
}
