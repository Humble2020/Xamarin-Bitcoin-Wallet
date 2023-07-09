using Newtonsoft.Json;
using SmallWallet2.Common;
using SmallWallet2.Models;
using SmallWallet2.src;
using SmallWallet2.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SmallWallet2.ViewModels.VM
{
    public class MyWalletsViewModel : BaseViewModel, INotifyPropertyChanged
    {
        public List<WalletInfo> Wallets { get; set; }
        public walletViewModel wVm { get; set; }
        public INavigation Navigation;
        WalletInfo WalletInfo = new WalletInfo();
        public MyWalletsViewModel(INavigation navigation)
        {
            Navigation = navigation;
             Wallets = WalletInfo.AvailableWallets().ToList();
        }
        public async void CheckIfEmpty()
        {
            IsLoading = true;
            if (Wallets.Count() == 0)
            {
                await Device.InvokeOnMainThreadAsync(async () =>
                {
                    await Navigation.PushAsync(new CreatePage(Navigation)).ConfigureAwait(false);

                });
               
                // Routing.RegisterRoute("CreateWallet", typeof(CreateBitcoin));

            }
            IsLoading = false;
        }
        private string btcstring;

        public string BTCString
        {
            get { return btcstring; }
            set { btcstring = value;
                OnPropertyChanged();
            }
        }
        private bool onlinebool;

        public bool Onlinebool
        {
            get { return onlinebool; }
            set { onlinebool = value;
                OnPropertyChanged();
            }
        }
        private bool offlinebool;

        public bool Offlinebool
        {
            get { return offlinebool; }
            set { offlinebool = value;
                OnPropertyChanged();
            }
        }


        private string BASE_URI = "https://min-api.cryptocompare.com/data/price?fsym=BTC&tsyms=USD";
        public double hj;
        public async void firstrun()
        {
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                IsLoading = true;
                if (CheckForInternetConnection())
                {
                    HttpResponseMessage response2;
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                        response2 = await httpClient.GetAsync($"{BASE_URI}").ConfigureAwait(false);
                    }
                    string stud = await response2.Content.ReadAsStringAsync().ConfigureAwait(false);
                    dynamic array = JsonConvert.DeserializeObject(stud);
                    decimal jhk;
                  
                    foreach (var item in array)
                    {
                        var nk = item;
                        var j = nk.ToString();
                        var bk = j.Replace("USD", "").Replace(":", "").Replace(@"""", "").Replace(@"\", "").Replace(@"/", "");
                        var jh = bk;
                        jhk = Decimal.Parse(jh); // BTC price in USD
                        BTCString = "Exchange: " + jhk.ToString("C", new CultureInfo("en-US"));
                    }

                }
                else
                {
                }
                IsLoading = false;
            });
           
        }
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("http://clients3.google.com/generate_204"))
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
        private ICommand _selectWalletCommand;
        public ICommand SelectWalletCommand => _selectWalletCommand ??= new Command<WalletInfo>(async (value) => await Task.WhenAll( OnWalletTapped(value)));
        private async Task OnWalletTapped(WalletInfo wallet)
        {
            IsLoading = true;
            var lstColltn = wallet;
                   //await Task.Delay(0);
                   //wVm = new walletViewModel(Navigation, "password9s0ru89iwuQO7852keirsopsovjisolijsntnlrsuhtusilIKSNCIH937484kgd", lstColltn.Path);//wallet path
                   //Preferences.Set("Pref_WalletPath", lstColltn.Path);
                   //wVm.Update();
                   //await Navigation.PushAsync(new ClickPage(wVm)).ConfigureAwait(false);
            walletViewModel mainViewModel = null;

            await Task.Run(() =>
            {
               
                mainViewModel = new walletViewModel(Navigation, "password9s0ru89iwuQO7852keirsopsovjisolijsntnlrsuhtusilIKSNCIH937484kgd", lstColltn.Path);
                mainViewModel.Update();
            });

            await Navigation.PushAsync(new ClickPage(mainViewModel, Navigation)).ConfigureAwait(false);
       
            IsLoading = false;
        }
        private float _opacity = 1f;
        public float Opacity
        {
            get => _opacity;
            set { _opacity = value; OnPropertyChanged(nameof(Opacity)); }
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading == value)
                    return;

                _isLoading = value;

                if (_isLoading)
                    Opacity = 0.3f;
                else
                    Opacity = 1f;

                OnPropertyChanged(nameof(IsLoading));
            }
        }

    }
}
