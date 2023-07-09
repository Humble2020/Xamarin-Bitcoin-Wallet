using Newtonsoft.Json;
using SmallWallet2.Common;
using SmallWallet2.src;
using SmallWallet2.ViewModels.VM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmallWallet2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyWalletPage : ContentPage
    {
        MyWalletsViewModel MywVM;
       public MyWalletPage()
        {
           InitializeComponent();
            BindingContext = MywVM = new MyWalletsViewModel(Navigation);
        }
        public walletViewModel wVm;
        private async void OnItemTapped(object sender, EventArgs args)
        {
       
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                Frame selectedItem = (Frame)sender;
                selectedItem.IsEnabled = false;
                Color initColor = selectedItem.BackgroundColor;
                selectedItem.BackgroundColor = Color.FromHex("#6A2C8B");
                await Task.Delay(500);
                selectedItem.BackgroundColor = initColor;
                selectedItem.IsEnabled = true;

            });
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("http://clients3.google.com/generate_204"))//http://clients3.google.com/generate_204
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        protected override void OnAppearing()
        {
         base.OnAppearing();
            MywVM.CheckIfEmpty();
                if (CheckForInternetConnection())
                {
                     onlinet.IsVisible = true;
                        offlinet.IsVisible = false;
                  
                    
                    MywVM.firstrun();
                }
                else
                {
                      offlinet.IsVisible = true;
                        onlinet.IsVisible = false;
                  
                   
                }
               
       
        }
               
    }
}