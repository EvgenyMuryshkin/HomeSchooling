using System;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PhotosClient
{
    public partial class App : Application
    {
        public static int ThreadId;

        public static bool IsOnMainThread()
        {
            return ThreadId == Thread.CurrentThread.ManagedThreadId;
        }

        public App()
        {
            InitializeComponent();

            ThreadId = Thread.CurrentThread.ManagedThreadId;

            MainPage = new NavigationPage(new MainPage()) { };
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
