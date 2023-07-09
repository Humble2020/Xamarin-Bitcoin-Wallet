using SmallWallet2.Services;
using SmallWallet2.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmallWallet2
{
    public partial class App : Application
    {
        private const string StateKey = "State";
        public App()
        {
            //AppCenterSetup.Instance.Start(
            //"",
            //"2cb54d83cd8c51801408298ceb0b7eaf3a43b362",
            //true);
            //Trace.Listeners.Add(new AppCenterTraceListener());
            InitializeComponent();
            // The navigation logic startup needs to diverge per platform in order to meet the UX design requirements
            MainPage = new SplashPage();
           
        }
        //ios
        //android key
        //DependencyService.Register<MockDataStore>();
        //MainPage = new NavigationPage(new MyWalletPage());

        //var splashPage = new NavigationPage(new SplashPage());
        //MainPage = splashPage;
    
        //09529625359aee8ae528a3a905a17d3678aa27dd               appcenter api

       // protected override void OnStart()
       //=> Trace.Write(
       //    new AnalyticsEvent(
       //        nameof(Application),
       //        new Dictionary<string, string>
       //        {
       //             { StateKey, nameof(OnStart) }
       //        }));

        //protected override void OnSleep()
        //    => Trace.Write(
        //        new AnalyticsEvent(
        //            nameof(Application),
        //            new Dictionary<string, string>
        //            {
        //            { StateKey, nameof(OnSleep) }
        //            }));

        //protected override void OnResume()
        //    => Trace.Write(
        //        new AnalyticsEvent(
        //            nameof(Application),
        //            new Dictionary<string, string>
        //            {
        //            { StateKey, nameof(OnResume) }
        //            }));
    }
}
