using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmallWallet2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SplashPage : ContentPage
    {
        bool _ShouldDelayForSplash = true;
        public SplashPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            sjd();
        }
        private async void sjd()
        {
            if (_ShouldDelayForSplash)
            {
                // delay for a few seconds on the splash screen
                await Task.Delay(1000);
             Application.Current.MainPage = new AppShell();
              
            }
        }
    }
}